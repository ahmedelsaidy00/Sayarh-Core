using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Helpers.StoredProcedures;
using Sayarah.Application.Helpers.Zatca;
using Sayarah.Application.Invoices.Dto;
using Sayarah.Authorization;
using Sayarah.Authorization.Users;
using Sayarah.Configuration;
using Sayarah.Core.Helpers;
using Sayarah.Invoices;
using Sayarah.Journals;
using Sayarah.Providers;
using Sayarah.Veichles;
using System.Linq.Dynamic.Core;
using static Sayarah.SayarahConsts;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Helpers.Dto;

namespace Sayarah.Application.Invoices
{
    public class InvoiceAppService : AsyncCrudAppService<Invoice, InvoiceDto, long, GetAllInvoices, CreateInvoiceDto, UpdateInvoiceDto>, IInvoiceAppService
    {
        private readonly IRepository<Invoice, long> _invoiceRepository;
        private readonly IRepository<InvoiceTransaction, long> _invoiceTransactionRepository;
        private readonly IRepository<InvoiceDetail, long> _invoiceDetailRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        private readonly IRepository<Journal, long> _journalRepository;
        private readonly IRepository<JournalDetail, long> _journalDetailRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly IRepository<MainProvider, long> _mainProviderRepository;
        public ICommonAppService _commonAppService { get; set; }
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly IStoredProcedureAppService _storedProcedureAppService;

