using Sayarah.Api.Models;
using System.Globalization;
using Sayarah.Providers;
using Abp.Application.Services.Dto;
using Newtonsoft.Json;
using Abp.Domain.Repositories;
using Sayarah.Companies;
using Abp.UI;
using Sayarah.Configuration;
using Microsoft.AspNetCore.Mvc;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Application.Veichles;
using Sayarah.Application.Helpers.StoredProcedures;
using Sayarah.Application.Providers;
using Sayarah.Application.Transactions.FuelTransactions;
using Sayarah.Application.CompanyInvoices;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Core.Helpers;
using Abp.AspNetCore.Mvc.Controllers;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Wallets.Dto;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Helpers;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Sayarah.Api.Controllers
{
    public class WorkerController : AbpController
    {
        private readonly IVeichleAppService _veichleAppService;
        public readonly IRepository<Worker, long> _workerRepository;
        public readonly IRepository<Provider, long> _providerRepository;
        public readonly IRepository<Branch, long> _branchRepository;
        private readonly IFuelTransOutAppService _fuelTransOutAppService;
        private readonly IFuelPumpAppService _fuelPumpAppService;
        private readonly IStoredProcedureAppService _storedProcedureAppService;
        private readonly ICompanyInvoiceJopAppService _companyInvoiceJopAppService;
        private readonly UploadWebPController _uploadController;

        public WorkerController(

        IVeichleAppService veichleAppService,
        IRepository<Worker, long> workerRepository,
        IRepository<Provider, long> providerRepository,
        IFuelTransOutAppService fuelTransOutAppService,
        IFuelPumpAppService fuelPumpAppService,
        IRepository<Branch, long> branchRepository,
        IStoredProcedureAppService storedProcedureAppService,
        ICompanyInvoiceJopAppService companyInvoiceJopAppService,
                   UploadWebPController uploadController
                               )
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;

            _veichleAppService = veichleAppService;
            _fuelPumpAppService = fuelPumpAppService;
            _fuelTransOutAppService = fuelTransOutAppService;
            _workerRepository = workerRepository;
            _providerRepository = providerRepository;
            _branchRepository = branchRepository;
            _storedProcedureAppService = storedProcedureAppService;
            _companyInvoiceJopAppService = companyInvoiceJopAppService;
            _uploadController=uploadController;
        }

        CultureInfo new_lang = new CultureInfo("ar");


        [HttpPost]
        [Language("Lang")]
        public async Task<GetProviderDetailsOutput> GetProviderDetails(EntityDto<long> input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new GetProviderDetailsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var provider = await _providerRepository.FirstOrDefaultAsync(a => a.Id == input.Id);
                if (provider != null)
                {
                    // get list of fuel pumps 
                    var fuelPumps = await _fuelPumpAppService.GetAllAsync(new GetFuelPumpsInput { ProviderId = provider.Id, MaxCount = true });

                    return new GetProviderDetailsOutput
                    {
                        Success = true,
                        Provider = ObjectMapper.Map<ApiProviderDto>(provider),
                        FuelPumps = ObjectMapper.Map<List<ApiFuelPumpDto>>(fuelPumps.Items)
                    };
                }
                return new GetProviderDetailsOutput { };
            }
            catch (Exception ex)
            {
                return new GetProviderDetailsOutput { Message = ex.Message };
            }
        }

      


        [HttpPost]
        [Language("Lang")]
        public async Task<GetVeichleDetailsOutput> GetVeichleDetails(GetVeichlesInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new GetVeichleDetailsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var result = await _veichleAppService.GetBySim(input);


                if (result != null && result.Success == true)
                {

                    // get list of latest fuel transactions
                    //var transactions = await _fuelTransOutAppService.GetAllAsync(new GetFuelTransOutsInput
                    //{
                    //    VeichleId = result.Veichle.Id,
                    //    SkipCount = 0,
                    //    MaxResultCount = 10
                    //});

                    CheckWorkingDays(new WorkingDaysInput { workingDays = result.Veichle.WorkingDays });

                    if (result.Veichle.FuelType.HasValue == false)
                        throw new UserFriendlyException(L("Pages.Veichles.Messages.FuelTypeError"));

                    var fuelPrice = await _fuelTransOutAppService.GetFuelPrice(new GetFuelPriceInput { FuelType = result.Veichle.FuelType.Value, VeichleId = result.Veichle.Id });

                    string _fuelColor = await ReturnFuelColor(result.Veichle.FuelType.Value);



                    if (result.VeichleBalanceInSar == 0 && result.VeichleBalanceInLitre > 0)
                    {
                        // calculate VeichleBalanceInSar
                        result.VeichleBalanceInSar = fuelPrice * result.VeichleBalanceInLitre;
                    }

                    else if (result.VeichleBalanceInSar > 0 && result.VeichleBalanceInLitre == 0)
                    {
                        // calculate VeichleBalanceInLitre
                        result.VeichleBalanceInLitre = result.VeichleBalanceInSar / fuelPrice;
                    }
                    //decimal availableBranchAmount = result.Veichle.Branch.WalletAmount - result.Veichle.Branch.Reserved;
                    decimal availableBranchAmount = result.Veichle.Branch.WalletAmount;
                    decimal availableBranchLitres = 0;
                    if (fuelPrice > 0)
                        availableBranchLitres = availableBranchAmount / fuelPrice;
                    else
                        availableBranchLitres = 0;

                    return new GetVeichleDetailsOutput
                    {
                        Success = true,
                        Veichle = ObjectMapper.Map<ApiVeichleDto>(result.Veichle),
                        //Transactions = ObjectMapper.Map<List<ApiFuelTransOutDto>>(transactions.Items),
                        Transactions = new List<ApiFuelTransOutDto>(),
                        FuelPrice = fuelPrice,

                        VeichleBalanceInSar = result.VeichleBalanceInSar > result.Veichle.Branch.FuelAmount ? result.Veichle.Branch.FuelAmount : result.VeichleBalanceInSar,
                        VeichleBalanceInLitre = result.VeichleBalanceInLitre > availableBranchLitres ? availableBranchLitres : result.VeichleBalanceInLitre,
                        GroupType = result.GroupType,
                        PeriodConsumptionType = result.PeriodConsumptionType,
                        MaximumRechargeAmount = result.MaximumRechargeAmount,
                        BranchReserved = result.Veichle.Branch.Reserved,

                        BranchWalletAmount = result.Veichle.Branch.WalletAmount  ,
                        BranchFuelAmount = result.Veichle.Branch.FuelAmount ,
                        BranchCleanAmount = result.Veichle.Branch.CleanAmount,
                        BranchMaintainAmount = result.Veichle.Branch.MaintainAmount,

                        CounterPicIsRequired = result.CounterPicIsRequired,
                        FuelColor = _fuelColor
                    };
                }
                else
                {
                    // here => mobile will call the update vechile service
                    return new GetVeichleDetailsOutput
                    {
                        Success = true,
                        NotFound = true,
                        NotFoundSim = result.NotFoundSim,
                        Message = result.Message
                    };
                }
            }
            catch (Exception ex)
            {
                return new GetVeichleDetailsOutput { Message = ex.Message };
            }
        }


        [HttpPost]
        [Language("Lang")]
        public void CheckWorkingDays(WorkingDaysInput input)
        {

            DayOfWeek currentDay = DateTime.UtcNow.Date.DayOfWeek;
            if (string.IsNullOrEmpty(input.workingDays))
                throw new UserFriendlyException(L("Pages.Veichles.Messages.NotAvailableInThisDay"));

            var daysString = input.workingDays.Split(';');

            daysString = daysString.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            var exsit = daysString.Any(a => Convert.ToInt32(a) == Convert.ToInt32(currentDay));
            if (exsit == false)
                throw new UserFriendlyException(L("Pages.Veichles.Messages.NotAvailableInThisDay"));

        }




        [HttpPost]
        [Language("Lang")]
        public async Task<string> ReturnFuelColor(FuelType fuelType)
        {
            try
            {
                string _91color = await SettingManager.GetSettingValueAsync(AppSettingNames._91color);
                string _95color = await SettingManager.GetSettingValueAsync(AppSettingNames._95color);
                string _Dieselcolor = await SettingManager.GetSettingValueAsync(AppSettingNames._Dieselcolor);

                string _fuelColor = string.Empty;

                if (fuelType == FuelType._91)
                    _fuelColor = _91color;
                else if (fuelType == FuelType._95)
                    _fuelColor = _95color;
                else if (fuelType == FuelType.diesel)
                    _fuelColor = _Dieselcolor;

                return _fuelColor;

            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }




        [HttpPost]
        [Language("Lang")]
        [Consumes("multipart/form-data")]
        public async Task<GetVeichleDetailsOutput> UpdateVeichleDetails()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new GetVeichleDetailsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var form = HttpContext.Request.Form;

                if (!form.ContainsKey("Data"))
                    return new GetVeichleDetailsOutput { Message = L("MobileApi.Messages.InvalidRequest") };

                var jsonData = form["Data"];
                var input = JsonConvert.DeserializeObject<UpdateVeichleSimPicDto>(jsonData);

                // Check if vehicle exists
                await _veichleAppService.CheckVeichleExists(input);

                var _medias = new List<CreateVeichlePicDto>();

                if (form.Files != null && form.Files.Count > 0)
                {
                    foreach (var file in form.Files)
                    {
                        if (file.Length == 0)
                            continue;

                        var uploadOptions = new NewUploadFilesDto
                        {
                            StorageLocation = 5,
                            AllowedTypes = 0,
                            Sizes = "800&600",
                            UploadStyle = NewUploadStyle.BothOfThem
                        };
                        // Make sure _uploadController is injected or refactor this method to a service
                        var uploadedFileName = await _uploadController.UploadPhotoWebP(file, uploadOptions);

                        _medias.Add(new CreateVeichlePicDto
                        {
                            FilePath = uploadedFileName.Value
                        });
                    }
                }

                input.VeichleMedias = _medias;

                var updatedVechile = await _veichleAppService.UpdateVeichleDetails(input);

                if (updatedVechile != null)
                {
                    var details = await GetVeichleDetails(new GetVeichlesInput { SimNumber = input.SimNumber });

                    return new GetVeichleDetailsOutput
                    {
                        Message = L("Pages.Veichles.Messages.VeichleUpdated"),
                        Veichle = details.Veichle,
                        Transactions = details.Transactions,
                        FuelPrice = details.FuelPrice,
                        Success = true
                    };
                }
                else
                {
                    return new GetVeichleDetailsOutput { Message = L("MobileApi.Messages.ErrorOccurred") };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return new GetVeichleDetailsOutput { Message = ex.Message };
            }
        }


        [HttpPost]
        [Language("Lang")]
        public async Task<UpdateDriverCodeOutput> ConfirmDriverCode(UpdateDriverCodeInput input)
        {
            try
            {
                if (string.IsNullOrEmpty(input.SimNumber))
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.ChipNumberError"));

                var result = await _veichleAppService.ConfirmDriverCode(input);

                var fuelPrice = await _fuelTransOutAppService.GetFuelPrice(new GetFuelPriceInput { FuelType = result.Veichle.FuelType.Value, VeichleId = result.Veichle.Id });

                CheckWorkingDays(new WorkingDaysInput { workingDays = result.Veichle.WorkingDays });

                return result;
            }
            catch (Exception ex)
            {
                return new UpdateDriverCodeOutput { Message = ex.Message };
            }
        }


        [HttpPost]
        [Language("Lang")]
        [Consumes("multipart/form-data")]

        public async Task<CreateTransactionOutput> InitiateFuelTransOut()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new CreateTransactionOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var form = HttpContext.Request.Form;

                if (!form.ContainsKey("Data"))
                    return new CreateTransactionOutput { Message = L("MobileApi.Messages.InvalidRequest") };

                var jsonData = form["Data"];
                var input = JsonConvert.DeserializeObject<CreateFuelTransOutDto>(jsonData);

                var veichle = await _veichleAppService.GetAsync(new EntityDto<long> { Id = input.VeichleId.Value });
                input.BranchId = veichle.BranchId;
                input.VeichleId = veichle.Id;
                input.DriverId = veichle.DriverId;

                await CheckBranchWallet(new CheckBranchWalletInput
                {
                    Amount = input.Price,
                    BranchId = input.BranchId.Value,
                    WalletType = WalletType.Fuel
                });

                if (form.Files != null && form.Files.Count > 0)
                {
                    for (int i = 0; i < form.Files.Count; i++)
                    {
                        var file = form.Files[i];
                        if (file.Length == 0)
                            continue;

                        var uploadOptions = new NewUploadFilesDto
                        {
                            StorageLocation = 9,
                            AllowedTypes = 0,
                            UploadStyle = NewUploadStyle.BothOfThem
                        };

                        var uploadedFileName = await _uploadController.UploadPhotoWebP(file, uploadOptions);
                        var fileKey = file.Name;

                        if (fileKey.Contains("BeforeBoxPic"))
                        {
                            input.BeforeBoxPic = uploadedFileName.Value;
                        }
                        if (fileKey.Contains("BeforeCounterPic"))
                        {
                            input.BeforeCounterPic = uploadedFileName.Value;
                        }
                    }
                }

                var worker = await _workerRepository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);
                if (worker != null)
                {
                    input.ProviderId = worker.ProviderId;
                    input.WorkerId = worker.Id;
                }

                var fuelPrice = await _fuelTransOutAppService.GetFuelPrice(new GetFuelPriceInput
                {
                    FuelType = veichle.FuelType.Value,
                    VeichleId = input.VeichleId
                });
                input.FuelPrice = fuelPrice;

                var transaction = await _fuelTransOutAppService.CreateAsync(input);

                if (transaction != null)
                    return new CreateTransactionOutput
                    {
                        Message = L("MobileApi.Messages.BeforePicsCreated"),
                        Success = true,
                        Id = transaction.Id,
                        Reserved = transaction.Reserved ?? 0
                    };
                else
                    return new CreateTransactionOutput { Message = L("MobileApi.Messages.FaildTransaction") };
            }
            catch (Exception ex)
            {
                return new CreateTransactionOutput { Message = ex.Message };
            }
        }

        [HttpPost]
        [Language("Lang")]
        [Consumes("multipart/form-data")]

        public async Task<CreateTransactionOutput> InitiateFuelTransOuOldt()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new CreateTransactionOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var form = HttpContext.Request.Form;

                if (!form.ContainsKey("Data"))
                    return new CreateTransactionOutput { Message = L("MobileApi.Messages.InvalidRequest") };

                var jsonData = form["Data"];
                var input = JsonConvert.DeserializeObject<CreateFuelTransOutDto>(jsonData);

                var veichle = await _veichleAppService.GetAsync(new EntityDto<long> { Id = input.VeichleId.Value });
                input.BranchId = veichle.BranchId;
                input.VeichleId = veichle.Id;
                input.DriverId = veichle.DriverId;

                await CheckBranchWallet(new CheckBranchWalletInput
                {
                    Amount = input.Price,
                    BranchId = input.BranchId.Value,
                    WalletType = WalletType.Fuel
                });

                if (form.Files != null && form.Files.Count > 0)
                {
                    for (int i = 0; i < form.Files.Count; i++)
                    {
                        var file = form.Files[i];
                        if (file.Length == 0)
                            continue;

                        var uploadOptions = new NewUploadFilesDto
                        {
                            StorageLocation = 9,
                            AllowedTypes = 0,
                            UploadStyle = NewUploadStyle.BothOfThem
                        };

                        var uploadedFileName = await _uploadController.UploadPhotoWebP(file, uploadOptions);
                        var fileKey = file.Name;

                        if (fileKey.Contains("BeforeBoxPic"))
                        {
                            input.BeforeBoxPic = uploadedFileName.Value;
                        }
                        if (fileKey.Contains("BeforeCounterPic"))
                        {
                            input.BeforeCounterPic = uploadedFileName.Value;
                        }
                    }
                }

                var worker = await _workerRepository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);
                if (worker != null)
                {
                    input.ProviderId = worker.ProviderId;
                    input.WorkerId = worker.Id;
                }

                var fuelPrice = await _fuelTransOutAppService.GetFuelPrice(new GetFuelPriceInput
                {
                    FuelType = veichle.FuelType.Value,
                    VeichleId = input.VeichleId
                });
                input.FuelPrice = fuelPrice;

                var transaction = await _fuelTransOutAppService.CreateAsync(input);

                if (transaction != null)
                    return new CreateTransactionOutput
                    {
                        Message = L("MobileApi.Messages.BeforePicsCreated"),
                        Success = true,
                        Id = transaction.Id
                    };
                else
                    return new CreateTransactionOutput { Message = L("MobileApi.Messages.FaildTransaction") };
            }
            catch (Exception ex)
            {
                return new CreateTransactionOutput { Message = ex.Message };
            }
        }


        [HttpPost]
        [Language("Lang")]
        [Consumes("multipart/form-data")]

        public async Task<StringOutput> CompleteFuelTransOut()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new StringOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var form = HttpContext.Request.Form;

                if (!form.ContainsKey("Data"))
                    return new StringOutput { Message = L("MobileApi.Messages.InvalidRequest") };

                var jsonData = form["Data"];
                var input = JsonConvert.DeserializeObject<UpdateFuelTransOutDto>(jsonData);

                if (form.Files != null && form.Files.Count > 0)
                {
                    for (int i = 0; i < form.Files.Count; i++)
                    {
                        var file = form.Files[i];
                        if (file.Length == 0)
                            continue;

                        var uploadOptions = new NewUploadFilesDto
                        {
                            StorageLocation = 9,
                            AllowedTypes = 0,
                            UploadStyle = NewUploadStyle.BothOfThem
                        };

                        var uploadedFileName = await _uploadController.UploadPhotoWebP(file, uploadOptions);

                        var fileKey = form.Files[i].Name;

                        if (fileKey.Contains("AfterBoxPic"))
                        {
                            input.AfterBoxPic = uploadedFileName.Value;
                        }
                        if (fileKey.Contains("AfterCounterPic"))
                        {
                            input.AfterCounterPic = uploadedFileName.Value;
                        }
                    }
                }

                var transaction = await _fuelTransOutAppService.UpdateTransaction(input);

                if (transaction != null)
                    return new StringOutput { Message = L("MobileApi.Messages.SuccessTransaction"), Success = true };
                else
                    return new StringOutput { Message = L("MobileApi.Messages.FaildTransaction") };
            }
            catch (Exception ex)
            {
                return new StringOutput { Message = ex.Message };
            }
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<StringOutput> CancelFuelTransOut(CancelFuelTransOutDto input)
        {
            try
            {
                // get branch first 
                var transOut = await _fuelTransOutAppService.GetAsync(new EntityDto<long> { Id = input.Id });
                if (transOut == null)
                    throw new UserFriendlyException(L("Common.ErrorOccurred"));


                if (transOut.Completed == true)
                    return new StringOutput { Success = false };


                var cancelation = await _fuelTransOutAppService.CancelFuelTransOut(input);
                if (cancelation == true)
                    return new StringOutput { Message = L("MobileApi.Messages.SuccessTransaction"), Success = true };
                else
                    return new StringOutput { Message = L("MobileApi.Messages.FaildTransaction"), Success = false };

            }
            catch (Exception ex)
            {
                return new StringOutput { Success = false };
            }
        }


        [HttpPost]
        [Language("Lang")]
        public async Task<CheckBranchWalletOutput> CheckBranchWallet(CheckBranchWalletInput input)
        {
            try
            {
                // get branch first 
                var branch = await _branchRepository.FirstOrDefaultAsync(a => a.Id == input.BranchId);
                if (branch == null)
                    throw new UserFriendlyException(L("Common.ErrorOccurred"));


                if (input.WalletType == WalletType.Fuel && input.Amount > branch.FuelAmount)
                {
                    throw new UserFriendlyException(L("Pages.Wallets.BranchAmountNotEnoughForCarAmount"));
                }
                else if (input.WalletType == WalletType.Clean && input.Amount > branch.CleanAmount)
                {
                    throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));
                }
                else if (input.WalletType == WalletType.Maintain && input.Amount > branch.MaintainAmount)
                {
                    throw new UserFriendlyException(L("Pages.Wallets.BranchWalletAmountNotEnough"));
                }

                return new CheckBranchWalletOutput { Success = true };

            }
            catch (Exception ex)
            {
                return new CheckBranchWalletOutput { Message = ex.Message };
            }
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<CheckBranchWalletOutput> runJobManualy()
        {
            try
            {
                await _companyInvoiceJopAppService.CreateMonthlyCompanyInvoice();
                return new CheckBranchWalletOutput { Success = true };
            }
            catch (Exception ex)
            {
                return new CheckBranchWalletOutput { Message = ex.Message };
            }
        }
        private async Task<GetVeichleDetailsOutput> GetVeichleDetailsForNotCompleted(long vechileId)
        {
            try
            {

                var result = await _veichleAppService.GetByIdForNotCompleted(vechileId);

                if (result != null && result.Success == true)
                {

                    CheckWorkingDays(new WorkingDaysInput { workingDays = result.Veichle.WorkingDays });

                    if (result.Veichle.FuelType.HasValue == false)
                        throw new UserFriendlyException(L("Pages.Veichles.Messages.FuelTypeError"));

                    var fuelPrice = await _fuelTransOutAppService.GetFuelPrice(new GetFuelPriceInput { FuelType = result.Veichle.FuelType.Value, VeichleId = result.Veichle.Id });

                    string _fuelColor = await ReturnFuelColor(result.Veichle.FuelType.Value);



                    if (result.VeichleBalanceInSar == 0 && result.VeichleBalanceInLitre > 0)
                    {
                        // calculate VeichleBalanceInSar
                        result.VeichleBalanceInSar = fuelPrice * result.VeichleBalanceInLitre;
                    }

                    else if (result.VeichleBalanceInSar > 0 && result.VeichleBalanceInLitre == 0)
                    {
                        // calculate VeichleBalanceInLitre
                        result.VeichleBalanceInLitre = result.VeichleBalanceInSar / fuelPrice;
                    }
                    //decimal availableBranchAmount = result.Veichle.Branch.WalletAmount - result.Veichle.Branch.Reserved;
                    decimal availableBranchAmount = result.Veichle.Branch.WalletAmount;
                    decimal availableBranchLitres = 0;
                    if (fuelPrice > 0)
                        availableBranchLitres = availableBranchAmount / fuelPrice;
                    else
                        availableBranchLitres = 0;

                    return new GetVeichleDetailsOutput
                    {
                        Success = true,
                        Veichle = ObjectMapper.Map<ApiVeichleDto>(result.Veichle),
                        //Transactions = ObjectMapper.Map<List<ApiFuelTransOutDto>>(transactions.Items),
                        Transactions = new List<ApiFuelTransOutDto>(),
                        FuelPrice = fuelPrice,

                        VeichleBalanceInSar = result.VeichleBalanceInSar,
                        VeichleBalanceInLitre = result.VeichleBalanceInLitre > availableBranchLitres ? availableBranchLitres : result.VeichleBalanceInLitre,
                        GroupType = result.GroupType,
                        PeriodConsumptionType = result.PeriodConsumptionType,
                        MaximumRechargeAmount = result.MaximumRechargeAmount,
                        BranchReserved = result.Veichle.Branch.Reserved,

                        BranchWalletAmount = result.Veichle.Branch.WalletAmount,
                        BranchFuelAmount = result.Veichle.Branch.FuelAmount,
                        BranchCleanAmount = result.Veichle.Branch.CleanAmount,
                        BranchMaintainAmount = result.Veichle.Branch.MaintainAmount,

                        CounterPicIsRequired = result.CounterPicIsRequired,
                        FuelColor = _fuelColor
                    };
                }
                else
                {
                    // here => mobile will call the update vechile service
                    return new GetVeichleDetailsOutput
                    {
                        Success = true,
                        NotFound = true,
                        NotFoundSim = result.NotFoundSim,
                        Message = result.Message
                    };
                }
            }
            catch (Exception ex)
            {
                return new GetVeichleDetailsOutput { Message = ex.Message };
            }
        }

        [HttpPost]
        [Language("Lang")]
        public async Task<MappedGetTransactionsOutputNotCompleted> GetWorkerTransactionsNotCompleted()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new MappedGetTransactionsOutputNotCompleted { Message = L("MobileApi.Messages.LoginFirst") };

                var worker = await _workerRepository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);
                if (worker == null)
                    return new MappedGetTransactionsOutputNotCompleted { Message = L("MobileApi.Messages.LoginFirst") };

                var WorkerId = worker.Id;
                var transactions = await _fuelTransOutAppService.GetAllAsyncByWorkerIdNotCompleted(WorkerId);
              

                // implement cancel transaction
                var cancelationTransactions = await _fuelTransOutAppService.GetAllAsyncByWorkerIdMustCancel(WorkerId);
                var transactionsList = cancelationTransactions.ToList(); // Forces evaluation
                foreach (var transaction in transactionsList)
                {
                    await _fuelTransOutAppService.CancelFuelTransOut(new CancelFuelTransOutDto { Id = transaction.Id, CancelReason = "تم الغاء العملية تلقائيا لانقضاء الوقت", CancelNote = 0 });
                }


                if (transactions != null)
                {
                    // Initialize a list for storing vehicle details
                    var vehicleDetailsList = new List<GetVeichleDetailsOutput>();

                    foreach (var transaction in transactions)
                    { 
                       // Fetch vehicle details for the transaction
                        GetVeichleDetailsOutput veichleDetailsOutput = await GetVeichleDetailsForNotCompleted(
                            transaction.Veichle.Id);
                        veichleDetailsOutput.dateTime = transaction.CreationTime;
                        veichleDetailsOutput.fuelIntTransId = transaction.Id;
                        veichleDetailsOutput.Reserved = transaction.Reserved;
                        veichleDetailsOutput.QrCode = transaction.QrCode;
                        // Add the fetched details to the list
                        vehicleDetailsList.Add(veichleDetailsOutput);
                    }


                    return new MappedGetTransactionsOutputNotCompleted
                    {
                        Success = true,
                        Transactions = vehicleDetailsList.Any() 
                         ? ObjectMapper.Map<List<GetVeichleDetailsOutput>>(vehicleDetailsList)
                         : new List<GetVeichleDetailsOutput>()
                    };

                }

                return new MappedGetTransactionsOutputNotCompleted { Transactions = new List<GetVeichleDetailsOutput>() };
            }
            catch (Exception ex)
            {
                return new MappedGetTransactionsOutputNotCompleted { Message = ex.Message };
            }
        }
        [HttpPost]
        [Language("Lang")]
        public async Task<MappedGetTransactionsOutput> GetWorkerTransactions(GetTransactionsReportInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new MappedGetTransactionsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var worker = await _workerRepository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);
                if (worker == null)
                    return new MappedGetTransactionsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                input.WorkerId = worker.Id;
                var transactions = await _storedProcedureAppService.GetTransactionsReport(input);


                if (input.SkipCount.HasValue && input.MaxResultCount.HasValue)
                    transactions.Transactions = transactions.Transactions.Skip(input.SkipCount.Value).Take(input.MaxResultCount.Value).ToList();

                if (transactions != null)
                {
                    return new MappedGetTransactionsOutput
                    {
                        Success = true,
                        Transactions = ObjectMapper.Map<List<MappedTransactionDto>>(transactions.Transactions)
                    };
                }

                return new MappedGetTransactionsOutput { Transactions = new List<MappedTransactionDto>() };
            }
            catch (Exception ex)
            {
                return new MappedGetTransactionsOutput { Message = ex.Message };
            }
        }


        [HttpPost]
        [Language("Lang")]
        public async Task<MappedGetFuelTypesStatisticsOutput> GetFuelTypesStatistics(GetDashboardStatisticsInput input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new MappedGetFuelTypesStatisticsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var worker = await _workerRepository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);
                if (worker == null)
                    return new MappedGetFuelTypesStatisticsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var result = await _storedProcedureAppService.GetFuelTypesStatistics(new GetDashboardStatisticsInput
                {
                    //ProviderId = worker.ProviderId,
                    FullPeriodFromString = input.FullPeriodFromString,
                    FullPeriodToString = input.FullPeriodToString,
                    //FuelType = input.FuelType,
                    ProviderId = worker.ProviderId,
                    WorkerId = worker.Id,
                    Date = input.Date
                });

                var x = ObjectMapper.Map<MappedGetFuelTypesStatisticsOutput>(result);
                return x;
            }
            catch (Exception ex)
            {
                return new MappedGetFuelTypesStatisticsOutput { Message = ex.Message };
            }
        }
    }
}