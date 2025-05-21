using Abp.WebApi.Controllers;
using Sayarah.Api.Models;
using Sayarah.Helpers;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections.Generic;
using Sayarah.Drivers.Dto;
using Sayarah.Providers;
using Sayarah.Veichles.Dto;
using Sayarah.Veichles;
using Abp.Application.Services.Dto;
using Newtonsoft.Json;
using System.Web;
using Abp.Domain.Repositories;
using System.IO;
using Sayarah.Transactions.Dto;
using Sayarah.Transactions;
using Sayarah.Providers.Dto;
using Sayarah.Companies;
using Abp.UI;
using Sayarah.Wallets.Dto;
using Sayarah.Helpers.Dtos;
using Abp.AutoMapper;
using System.Linq;
using Sayarah.Helpers.Enums;
using Sayarah.Configuration;
using Sayarah.Chips;
using System.Web.Http.Results;
using Sayarah.CompanyInvoices;
using Abp.Auditing;
using Microsoft.AspNetCore.Mvc;

namespace Sayarah.Api.Controllers
{
    public class WorkerController : AbpApiController
    {
        private readonly IVeichleAppService _veichleAppService;
        public readonly IRepository<Worker, long> _workerRepository;
        public readonly IRepository<Provider, long> _providerRepository;
        public readonly IRepository<Branch, long> _branchRepository;
        private readonly IFuelTransOutAppService _fuelTransOutAppService;
        private readonly IFuelPumpAppService _fuelPumpAppService;
        private readonly IStoredProcedureAppService _storedProcedureAppService;
        private readonly ICompanyInvoiceJopAppService _companyInvoiceJopAppService;
        public UploadWebPController uploadController { get; set; }

