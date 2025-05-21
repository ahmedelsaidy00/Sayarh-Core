using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Invoices.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Core.Helpers;
using Sayarah.Invoices;
using Sayarah.Journals;
using Sayarah.Providers;
using Sayarah.Veichles;
using static Sayarah.SayarahConsts;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers.Dto;

namespace Sayarah.Application.Invoices
{
    public class VoucherAppService : AsyncCrudAppService<Voucher, VoucherDto, long, GetAllVouchers, CreateVoucherDto, UpdateVoucherDto>, IVoucherAppService
    {
        private readonly IRepository<Voucher, long> _voucherRepository;
        private readonly IRepository<Invoice, long> _invoiceRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly IRepository<MainProvider, long> _mainProviderRepository;
        private readonly IRepository<Veichle, long> _veichleRepository;
        private readonly IRepository<Journal, long> _journalRepository;
        private readonly IRepository<JournalDetail, long> _journalDetailRepository;
        private readonly AbpNotificationHelper _abpNotificationHelper;

        public ICommonAppService _commonAppService { get; set; }
        public VoucherAppService(IRepository<Voucher, long> repository,
            IRepository<Invoice, long> invoiceRepository,
            IRepository<User, long> userRepository,
            IRepository<Veichle, long> veichleRepository,
            IRepository<Provider, long> providerRepository,
            IRepository<MainProvider, long> mainProviderRepository,
             IRepository<Journal, long> journalRepository,
            IRepository<JournalDetail, long> journalDetailRepository,
            AbpNotificationHelper abpNotificationHelper,
            ICommonAppService commonAppService)
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _voucherRepository = repository;
            _invoiceRepository = invoiceRepository;
            _userRepository = userRepository;
            _veichleRepository = veichleRepository;
            _providerRepository = providerRepository;
            _mainProviderRepository = mainProviderRepository;
            _commonAppService = commonAppService;
            _journalRepository = journalRepository;
            _abpNotificationHelper = abpNotificationHelper;
            _journalDetailRepository = journalDetailRepository;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<VoucherDto>> GetPaged(GetVouchersInput input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            int voucherId = Convert.ToInt32(input.ids[i]);
                            Voucher voucher = await _voucherRepository.GetAsync(voucherId);
                            if (voucher != null)
                            {
                                if (input.action == "Delete")//Delete
                                {



                                    //if (centersCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.Vouchers.Error.HasCenters"));

                                    await _voucherRepository.DeleteAsync(voucher);
                                }
                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            int voucherId = Convert.ToInt32(input.ids[0]);
                            Voucher voucher = await _voucherRepository.GetAsync(voucherId);
                            if (voucher != null)
                            {
                                if (input.action == "Delete")//Delete
                                {

                                    await _voucherRepository.DeleteAsync(voucher);
                                }

                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }

                    int count = await _voucherRepository.CountAsync();
                    var query = _voucherRepository.GetAll()
                        .Include(a => a.Journal)
                        .Include(a => a.Provider.MainProvider)
                        .Include(a => a.MainProvider).AsQueryable();


                    query = query.WhereIf(input.ProviderId.HasValue, m => m.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.MainProviderId.HasValue, m => m.MainProviderId == input.MainProviderId);
                    if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId) || a.ProviderId.HasValue == false);
                        }
                        else
                            return new DataTableOutputDto<VoucherDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<VoucherDto>()
                            };
                    }
                    count = query.Count();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    //query = query.WhereIf(!string.IsNullOrEmpty(input.NameAr), at => at.NameAr.Contains(input.NameAr) );

                    query = query.WhereIf(!string.IsNullOrEmpty(input.Note), at => at.Note.Contains(input.Note));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.MainProvider.NameAr.Contains(input.MainProviderName) || at.MainProvider.NameEn.Contains(input.MainProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName));


                    query = query.WhereIf(input.Id.HasValue, m => m.Id == input.Id);
                    query = query.WhereIf(input.JournalId.HasValue, m => m.JournalId == input.JournalId);
                    query = query.WhereIf(input.Amount.HasValue, m => m.Amount == input.Amount);
                    query = query.WhereIf(input.AmountFrom.HasValue, m => m.Amount >= input.AmountFrom);
                    query = query.WhereIf(input.AmountTo.HasValue, m => m.Amount <= input.AmountTo);

                    int filteredCount = await query.CountAsync();
                    List<Voucher> vouchers = new List<Voucher>();
                    if (input.columns[input.order[0].column].name.Equals("Code"))
                    {
                        vouchers = await query/*.Include(q => q.CreatorUser)*/
                        .OrderByDescending(x => x.CreationTime)
                        .Skip(input.start)
                        .Take(input.length)
                        .ToListAsync();
                    }
                    else
                    {
                        vouchers =
                         await query/*.Include(q => q.CreatorUser)*/
                          .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                           .Skip(input.start)
                           .Take(input.length)
                             .ToListAsync();
                    }
                    return new DataTableOutputDto<VoucherDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<VoucherDto>>(vouchers)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public override async Task<VoucherDto> GetAsync(EntityDto<long> input)
        {
            var voucher = _voucherRepository
                .GetAll()
                .Include(a => a.Provider.MainProvider)
                .Include(a => a.MainProvider)
                .Include(a => a.Journal)
                .FirstOrDefault(x => x.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<VoucherDto>(voucher));
        }


        public async Task<VoucherDto> GetVoucherDetails(GetAllVouchers input)
        {
            var query = _voucherRepository
                .GetAll()
                .Include(a => a.Provider.MainProvider)
                .Include(a => a.MainProvider)
                .Include(a => a.Journal).AsQueryable();

            query = query.WhereIf(input.ProviderId.HasValue, a => a.ProviderId == input.ProviderId);
            query = query.WhereIf(input.MainProviderId.HasValue, a => a.Provider.MainProviderId == input.MainProviderId);
            var voucher = query.FirstOrDefault(a => a.Id == input.Id);

            return await Task.FromResult(ObjectMapper.Map<VoucherDto>(voucher));
        }



        [AbpAuthorize]
        public override async Task<VoucherDto> CreateAsync(CreateVoucherDto input)
        {
            try
            {
                //int existingCount = await _voucherRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Vouchers.Error.AlreadyExist"));

                //input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Vouchers", CodeField = "Code" });

                var voucher = ObjectMapper.Map<Voucher>(input);
                await _voucherRepository.InsertAsync(voucher);
                return MapToEntityDto(voucher);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public override async Task<VoucherDto> UpdateAsync(UpdateVoucherDto input)
        {
            try
            {
                ////Check if Voucher exists
                //int existingCount = await _voucherRepository.CountAsync(at => (at.NameAr == input.NameAr && at.NameEn == input.NameEn && at.Id != input.Id));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Vouchers.Error.AlreadyExist"));
                var voucher = await _voucherRepository.GetAsync(input.Id);
                ObjectMapper.Map(input, voucher);
                await _voucherRepository.UpdateAsync(voucher);
                return MapToEntityDto(voucher);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<PagedResultDto<VoucherDto>> GetAllAsync(GetAllVouchers input)
        {
            try
            {
                var query = _voucherRepository.GetAll()
                    .Include(a => a.Journal)
                    .Include(a => a.Provider.MainProvider)
                    .Include(a => a.MainProvider).AsQueryable();

                //query = query.WhereIf(!string.IsNullOrEmpty(input.Name), m => m.NameAr.Contains(input.Name) || m.NameEn.Contains(input.Name));
                query = query.WhereIf(input.ProviderId.HasValue, m => m.ProviderId == input.ProviderId);
                query = query.WhereIf(input.MainProviderId.HasValue, m => m.MainProviderId == input.MainProviderId);

                query = query.WhereIf(input.JournalId.HasValue, m => m.JournalId == input.JournalId);
                query = query.WhereIf(input.Amount.HasValue, m => m.Amount == input.Amount);

                if (input.MaxCount == true)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }
                var vouchers = await query
                    .OrderBy(x => x.Id).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<VoucherDto>(
                   query.Count(), ObjectMapper.Map<List<VoucherDto>>(vouchers)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<VoucherDto> CreateVoucher(CreateVoucherDto input)
        {
            try
            {

                input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Vouchers", CodeField = "Code" });

                var voucher = ObjectMapper.Map<Voucher>(input);
                await _voucherRepository.InsertAsync(voucher);
                await UnitOfWorkManager.Current.SaveChangesAsync();


                // create journals here 
                Journal _journal = new Journal
                {
                    // BranchId = voucher.BranchId,
                    JournalType = JournalType.Vouchar,
                    ProviderId = voucher.ProviderId
                };

                _journal.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Journals", CodeField = "Code" });

                await _journalRepository.InsertAsync(_journal);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                voucher.JournalId = _journal.Id;
                await _voucherRepository.UpdateAsync(voucher);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                // create journal details


                // 1st row 
                await _journalDetailRepository.InsertAsync(new JournalDetail
                {
                    JournalId = _journal.Id,
                    //AccountId = voucher.BranchId,
                    AccountType = AccountTypes.Provider,
                    Credit = 0,
                    Debit = voucher.Amount,
                });

                // 2nd row 
                await _journalDetailRepository.InsertAsync(new JournalDetail
                {
                    JournalId = _journal.Id,
                    //AccountId = voucher.BranchId,
                    AccountType = AccountTypes.SayarahApp,
                    Credit = voucher.Amount,
                    Debit = 0,
                });

                // send notifiction 

                // get provider 
                var mainProvider = await _mainProviderRepository.FirstOrDefaultAsync(a => a.Id == input.MainProviderId);
                if (mainProvider != null)
                {

                    List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
                    string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");

                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: mainProvider.UserId.Value));

                    CreateNotificationDto CreateNotificationData = new CreateNotificationDto
                    {
                        SenderUserName = L("Pages.JournalDetails.AccountType.SayarahApp"),
                        Message = "Pages.Vouchers.Messages.New",
                        EntityType = Entity_Type.NewVoucher,
                        EntityId = voucher.Id
                    };
                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewVoucher, CreateNotificationData, targetUsersId.ToArray());
                }



                // get provider 
                var provider = await _providerRepository.FirstOrDefaultAsync(a => a.Id == input.ProviderId);
                if (provider != null)
                {

                    List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
                    string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");

                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: provider.UserId.Value));

                    CreateNotificationDto CreateNotificationData = new CreateNotificationDto
                    {
                        SenderUserName = L("Pages.JournalDetails.AccountType.SayarahApp"),
                        Message = "Pages.Vouchers.Messages.New",
                        EntityType = Entity_Type.NewVoucher,
                        EntityId = voucher.Id
                    };
                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewVoucher, CreateNotificationData, targetUsersId.ToArray());
                }


                return MapToEntityDto(voucher);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public async Task<GetTotalVouchersAmountOutput> GetTotalVouchersAmount(GetAllVouchers input)
        {
            var query = _voucherRepository
                .GetAll()
                .Include(a => a.Provider.MainProvider)
                .Include(a => a.Journal).AsQueryable();

            query = query.WhereIf(input.ProviderId.HasValue, a => a.ProviderId == input.ProviderId);
            query = query.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);

            query = query.WhereIf(input.IsEmployee.HasValue && input.IsEmployee == true, a => input.ProviderIds.Any(p => p == a.ProviderId) || a.ProviderId.HasValue == false);


            var vouchers = await query.ToListAsync();

            decimal vouchers_amount = vouchers.Sum(a => a.Amount);


            var invoiceQuery = _invoiceRepository
                .GetAll()
                .Include(a => a.Provider.MainProvider)
                .Include(a => a.Journal).AsQueryable();

            invoiceQuery = invoiceQuery.WhereIf(input.ProviderId.HasValue, a => a.ProviderId == input.ProviderId);
            invoiceQuery = invoiceQuery.WhereIf(input.MainProviderId.HasValue, a => a.MainProviderId == input.MainProviderId);
            invoiceQuery = invoiceQuery.WhereIf(input.IsEmployee.HasValue && input.IsEmployee == true, a => input.ProviderIds.Any(p => p == a.ProviderId) || a.ProviderId.HasValue == false);


            var invoices = await invoiceQuery.ToListAsync();

            decimal invoices_amount = invoices.Sum(a => a.Net);


            return new GetTotalVouchersAmountOutput { Amount = invoices_amount - vouchers_amount };
        }

    }
}
