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
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Wallets.Dto;
using Sayarah.Authorization;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Veichles;
using Sayarah.Wallets;
using System.Globalization;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.Wallets
{
    public class BranchWalletTransactionAppService : AsyncCrudAppService<BranchWalletTransaction, BranchWalletTransactionDto, long, GetAllBranchWalletTransactions, CreateBranchWalletTransactionDto, UpdateBranchWalletTransactionDto>, IBranchWalletTransactionAppService
    {

        private readonly IRepository<Branch, long> _branchRepository;
        private readonly IRepository<Company, long> _companyRepository;
        private readonly IRepository<CompanyWalletTransaction, long> _companyWalletTransactionRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<UserDevice, long> _userDevicesRepository;
        //private readonly ISendingMailsAppService _sendingMailsAppService;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly IRepository<Veichle, long> _veichleRepository;
        private readonly IRepository<User, long> _userRepository;
        public ICommonAppService _commonAppService { get; set; }
        CultureInfo new_lang = new CultureInfo("ar");

        public BranchWalletTransactionAppService(IRepository<BranchWalletTransaction, long> repository,
            IRepository<Branch, long> branchRepository,
            IRepository<Company, long> companyRepository,
            IRepository<CompanyWalletTransaction, long> companyWalletTransactionRepository,
            IBackgroundJobManager backgroundJobManager,
             ICommonAppService commonAppService,
            IRepository<UserDevice, long> userDevicesRepository,
             //ISendingMailsAppService sendingMailsAppService,
             AbpNotificationHelper abpNotificationHelper,
             IRepository<Veichle, long> veichleRepository,
             IRepository<User, long> userRepository


            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _branchRepository = branchRepository;
            _companyRepository = companyRepository;
            _companyWalletTransactionRepository = companyWalletTransactionRepository;
            _commonAppService = commonAppService;
            _backgroundJobManager = backgroundJobManager;
            _userDevicesRepository = userDevicesRepository;
            // _sendingMailsAppService = sendingMailsAppService;
            _abpNotificationHelper = abpNotificationHelper;
            _veichleRepository = veichleRepository;
            _userRepository = userRepository;
        }
        [AbpAuthorize]
        public async Task<DataTableOutputDto<BranchWalletTransactionDto>> GetPaged(GetAllBranchWalletTransactionsInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    int id = 0;
                    if (input.actionType == "GroupAction")
                    {
                        for (int i = 0; i < input.ids.Length; i++)
                        {
                            id = Convert.ToInt32(input.ids[i]);
                            BranchWalletTransaction branchWalletTransaction = await Repository.FirstOrDefaultAsync(q => q.Id == id);
                            if (branchWalletTransaction != null)
                            {
                                if (input.action == "Delete")
                                {
                                    //await Repository.DeleteAsync(branchWalletTransaction);
                                }
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            id = Convert.ToInt32(input.ids[0]);
                            BranchWalletTransaction branchWalletTransaction = await Repository.FirstOrDefaultAsync(q => q.Id == id);
                            if (branchWalletTransaction != null)
                            {
                                if (input.action == "Delete")
                                {
                                    //await Repository.DeleteAsync(branchWalletTransaction);
                                }
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll().Include(at => at.Branch).AsQueryable();


                    query = query.WhereIf(input.BranchId.HasValue, x => x.BranchId == input.BranchId.Value);
                    query = query.WhereIf(input.CreatorUserId.HasValue, x => x.CreatorUserId == input.CreatorUserId.Value);
                    query = query.WhereIf(input.WalletType.HasValue, x => x.WalletType == input.WalletType);
                    int count = await query.CountAsync();
                    query = query.WhereIf(input.AmountFrom.HasValue, at => at.Amount >= input.AmountFrom);
                    query = query.WhereIf(input.AmountTo.HasValue, at => at.Amount <= input.AmountTo);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Note), at => at.Note.Contains(input.Note));
                    query = query.WhereIf(input.TransactionType.HasValue, at => at.TransactionType == input.TransactionType);
                    query = query.FilterDataTable((DataTableInputDto)input);

                    int filteredCount = await query.CountAsync();
                    var branchWalletTransactions = await query
                        //.OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .OrderByDescending(a => a.CreationTime)
                        .Skip(input.start).Take(input.length).ToListAsync();

                    return new DataTableOutputDto<BranchWalletTransactionDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<BranchWalletTransactionDto>>(branchWalletTransactions)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async override Task<BranchWalletTransactionDto> GetAsync(EntityDto<long> input)
        {
            try
            {
                var branchWalletTransaction = Repository.GetAll().FirstOrDefault(x => x.Id == input.Id);
                return await Task.FromResult(ObjectMapper.Map<BranchWalletTransactionDto>(branchWalletTransaction));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        public async override Task<BranchWalletTransactionDto> CreateAsync(CreateBranchWalletTransactionDto input)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                var branch = await _branchRepository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(x => x.Id == input.BranchId);
                var veichle = _veichleRepository.FirstOrDefault(x => x.Id == input.VeichleId);

                if (input.TransactionType == TransactionType.Refund || input.TransactionType == TransactionType.RefundAdmin || input.TransactionType == TransactionType.Consumption)
                {

                    if (input.WalletType == WalletType.Fuel && branch.FuelAmount < input.Amount)
                    {
                        if (input.TransType.HasValue && input.TransType == TransOutTypes.Fuel)
                            throw new UserFriendlyException(L("Pages.Wallets.BranchAmountNotEnoughForCarAmount"));
                        else
                            throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));
                    }
                    if (input.WalletType == WalletType.Clean && branch.CleanAmount < input.Amount)
                    {
                        throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));
                    }
                    if (input.WalletType == WalletType.Maintain && branch.MaintainAmount < input.Amount)
                    {
                        throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));
                    }
                }

                input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "BranchWalletTransactions", CodeField = "Code" });
                var branchWalletTransaction = ObjectMapper.Map<BranchWalletTransaction>(input);
                await Repository.InsertAsync(branchWalletTransaction);



                if (input.IgnoreCompanyTransaction == false && (input.TransactionType == TransactionType.Deposit || input.TransactionType == TransactionType.DepositAdmin))
                {
                    // insure that the company wallet has enough money for this process 
                    var company = await _companyRepository.FirstOrDefaultAsync(a => a.Id == branch.CompanyId);
                    if (company != null)
                    {
                        if (company.WalletAmount < input.Amount)
                        {
                            throw new UserFriendlyException(L("Pages.Wallets.CompanyBranchDepositFailed"));
                        }
                    }
                    else
                        throw new UserFriendlyException(L("Common.ErrorOccurred"));
                }


                var branchUpdateInput = new UpdateReservedBalanceBranchDto { };
                await CurrentUnitOfWork.SaveChangesAsync();
                if (input.IsTransOperation)
                {
                    branchUpdateInput = new UpdateReservedBalanceBranchDto
                    {
                        Id = branch.Id,
                        Reserved = Math.Min(branch.FuelAmount, input.Reserved),
                        OperationType = input.OperationType,
                        Price = input.Price,
                        IsTransOperation = true
                    };
                }


                if (branchUpdateInput.IsTransOperation == true)
                    await CalculateBranchBalance(branch, branchUpdateInput);
                else
                    await CalculateBranchBalance(branch);



                if (input.TransactionType == TransactionType.Deposit || input.TransactionType == TransactionType.DepositAdmin)
                {

                    if (input.IgnoreCompanyTransaction == false)
                    {

                        // decrease the same amount from company wallet 
                        await ManageCompanyWallet(new CreateCompanyWalletTransactionDto
                        {
                            Amount = input.Amount,
                            CompanyId = branch.CompanyId.Value,
                            TransactionType = TransactionType.Refund,
                            Note = L("Pages.Wallets.BranchDepositNote", currentCulture.Name.Contains("ar") ? branch.NameAr : branch.NameEn)
                        });
                    }
                }
                else if (input.TransactionType == TransactionType.Refund || input.TransactionType == TransactionType.RefundAdmin || input.TransactionType == TransactionType.Consumption)
                {
                    if (input.IgnoreCompanyTransaction == false)
                    {
                        // increase the same amount into company wallet
                        await ManageCompanyWallet(new CreateCompanyWalletTransactionDto
                        {
                            Amount = input.Amount,
                            CompanyId = branch.CompanyId.Value,
                            TransactionType = TransactionType.Deposit,
                            DepositStatus = DepositStatus.Accepted,
                            Note = L("Pages.Wallets.BranchRefundNote", currentCulture.Name.Contains("ar") ? branch.NameAr : branch.NameEn)
                        });
                    }
                }


                if (input.NotifyBranch == true)
                {
                    string messageAr = string.Empty;
                    string messageEn = string.Empty;
                    // send notification to branch and emps 

                    if (input.TransactionType == TransactionType.Deposit || input.TransactionType == TransactionType.DepositAdmin)
                    {
                        messageAr = L("Pages.Notifications.BranchWalletDepositMsg", new CultureInfo("ar"), input.Amount);
                        messageEn = L("Pages.Notifications.BranchWalletDepositMsg", new CultureInfo("en"), input.Amount);
                    }
                    else if (input.TransactionType == TransactionType.Refund || input.TransactionType == TransactionType.RefundAdmin || input.TransactionType == TransactionType.Consumption)
                    {
                        messageAr = L("Pages.Notifications.BranchWalletRefundMsg", new CultureInfo("ar"), input.Amount);
                        messageEn = L("Pages.Notifications.BranchWalletRefundMsg", new CultureInfo("en"), input.Amount);
                    }


                    await NotifyUsers(new NotifyInputDto
                    {
                        BranchId = branch.Id,
                        BranchUserId = branch.UserId,
                        CompanyId = branch.CompanyId,
                        MessageAr = messageAr,
                        MessageEn = messageEn
                    });
                }


                return MapToEntityDto(branchWalletTransaction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        public async Task<BranchWalletTransactionDto> CreateFuelWalletTransaction(CreateBranchWalletTransactionDto input)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                var branch = await _branchRepository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(x => x.Id == input.BranchId);
                var veichle = _veichleRepository.FirstOrDefault(x => x.Id == input.VeichleId);

                input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "BranchWalletTransactions", CodeField = "Code" });
                var branchWalletTransaction = ObjectMapper.Map<BranchWalletTransaction>(input);
                await Repository.InsertAsync(branchWalletTransaction);

                var branchUpdateInput = new UpdateReservedBalanceBranchDto { };
                await CurrentUnitOfWork.SaveChangesAsync();
                if (input.IsTransOperation)
                {
                    branchUpdateInput = new UpdateReservedBalanceBranchDto
                    {
                        Id = branch.Id,
                        Reserved = Math.Min(branch.FuelAmount, input.Reserved),
                        OperationType = input.OperationType,
                        Price = input.Price,
                        IsTransOperation = true
                    };
                }

                if (branchUpdateInput.IsTransOperation == true)
                    await CalculateBranchBalance(branch, branchUpdateInput);
                else
                    await CalculateBranchBalance(branch);


                return MapToEntityDto(branchWalletTransaction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }






        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public async Task CalculateBranchBalance(Branch branch, UpdateReservedBalanceBranchDto input = null)
        {
            try
            {
                var _branchWalletTransactions = await Repository.GetAll()
                    .Where(q => q.BranchId == branch.Id).ToListAsync();
                if (input != null && input.IsTransOperation)
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        if (input.OperationType == OperationType.Begin)
                        {
                            branch.Reserved = branch.Reserved + input.Reserved;
                        }
                        else if (input.OperationType == OperationType.End)
                        {
                            branch.Reserved = branch.Reserved - input.Reserved;
                            branch.FuelAmount = branch.FuelAmount - input.Price;
                            branch.WalletAmount = branch.WalletAmount - input.Price;

                        }
                        else
                        {
                            branch.Reserved = branch.Reserved - input.Reserved;
                        }
                        await _branchRepository.UpdateAsync(branch);
                        await CurrentUnitOfWork.SaveChangesAsync();

                    }
                    finally
                    {

                        _semaphore.Release();

                    }

                }
                else
                {
                    decimal total_add = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin).Sum(a => a.Amount);
                    decimal total_sub = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin || q.TransactionType == TransactionType.Consumption).Sum(a => a.Amount);
                    decimal total = total_add - total_sub;

                    decimal fuel_add = _branchWalletTransactions.Where(q => q.WalletType == WalletType.Fuel && (q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin)).Sum(a => a.Amount);
                    decimal fuel_sub = _branchWalletTransactions.Where(q => q.WalletType == WalletType.Fuel && (q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin || q.TransactionType == TransactionType.Consumption)).Sum(a => a.Amount);
                    decimal fuel = fuel_add - fuel_sub;

                    decimal clean_add = _branchWalletTransactions.Where(q => q.WalletType == WalletType.Clean && (q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin)).Sum(a => a.Amount);
                    decimal clean_sub = _branchWalletTransactions.Where(q => q.WalletType == WalletType.Clean && (q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin || q.TransactionType == TransactionType.Consumption)).Sum(a => a.Amount);
                    decimal clean = clean_add - clean_sub;

                    decimal maintain_add = _branchWalletTransactions.Where(q => q.WalletType == WalletType.Maintain && (q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin)).Sum(a => a.Amount);
                    decimal maintain_sub = _branchWalletTransactions.Where(q => q.WalletType == WalletType.Maintain && (q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin || q.TransactionType == TransactionType.Consumption)).Sum(a => a.Amount);
                    decimal maintain = maintain_add - maintain_sub;

                    decimal consumptionAmount = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.Consumption).Sum(a => a.Amount);

                    //var branch = await _branchRepository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(x => x.Id == input.BranchId);

                    branch.WalletAmount = total;

                    branch.FuelAmount = fuel;
                    branch.CleanAmount = clean;
                    branch.MaintainAmount = maintain;

                    branch.ConsumptionAmount = consumptionAmount;

                    await _branchRepository.UpdateAsync(branch);
                    await CurrentUnitOfWork.SaveChangesAsync();

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CalculateBranchBalanceById(EntityDto<long> input)
        {
            try
            {
                var branch = await _branchRepository.FirstOrDefaultAsync(a => a.Id == input.Id);
                await CalculateBranchBalance(branch);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> RecalculateBranchAmounts()
        {
            try
            {
                var branches = await _branchRepository.GetAll().Include(x => x.User).Where(a => a.IsDeleted == false).ToListAsync();

                foreach (var branch in branches)
                {
                    await CalculateBranchBalance(branch);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<CompanyWalletTransactionDto> ManageCompanyWallet(CreateCompanyWalletTransactionDto input)
        {
            try
            {
                var company = await _companyRepository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(x => x.Id == input.CompanyId);

                if ((input.TransactionType == TransactionType.Refund || input.TransactionType == TransactionType.RefundAdmin || input.TransactionType == TransactionType.Consumption) && company.WalletAmount < input.Amount)
                    throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));



                input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "CompanyWalletTransactions", CodeField = "Code" });
                var companyWalletTransaction = ObjectMapper.Map<CompanyWalletTransaction>(input);
                await _companyWalletTransactionRepository.InsertAsync(companyWalletTransaction);
                await CurrentUnitOfWork.SaveChangesAsync();

                // companyWalletTransactions 
                var _companyWalletTransactions = await _companyWalletTransactionRepository.GetAll().Where(q => q.CompanyId == input.CompanyId).ToListAsync();

                #region Calculate total amount
                decimal add = _companyWalletTransactions.Where(q => (q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin) && q.DepositStatus == DepositStatus.Accepted).Sum(a => a.Amount);
                decimal sub = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin || q.TransactionType == TransactionType.Consumption).Sum(a => a.Amount);

                decimal total = add - sub;
                decimal _totalAmount = total;

                #endregion


                company.WalletAmount = _totalAmount;

                await _companyRepository.UpdateAsync(company);
                await CurrentUnitOfWork.SaveChangesAsync();
                return ObjectMapper.Map<CompanyWalletTransactionDto>(companyWalletTransaction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }







        public async Task<bool> ManageTransferWallet(ManageTransferWalletInput input)
        {
            try
            {

                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

                // get branch first 
                var branch = await _branchRepository.FirstOrDefaultAsync(a => a.Id == input.BranchId);
                if (branch == null)
                    throw new UserFriendlyException(L("Common.ErrorOccurred"));


                if (input.WalletType == WalletType.Fuel && branch.FuelAmount < input.Amount)
                {
                    throw new UserFriendlyException(L("Pages.Wallets.TransferFailed"));
                }

                if (input.WalletType == WalletType.Clean && branch.CleanAmount < input.Amount)
                {
                    throw new UserFriendlyException(L("Pages.Wallets.TransferFailed"));
                }

                if (input.WalletType == WalletType.Maintain && branch.MaintainAmount < input.Amount)
                {
                    throw new UserFriendlyException(L("Pages.Wallets.TransferFailed"));
                }


                // get branch first 
                var targetBranch = await _branchRepository.FirstOrDefaultAsync(a => a.Id == input.TragetBranchId);
                if (targetBranch == null)
                    throw new UserFriendlyException(L("Common.ErrorOccurred"));

                // make the transfer :D

                // increase target branch wallet amount

                await CreateAsync(new CreateBranchWalletTransactionDto
                {
                    BranchId = input.TragetBranchId,
                    Amount = input.Amount,
                    TransactionType = TransactionType.Deposit,
                    Note = L("Pages.Wallets.TransferDepositWalletAmount", currentCulture.Name.Contains("ar") ? branch.NameAr : branch.NameEn),
                    WalletType = input.WalletType,
                    IgnoreCompanyTransaction = true
                });

                // decrease current branch wallet amount

                await CreateAsync(new CreateBranchWalletTransactionDto
                {
                    BranchId = input.BranchId,
                    Amount = input.Amount,
                    TransactionType = TransactionType.Refund,
                    WalletType = input.WalletType,
                    Note = L("Pages.Wallets.TransferRefundWalletAmount", currentCulture.Name.Contains("ar") ? targetBranch.NameAr : targetBranch.NameEn),
                    IgnoreCompanyTransaction = true
                });

                // here create send notification to target branch


                string messageAr = string.Empty;
                string messageEn = string.Empty;
                // send notification to branch and emps 


                messageAr = L("Pages.Notifications.BranchWalletTransferDepositMsg", new CultureInfo("ar"), input.Amount, targetBranch.NameAr);
                messageEn = L("Pages.Notifications.BranchWalletTransferDepositMsg", new CultureInfo("en"), input.Amount, targetBranch.NameEn);


                await NotifyUsers(new NotifyInputDto
                {
                    BranchId = targetBranch.Id,
                    BranchUserId = targetBranch.UserId,
                    CompanyId = targetBranch.CompanyId,
                    MessageAr = messageAr,
                    MessageEn = messageEn
                });

                if (input.NotifyCurrentBranch == true)
                {
                    messageAr = L("Pages.Notifications.BranchWalletTransferRefundMsg", new CultureInfo("ar"), branch.NameAr, input.Amount);
                    messageEn = L("Pages.Notifications.BranchWalletTransferRefundMsg", new CultureInfo("en"), branch.NameEn, input.Amount);

                    await NotifyUsers(new NotifyInputDto
                    {
                        BranchId = branch.Id,
                        BranchUserId = branch.UserId,
                        CompanyId = branch.CompanyId,
                        MessageAr = messageAr,
                        MessageEn = messageEn
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //public async Task<CheckBranchWalletOutput> CheckBranchWallet(CheckBranchWalletInput input)
        //{
        //    try
        //    {

        //        // get branch first 
        //        var branch = await _branchRepository.FirstOrDefaultAsync(a => a.Id == input.BranchId);
        //        if (branch == null)
        //            throw new UserFriendlyException(L("Common.ErrorOccurred"));

        //        if (input.WalletType == WalletType.Fuel && input.Amount > branch.FuelAmount)
        //        {
        //            throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));
        //        }
        //        else if (input.WalletType == WalletType.Clean && input.Amount > branch.CleanAmount)
        //        {
        //            throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));
        //        }
        //        else if (input.WalletType == WalletType.Maintain && input.Amount > branch.MaintainAmount)
        //        {
        //            throw new UserFriendlyException(L("Pages.Wallets.BranchWalletAmountNotEnough"));
        //        }

        //        return new CheckBranchWalletOutput { Success = true };

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        public async Task<bool> NotifyUsers(NotifyInputDto input)
        {
            try
            {

                // get veichle 
                var branch = await _branchRepository.GetAll()
                    .Include(a => a.Company)
                    .FirstOrDefaultAsync(a => a.Id == input.BranchId);


                #region To Branch
                var notiProperties = new Dictionary<string, object>();
                List<UserIdentifier> targetUsersId = new List<UserIdentifier>();

                if (branch != null && branch.Company != null)
                {

                    // dictionary
                    notiProperties.Add("MessageAr", input.MessageAr);
                    notiProperties.Add("MessageEn", input.MessageEn);


                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.BranchUserId.Value));

                    // get branch employees here first 
                    // all employees that has permission   in this branch
                    var employees = await _userRepository.GetAll().Include(a => a.Permissions)
                        .Where(a => a.IsDeleted == false && a.IsActive == true && a.UserType == UserTypes.Employee && a.BranchId == input.BranchId && a.Permissions.Any(aa => aa.Name == PermissionNames.BranchData.BranchWallets.Read && aa.IsGranted == true)).ToListAsync();

                    if (employees != null)
                    {
                        foreach (var employee in employees)
                        {
                            targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: employee.Id));
                        }
                    }


                    CreateNotificationDto _createUserNotificationData = new CreateNotificationDto
                    {
                        //SenderUserName = (await _userManager.GetUserByIdAsync(AbpSession.UserId.Value)).Name,
                        Message = input.MessageAr,
                        EntityType = Entity_Type.Wallet,
                        EntityId = input.EntityId,
                        BranchId = input.BranchId,
                        DriverId = input.DriverId,
                        CompanyId = input.CompanyId,
                        MainProviderId = input.MainProviderId,
                        ProviderId = input.ProviderId,
                        WorkerId = input.WorkerId,
                        Properties = notiProperties
                    };
                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.Wallet, _createUserNotificationData, targetUsersId.ToArray());

                }


                #endregion





                return true;
            }
            catch (Exception ex)
            {

                throw new UserFriendlyException(ex.Message);
            }
        }



        public async override Task<BranchWalletTransactionDto> UpdateAsync(UpdateBranchWalletTransactionDto input)
        {
            try
            {


                var branchWalletTransaction = await Repository.GetAsync(input.Id);
                branchWalletTransaction.Amount = branchWalletTransaction.FalseAmount;
                branchWalletTransaction.PaymentDate = input.PaymentDate;
                branchWalletTransaction.PayMethod = input.PayMethod;
                branchWalletTransaction.TrackId = input.TrackId;
                branchWalletTransaction.TransactionId = input.TransactionId;
                branchWalletTransaction.Note = "Paid by Payment Api with Id : " + input.TransactionId;
                await Repository.UpdateAsync(branchWalletTransaction);
                await CurrentUnitOfWork.SaveChangesAsync();

                // branchWalletTransactions 
                var _branchWalletTransactions = await Repository.GetAll().Where(q => q.BranchId == branchWalletTransaction.BranchId).ToListAsync();

                //#region Calculate total amount
                //decimal add = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin).Sum(a => a.Amount);
                //decimal sub = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin).Sum(a => a.Amount);

                //decimal _add = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.InsuranceRefund).Sum(a => a.Amount);
                //decimal _sub = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.AdvertisingDiscount || q.TransactionType == TransactionType.AuctionInsuranceDiscount).Sum(a => a.Amount);

                //decimal total = add - sub;
                //decimal _total = _add - _sub;
                //decimal _totalAmount = total + _total;
                //#endregion

                //#region Calculate advertisements fees
                //decimal _advertisementsFees = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.AdvertisingDiscount).Sum(a => a.Amount);

                //#endregion

                //#region Calculate advertisements fees
                //decimal _addBidsAmount = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.InsuranceRefund).Sum(a => a.Amount);
                //decimal _subBidsAmount = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.AuctionInsuranceDiscount).Sum(a => a.Amount);
                //decimal _bidsAmount = _addBidsAmount - _subBidsAmount;
                //#endregion

                var branch = await _branchRepository.FirstOrDefaultAsync(x => x.Id == branchWalletTransaction.BranchId);

                //if ((branchWalletTransaction.TransactionType == TransactionType.Refund || branchWalletTransaction.TransactionType == TransactionType.RefundAdmin || branchWalletTransaction.TransactionType == TransactionType.AdvertisingDiscount || branchWalletTransaction.TransactionType == TransactionType.AuctionInsuranceDiscount) && (branch.WalletAmount < branchWalletTransaction.Amount))
                //    throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));

                branch.WalletAmount = branch.WalletAmount + branchWalletTransaction.Amount;
                //branch.WalletAmount = _totalAmount;
                //branch.AdvertisementsFees = _advertisementsFees;
                //branch.BidsAmount = Math.Abs(_bidsAmount);
                await _branchRepository.UpdateAsync(branch);
                await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(branchWalletTransaction);
            }
            catch (Exception)
            {
                throw;
            }
        }






        [AbpAuthorize]
        public override async Task<PagedResultDto<BranchWalletTransactionDto>> GetAllAsync(GetAllBranchWalletTransactions input)
        {
            try
            {
                var query = Repository.GetAll().Where(x => x.Branch.UserId == AbpSession.UserId.Value);
                int Count = query.Count();
                if (input.MaxCount)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = Count;
                }
                var branchWalletTransaction = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return new PagedResultDto<BranchWalletTransactionDto>(Count, ObjectMapper.Map<List<BranchWalletTransactionDto>>(branchWalletTransaction));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<bool> DeleteTransaction(EntityDto<long> input)
        {
            try
            {

                return false;

                var branchWalletTransaction = Repository.FirstOrDefault(x => x.Id == input.Id);

                // calculate total then check
                if (branchWalletTransaction != null)
                {
                    // branchWalletTransactions 
                    var _branchWalletTransactions = await Repository.GetAll().Where(q => q.Branch.UserId == AbpSession.UserId.Value).ToListAsync();
                    decimal add = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin).Sum(a => a.Amount);
                    decimal sub = _branchWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin || q.TransactionType == TransactionType.Consumption).Sum(a => a.Amount);

                    var _totalAmount = add - sub;

                    if ((branchWalletTransaction.TransactionType == TransactionType.Deposit || branchWalletTransaction.TransactionType == TransactionType.DepositAdmin) && branchWalletTransaction.Amount > _totalAmount)
                        throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));

                    await Repository.DeleteAsync(branchWalletTransaction);
                    await UnitOfWorkManager.Current.SaveChangesAsync();
                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}