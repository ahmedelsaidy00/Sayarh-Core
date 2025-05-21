using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.CompanyInvoices.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Configuration;
using Sayarah.Core.Repositories;
using Sayarah.Transactions;
using Sayarah.Veichles;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Sayarah.CompanyInvoices;
using QRCoder;
using Sayarah.Application.Helpers.Zatca;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Application.Helpers.StoredProcedures;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Core.Helpers;
using Sayarah.Application.Helpers.StoredProcedures.Dto;

namespace Sayarah.Application.CompanyInvoices
{
    public class CompanyInvoiceJopAppService : AsyncCrudAppService<CompanyInvoice, CompanyInvoiceDto, long, GetAllCompanyInvoices, CreateCompanyInvoiceDto, UpdateCompanyInvoiceDto>, ICompanyInvoiceJopAppService
    {
        private readonly IRepository<CompanyInvoice, long> _companyInvoiceRepository;
        private readonly IRepository<CompanyInvoiceTransaction, long> _companyInvoiceTransactionRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        private readonly IRepository<Company, long> _companyRepository;
        private readonly IRepository<FuelTransOut, long> _fuelTransOutRepository;
        public ICommonAppService _commonAppService { get; set; }
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly IStoredProcedureAppService _storedProcedureAppService;
        private readonly ISettingManager _settingManager;
        private readonly ICommonRepository _commonRepository;
        public CompanyInvoiceJopAppService(IRepository<CompanyInvoice, long> repository,
            IRepository<CompanyInvoiceTransaction, long> companyInvoiceTransactionRepository,
            IRepository<User, long> userRepository,
            IRepository<Veichle, long> veichleRepository,
            IRepository<Company, long> companyRepository,
            IRepository<FuelTransOut, long> fuelTransOutRepository,
            AbpNotificationHelper abpNotificationHelper,
            ICommonAppService commonAppService,
            IStoredProcedureAppService storedProcedureAppService,
            ISettingManager settingManager,
            ICommonRepository commonRepository)
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _companyInvoiceRepository = repository;
            _companyInvoiceTransactionRepository = companyInvoiceTransactionRepository;
            _userRepository = userRepository;
            _veichleRepository = veichleRepository;
            _fuelTransOutRepository = fuelTransOutRepository;
            _commonAppService = commonAppService;
            _abpNotificationHelper = abpNotificationHelper;
            _companyRepository = companyRepository;
            _storedProcedureAppService = storedProcedureAppService;
            _settingManager = settingManager;
            _commonRepository = commonRepository;
        }


        public override async Task<CompanyInvoiceDto> GetAsync(EntityDto<long> input)
        {
            var companyInvoice = _companyInvoiceRepository.FirstOrDefault(x => x.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<CompanyInvoiceDto>(companyInvoice));
        }




