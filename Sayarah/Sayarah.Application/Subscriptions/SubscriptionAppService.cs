using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.BackgroundJobs;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Helpers.StoredProcedures;
using Sayarah.Application.Subscriptions.Dto;
using Sayarah.Application.Wallets;
using Sayarah.Application.Wallets.Dto;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Packages;
using System.Data;
using System.Globalization;
using static Sayarah.SayarahConsts;
using System.Linq.Dynamic.Core;
using Sayarah.Application.DataTables.Dto;

namespace Sayarah.Application.Subscriptions
{
    public class SubscriptionAppService : AsyncCrudAppService<Subscription, SubscriptionDto, long, GetAllSubscriptions, CreateSubscriptionDto, UpdateSubscriptionDto>, ISubscriptionAppService
    {
        public UserManager _userManager { get; set; }
        private readonly ICommonAppService _commonService;
        private readonly RoleManager _roleManager;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<UserDevice, long> _userDeviceRepository;
        private readonly IRepository<Package, long> _packageRepository;
        private readonly IRepository<Company, long> _companyRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ICompanyWalletTransactionAppService _companyWalletTransactionAppService;
        private readonly IStoredProcedureAppService _storedProcedureAppService;
        private readonly ISubscriptionTransactionAppService _subscriptionTransactionAppService;
        public ICommonAppService _commonAppService { get; set; }

        CultureInfo new_lang = new CultureInfo("ar");
        public SubscriptionAppService(IRepository<Subscription, long> repository,
            UserManager userManager,
            ICommonAppService commonService
            , AbpNotificationHelper abpNotificationHelper
            , IRepository<User, long> userRepository
            , RoleManager roleManager
            , IRepository<UserDevice, long> userDeviceRepository,
            IRepository<Package, long> packageRepository,
            IRepository<Company, long> companyRepository,
            IBackgroundJobManager backgroundJobManager,
            ICommonAppService commonAppService,
            ICompanyWalletTransactionAppService companyWalletTransactionAppService,
            IStoredProcedureAppService storedProcedureAppService,
            ISubscriptionTransactionAppService subscriptionTransactionAppService
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userManager = userManager;
            _commonService = commonService;
            _abpNotificationHelper = abpNotificationHelper;
            _userRepository = userRepository;
            _roleManager = roleManager;
            _userDeviceRepository = userDeviceRepository;
            _packageRepository = packageRepository;
            _companyRepository = companyRepository;
            _backgroundJobManager = backgroundJobManager;
            _companyWalletTransactionAppService = companyWalletTransactionAppService;
            _commonAppService = commonAppService;
            _storedProcedureAppService = storedProcedureAppService;
            _subscriptionTransactionAppService = subscriptionTransactionAppService;
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<SubscriptionDto>> GetPaged(GetSubscriptionsInput input)
        {
            try
            {
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                {

                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            int subscriptionId = Convert.ToInt32(input.ids[i]);
                            Subscription subscription = await Repository.GetAll()
                                .FirstOrDefaultAsync(m => m.Id == subscriptionId);
                            if (subscription != null)
                            {
                                if (input.action == "Delete")//Delete
                                {
                                    await Repository.DeleteAsync(subscription);
                                }

                                else if (input.action == "Accept")
                                {
                                    subscription.Status = DepositStatus.Accepted;

                                    // get provider and update price then send notification 
                                    await UpdateSubscription(new UpdateSubscriptionDto
                                    {
                                        Id = subscription.Id,
                                    });


                                    await SendAcceptNotification(new UpdateCompanyWalletTransactionDto
                                    {
                                        Id = subscription.Id,
                                        CompanyId = subscription.CompanyId.Value
                                    });
                                }

                                else if (input.action == "Refuse")
                                {
                                    subscription.Status = DepositStatus.Refused;

                                    // send refuse notification 

                                    await SendRefuseNotification(new UpdateCompanyWalletTransactionDto
                                    {
                                        Id = subscription.Id,
                                        CompanyId = subscription.CompanyId.Value
                                    });
                                }

                            }
                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            int subscriptionId = Convert.ToInt32(input.ids[0]);

                            Subscription subscription = await Repository.GetAll()
                                .FirstOrDefaultAsync(m => m.Id == subscriptionId);
                            if (subscription != null)
                            {
                                if (input.action == "Delete")//Delete
                                {

                                    await Repository.DeleteAsync(subscription);
                                }

                                else if (input.action == "Accept")
                                {
                                    subscription.Status = DepositStatus.Accepted;

                                    // get provider and update price then send notification 
                                    await UpdateSubscription(new UpdateSubscriptionDto
                                    {
                                        Id = subscription.Id,
                                    });


                                    await SendAcceptNotification(new UpdateCompanyWalletTransactionDto
                                    {
                                        Id = subscription.Id,
                                        CompanyId = subscription.CompanyId.Value
                                    });
                                }

                                else if (input.action == "Refuse")
                                {
                                    subscription.Status = DepositStatus.Refused;

                                    // send refuse notification 

                                    await SendRefuseNotification(new UpdateCompanyWalletTransactionDto
                                    {
                                        Id = subscription.Id,
                                        CompanyId = subscription.CompanyId.Value
                                    });
                                }

                            }
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                    var _endedsubscripedPackages = await Repository.GetAll().Where(x => x.Status == DepositStatus.Accepted && x.IsExpired == false && DateTime.Now > x.EndDate).ToListAsync();
                    if (_endedsubscripedPackages != null)
                    {
                        foreach (var item in _endedsubscripedPackages.ToList())
                        {
                            item.IsExpired = true;
                            await Repository.UpdateAsync(item);
                            await UnitOfWorkManager.Current.SaveChangesAsync();
                        }
                    }

                    int count = await Repository.CountAsync();
                    var query = Repository.GetAll()
                        .Include(x => x.Company)
                        .Include(x => x.Bank)
                        .Include(x => x.Package).AsQueryable();
                    count = query.Count();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(input.PackageId.HasValue, a => a.PackageId == input.PackageId);
                    query = query.WhereIf(input.Price.HasValue, a => a.Price == input.Price);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.UserName), a => a.Company.NameAr.Contains(input.UserName) || a.Company.NameEn.Contains(input.UserName));
                    query = query.WhereIf(input.EndDateFrom.HasValue, a => a.EndDate >= input.EndDateFrom);
                    query = query.WhereIf(input.IsSpecialId.HasValue, a => a.Id == input.IsSpecialId);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.PackageName), a => a.Package.NameAr.Contains(input.PackageName) || a.Package.NameEn.Contains(input.PackageName));
                    query = query.WhereIf(input.Free.HasValue, a => a.Free == input.Free);
                    query = query.WhereIf(input.PayMethod.HasValue, a => a.PayMethod == input.PayMethod);
                    query = query.WhereIf(input.Status.HasValue, a => a.Status == input.Status);
                    query = query.WhereIf(input.PackageType.HasValue, a => a.PackageType == input.PackageType);
                    query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);