        public WorkerController(

        IVeichleAppService veichleAppService,
        IRepository<Worker, long> workerRepository,
        IRepository<Provider, long> providerRepository,
        IFuelTransOutAppService fuelTransOutAppService,
        IFuelPumpAppService fuelPumpAppService,
        IRepository<Branch, long> branchRepository,
        IStoredProcedureAppService storedProcedureAppService,
        ICompanyInvoiceJopAppService companyInvoiceJopAppService
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
        public async Task<GetVeichleDetailsOutput> UpdateVeichleDetails()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new GetVeichleDetailsOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var cntx = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                UpdateVeichleSimPicDto input = JsonConvert.DeserializeObject<UpdateVeichleSimPicDto>(cntx.Request.Params["Data"]);

                // check if car exists in system or not first 
                await _veichleAppService.CheckVeichleExists(input);


                List<CreateVeichlePicDto> _medias = new List<CreateVeichlePicDto>();
                if (HttpContext.Current.Request.Files != null && HttpContext.Current.Request.Files.Count > 0)
                {
                    var keys = HttpContext.Current.Request.Files.AllKeys;
                    for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                    {
                        var file = HttpContext.Current.Request.Files[i];
                        if (file.ContentLength == 0)
                            continue;

                        var orderFile = cntx.Request.Files[i];
                        NewUploadFilesDto _newUploadFilesDto = new NewUploadFilesDto();
                        _newUploadFilesDto.StorageLocation = 5;
                        _newUploadFilesDto.AllowedTypes = 0;
                        _newUploadFilesDto.Sizes = "800&600";
                        _newUploadFilesDto.UploadStyle = NewUploadStyle.BothOfThem;
                        string _uniqueFileName = uploadController.UploadPhotoWebP(orderFile, _newUploadFilesDto);
                        CreateVeichlePicDto _media = new CreateVeichlePicDto();

                        _media.FilePath = _uniqueFileName;
                        _medias.Add(_media);
                    }
                }


                input.VeichleMedias = _medias;

                // call update 
                var updatedVechile = await _veichleAppService.UpdateVeichleDetails(input);

                if (updatedVechile != null)
                {
                    // get viechle details here 
                    var _details = await GetVeichleDetails(new GetVeichlesInput { SimNumber = input.SimNumber });

                    return new GetVeichleDetailsOutput
                    {
                        Message = L("Pages.Veichles.Messages.VeichleUpdated"),
                        Veichle = _details.Veichle,
                        Transactions = _details.Transactions,
                        FuelPrice = _details.FuelPrice,
                        Success = true
                    };
                }
                else
                    return new GetVeichleDetailsOutput { Message = L("MobileApi.Messages.ErrorOccurred") };


            }
            catch (Exception ex)
            {
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
        public async Task<CreateTransactionOutput> InitiateFuelTransOut()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new CreateTransactionOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var cntx = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                CreateFuelTransOutDto input = JsonConvert.DeserializeObject<CreateFuelTransOutDto>(cntx.Request.Params["Data"]);


                var veichle = await _veichleAppService.GetAsync(new EntityDto<long> { Id = input.VeichleId.Value });
                input.BranchId = veichle.BranchId;
                input.VeichleId = veichle.Id;
                input.DriverId = veichle.DriverId;

                await CheckBranchWallet(new CheckBranchWalletInput { Amount = input.Price, BranchId = input.BranchId.Value, WalletType = Helpers.Enums.WalletType.Fuel });

                List<CreateFuelTransOutDto> _medias = new List<CreateFuelTransOutDto>();
                if (HttpContext.Current.Request.Files != null && HttpContext.Current.Request.Files.Count > 0)
                {
                    var keys = HttpContext.Current.Request.Files.AllKeys;
                    for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                    {
                        var file = HttpContext.Current.Request.Files[i];
                        if (file.ContentLength == 0)
                            continue;
                        var orderFile = cntx.Request.Files[i];
                        NewUploadFilesDto _newUploadFilesDto = new NewUploadFilesDto();
                        _newUploadFilesDto.StorageLocation = 9;
                        _newUploadFilesDto.AllowedTypes = 0;
                        _newUploadFilesDto.UploadStyle = NewUploadStyle.BothOfThem;
                        string _uniqueFileName = uploadController.UploadPhotoWebP(orderFile, _newUploadFilesDto);
                        CreateFuelTransOutDto _media = new CreateFuelTransOutDto();

                        if (cntx.Request.Files.AllKeys[i].Contains("BeforeBoxPic"))
                        {
                            input.BeforeBoxPic = _uniqueFileName;
                        }
                        //if (cntx.Request.Files.AllKeys[i].Contains("AfterBoxPic"))
                        //{
                        //    input.AfterBoxPic = _uniqueFileName;
                        //}
                        if (cntx.Request.Files.AllKeys[i].Contains("BeforeCounterPic"))
                        {
                            input.BeforeCounterPic = _uniqueFileName;
                        }
                        //if (cntx.Request.Files.AllKeys[i].Contains("AfterCounterPic"))
                        //{
                        //    input.AfterCounterPic = _uniqueFileName;
                        //}
                    }
                }


                // get worker by id 

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
                // call Create 
                var transaction = await _fuelTransOutAppService.CreateAsync(input);

                if (transaction != null)
                    return new CreateTransactionOutput { Message = L("MobileApi.Messages.BeforePicsCreated"), Success = true, Id = transaction.Id, Reserved = transaction.Reserved??0 };
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
        public async Task<CreateTransactionOutput> InitiateFuelTransOuOldt()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new CreateTransactionOutput { Message = L("MobileApi.Messages.LoginFirst") };

                var cntx = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                CreateFuelTransOutDto input = JsonConvert.DeserializeObject<CreateFuelTransOutDto>(cntx.Request.Params["Data"]);


                var veichle = await _veichleAppService.GetAsync(new EntityDto<long> { Id = input.VeichleId.Value });
                input.BranchId = veichle.BranchId;
                input.VeichleId = veichle.Id;
                input.DriverId = veichle.DriverId;

                await CheckBranchWallet(new CheckBranchWalletInput { Amount = input.Price, BranchId = input.BranchId.Value, WalletType = Helpers.Enums.WalletType.Fuel });

                List<CreateFuelTransOutDto> _medias = new List<CreateFuelTransOutDto>();
                if (HttpContext.Current.Request.Files != null && HttpContext.Current.Request.Files.Count > 0)
                {
                    var keys = HttpContext.Current.Request.Files.AllKeys;
                    for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                    {
                        var file = HttpContext.Current.Request.Files[i];
                        if (file.ContentLength == 0)
                            continue;
                        var orderFile = cntx.Request.Files[i];
                        NewUploadFilesDto _newUploadFilesDto = new NewUploadFilesDto();
                        _newUploadFilesDto.StorageLocation = 9;
                        _newUploadFilesDto.AllowedTypes = 0;
                        _newUploadFilesDto.UploadStyle = NewUploadStyle.BothOfThem;
                        string _uniqueFileName = uploadController.UploadPhotoWebP(orderFile, _newUploadFilesDto);
                        CreateFuelTransOutDto _media = new CreateFuelTransOutDto();

                        if (cntx.Request.Files.AllKeys[i].Contains("BeforeBoxPic"))
                        {
                            input.BeforeBoxPic = _uniqueFileName;
                        }
                        //if (cntx.Request.Files.AllKeys[i].Contains("AfterBoxPic"))
                        //{
                        //    input.AfterBoxPic = _uniqueFileName;
                        //}
                        if (cntx.Request.Files.AllKeys[i].Contains("BeforeCounterPic"))
                        {
                            input.BeforeCounterPic = _uniqueFileName;
                        }
                        //if (cntx.Request.Files.AllKeys[i].Contains("AfterCounterPic"))
                        //{
                        //    input.AfterCounterPic = _uniqueFileName;
                        //}
                    }
                }


                // get worker by id 

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
                // call Create 
                var transaction = await _fuelTransOutAppService.CreateAsync(input);

                if (transaction != null)
                    return new CreateTransactionOutput { Message = L("MobileApi.Messages.BeforePicsCreated"), Success = true, Id = transaction.Id };
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
        public async Task<StringOutput> CompleteFuelTransOut()
        {
            try
            {
                if (!AbpSession.UserId.HasValue || AbpSession.UserId.Value <= 0)
                    return new StringOutput { Message = L("MobileApi.Messages.LoginFirst") };


                var cntx = Request.Properties["MS_HttpContext"] as HttpContextWrapper;
                UpdateFuelTransOutDto input = JsonConvert.DeserializeObject<UpdateFuelTransOutDto>(cntx.Request.Params["Data"]);

                List<CreateFuelTransOutDto> _medias = new List<CreateFuelTransOutDto>();
                if (HttpContext.Current.Request.Files != null && HttpContext.Current.Request.Files.Count > 0)
                {
                    var keys = HttpContext.Current.Request.Files.AllKeys;
                    for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                    {
                        var file = HttpContext.Current.Request.Files[i];
                        if (file.ContentLength == 0)
                            continue;

                        var orderFile = cntx.Request.Files[i];
                        NewUploadFilesDto _newUploadFilesDto = new NewUploadFilesDto();
                        _newUploadFilesDto.StorageLocation = 9;
                        _newUploadFilesDto.AllowedTypes = 0;
                        _newUploadFilesDto.UploadStyle = NewUploadStyle.BothOfThem;
                        string _uniqueFileName = uploadController.UploadPhotoWebP(orderFile, _newUploadFilesDto);
                        CreateFuelTransOutDto _media = new CreateFuelTransOutDto();

                        //if (cntx.Request.Files.AllKeys[i].Contains("BeforeBoxPic"))
                        //{
                        //    input.BeforeBoxPic = _uniqueFileName;
                        //}
                        if (cntx.Request.Files.AllKeys[i].Contains("AfterBoxPic"))
                        {
                            input.AfterBoxPic = _uniqueFileName;
                        }
                        //if (cntx.Request.Files.AllKeys[i].Contains("BeforeCounterPic"))
                        //{
                        //    input.BeforeCounterPic = _uniqueFileName;
                        //}
                        if (cntx.Request.Files.AllKeys[i].Contains("AfterCounterPic"))
                        {
                            input.AfterCounterPic = _uniqueFileName;
                        }
                    }
                }


                // call update 
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