        [AbpAuthorize]
        public override async Task<CompanyInvoiceDto> CreateAsync(CreateCompanyInvoiceDto input)
        {
            try
            {
                var companyInvoice = ObjectMapper.Map<CompanyInvoice>(input);
                await _companyInvoiceRepository.InsertAsync(companyInvoice);
                return MapToEntityDto(companyInvoice);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public override async Task<CompanyInvoiceDto> UpdateAsync(UpdateCompanyInvoiceDto input)
        {
            try
            {
                ////Check if CompanyInvoice exists
                //int existingCount = await _companyInvoiceRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn && at.Id != input.Id));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.CompanyInvoices.Error.AlreadyExist"));
                var companyInvoice = await _companyInvoiceRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, companyInvoice);
                await _companyInvoiceRepository.UpdateAsync(companyInvoice);
                return MapToEntityDto(companyInvoice);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<PagedResultDto<CompanyInvoiceDto>> GetAllAsync(GetAllCompanyInvoices input)
        {
            try
            {
                var query = _companyInvoiceRepository.GetAll()
                    .Include(a => a.Company).AsQueryable();

                //query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
                query = query.WhereIf(input.CompanyId.HasValue, m => m.CompanyId == input.CompanyId);

                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var companyInvoices = await query
                    .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<CompanyInvoiceDto>(
                   query.Count(), ObjectMapper.Map<List<CompanyInvoiceDto>>(companyInvoices)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public async Task<CompanyInvoiceDto> CreateMonthlyCompanyInvoice()
        {
            try
            {
                CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete);
                // get current month
                var currentDate = DateTime.UtcNow;
                var lastMonth = currentDate.AddMonths(-1);

                // Query all relevant transOuts from the database
                var transOuts = await _fuelTransOutRepository.GetAll()
                    .Include(a => a.Branch.Company)
                    .Include(a => a.Provider.MainProvider)
                    .Where(a => a.Completed == true &&
                                a.BranchWalletTransactionId.HasValue &&
                                string.IsNullOrEmpty(a.CompanyInvoiceCode) &&
                                a.CreationTime.Month == lastMonth.Month &&
                                a.CreationTime.Year == lastMonth.Year)

                    .ToListAsync(); // Fetch data into memory

                // Group by company and map the results in memory
                var companyTransOuts = transOuts
                    .GroupBy(a => new { a.Branch.CompanyId, a.Provider.MainProviderId})
                    .Select(a => new CompanyInvoiceWithTransouts
                    {
                        CompanyId = a.Key.CompanyId,
                        MainProviderId = a.Key.MainProviderId,
                        Company = ObjectMapper.Map<CompanyDto>(a.FirstOrDefault()?.Branch.Company),
                        MainProvider = ObjectMapper.Map<MainProviderDto>(a.FirstOrDefault()?.Provider.MainProvider),
                        FuelTransOuts = ObjectMapper.Map<List<FuelTransOutDto>>(a.ToList())
                    }).ToList();

                if (companyTransOuts != null)
                {
                    foreach (var companyTransOut in companyTransOuts)
                    {
                        // create company invoice
                        InvoiceLatestCode code = await _commonRepository.GetAsync<InvoiceLatestCode>("SELECT * FROM CompanyInvoice_LatestCode");


                        // select next code from db 

                        DateTime _priodFrom = new DateTime(lastMonth.Year, lastMonth.Month, 1, 0, 0, 0);
                        DateTime _periodTo = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month), 23, 59, 59);

                        var utcTimeFrom = _priodFrom.ToUniversalTime();
                        var utcTimeTo = _periodTo.ToUniversalTime();


                        decimal _amountWithOutVat = companyTransOut.FuelTransOuts.Sum(a => a.Price) / 1.15m;
                        var companyInvoice = new CompanyInvoice
                        {
                            CompanyId = companyTransOut.CompanyId,
                            MainProviderId = companyTransOut.MainProviderId,
                            Code = code.LatestCode,
                            Net = companyTransOut.FuelTransOuts.Sum(a => a.Price),
                            Amount = companyTransOut.FuelTransOuts.Sum(a => a.Price),
                            AmountWithOutVat = _amountWithOutVat,
                            Quantity = companyTransOut.FuelTransOuts.Sum(a => a.Quantity),
                            VatValue = _amountWithOutVat * 15 / 100,
                            Month = lastMonth.Month.ToString(),
                            Year = lastMonth.Year.ToString(),
                            PeriodFrom = utcTimeFrom,
                            PeriodTo = utcTimeTo
                        };

                        companyInvoice = await _companyInvoiceRepository.InsertAsync(companyInvoice);
                        //await UnitOfWorkManager.Current.SaveChangesAsync();

                        string transIds = string.Empty;
                        // create companyInvoiceTransactions
                        if (companyTransOut.FuelTransOuts != null)
                        {
                             foreach (var transOut in companyTransOut.FuelTransOuts)
                             {
                                 await _companyInvoiceTransactionRepository.InsertAsync(new CompanyInvoiceTransaction
                                 {
                                     CompanyInvoice = companyInvoice,
                                     TransId = transOut.Id,
                                     TransType = TransOutTypes.Fuel,
                                     Price = transOut.Price,
                                     FuelPrice = transOut.FuelPrice,
                                     TransFuelType = transOut.FuelType,
                                     Quantity = transOut.Quantity,
                                 });
                             }

                            transIds = string.Join(",", companyTransOut.FuelTransOuts.Select(t => t.Id));

                            // update companyInvoiceCode in fuelTransOuts
                            await _storedProcedureAppService.UpdateCompanyInvoiceCodeInFuelTransOuts(new UpdateInvoiceCodeInFuelTransOutsInput
                            {
                                InvoiceCode = companyInvoice.Code,
                                Ids = transIds
                            });
                        }
                    }
                }

                return new CompanyInvoiceDto { };
            }
            catch (Exception ex)
            {
                throw;
            }

        }





        public async Task<List<CompanyInvoiceOutput>> PrintCompanyInvoiceDetails(GetAllCompanyInvoices input)
        {
            try
            {

                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {

                    List<CompanyInvoiceOutput> companyInvoices = [];

                    var companyName = await SettingManager.GetSettingValueAsync(AppSettingNames.CompanyName);
                    var taxNumber = await SettingManager.GetSettingValueAsync(AppSettingNames.TaxNumber);

                    var wantedInvoice = _companyInvoiceRepository.FirstOrDefault(x => x.Id == input.Id);
                    var currentMonth = wantedInvoice.Month + "/" + wantedInvoice.Year;


                    var query = _companyInvoiceRepository.GetAll()

                 .Include(a => a.Company)
                 .Include(a => a.MainProvider)
                 .Include(a => a.CompanyInvoiceTransactions.Select(aa => aa.Veichle.Branch.Company)).Where(x => x.Month + "/" + x.Year == currentMonth);


                    query = query.WhereIf(input.CompanyId.HasValue, a => a.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);
                    //var _companyInvoice = await query.FirstOrDefaultAsync(a => a.Id == input.Id);


                    var companyInvoicesBegin = ObjectMapper.Map<List<CompanyInvoiceDto>>(query);

                    foreach (var companyInvoice in companyInvoicesBegin)
                    {
                        //List<FuelOutput> _fuelOutput = new List<FuelOutput>();
                        if (companyInvoice != null && companyInvoice.CompanyInvoiceTransactions != null)
                        {
                            double companyInvoiceNet = Convert.ToDouble(Math.Round(companyInvoice.Net, 2, MidpointRounding.AwayFromZero).ToString("F2"));
                            double companyInvoiceTaxes = Convert.ToDouble(Math.Round(companyInvoice.Taxes, 2, MidpointRounding.AwayFromZero).ToString("F2"));

                            TLVCls tlv = new(
                            companyInvoice.MainProvider.Name,
                            companyInvoice.MainProvider.TaxNo != null ? companyInvoice.MainProvider.TaxNo : "" , // tax number
                            companyInvoice.CreationTime,
                            companyInvoiceNet,
                            companyInvoiceTaxes);

                            var taxQRString = tlv.ToBase64();

                            //From here on, you can implement your platform-dependent byte[]-to-image code 
                            QRCodeGenerator qrGenerator = new QRCodeGenerator();
                            QRCodeData qrCodeData = qrGenerator.CreateQrCode(taxQRString, QRCodeGenerator.ECCLevel.M);

                            // Create base64-encoded QR code image
                            var qrCodeImage = new Base64QRCode(qrCodeData);
                            var qrCodeImageSrc = qrCodeImage.GetGraphic(20);

                            // Get QR code image source as string
                            var qrCodeImageSrcString = "data:image/png;base64," + qrCodeImageSrc;

                            //companyInvoice.VatValue = _fuelOutputobject.VatValue;
                            //companyInvoice.AmountWithOutVat = _fuelOutputobject.PriceWithoutVat; 

                            companyInvoices.Add(new CompanyInvoiceOutput
                            {
                                CompanyInvoice = ObjectMapper.Map<CompanyInvoiceDto>(companyInvoice),
                                //FuelDetails = _fuelOutput, // Ensure this is a List<FuelOutput>
                                QrCode = qrCodeImageSrcString
                            });
                            if (companyInvoice.PeriodFrom != null)
                            {
                                companyInvoice.PeriodFrom = companyInvoice.PeriodFrom.Value.AddHours(4);
                            }
                            if (companyInvoice.PeriodTo != null)
                            {
                                companyInvoice.PeriodTo = companyInvoice.PeriodTo.Value.AddHours(-6);
                            }
                        }
                        else
                            return null;

                    }
                    return companyInvoices;
                    ///return new CompanyInvoiceOutput { FuelDetails = _fuelOutput };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