                    if (input.EndDateTo.HasValue)
                    {
                        input.EndDateTo = input.EndDateTo.Value.Add(TimeSpan.FromSeconds(86399));
                        query = query.WhereIf(input.EndDateTo.HasValue, a => a.EndDate <= input.EndDateTo);

                    }
                    int filteredCount = await query.CountAsync();

                    //var subscriptions = await query.Include(q => q.LastModifierUser).Include(q => q.CreatorUser).ToListAsync();

                    var subscriptions = await query.Include(x => x.CreatorUser)
                         .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length)
                         .ToListAsync();
                    //if (input.IsSpecialId.HasValue)
                    //{
                    //    subscriptionsList.ForEach(m => { if (m.Id == input.IsSpecialId.Value) m.IsSpecial = true; });
                    //    subscriptionsList = subscriptionsList.OrderByDescending(x => x.IsSpecial).Skip(input.start)
                    //        .Take(input.length).ToList();
                    //}
                    //else
                    //{

                    //var  subscriptionsList = subscriptionsList.OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                    //      .Skip(input.start)
                    //      .Take(input.length).ToList();
                    //}
                    return new DataTableOutputDto<SubscriptionDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<SubscriptionDto>>(subscriptions)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //[AbpAuthorize]
        public override async Task<SubscriptionDto> GetAsync(EntityDto<long> input)
        {
            var subscription = await Repository.GetAll()
                .Include(m => m.CreatorUser)
                .Include(m => m.Company)
                .Include(x => x.Bank)
                .Include(m => m.Package)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            var _subscription = ObjectMapper.Map<SubscriptionDto>(subscription);
            return _subscription;
        }

        [AbpAuthorize]
        public async Task<SubscriptionDto> GetSubscription(GetAllSubscriptions input)
        {
            var subscription = await Repository.GetAll()
                .Include(m => m.CreatorUser)
                .Include(m => m.Company)
                .Include(x => x.Bank)
                .Include(m => m.Package)
                .FirstOrDefaultAsync(x => x.Id == input.Id.Value && x.Company.UserId == AbpSession.UserId);
            var _subscription = ObjectMapper.Map<SubscriptionDto>(subscription);
            return _subscription;
        }

        [AbpAuthorize]
        public override async Task<SubscriptionDto> CreateAsync(CreateSubscriptionDto input)
        {
            try
            {
                var company = await _companyRepository.FirstOrDefaultAsync(x => x.Id == input.CompanyId);

                var _hasPacakge = await UserHasPackage(new ManagePackageStateInPut { CompanyId = input.CompanyId });
                if (_hasPacakge.InPackage)
                    throw new UserFriendlyException(L("Pages.Subscriptions.YouHaveSubscribe"));

                var package = await _packageRepository.GetAsync(input.PackageId.Value);
                if (package == null)
                {
                    throw new UserFriendlyException(L("Pages.Packages.Error.NotExist"));
                }
                //input.Price = package.Free ? 0 : package.Price;
                //input.Duration = package.Duration;
                input.NameAr = package.NameAr;
                input.NameEn = package.NameEn;
                input.DescAr = package.DescAr;
                input.DescEn = package.DescEn;
                input.IsPaid = true;

                //if (package.PackageType.HasValue && package.PackageType == PackageType.Day)
                //{
                //    input.EndDate = DateTime.Now.AddDays(package.Duration);
                //}
                //if (package.PackageType.HasValue && package.PackageType == PackageType.Month)
                //{
                //    input.EndDate = DateTime.Now.AddMonths(package.Duration);
                //}
                //if (package.PackageType.HasValue && package.PackageType == PackageType.Year)
                //{
                //    input.EndDate = DateTime.Now.AddYears(package.Duration);
                //}
                input.Free = package.Free;
                input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Subscriptions", CodeField = "Code" });

                var subscription = ObjectMapper.Map<Subscription>(input);
                await Repository.InsertAsync(subscription);
                await CurrentUnitOfWork.SaveChangesAsync();

                await CreateBackGroundJobs(new EntityDto<long> { Id = subscription.Id });


                return MapToEntityDto(subscription);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public override async Task<SubscriptionDto> UpdateAsync(UpdateSubscriptionDto input)
        {
            try
            {

                var subscriptionDetails = await Repository.GetAsync(input.Id);
                subscriptionDetails.EndDate = input.EndDate.Value;
                //ObjectMapper.Map(input, subscriptionDetails);
                await Repository.UpdateAsync(subscriptionDetails);
                await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(subscriptionDetails);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override async Task<PagedResultDto<SubscriptionDto>> GetAllAsync(GetAllSubscriptions input)
        {
            try
            {
                var query = Repository.GetAll();
                if (input.MaxCount)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }


                query = query.WhereIf(input.CreatorUserId.HasValue && input.CreatorUserId.Value > 0, m => m.CreatorUserId == input.CreatorUserId.Value);
                var subscriptions = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount)
                    .ToListAsync();
                return new PagedResultDto<SubscriptionDto>(
                   query.Count(), ObjectMapper.Map<List<SubscriptionDto>>(subscriptions)
                    );
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<PagedResultDto<SubscriptionDto>> GetCompanySubscriptions(GetAllSubscriptions input)
        {
            try
            {

                var query = Repository.GetAll()
                    .Include(a => a.Package)
                    .Where(a => a.Company.UserId == AbpSession.UserId);

                if (input.MaxCount)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = query.Count();
                }

                query = query.WhereIf(input.CreatorUserId.HasValue && input.CreatorUserId.Value > 0, m => m.CreatorUserId == input.CreatorUserId.Value);
                var subscriptions = await query.OrderByDescending(x => x.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .ToListAsync();

                return new PagedResultDto<SubscriptionDto>(
                   query.Count(), ObjectMapper.Map<List<SubscriptionDto>>(subscriptions)
                    );

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public async Task<SubscriptionDto> GetCurrentSubscription()
        {
            try
            {

                // get current user 
                var user = await _userRepository.GetAll().Include(a => a.Company).FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);
                if (user == null)
                    return null;

                long companyUserId = AbpSession.UserId.Value;
                if (user.UserType == UserTypes.Company)
                    companyUserId = user.Id;

                else if (user.UserType == UserTypes.Employee && user.CompanyId.HasValue == true)
                    companyUserId = user.Company.UserId.Value;
                else
                    return null;

                var query = Repository.GetAll()
               .Include(a => a.Company)
               .Include(x => x.Package)
               .Include(x => x.Bank)
               .Where(at => at.Company.UserId == companyUserId && at.Status == DepositStatus.Accepted);

                var subscriptions = await query.OrderByDescending(a => a.CreationTime).ToListAsync();
                var subscription = subscriptions.FirstOrDefault();

                return ObjectMapper.Map<SubscriptionDto>(subscription);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<InPackageOutPut> UserHasPackage(ManagePackageStateInPut input)
        {
            try
            {
                var userPackage = await Repository.GetAll()
                    .FirstOrDefaultAsync(at => at.CompanyId == input.CompanyId && DateTime.Now <= at.EndDate && at.IsPaid);

                return new InPackageOutPut
                {
                    InPackage = userPackage != null ? true : false,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new InPackageOutPut { Message = ex.Message };
            }
        }

        public async Task<SubscriptionDto> UpdatePayment(UpdateSubscriptionDto input)
        {

            var subscription = await Repository.GetAll().Include(x => x.Company).Include(x => x.Package).FirstOrDefaultAsync(x => x.Id == input.Id);



            var package = await _packageRepository.FirstOrDefaultAsync(at => at.Id == subscription.PackageId);
            //subscription.Price = package.Price;
            subscription.IsPaid = true;
            await Repository.UpdateAsync(subscription);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            await CreateBackGroundJobs(new EntityDto<long> { Id = subscription.Id });
            //await SendNotify(new SendNotifyInput
            //{
            //    PackageId = package.Id,
            //    SubscriptionId = subscription.Id,
            //    SenderUserId = subscription.Company.UserId.Value,
            //    SenderName = subscription.Company.NameAr,
            //    ToAdmins = true
            //});
            return ObjectMapper.Map<SubscriptionDto>(subscription);
        }


        [AbpAuthorize]
        //[UnitOfWork(isolationLevel: System.Transactions.IsolationLevel.Serializable)]
        public async Task<HandleSubscriptionOutput> SubscribePackage(CreateSubscriptionDto input)
        {



            // get current user 
            var user = await _userRepository.GetAll().Include(a => a.Company).FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);
            if (user == null)
                return null;

            long companyUserId = AbpSession.UserId.Value;
            if (user.UserType == UserTypes.Company)
                companyUserId = user.Id;

            else if (user.UserType == UserTypes.Employee && user.CompanyId.HasValue == true)
                companyUserId = user.Company.UserId.Value;


            else
                return null;


            var company = await _companyRepository.FirstOrDefaultAsync(x => x.UserId == companyUserId);

            var _hasPacakge = await UserHasPackage(new ManagePackageStateInPut { CompanyId = company.Id });
            if (_hasPacakge.InPackage)
                throw new UserFriendlyException(L("Pages.Subscriptions.YouHaveSubscribe"));

            //var _hasPendingSubscription = await Repository.GetAll()
            //   .Include(a => a.Company)
            //   .Include(x => x.Package)
            //   .Include(x => x.Bank)
            //   .FirstOrDefaultAsync(at => at.Company.UserId == companyUserId && at.Status == DepositStatus.Pending && at.IsExpired == false);

            //if (_hasPendingSubscription != null)
            //    throw new UserFriendlyException(L("Pages.Subscriptions.YouHavePendingSubscription"));

            var subscription = new Subscription();
            var package = await _packageRepository.GetAsync(input.PackageId.Value);
            if (package == null)
            {
                throw new UserFriendlyException(L("Pages.Packages.Error.NotExist"));
            }


            if (input.PayMethod == PayMethod.Wallet)
            {
                // check company wallet amount 
                if (company.WalletAmount < input.Price)
                {
                    throw new UserFriendlyException(L("Pages.Wallets.WalletAmountNotEnough"));
                }

                input.Status = DepositStatus.Accepted;
                input.IsPaid = true;
            }

            //if (input.PayMethod == PayMethod.BankTransfer)
            //{
            //    // check company wallet amount 
            //    if (string.IsNullOrEmpty(input.ReceiptImage))
            //    {
            //        throw new UserFriendlyException(L("Pages.Wallets.Messages.AttachReceiptImage"));
            //    }

            //    input.Status = DepositStatus.Pending;
            //}

            //subscription = await Repository.FirstOrDefaultAsync(at => at.CompanyId == company.Id && !at.IsPaid);
            //if (subscription != null)
            //{
            //    //subscription.Price = package.Free ? 0 : package.Price;
            //    subscription.CompanyId = company.Id;
            //    subscription.PackageId = package.Id;
            //    subscription.IsPaid = package.Free;

            //    subscription.Free = package.Free;
            //    await Repository.UpdateAsync(subscription);
            //    await UnitOfWorkManager.Current.SaveChangesAsync();
            //}
            //else
            //{

            input.CompanyId = company.Id;
            input.NameAr = package.NameAr;
            input.NameEn = package.NameEn;
            input.DescAr = package.DescAr;
            input.DescEn = package.DescEn;
            input.Free = package.Free;

            input.AttachNfcPrice = package.AttachNfcPrice;
            input.YearlyPrice = package.YearlyPrice;
            input.MonthlyPrice = package.MonthlyPrice;
            input.VeichlesFrom = package.VeichlesFrom;
            input.VeichlesTo = package.VeichlesTo;

            if (input.PayMethod == PayMethod.Wallet)
            {
                if (input.PackageType == PackageType.Monthly)
                    input.EndDate = DateTime.UtcNow.AddDays(30);
                else if (input.PackageType == PackageType.Yearly)
                    input.EndDate = DateTime.UtcNow.AddDays(365);
            }
            input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Subscriptions", CodeField = "Code" });

            subscription = ObjectMapper.Map<Subscription>(input);
            await Repository.InsertAsync(subscription);
            await UnitOfWorkManager.Current.SaveChangesAsync();


            if (input.PayMethod == PayMethod.Wallet)
            {
                // discount from wallet 
                var wallet = await _companyWalletTransactionAppService.CreateAsync(new CreateCompanyWalletTransactionDto
                {
                    CompanyId = subscription.CompanyId.Value,
                    Amount = subscription.Price,
                    TransactionType = TransactionType.Refund,
                    Note = L("Pages.Wallets.SubscriptionNote"),
                    SubscriptionId = subscription.Id
                });


                subscription.CompanyWalletTransactionId = wallet.Id;
                await Repository.UpdateAsync(subscription);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                await CreateBackGroundJobs(new EntityDto<long> { Id = subscription.Id });
            }

            await SendNotify(new SendNotifyInput
            {
                PackageId = package.Id,
                SubscriptionId = subscription.Id,
                SenderUserId = company.UserId.Value,
                SenderName = company.NameAr,
                ToAdmins = true
            });


            // create subscriptionTransaction
            var upgrade = await _subscriptionTransactionAppService.CreateAsync(new CreateSubscriptionTransactionDto
            {
                TransactionType = SubscriptionTransactionType.Subscribe,
                SubscriptionId = subscription.Id,
                AttachNfc = subscription.AttachNfc,
                AttachNfcPrice = subscription.AttachNfcPrice,
                AutoRenewal = subscription.AutoRenewal,
                CompanyId = subscription.CompanyId,
                CompanyWalletTransactionId = subscription.CompanyWalletTransactionId,
                DescAr = subscription.DescAr,
                DescEn = subscription.DescEn,
                EndDate = subscription.EndDate,
                IsPaid = subscription.IsPaid,
                Free = subscription.Free,
                MonthlyPrice = subscription.MonthlyPrice,
                NameAr = subscription.NameAr,
                NameEn = subscription.NameEn,
                NfcCount = subscription.NfcCount,
                NetPrice = subscription.NetPrice,
                PayMethod = subscription.PayMethod,
                VeichlesFrom = subscription.VeichlesFrom,
                VeichlesTo = subscription.VeichlesTo,
                VeichlesCount = subscription.VeichlesCount,
                Price = subscription.Price,
                YearlyPrice = subscription.YearlyPrice,
                BankId = subscription.BankId,
                PackageId = subscription.PackageId,
                PackageType = subscription.PackageType,
                Tax = subscription.Tax,
                TaxAmount = subscription.TaxAmount,
                ReceiptImage = subscription.ReceiptImage,
            });

            //// create subscriptionTransaction
            //await _storedProcedureAppService.CopySubscriptionToTransactions(new Helpers.Dtos.CopySubscriptionToTransactionsInput
            //{
            //    SubscriptionId = newSubscription.Id,
            //    TransactionType = SubscriptionTransactionType.Renew
            //});

            return new HandleSubscriptionOutput { Id = subscription.Id, SubscriptionTransactionId = upgrade.Id, Success = true };


            //// create subscriptionTransaction
            //await _storedProcedureAppService.CopySubscriptionToTransactions(new Helpers.Dtos.CopySubscriptionToTransactionsInput
            //{
            //    SubscriptionId = subscription.Id,
            //    TransactionType = SubscriptionTransactionType.Subscribe
            //});

            //return MapToEntityDto(subscription);
        }

        [AbpAuthorize]
        public async Task<HandleSubscriptionOutput> RenewSubscription(RenewSubscriptionDto input)
        {



            // get current user 
            var user = await _userRepository.GetAll().Include(a => a.Company).FirstOrDefaultAsync(a => a.Id == AbpSession.UserId);
            if (user == null)
                return null;

            long companyUserId = AbpSession.UserId.Value;
            if (user.UserType == UserTypes.Company)
                companyUserId = user.Id;

            else if (user.UserType == UserTypes.Employee && user.CompanyId.HasValue == true)
                companyUserId = user.Company.UserId.Value;


            else
                return null;


            var company = await _companyRepository.FirstOrDefaultAsync(x => x.UserId == companyUserId);

            var _hasPacakge = await UserHasPackage(new ManagePackageStateInPut { CompanyId = company.Id });
            if (_hasPacakge.InPackage)
                throw new UserFriendlyException(L("Pages.Subscriptions.YouHaveSubscribe"));

            var subscription = await Repository.FirstOrDefaultAsync(a => a.Id == input.SubscriptionId);
            if (subscription == null)
            {
                throw new UserFriendlyException(L("Pages.Subscriptions.Error.NotExist"));
            }

            var package = await _packageRepository.GetAsync(subscription.PackageId.Value);
            if (package == null)
            {
                throw new UserFriendlyException(L("Pages.Packages.Error.NotExist"));
            }


            // calculate the new prices 

            var prices = CalculateSupscriptionPrice(new CalculateSupscriptionPriceInput
            {
                AttachNfc = subscription.AttachNfc,
                AttachNfcPrice = package.AttachNfcPrice,
                MonthlyPrice = package.MonthlyPrice,
                YearlyPrice = package.YearlyPrice,
                NfcCount = subscription.NfcCount,
                PackageType = subscription.PackageType ?? PackageType.Monthly,
                Tax = 15,
                VeichlesCount = subscription.VeichlesCount
            });



            // check company wallet amount 
            if (company.WalletAmount < prices.Price)
            {
                throw new UserFriendlyException(L("Pages.Wallets.WalletAmountNotEnough"));
            }

            var newSubscription = new Subscription
            {
                NameAr = package.NameAr,
                NameEn = package.NameEn,
                DescAr = package.DescAr,
                DescEn = package.DescEn,
                AttachNfc = subscription.AttachNfc,
                AttachNfcPrice = package.AttachNfcPrice,
                AutoRenewal = subscription.AutoRenewal,
                BankId = subscription.BankId,
                CompanyId = company.Id,
                Free = subscription.Free,
                IsPaid = subscription.IsPaid,
                MonthlyPrice = package.MonthlyPrice,
                NetPrice = prices.NetPrice,
                NfcCount = subscription.NfcCount,
                PackageId = package.Id,
                PackageType = subscription.PackageType,
                PayMethod = subscription.PayMethod,
                Price = prices.Price,
                Status = subscription.Status,
                Tax = subscription.Tax,
                TaxAmount = prices.TaxAmount,
                VeichlesCount = subscription.VeichlesCount,
                VeichlesFrom = package.VeichlesFrom,
                VeichlesTo = package.VeichlesTo,
                YearlyPrice = package.YearlyPrice,
                ReceiptImage = subscription.ReceiptImage,
            };

            newSubscription.Status = DepositStatus.Accepted;
            newSubscription.IsPaid = true;


            if (newSubscription.PayMethod == PayMethod.Wallet)
            {
                if (newSubscription.PackageType == PackageType.Monthly)
                    newSubscription.EndDate = DateTime.UtcNow.AddDays(30);
                else if (newSubscription.PackageType == PackageType.Yearly)
                    newSubscription.EndDate = DateTime.UtcNow.AddDays(365);
            }
            newSubscription.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Subscriptions", CodeField = "Code" });


            await Repository.InsertAsync(newSubscription);
            await UnitOfWorkManager.Current.SaveChangesAsync();


            if (newSubscription.PayMethod == PayMethod.Wallet)
            {
                // discount from wallet 
                var wallet = await _companyWalletTransactionAppService.CreateAsync(new CreateCompanyWalletTransactionDto
                {
                    CompanyId = newSubscription.CompanyId.Value,
                    Amount = newSubscription.Price,
                    TransactionType = TransactionType.Refund,
                    Note = L("Pages.Wallets.SubscriptionNote"),
                    SubscriptionId = newSubscription.Id
                });


                newSubscription.CompanyWalletTransactionId = wallet.Id;
                await Repository.UpdateAsync(newSubscription);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                await CreateBackGroundJobs(new EntityDto<long> { Id = newSubscription.Id });
            }

            await SendNotify(new SendNotifyInput
            {
                PackageId = package.Id,
                SubscriptionId = newSubscription.Id,
                SenderUserId = company.UserId.Value,
                SenderName = company.NameAr,
                ToAdmins = true
            });


            // create subscriptionTransaction
            var upgrade = await _subscriptionTransactionAppService.CreateAsync(new CreateSubscriptionTransactionDto
            {
                TransactionType = SubscriptionTransactionType.Renew,
                SubscriptionId = newSubscription.Id,
                AttachNfc = newSubscription.AttachNfc,
                AttachNfcPrice = newSubscription.AttachNfcPrice,
                AutoRenewal = newSubscription.AutoRenewal,
                CompanyId = newSubscription.CompanyId,
                CompanyWalletTransactionId = newSubscription.CompanyWalletTransactionId,
                DescAr = newSubscription.DescAr,
                DescEn = newSubscription.DescEn,
                EndDate = newSubscription.EndDate,
                IsPaid = newSubscription.IsPaid,
                Free = newSubscription.Free,
                MonthlyPrice = newSubscription.MonthlyPrice,
                NameAr = newSubscription.NameAr,
                NameEn = newSubscription.NameEn,
                NfcCount = newSubscription.NfcCount,
                NetPrice = newSubscription.NetPrice,
                PayMethod = newSubscription.PayMethod,
                VeichlesFrom = newSubscription.VeichlesFrom,
                VeichlesTo = newSubscription.VeichlesTo,
                VeichlesCount = newSubscription.VeichlesCount,
                Price = newSubscription.Price,
                YearlyPrice = newSubscription.YearlyPrice,
                BankId = newSubscription.BankId,
                PackageId = newSubscription.PackageId,
                PackageType = newSubscription.PackageType,
                Tax = newSubscription.Tax,
                TaxAmount = newSubscription.TaxAmount,
                ReceiptImage = newSubscription.ReceiptImage,
            });

            //// create subscriptionTransaction
            //await _storedProcedureAppService.CopySubscriptionToTransactions(new Helpers.Dtos.CopySubscriptionToTransactionsInput
            //{
            //    SubscriptionId = newSubscription.Id,
            //    TransactionType = SubscriptionTransactionType.Renew
            //});

            return new HandleSubscriptionOutput { Id = newSubscription.Id, SubscriptionTransactionId = upgrade.Id, Success = true };


            //// create subscriptionTransaction
            //await _storedProcedureAppService.CopySubscriptionToTransactions(new Helpers.Dtos.CopySubscriptionToTransactionsInput
            //{
            //    SubscriptionId = newSubscription.Id,
            //    TransactionType = SubscriptionTransactionType.Renew
            //});

            //return MapToEntityDto(newSubscription);
        }



        [UnitOfWork(false)]
        public async Task<bool> CreateBackGroundJobs(EntityDto<long> input)
        {
            try
            {

                var subscription = await Repository.GetAll()
                    .FirstOrDefaultAsync(a => a.Id == input.Id);

                var currentTime = Abp.Timing.Clock.Now;
                var resDateTime = subscription.EndDate;
                var remainingMinutes = resDateTime.Value.Subtract(Abp.Timing.Clock.Now).TotalMinutes;


                await _backgroundJobManager.EnqueueAsync<AfterSubscriptionEndDateScheduleJob, AfterSubscriptionEndDateScheduleJobArgs>(new AfterSubscriptionEndDateScheduleJobArgs()
                {
                    SubscriptionId = subscription.Id,
                }, BackgroundJobPriority.High, TimeSpan.FromMinutes(remainingMinutes));


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //[UnitOfWork(false)]
        //public async Task<bool> CreateBackGroundJobs(EntityDto<long> input)
        //{
        //    try
        //    {
        //        // Fetch subscription data outside of any transactional scope
        //        var subscription = await Repository.GetAll()
        //            .FirstOrDefaultAsync(a => a.Id == input.Id);

        //        //if (subscription == null)
        //        //{
        //        //    throw new Exception("Subscription not found.");
        //        //}

        //        var currentTime = Abp.Timing.Clock.Now;
        //        var resDateTime = subscription.EndDate;
        //        //if (!resDateTime.HasValue)
        //        //{
        //        //    throw new Exception("Subscription end date is not defined.");
        //        //}

        //        var remainingMinutes = resDateTime.Value.Subtract(currentTime).TotalMinutes;

        //        // Perform Hangfire job scheduling outside of a database transaction
        //        await Task.Run(() =>
        //        {
        //            _backgroundJobManager.EnqueueAsync<AfterSubscriptionEndDateScheduleJob, AfterSubscriptionEndDateScheduleJobArgs>(
        //                new AfterSubscriptionEndDateScheduleJobArgs
        //                {
        //                    SubscriptionId = subscription.Id,
        //                },
        //                BackgroundJobPriority.High,
        //                TimeSpan.FromMinutes(remainingMinutes)
        //            );
        //        });

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle or log the exception appropriately
        //        throw new Exception($"Error creating background jobs: {ex.Message}", ex);
        //    }
        //}



        public async Task<bool> HandleAfterSubscriptionEndDate(EntityDto<long> input)
        {
            var _subscripePackage = await Repository.GetAll().Include(x => x.Company).Include(x => x.Package).FirstOrDefaultAsync(x => x.Id == input.Id);
            if (_subscripePackage != null)
            {

                if (_subscripePackage.AutoRenewal == true)
                {
                    await HandleAutoRenewSubscription(new RenewSubscriptionDto
                    {
                        CompanyId = _subscripePackage.CompanyId.Value,
                        SubscriptionId = input.Id
                    });
                }
                else
                {
                    _subscripePackage.IsExpired = true;
                    await Repository.UpdateAsync(_subscripePackage);

                    // send notification to company 
                    await SendExpireNotification(new UpdateCompanyWalletTransactionDto
                    {
                        Id = _subscripePackage.Id,
                        CompanyId = _subscripePackage.CompanyId ?? 0
                    });
                }

                return true;
            }
            else
            {
                return false;
            }
        }


        public CalculateSupscriptionPriceOutput CalculateSupscriptionPrice(CalculateSupscriptionPriceInput input)
        {

            decimal priceForVeichle = input.MonthlyPrice;
            decimal attachNfcForVeichle = 0;

            if (input.PackageType == PackageType.Monthly)
                priceForVeichle = input.MonthlyPrice;
            else if (input.PackageType == PackageType.Yearly)
                priceForVeichle = input.YearlyPrice;

            if (input.AttachNfc == true)
                attachNfcForVeichle = input.AttachNfcPrice;


            decimal veichlesCountPrices = input.VeichlesCount * priceForVeichle;
            decimal nfcPrices = input.NfcCount * attachNfcForVeichle;

            decimal netPrice = veichlesCountPrices + nfcPrices;

            decimal taxAmount = netPrice * input.Tax / 100;

            decimal price = netPrice + taxAmount;


            return new CalculateSupscriptionPriceOutput
            {
                NetPrice = netPrice,
                NfcPrices = nfcPrices,
                Price = price,
                TaxAmount = taxAmount,
                VeichlesCountPrices = veichlesCountPrices
            };
        }


        public async Task<HandleSubscriptionOutput> HandleAutoRenewSubscription(RenewSubscriptionDto input)
        {
            var company = await _companyRepository.FirstOrDefaultAsync(x => x.Id == input.CompanyId);

            var _hasPacakge = await UserHasPackage(new ManagePackageStateInPut { CompanyId = company.Id });
            if (_hasPacakge.InPackage)
                throw new UserFriendlyException(L("Pages.Subscriptions.YouHaveSubscribe"));

            var subscription = await Repository.FirstOrDefaultAsync(a => a.Id == input.SubscriptionId);
            if (subscription == null)
            {
                throw new UserFriendlyException(L("Pages.Subscriptions.Error.NotExist"));
            }

            var package = await _packageRepository.GetAsync(subscription.PackageId.Value);
            if (package == null)
            {
                throw new UserFriendlyException(L("Pages.Packages.Error.NotExist"));
            }


            // calculate the new prices 

            var prices = CalculateSupscriptionPrice(new CalculateSupscriptionPriceInput
            {
                AttachNfc = subscription.AttachNfc,
                AttachNfcPrice = package.AttachNfcPrice,
                MonthlyPrice = package.MonthlyPrice,
                YearlyPrice = package.YearlyPrice,
                NfcCount = subscription.NfcCount,
                PackageType = subscription.PackageType ?? PackageType.Monthly,
                Tax = 15,
                VeichlesCount = subscription.VeichlesCount
            });

            // check company wallet amount 
            if (company.WalletAmount < prices.Price)
            {

                await SendNotifyWalletError(new SendNotifyInput
                {
                    PackageId = package.Id,
                    SenderUserId = company.UserId.Value,
                    SenderName = company.NameAr
                });

                return new HandleSubscriptionOutput { };
                //throw new UserFriendlyException(L("Pages.Wallets.WalletAmountNotEnough"));
            }

            var newSubscription = new Subscription
            {
                NameAr = package.NameAr,
                NameEn = package.NameEn,
                DescAr = package.DescAr,
                DescEn = package.DescEn,
                AttachNfc = subscription.AttachNfc,
                AttachNfcPrice = package.AttachNfcPrice,
                AutoRenewal = subscription.AutoRenewal,
                MonthlyPrice = package.MonthlyPrice,
                YearlyPrice = package.YearlyPrice,
                Tax = subscription.Tax,
                TaxAmount = prices.TaxAmount,
                NetPrice = prices.NetPrice,
                Price = prices.Price,
                NfcCount = subscription.NfcCount,
                CompanyId = company.Id,
                Free = subscription.Free,
                IsPaid = subscription.IsPaid,
                PackageId = package.Id,
                PackageType = subscription.PackageType,
                PayMethod = subscription.PayMethod,
                Status = subscription.Status,
                VeichlesCount = subscription.VeichlesCount,
                VeichlesFrom = package.VeichlesFrom,
                VeichlesTo = package.VeichlesTo,
            };

            newSubscription.Status = DepositStatus.Accepted;
            newSubscription.IsPaid = true;


            if (newSubscription.PayMethod == PayMethod.Wallet)
            {
                if (newSubscription.PackageType == PackageType.Monthly)
                    newSubscription.EndDate = DateTime.UtcNow.AddDays(30);
                else if (newSubscription.PackageType == PackageType.Yearly)
                    newSubscription.EndDate = DateTime.UtcNow.AddDays(365);
            }
            newSubscription.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "Subscriptions", CodeField = "Code" });


            await Repository.InsertAsync(newSubscription);
            await UnitOfWorkManager.Current.SaveChangesAsync();


            if (newSubscription.PayMethod == PayMethod.Wallet)
            {
                // discount from wallet 
                var wallet = await _companyWalletTransactionAppService.CreateAsync(new CreateCompanyWalletTransactionDto
                {
                    CompanyId = newSubscription.CompanyId.Value,
                    Amount = newSubscription.Price,
                    TransactionType = TransactionType.Refund,
                    Note = L("Pages.Wallets.SubscriptionNote"),
                    SubscriptionId = newSubscription.Id
                });


                newSubscription.CompanyWalletTransactionId = wallet.Id;
                await Repository.UpdateAsync(newSubscription);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                await CreateBackGroundJobs(new EntityDto<long> { Id = newSubscription.Id });
            }

            await SendAutoRenewNotify(new SendNotifyInput
            {
                PackageId = package.Id,
                SubscriptionId = newSubscription.Id,
                SenderUserId = company.UserId.Value,
                SenderName = company.NameAr,
                ToAdmins = true
            });



            // create subscriptionTransaction
            var upgrade = await _subscriptionTransactionAppService.CreateAsync(new CreateSubscriptionTransactionDto
            {
                TransactionType = SubscriptionTransactionType.Renew,
                SubscriptionId = newSubscription.Id,
                AttachNfc = newSubscription.AttachNfc,
                AttachNfcPrice = newSubscription.AttachNfcPrice,
                AutoRenewal = newSubscription.AutoRenewal,
                CompanyId = newSubscription.CompanyId,
                CompanyWalletTransactionId = newSubscription.CompanyWalletTransactionId,
                DescAr = newSubscription.DescAr,
                DescEn = newSubscription.DescEn,
                EndDate = newSubscription.EndDate,
                IsPaid = newSubscription.IsPaid,
                Free = newSubscription.Free,
                MonthlyPrice = newSubscription.MonthlyPrice,
                NameAr = newSubscription.NameAr,
                NameEn = newSubscription.NameEn,
                NfcCount = newSubscription.NfcCount,
                NetPrice = newSubscription.NetPrice,
                PayMethod = newSubscription.PayMethod,
                VeichlesFrom = newSubscription.VeichlesFrom,
                VeichlesTo = newSubscription.VeichlesTo,
                VeichlesCount = newSubscription.VeichlesCount,
                Price = newSubscription.Price,
                YearlyPrice = newSubscription.YearlyPrice,
                BankId = newSubscription.BankId,
                PackageId = newSubscription.PackageId,
                PackageType = newSubscription.PackageType,
                Tax = newSubscription.Tax,
                TaxAmount = newSubscription.TaxAmount,
                ReceiptImage = newSubscription.ReceiptImage,
            });

            //// create subscriptionTransaction
            //await _storedProcedureAppService.CopySubscriptionToTransactions(new Helpers.Dtos.CopySubscriptionToTransactionsInput
            //{
            //    SubscriptionId = newSubscription.Id,
            //    TransactionType = SubscriptionTransactionType.Renew
            //});

            return new HandleSubscriptionOutput { Id = newSubscription.Id, SubscriptionTransactionId = upgrade.Id, Success = true };
        }


        [AbpAuthorize]
        public async Task<HandleSubscriptionOutput> UpgradeSubscription(UpgradeSubscriptionDto input)
        {

            var subscription = await Repository.FirstOrDefaultAsync(a => a.Id == input.Id);
            if (subscription == null)
            {
                throw new UserFriendlyException(L("Pages.Subscriptions.Error.NotExist"));
            }

            if (subscription.IsExpired == true)
            {
                throw new UserFriendlyException(L("Pages.Subscriptions.Error.NotExist"));
            }

            var package = await _packageRepository.GetAsync(input.PackageId.Value);
            if (package == null)
            {
                throw new UserFriendlyException(L("Pages.Packages.Error.NotExist"));
            }

            var company = await _companyRepository.FirstOrDefaultAsync(x => x.Id == subscription.CompanyId);
            // check company wallet amount 
            if (company.WalletAmount < input.Price)
            {
                throw new UserFriendlyException(L("Pages.Wallets.WalletAmountNotEnough"));
            }


            subscription.PackageId = package.Id;
            subscription.CompanyId = company.Id;
            subscription.VeichlesCount += input.VeichlesCount;
            subscription.VeichlesFrom = package.VeichlesFrom;
            subscription.VeichlesTo = package.VeichlesTo;
            subscription.Price += input.Price;
            subscription.MonthlyPrice = package.MonthlyPrice;
            subscription.YearlyPrice = package.YearlyPrice;
            subscription.Tax = input.Tax;
            subscription.TaxAmount += input.TaxAmount;
            subscription.AttachNfcPrice = input.AttachNfcPrice;
            subscription.NetPrice += input.NetPrice;
            subscription.NfcCount += input.NfcCount;
            subscription.NameAr = package.NameAr;
            subscription.NameEn = package.NameEn;
            subscription.DescAr = package.DescAr;
            subscription.DescEn = package.DescEn;

            // update subscription 
            await Repository.UpdateAsync(subscription);
            await UnitOfWorkManager.Current.SaveChangesAsync();


            // discount from wallet 
            var wallet = await _companyWalletTransactionAppService.CreateAsync(new CreateCompanyWalletTransactionDto
            {
                CompanyId = subscription.CompanyId.Value,
                Amount = input.Price,
                TransactionType = TransactionType.Refund,
                Note = L("Pages.Wallets.UpgradeSubscriptionNote"),
                SubscriptionId = subscription.Id
            });


            // create subscriptionTransaction
            var upgrade = await _subscriptionTransactionAppService.CreateAsync(new CreateSubscriptionTransactionDto
            {
                TransactionType = SubscriptionTransactionType.Upgrade,
                SubscriptionId = subscription.Id,
                AttachNfc = input.AttachNfc,
                AttachNfcPrice = input.AttachNfcPrice,
                AutoRenewal = subscription.AutoRenewal,
                CompanyId = subscription.CompanyId,
                CompanyWalletTransactionId = wallet.Id,
                DescAr = subscription.DescAr,
                DescEn = subscription.DescEn,
                EndDate = subscription.EndDate,
                IsPaid = subscription.IsPaid,
                Free = subscription.Free,
                MonthlyPrice = subscription.MonthlyPrice,
                NameAr = subscription.NameAr,
                NameEn = subscription.NameEn,
                NfcCount = input.NfcCount,
                NetPrice = input.NetPrice,
                PayMethod = subscription.PayMethod,
                VeichlesFrom = subscription.VeichlesFrom,
                VeichlesTo = subscription.VeichlesTo,
                VeichlesCount = input.VeichlesCount,
                Price = input.Price,
                YearlyPrice = subscription.YearlyPrice,
                BankId = subscription.BankId,
                PackageId = subscription.PackageId,
                PackageType = subscription.PackageType,
                Tax = input.Tax,
                TaxAmount = input.TaxAmount,
                ReceiptImage = input.ReceiptImage,
            });

            await SendUpgradeNotify(new SendNotifyInput
            {
                PackageId = package.Id,
                SubscriptionId = subscription.Id,
                SenderUserId = company.UserId.Value,
                SenderName = company.NameAr,
                Message = "Pages.Notifications.UpgradeSubscription",
                ToAdmins = true
            });
            return new HandleSubscriptionOutput { Id = subscription.Id, SubscriptionTransactionId = upgrade.Id, Success = true };
        }

        public async Task<bool> UpdateSubscription(UpdateSubscriptionDto input)
        {
            try
            {
                // companyWalletTransactions 
                var subscription = await Repository.GetAll().FirstOrDefaultAsync(q => q.Id == input.Id);

                subscription.IsExpired = false;
                subscription.IsPaid = true;
                subscription.Status = DepositStatus.Accepted;

                if (subscription.PackageType == PackageType.Monthly)
                    subscription.EndDate = DateTime.UtcNow.AddDays(30);
                else if (subscription.PackageType == PackageType.Yearly)
                    subscription.EndDate = DateTime.UtcNow.AddMonths(365);

                await Repository.UpdateAsync(subscription);
                await UnitOfWorkManager.Current.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> SendNotify(SendNotifyInput input)
        {

            try
            {


                List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
                if (input.ToAdmins)
                {
                    //var role = await _roleManager.GetRoleByNameAsync(SayarahConsts.RolesNames.Admin);
                    var admins = _userManager.Users.Where(x => x.UserType == UserTypes.Admin);
                    foreach (var usr in admins)
                    {
                        targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: usr.Id));
                    }
                }
                else
                {
                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.RecieverUserId));
                }



                CreateNotificationDto CreateNotificationData = new CreateNotificationDto
                {
                    SenderUserId = input.SenderUserId,
                    SenderUserName = input.SenderName,
                    Message = input.ToAdmins ? "Pages.Notifications.NewSubscribePackage" : "Pages.Notifications.NewSubscribePackageToYou",
                    EntityType = Entity_Type.NewSubscribePackage,
                    EntityId = input.SubscriptionId,
                    SubscriptionId = input.SubscriptionId
                };
                //Publish Notification Data
                await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewSubscribePackage, CreateNotificationData, targetUsersId.ToArray());

                return true;


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> SendUpgradeNotify(SendNotifyInput input)
        {

            try
            {
                List<UserIdentifier> targetUsersId = new List<UserIdentifier>();

                //var role = await _roleManager.GetRoleByNameAsync(SayarahConsts.RolesNames.Admin);
                var admins = _userManager.Users.Where(x => x.UserType == UserTypes.Admin);
                foreach (var usr in admins)
                {
                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: usr.Id));
                }

                CreateNotificationDto CreateNotificationData = new CreateNotificationDto
                {
                    SenderUserId = input.SenderUserId,
                    SenderUserName = input.SenderName,
                    Message = "Pages.Notifications.UpgradeSubscription",
                    EntityType = Entity_Type.UpgradeSubscription,
                    EntityId = input.SubscriptionId,
                    SubscriptionId = input.SubscriptionId
                };
                //Publish Notification Data
                await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.UpgradeSubscription, CreateNotificationData, targetUsersId.ToArray());

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<bool> SendAutoRenewNotify(SendNotifyInput input)
        {

            try
            {
                List<UserIdentifier> targetUsersId = new List<UserIdentifier>();

                var admins = _userManager.Users.Where(x => x.UserType == UserTypes.Admin);
                foreach (var usr in admins)
                {
                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: usr.Id));
                }


                CreateNotificationDto CreateNotificationData = new CreateNotificationDto
                {
                    SenderUserId = input.SenderUserId,
                    SenderUserName = input.SenderName,
                    Message = "Pages.Notifications.NewSubscribePackage",
                    EntityType = Entity_Type.NewSubscribePackage,
                    EntityId = input.SubscriptionId,
                    SubscriptionId = input.SubscriptionId
                };
                //Publish Notification Data
                await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewSubscribePackage, CreateNotificationData, targetUsersId.ToArray());

                // send notification to company 

                targetUsersId = new List<UserIdentifier>();
                targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.SenderUserId));

                // get all employees in company 

                var emps = await _userRepository.GetAll()
                    .Include(a => a.Company)
                    .Where(a => a.Company.UserId == input.SenderUserId && a.UserType == UserTypes.Employee && a.BranchId.HasValue == false)
                    .ToListAsync();

                if (emps != null)
                {
                    foreach (var usr in emps.ToList())
                    {
                        targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: usr.Id));
                    }
                }

                CreateNotificationData = new CreateNotificationDto
                {
                    SenderUserId = input.SenderUserId,
                    SenderUserName = input.SenderName,
                    Message = "Pages.Notifications.RenewSubscription",
                    EntityType = Entity_Type.NewSubscribePackage,
                    EntityId = input.SubscriptionId,
                    SubscriptionId = input.SubscriptionId
                };
                //Publish Notification Data
                await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewSubscribePackage, CreateNotificationData, targetUsersId.ToArray());

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> SendNotifyWalletError(SendNotifyInput input)
        {

            try
            {
                List<UserIdentifier> targetUsersId = new List<UserIdentifier>();

                targetUsersId = new List<UserIdentifier>();
                targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.SenderUserId));

                // get all employees in company 

                var emps = await _userRepository.GetAll()
                    .Include(a => a.Company)
                    .Where(a => a.Company.UserId == input.SenderUserId && a.UserType == UserTypes.Employee && a.BranchId.HasValue == false)
                    .ToListAsync();

                if (emps != null)
                {
                    foreach (var usr in emps.ToList())
                    {
                        targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: usr.Id));
                    }
                }

                CreateNotificationDto CreateNotificationData = new CreateNotificationDto
                {
                    SenderUserId = input.SenderUserId,
                    //SenderUserName = input.SenderName,
                    Message = "Pages.Notifications.RenewSubscriptionWalletError",
                    EntityType = Entity_Type.SubscribePackageWalletError,
                    //EntityId = input.SubscriptionId,
                    //SubscriptionId = input.SubscriptionId
                };
                //Publish Notification Data
                await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.SubscribePackageWalletError, CreateNotificationData, targetUsersId.ToArray());

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<bool> SendAcceptNotification(UpdateCompanyWalletTransactionDto input)
        {

            var company = await _companyRepository.FirstOrDefaultAsync(input.CompanyId);
            if (company == null)
                return false;

            // send notification to provider here 

            List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
            string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");

            targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: company.UserId.Value));

            CreateNotificationDto CreateNotificationData = new CreateNotificationDto
            {
                SenderUserId = AbpSession.UserId.Value,
                Message = L("Pages.Subscriptions.Messages.Approved"),
                EntityType = Entity_Type.AcceptSubscriptionTransfer,
                EntityId = input.Id,
                CompanyId = company.Id,
            };
            //Publish Notification Data
            await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.AcceptSubscriptionTransfer, CreateNotificationData, targetUsersId.ToArray());

            return true;
        }

        [AbpAuthorize]
        public async Task<bool> SendRefuseNotification(UpdateCompanyWalletTransactionDto input)
        {

            var company = await _companyRepository.FirstOrDefaultAsync(input.CompanyId);
            if (company == null)
                return false;

            // send notification to provider here 

            List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
            string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");

            targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: company.UserId.Value));

            CreateNotificationDto CreateNotificationData = new CreateNotificationDto
            {
                SenderUserId = AbpSession.UserId.Value,
                Message = L("Pages.Subscriptions.Messages.Refused"),
                EntityType = Entity_Type.RefuseSubscriptionTransfer,
                EntityId = input.Id,
                CompanyId = company.Id,
            };
            //Publish Notification Data
            await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.RefuseSubscriptionTransfer, CreateNotificationData, targetUsersId.ToArray());

            return true;
        }

        public async Task<bool> SendExpireNotification(UpdateCompanyWalletTransactionDto input)
        {

            var company = await _companyRepository.FirstOrDefaultAsync(input.CompanyId);
            if (company == null)
                return false;

            // send notification to provider here 

            List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
            string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");

            targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: company.UserId.Value));

            CreateNotificationDto CreateNotificationData = new CreateNotificationDto
            {

                Message = "Pages.Subscriptions.Messages.Expired",
                EntityType = Entity_Type.ExpireSubscription,
                EntityId = input.Id,
                CompanyId = company.Id,
            };
            //Publish Notification Data
            await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.ExpireSubscription, CreateNotificationData, targetUsersId.ToArray());

            return true;
        }

    }
}