        public InvoiceAppService(IRepository<Invoice, long> repository,
            IRepository<InvoiceTransaction, long> invoiceTransactionRepository,
            IRepository<InvoiceDetail, long> invoiceDetailRepository,
            IRepository<User, long> userRepository,
            IRepository<Veichle, long> veichleRepository,
            IRepository<Journal, long> journalRepository,
            IRepository<JournalDetail, long> journalDetailRepository,
            IRepository<Provider, long> providerRepository,
            IRepository<MainProvider, long> mainProviderRepository,
            AbpNotificationHelper abpNotificationHelper,
            ICommonAppService commonAppService,
            IStoredProcedureAppService storedProcedureAppService)
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _invoiceRepository = repository;
            _invoiceTransactionRepository = invoiceTransactionRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _userRepository = userRepository;
            _veichleRepository = veichleRepository;
            _commonAppService = commonAppService;
            _journalRepository = journalRepository;
            _abpNotificationHelper = abpNotificationHelper;
            _providerRepository = providerRepository;
            _mainProviderRepository = mainProviderRepository;
            _journalDetailRepository = journalDetailRepository;
            _storedProcedureAppService = storedProcedureAppService;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<InvoiceDto>> GetPaged(GetInvoicesInput input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            int invoiceId = Convert.ToInt32(input.ids[i]);
                            Invoice invoice = await _invoiceRepository.GetAsync(invoiceId);
                            if (invoice != null)
                            {
                                if (input.action == "Delete")//Delete
                                {



                                    //if (centersCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.Invoices.Error.HasCenters"));

                                    await _invoiceRepository.DeleteAsync(invoice);
                                }
                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            int invoiceId = Convert.ToInt32(input.ids[0]);
                            Invoice invoice = await _invoiceRepository.GetAsync(invoiceId);
                            if (invoice != null)
                            {
                                if (input.action == "Delete")//Delete
                                {

                                    await _invoiceRepository.DeleteAsync(invoice);
                                }

                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }

                    int count = await _invoiceRepository.CountAsync();
                    var query = _invoiceRepository.GetAll()
                        .Include(a => a.Journal)
                        .Include(a => a.Provider.MainProvider)
                        .Include(a => a.MainProvider).AsQueryable();


                    query = query.WhereIf(input.ProviderId.HasValue, m => m.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.MainProviderId.HasValue, m => m.MainProviderId == input.MainProviderId);
                    if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId) || a.ProviderId.HasValue == false );
                        }
                        else
                            return new DataTableOutputDto<InvoiceDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<InvoiceDto>()
                            };
                    }
                    count = query.Count();
                    query = query.FilterDataTable(input);

                    query = query.WhereIf(input.Id.HasValue, m => m.Id == input.Id);

                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.MainProvider.NameAr.Contains(input.MainProviderName) || at.MainProvider.NameEn.Contains(input.MainProviderName) || at.MainProvider.Code.Contains(input.MainProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName) || at.Provider.Code.Contains(input.ProviderName));


                    query = query.WhereIf(input.AmountFrom.HasValue, at => at.Amount >= input.AmountFrom);
                    query = query.WhereIf(input.AmountTo.HasValue, at => at.Amount <= input.AmountTo);

                    query = query.WhereIf(input.DiscountFrom.HasValue, at => at.Discount >= input.DiscountFrom);
                    query = query.WhereIf(input.DiscountTo.HasValue, at => at.Discount <= input.DiscountTo);

                    query = query.WhereIf(input.TaxesFrom.HasValue, at => at.Taxes >= input.TaxesFrom);
                    query = query.WhereIf(input.TaxesTo.HasValue, at => at.Taxes <= input.TaxesTo);

                    query = query.WhereIf(input.NetFrom.HasValue, at => at.Net >= input.NetFrom);
                    query = query.WhereIf(input.NetTo.HasValue, at => at.Net <= input.NetTo);

                    query = query.WhereIf(input.JournalId.HasValue, m => m.JournalId == input.JournalId);


                    if (input.PeriodFrom.HasValue || input.PeriodTo.HasValue)
                    {
                        DateTime periodFrom = new DateTime();
                        DateTime periodTo = new DateTime();

                        if (input.PeriodTo.HasValue)
                        {
                            periodTo = input.PeriodTo.Value.Date.AddSeconds(86399).AddDays(1);
                        }
                        query = query.WhereIf(input.PeriodFrom.HasValue, at => at.PeriodFrom >= periodFrom);
                        query = query.WhereIf(input.PeriodTo.HasValue, at => at.PeriodTo <= periodTo);
                    }


                    int filteredCount = await query.CountAsync();
                    var invoices =
                          await query.Include(a => a.Journal)
                        .Include(a => a.MainProvider)
                        .Include(a => a.Provider.MainProvider)

                           .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                            .Skip(input.start)
                            .Take(input.length)
                              .ToListAsync();
                    return new DataTableOutputDto<InvoiceDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<InvoiceDto>>(invoices)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public override async Task<InvoiceDto> GetAsync(EntityDto<long> input)
        {
            var invoice = _invoiceRepository.FirstOrDefault(x => x.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<InvoiceDto>(invoice));
        }


        public async Task<InvoiceDto> GetInVoiceDetails(GetAllInvoices input)
        {
            try
            {

                var query = _invoiceRepository
                    .GetAll()
                    .Include(a => a.MainProvider)
                    .Include(a => a.Provider.MainProvider)
                    .Include(a => a.InvoiceTransactions.Select(aa => aa.Veichle.Branch.Company))
                    .Include(a => a.InvoiceDetails).AsQueryable();


                query = query.WhereIf(input.ProviderId.HasValue, a => a.ProviderId == input.ProviderId);
                query = query.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);
                var invoice = await query.FirstOrDefaultAsync(a => a.Id == input.Id);

                var mappedInvoice = ObjectMapper.Map<InvoiceDto>(invoice);

                if (mappedInvoice != null && mappedInvoice.InvoiceTransactions != null)
                {
                    mappedInvoice.BranchInvoiceDetails = mappedInvoice.InvoiceTransactions.GroupBy(a => a.Veichle.BranchId)
                        .Select(a => new BranchInvoiceDetailDto
                        {
                            Price = a.ToList().Sum(aa => aa.Price),
                            BranchName = a.ToList().FirstOrDefault().Veichle.Branch.Name,
                            CompanyId = a.ToList().FirstOrDefault().Veichle.Branch.CompanyId,
                            CompanyName = a.ToList().FirstOrDefault().Veichle.Branch.Company.Name,
                        }).ToList();
                }

                return mappedInvoice;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [AbpAuthorize]
        public override async Task<InvoiceDto> CreateAsync(CreateInvoiceDto input)
        {
            try
            {
                //int existingCount = await _invoiceRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Invoices.Error.AlreadyExist"));

                //input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Invoices", CodeField = "Code" });

                var invoice = ObjectMapper.Map<Invoice>(input);
                await _invoiceRepository.InsertAsync(invoice);
                return MapToEntityDto(invoice);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public override async Task<InvoiceDto> UpdateAsync(UpdateInvoiceDto input)
        {
            try
            {
                ////Check if Invoice exists
                //int existingCount = await _invoiceRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn && at.Id != input.Id));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Invoices.Error.AlreadyExist"));
                var invoice = await _invoiceRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, invoice);
                await _invoiceRepository.UpdateAsync(invoice);
                return MapToEntityDto(invoice);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<PagedResultDto<InvoiceDto>> GetAllAsync(GetAllInvoices input)
        {
            try
            {
                var query = _invoiceRepository.GetAll()
                    .Include(a => a.Journal)
                    .Include(a => a.Provider.MainProvider)
                        .Include(a => a.MainProvider).AsQueryable();

                //query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
                query = query.WhereIf(input.ProviderId.HasValue, m => m.ProviderId == input.ProviderId);
                query = query.WhereIf(input.MainProviderId.HasValue, m => m.MainProviderId == input.MainProviderId);

                query = query.WhereIf(input.JournalId.HasValue, m => m.JournalId == input.JournalId);

                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var invoices = await query
                    .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<InvoiceDto>(
                   query.Count(), ObjectMapper.Map<List<InvoiceDto>>(invoices)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        // custom functions

        [AbpAuthorize]
        public async Task<InvoiceDto> CreateInvoice(CreateInvoiceInput input)
        {
            try
            {


                input.Invoice.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Invoices", CodeField = "Code" });

                var invoice = ObjectMapper.Map<Invoice>(input.Invoice);
                await _invoiceRepository.InsertAsync(invoice);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                // create invoice trans

                if (input.Transactions != null)
                {
                    int i = 1;
                    foreach (var transaction in input.Transactions)
                    {
                        await _invoiceTransactionRepository.InsertAsync(new InvoiceTransaction
                        {
                            InvoiceId = invoice.Id,
                            Serial = i,
                            TransId = transaction.Id,
                            TransType = transaction.TransactionType,
                            VeichleId = transaction.VeichleId,
                            Price = transaction.Price ?? 0,
                            FuelPrice = transaction.FuelPrice ?? 0,
                            TransFuelType = transaction.TransFuelType,
                            Quantity = transaction.Quantity ?? 0
                        });

                        i++;
                    }

                    await UnitOfWorkManager.Current.SaveChangesAsync();
                }


                // create invoice details
                if (input.InvoiceDetails != null)
                {
                    int i = 1;
                    foreach (var transaction in input.InvoiceDetails)
                    {
                        await _invoiceDetailRepository.InsertAsync(new InvoiceDetail
                        {
                            InvoiceId = invoice.Id,
                            Serial = i,
                            IsTaxable = transaction.IsTaxable,
                            Price = transaction.Price,
                            Note = transaction.Note,
                            ItemId = transaction.ItemId,
                        });

                        i++;
                    }

                    await UnitOfWorkManager.Current.SaveChangesAsync();
                }


                // create journals here 
                Journal _journal = new Journal
                {
                    //BranchId = invoice.BranchId,
                    JournalType = JournalType.Invoice,
                    ProviderId = invoice.ProviderId,
                    MainProviderId = invoice.MainProviderId,
                };

                _journal.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Journals", CodeField = "Code" });

                await _journalRepository.InsertAsync(_journal);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                invoice.JournalId = _journal.Id;
                await _invoiceRepository.UpdateAsync(invoice);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                // create journal details


                // 1st row 
                await _journalDetailRepository.InsertAsync(new JournalDetail
                {
                    JournalId = _journal.Id,
                    //AccountId = invoice.BranchId,
                    AccountType = AccountTypes.SayarahApp,
                    Credit = 0,
                    Debit = invoice.Net,
                });

                // 2nd row 
                await _journalDetailRepository.InsertAsync(new JournalDetail
                {
                    JournalId = _journal.Id,
                    AccountId = invoice.ProviderId,
                    AccountType = AccountTypes.Provider,
                    Credit = invoice.Net,
                    Debit = 0,
                });


                await _storedProcedureAppService.UpdateInvoiceCodeInFuelTransOuts(new UpdateInvoiceCodeInFuelTransOutsInput
                {
                    InvoiceCode = invoice.ExternalInvoice ? invoice.ExternalCode : invoice.Code,
                    Ids = input.TransactionIds
                });

                // send notification to admin 
                await SendNotificationToEmployees(new GetAllInvoiceDetails
                {
                    ProviderId = invoice.ProviderId,
                    MainProviderId = invoice.MainProviderId,
                    InvoiceId = invoice.Id
                });

                return MapToEntityDto(invoice);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public async Task<bool> SendNotificationToEmployees(GetAllInvoiceDetails input)
        {
            try
            {
                string providerName = string.Empty;
                if (input.ProviderId.HasValue)
                {

                var _provider = await _providerRepository.FirstOrDefaultAsync(a => a.Id == input.ProviderId);
                    providerName = _provider.NameAr;
                }
                else
                {
                    var _provider = await _mainProviderRepository.FirstOrDefaultAsync(a => a.Id == input.MainProviderId);
                    providerName = _provider.NameAr;
                }

                List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
                string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");


                // send notification  
                var users = await _userRepository.GetAll()
                    .Include(a => a.Permissions)
                    .Where(a => a.IsDeleted == false && a.IsActive == true && a.UserType == UserTypes.Admin && a.Permissions.Any(aa => aa.Name == PermissionNames.AdminsData.AdminInvoices.Read && aa.IsGranted == true)).ToListAsync();



                if (users != null && users.Count > 0)
                {
                    //var currentUser = await _userRepository.FirstOrDefaultAsync(AbpSession.UserId.Value);
                    foreach (var item in users)
                    {
                        List<UserIdentifier> targetAdminUsersId = new List<UserIdentifier>();
                        targetAdminUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: item.Id));
                        CreateNotificationDto _createNotificationData = new CreateNotificationDto
                        {
                            SenderUserName = providerName,
                            EntityType = Entity_Type.NewInvoice,
                            EntityId = input.InvoiceId,
                            Message = L("Pages.Invoices.Messages.New")
                        };
                        //Publish Notification Data
                        await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewInvoice, _createNotificationData, targetAdminUsersId.ToArray());

                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public async Task<InvoiceOutput> PrintInvoiceDetails(GetAllInvoices input)
        {
            try
            {

                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {

                    var companyName = await SettingManager.GetSettingValueAsync(AppSettingNames.CompanyName);
                    var taxNumber = await SettingManager.GetSettingValueAsync(AppSettingNames.TaxNumber);

                    var query = _invoiceRepository.GetAll()
                                                  .Include(a => a.MainProvider)
                                                  .Include(a => a.Provider.MainProvider)
                                                  .Include(a => a.InvoiceTransactions.Select(aa => aa.Veichle.Branch.Company))
                                                  .Include(a => a.InvoiceDetails).AsQueryable();

                    query = query.WhereIf(input.ProviderId.HasValue, a => a.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);
                    var _invoice = await query.FirstOrDefaultAsync(a => a.Id == input.Id);


                    var invoice = ObjectMapper.Map<InvoiceDto>(_invoice);

                    List<FuelOutput> _fuelOutput = new List<FuelOutput>();
                    if (invoice != null && invoice.InvoiceDetails != null)
                    {
                        invoice.InvoiceTransactions = invoice.InvoiceTransactions.Where(a => a.TransType == TransOutTypes.Fuel).ToList();


                        _fuelOutput = invoice.InvoiceTransactions
                               .GroupBy(a => new { a.TransFuelType, a.FuelPrice })
                               .Select(a => new FuelOutput
                               {
                                   FuelType = a.Key.TransFuelType.Value,
                                   Price = a.ToList().Sum(aa => aa.Price),
                                   Quantity = a.ToList().Sum(aa => aa.Quantity),
                                   FuelPrice = a.Key.FuelPrice
                               }).OrderBy(a=>a.FuelType).ToList();
                    }

                    if (invoice != null)
                    {


                        TLVCls tlv = new TLVCls(
                            companyName,
                            taxNumber, // tax number
                            invoice.CreationTime,
                            (double)invoice.Net,
                            (double)invoice.Taxes);

                        var taxQRString = tlv.ToBase64();

                        //From here on, you can implement your platform-dependent byte[]-to-image code 
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(taxQRString, QRCodeGenerator.ECCLevel.M);

                        // Create base64-encoded QR code image
                        var qrCodeImage = new Base64QRCode(qrCodeData);
                        var qrCodeImageSrc = qrCodeImage.GetGraphic(20);

                        // Get QR code image source as string
                        var qrCodeImageSrcString = "data:image/png;base64," + qrCodeImageSrc;

                        return new InvoiceOutput { Invoice = invoice, QrCode = qrCodeImageSrcString, FuelDetails = _fuelOutput };
                    }
                    return new InvoiceOutput { FuelDetails = _fuelOutput };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





    }
}
