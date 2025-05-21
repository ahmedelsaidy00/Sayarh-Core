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
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Wallets.Dto;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Wallets;
using System.Globalization;
using static Sayarah.SayarahConsts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Sayarah.Application.Helpers.Dto;

namespace Sayarah.Application.Wallets
{
    public class CompanyWalletTransactionAppService : AsyncCrudAppService<CompanyWalletTransaction, CompanyWalletTransactionDto, long, GetAllCompanyWalletTransactions, CreateCompanyWalletTransactionDto, UpdateCompanyWalletTransactionDto>, ICompanyWalletTransactionAppService
    {

        private readonly IRepository<Company, long> _companyRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<UserDevice, long> _userDevicesRepository;
        //private readonly ISendingMailsAppService _sendingMailsAppService;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly IRepository<User, long> _userRepository;
        public ICommonAppService _commonAppService { get; set; }
        CultureInfo new_lang = new CultureInfo("ar");

        public CompanyWalletTransactionAppService(IRepository<CompanyWalletTransaction, long> repository,
            IRepository<Company, long> companyRepository, IBackgroundJobManager backgroundJobManager,

            IRepository<UserDevice, long> userDevicesRepository,
             //ISendingMailsAppService sendingMailsAppService,
             AbpNotificationHelper abpNotificationHelper,
              ICommonAppService commonAppService,
             IRepository<User, long> userRepository
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _companyRepository = companyRepository;
            _commonAppService = commonAppService;
            _backgroundJobManager = backgroundJobManager;
            _userDevicesRepository = userDevicesRepository;
            // _sendingMailsAppService = sendingMailsAppService;
            _abpNotificationHelper = abpNotificationHelper;
            _userRepository = userRepository;
        }
        [AbpAuthorize]
        public async Task<DataTableOutputDto<CompanyWalletTransactionDto>> GetPaged(GetAllCompanyWalletTransactionsInput input)
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
                            CompanyWalletTransaction companyWalletTransaction = await Repository.FirstOrDefaultAsync(q => q.Id == id);
                            if (companyWalletTransaction != null)
                            {
                                if (input.action == "Delete")
                                {
                                    // await Repository.DeleteAsync(companyWalletTransaction);
                                }

                                else if (input.action == "Accept")
                                {
                                    companyWalletTransaction.DepositStatus = DepositStatus.Accepted;

                                    // get provider and update price then send notification 
                                    await UpdateTransactionRequest(new CreateCompanyWalletTransactionDto
                                    {
                                        CompanyId = companyWalletTransaction.CompanyId,
                                    });


                                    await SendAcceptNotification(new UpdateCompanyWalletTransactionDto
                                    {
                                        Id = companyWalletTransaction.Id,
                                        CompanyId = companyWalletTransaction.CompanyId
                                    });
                                }

                                //else if (input.action == "Refuse")
                                //{
                                //    companyWalletTransaction.DepositStatus = DepositStatus.Refused;

                                //    // send refuse notification 

                                //    await SendRefuseNotification(new UpdateCompanyWalletTransactionDto
                                //    {
                                //        Id = companyWalletTransaction.Id,
                                //        CompanyId = companyWalletTransaction.CompanyId
                                //    });
                                //}

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    else if (input.actionType == "SingleAction")
                    {
                        if (input.ids.Length > 0)
                        {
                            id = Convert.ToInt32(input.ids[0]);
                            CompanyWalletTransaction companyWalletTransaction = await Repository.FirstOrDefaultAsync(q => q.Id == id);
                            if (companyWalletTransaction != null)
                            {
                                if (input.action == "Delete")
                                {
                                    //  await Repository.DeleteAsync(companyWalletTransaction);
                                }

                                else if (input.action == "Accept")
                                {
                                    companyWalletTransaction.DepositStatus = DepositStatus.Accepted;

                                    // get provider and update price then send notification 
                                    await UpdateTransactionRequest(new CreateCompanyWalletTransactionDto
                                    {
                                        CompanyId = companyWalletTransaction.CompanyId,
                                    });


                                    await SendAcceptNotification(new UpdateCompanyWalletTransactionDto
                                    {
                                        Id = companyWalletTransaction.Id,
                                        CompanyId = companyWalletTransaction.CompanyId
                                    });
                                }
                                //else if (input.action == "Refuse")
                                //{
                                //    companyWalletTransaction.DepositStatus = DepositStatus.Refused;

                                //    // send refuse notification 

                                //    await SendRefuseNotification(new UpdateCompanyWalletTransactionDto
                                //    {
                                //        Id = companyWalletTransaction.Id,
                                //        CompanyId = companyWalletTransaction.CompanyId
                                //    });
                                //}
                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll().Include(at => at.Company).AsQueryable();


                    query = query.WhereIf(input.CompanyId.HasValue, x => x.CompanyId == input.CompanyId.Value);
                    query = query.WhereIf(input.CreatorUserId.HasValue, x => x.CreatorUserId == input.CreatorUserId.Value);
                    int count = await query.CountAsync();
                    query = query.WhereIf(input.AmountFrom.HasValue, at => at.Amount >= input.AmountFrom);
                    query = query.WhereIf(input.AmountTo.HasValue, at => at.Amount <= input.AmountTo);
                    query = query.WhereIf(input.Amount.HasValue, at => at.Amount == input.Amount);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Note), at => at.Note.Contains(input.Note));
                    query = query.WhereIf(input.TransactionType.HasValue, at => at.TransactionType == input.TransactionType);
                    query = query.WhereIf(input.DepositStatus.HasValue, at => at.DepositStatus == input.DepositStatus);
                    query = query.FilterDataTable((DataTableInputDto)input);

                    int filteredCount = await query.CountAsync();
                    var companyWalletTransactions = await query
                        //.OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .OrderByDescending(a => a.CreationTime)
                        .Skip(input.start).Take(input.length).ToListAsync();

                    return new DataTableOutputDto<CompanyWalletTransactionDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<CompanyWalletTransactionDto>>(companyWalletTransactions)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async override Task<CompanyWalletTransactionDto> GetAsync(EntityDto<long> input)
        {
            try
            {
                var companyWalletTransaction = Repository.GetAll()
                    .Include(a => a.Bank)
                    .FirstOrDefault(x => x.Id == input.Id);
                return await Task.FromResult(ObjectMapper.Map<CompanyWalletTransactionDto>(companyWalletTransaction));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [AbpAuthorize]
        public async Task<CompanyWalletTransactionDto> GetTransaction(GetAllCompanyWalletTransactionsInput input)
        {
            try
            {
                var companyWalletTransaction = Repository.GetAll()
                    .Include(a => a.Bank)
                    .FirstOrDefault(x => x.Id == input.Id && x.CompanyId == input.CompanyId);

                return await Task.FromResult(ObjectMapper.Map<CompanyWalletTransactionDto>(companyWalletTransaction));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async override Task<CompanyWalletTransactionDto> CreateAsync(CreateCompanyWalletTransactionDto input)
        {
            try
            {

                var company = await _companyRepository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(x => x.Id == input.CompanyId);

                if ((input.TransactionType == TransactionType.Refund || input.TransactionType == TransactionType.RefundAdmin) && company.WalletAmount < input.Amount)
                    throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));


                input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "CompanyWalletTransactions", CodeField = "Code" });
                var companyWalletTransaction = ObjectMapper.Map<CompanyWalletTransaction>(input);
                await Repository.InsertAsync(companyWalletTransaction);
                await CurrentUnitOfWork.SaveChangesAsync();

                // companyWalletTransactions 
                var _companyWalletTransactions = await Repository.GetAll().Where(q => q.CompanyId == input.CompanyId).ToListAsync();

                #region Calculate total amount
                decimal add = _companyWalletTransactions
                    .Where(q => (q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin) && q.DepositStatus == DepositStatus.Accepted).Sum(a => a.Amount);

                decimal sub = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin).Sum(a => a.Amount);


                decimal total = add - sub;

                decimal _totalAmount = total;

                #endregion





                company.WalletAmount = _totalAmount;

                await _companyRepository.UpdateAsync(company);
                await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(companyWalletTransaction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async override Task<CompanyWalletTransactionDto> UpdateAsync(UpdateCompanyWalletTransactionDto input)
        {
            try
            {


                var companyWalletTransaction = await Repository.GetAsync(input.Id);
                companyWalletTransaction.Amount = companyWalletTransaction.FalseAmount;
                companyWalletTransaction.PaymentDate = input.PaymentDate;
                companyWalletTransaction.PayMethod = input.PayMethod;
                companyWalletTransaction.TrackId = input.TrackId;
                companyWalletTransaction.TransactionId = input.TransactionId;
                companyWalletTransaction.Note = "Paid by Payment Api with Id : " + input.TransactionId;
                await Repository.UpdateAsync(companyWalletTransaction);
                await CurrentUnitOfWork.SaveChangesAsync();

                // companyWalletTransactions 
                var _companyWalletTransactions = await Repository.GetAll().Where(q => q.CompanyId == companyWalletTransaction.CompanyId).ToListAsync();

                //#region Calculate total amount
                //decimal add = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin).Sum(a => a.Amount);
                //decimal sub = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin).Sum(a => a.Amount);

                //decimal _add = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.InsuranceRefund).Sum(a => a.Amount);
                //decimal _sub = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.AdvertisingDiscount || q.TransactionType == TransactionType.AuctionInsuranceDiscount).Sum(a => a.Amount);

                //decimal total = add - sub;
                //decimal _total = _add - _sub;
                //decimal _totalAmount = total + _total;
                //#endregion

                //#region Calculate advertisements fees
                //decimal _advertisementsFees = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.AdvertisingDiscount).Sum(a => a.Amount);

                //#endregion

                //#region Calculate advertisements fees
                //decimal _addBidsAmount = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.InsuranceRefund).Sum(a => a.Amount);
                //decimal _subBidsAmount = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.AuctionInsuranceDiscount).Sum(a => a.Amount);
                //decimal _bidsAmount = _addBidsAmount - _subBidsAmount;
                //#endregion

                var company = await _companyRepository.FirstOrDefaultAsync(x => x.Id == companyWalletTransaction.CompanyId);

                //if ((companyWalletTransaction.TransactionType == TransactionType.Refund || companyWalletTransaction.TransactionType == TransactionType.RefundAdmin || companyWalletTransaction.TransactionType == TransactionType.AdvertisingDiscount || companyWalletTransaction.TransactionType == TransactionType.AuctionInsuranceDiscount) && (company.WalletAmount < companyWalletTransaction.Amount))
                //    throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));

                company.WalletAmount = company.WalletAmount + companyWalletTransaction.Amount;
                //company.WalletAmount = _totalAmount;
                //company.AdvertisementsFees = _advertisementsFees;
                //company.BidsAmount = Math.Abs(_bidsAmount);
                await _companyRepository.UpdateAsync(company);
                await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(companyWalletTransaction);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [AbpAuthorize]
        public override async Task<PagedResultDto<CompanyWalletTransactionDto>> GetAllAsync(GetAllCompanyWalletTransactions input)
        {
            try
            {
                var query = Repository.GetAll().Where(x => x.Company.UserId == AbpSession.UserId.Value);
                int Count = query.Count();
                if (input.MaxCount)
                {
                    input.SkipCount = 0;
                    input.MaxResultCount = Count;
                }
                var companyWalletTransaction = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return new PagedResultDto<CompanyWalletTransactionDto>(Count, ObjectMapper.Map<List<CompanyWalletTransactionDto>>(companyWalletTransaction));
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
                var companyWalletTransaction = Repository.FirstOrDefault(x => x.Id == input.Id);

                // calculate total then check
                if (companyWalletTransaction != null)
                {
                    // companyWalletTransactions 
                    var _companyWalletTransactions = await Repository.GetAll().Where(q => q.Company.UserId == AbpSession.UserId.Value).ToListAsync();
                    decimal add = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin).Sum(a => a.Amount);
                    decimal sub = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin).Sum(a => a.Amount);

                    var _totalAmount = add - sub;

                    if ((companyWalletTransaction.TransactionType == TransactionType.Deposit || companyWalletTransaction.TransactionType == TransactionType.DepositAdmin) && companyWalletTransaction.Amount > _totalAmount)
                        throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));

                    await Repository.DeleteAsync(companyWalletTransaction);
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

        public async Task<CompanyWalletTransactionDto> SendTransactionRequest(CreateCompanyWalletTransactionDto input)
        {
            try
            {
                var company = await _companyRepository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(x => x.Id == input.CompanyId);

                if ((input.TransactionType == TransactionType.Refund || input.TransactionType == TransactionType.RefundAdmin) && company.WalletAmount < input.Amount)
                    throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));


                input.DepositStatus = DepositStatus.Pending;
                input.TransactionType = TransactionType.Deposit;
                input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "CompanyWalletTransactions", CodeField = "Code" });
                var companyWalletTransaction = ObjectMapper.Map<CompanyWalletTransaction>(input);
                await Repository.InsertAsync(companyWalletTransaction);
                await CurrentUnitOfWork.SaveChangesAsync();

                // companyWalletTransactions 
                var _companyWalletTransactions = await Repository.GetAll().Where(q => q.CompanyId == input.CompanyId).ToListAsync();

                #region Calculate total amount

                decimal add = _companyWalletTransactions.Where(q => (q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin) && q.DepositStatus == DepositStatus.Accepted).Sum(a => a.Amount);

                decimal sub = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin).Sum(a => a.Amount);


                decimal total = add - sub;

                decimal _totalAmount = total;

                #endregion




                company.WalletAmount = _totalAmount;

                await _companyRepository.UpdateAsync(company);
                await CurrentUnitOfWork.SaveChangesAsync();


                await SendNotificationToEmployees(new UpdateCompanyWalletTransactionDto
                {
                    Id = companyWalletTransaction.Id,
                    CompanyId = companyWalletTransaction.CompanyId
                });

                return MapToEntityDto(companyWalletTransaction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<bool> SendNotificationToEmployees(UpdateCompanyWalletTransactionDto input)
        {
            try
            {

                var company = await _companyRepository.FirstOrDefaultAsync(input.CompanyId);
                if (company == null)
                    return false;


                List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
                string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");


                // send notification  
                var users = await _userRepository.GetAll().Include(a => a.Permissions)
                    .Where(x => x.UserType == UserTypes.Admin).ToListAsync();

                if (users != null && users.Count > 0)
                {
                    //var currentUser = await _userRepository.FirstOrDefaultAsync(AbpSession.UserId.Value);
                    foreach (var item in users)
                    {
                        List<UserIdentifier> targetAdminUsersId = new List<UserIdentifier>();
                        targetAdminUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: item.Id));
                        CreateNotificationDto _createNotificationData = new CreateNotificationDto
                        {
                            SenderUserName = company.NameAr,
                            EntityType = Entity_Type.NewWalletTransfer,
                            EntityId = input.Id,
                            CompanyId = input.CompanyId,
                            Message = L("Pages.Wallets.Messages.New")
                        };
                        //Publish Notification Data
                        await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewWalletTransfer, _createNotificationData, targetAdminUsersId.ToArray());

                    }
                }


                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateTransactionRequest(CreateCompanyWalletTransactionDto input)
        {
            try
            {
                // companyWalletTransactions 
                var _companyWalletTransactions = await Repository.GetAll().Where(q => q.CompanyId == input.CompanyId).ToListAsync();

                #region Calculate total amount

                decimal add = _companyWalletTransactions.Where(q => (q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin) && q.DepositStatus == DepositStatus.Accepted).Sum(a => a.Amount);
                decimal sub = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin).Sum(a => a.Amount);


                decimal total = add - sub;

                decimal _totalAmount = total;

                #endregion

                var company = await _companyRepository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(x => x.Id == input.CompanyId);

                company.WalletAmount = _totalAmount;

                await _companyRepository.UpdateAsync(company);
                await CurrentUnitOfWork.SaveChangesAsync();

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
                Message = L("Pages.Wallets.Messages.Approved"),
                EntityType = Entity_Type.AcceptWalletTransfer,
                EntityId = input.Id,
                CompanyId = company.Id,
            };
            //Publish Notification Data
            await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.AcceptWalletTransfer, CreateNotificationData, targetUsersId.ToArray());

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
                Message = L("Pages.Wallets.Messages.Refused"),
                EntityType = Entity_Type.RefuseWalletTransfer,
                EntityId = input.Id,
                CompanyId = company.Id,
            };
            //Publish Notification Data
            await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.RefuseWalletTransfer, CreateNotificationData, targetUsersId.ToArray());

            return true;
        }

        [AbpAuthorize]
        public async Task<CompanyWalletTransactionDto> RefuseCompanyWalletTransaction(UpdateCompanyWalletTransactionDto input)
        {
            try
            {

                var companyWalletTransaction = await Repository.GetAsync(input.Id);
                companyWalletTransaction.DepositStatus = DepositStatus.Refused;
                companyWalletTransaction.Note = input.Note;

                await Repository.UpdateAsync(companyWalletTransaction);
                await UnitOfWorkManager.Current.SaveChangesAsync();


                // send notification to trainer 

                await SendRefuseNotification(new UpdateCompanyWalletTransactionDto
                {
                    Id = companyWalletTransaction.Id,
                    CompanyId = companyWalletTransaction.CompanyId
                });


                return MapToEntityDto(companyWalletTransaction);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task CalculateCompanyBalance(Company company)
        {
            try
            {
                var _companyWalletTransactions = await Repository.GetAll()
                    .Where(q => q.CompanyId == company.Id).ToListAsync();

                decimal add = _companyWalletTransactions.Where(q => (q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin) && q.DepositStatus == DepositStatus.Accepted).Sum(a => a.Amount);
                decimal sub = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin).Sum(a => a.Amount);
                decimal total = add - sub;
                company.WalletAmount = total;
                await _companyRepository.UpdateAsync(company);
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> RecalculateCompanyAmounts()
        {
            try
            {
                var companies = await _companyRepository.GetAll().Where(a => a.IsDeleted == false).ToListAsync();

                foreach (var company in companies)
                {
                    await CalculateCompanyBalance(company);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<CompanyWalletTransactionDto> CreateFromAdmin(CreateFromAdminDto input)
        {
            try
            {
                input.TransactionType = TransactionType.DepositAdmin;
                var company = await _companyRepository.GetAllIncluding(a => a.User).FirstOrDefaultAsync(x => x.Id == input.CompanyId);

                if ((input.TransactionType == TransactionType.Refund || input.TransactionType == TransactionType.RefundAdmin) && company.WalletAmount < input.Amount)
                    throw new UserFriendlyException(L("Pages.Wallets.RefundFailed"));


                input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "CompanyWalletTransactions", CodeField = "Code" });
                var companyWalletTransaction = ObjectMapper.Map<CompanyWalletTransaction>(input);
                companyWalletTransaction.DepositStatus = DepositStatus.Accepted;

                await Repository.InsertAsync(companyWalletTransaction);
                await CurrentUnitOfWork.SaveChangesAsync();

                // companyWalletTransactions 
                var _companyWalletTransactions = await Repository.GetAll().Where(q => q.CompanyId == input.CompanyId).ToListAsync();

                #region Calculate total amount
                decimal add = _companyWalletTransactions
                    .Where(q => (q.TransactionType == TransactionType.Deposit || q.TransactionType == TransactionType.DepositAdmin) && q.DepositStatus == DepositStatus.Accepted).Sum(a => a.Amount);

                decimal sub = _companyWalletTransactions.Where(q => q.TransactionType == TransactionType.Refund || q.TransactionType == TransactionType.RefundAdmin).Sum(a => a.Amount);


                decimal total = add - sub;

                decimal _totalAmount = total;

                #endregion

                company.WalletAmount = _totalAmount;

                await _companyRepository.UpdateAsync(company);
                await CurrentUnitOfWork.SaveChangesAsync();
                return MapToEntityDto(companyWalletTransaction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<CompanyWalletTransactionDto> GetFullTransactionData(GetAllCompanyWalletTransactionsInput input)
        {
            try
            {
                var companyWalletTransaction = Repository.GetAll()
                    .Include(a => a.Bank)
                    .Include(a => a.Company.User)
                    .FirstOrDefault(x => x.Id == input.Id && x.CompanyId == input.CompanyId);
                return await Task.FromResult(ObjectMapper.Map<CompanyWalletTransactionDto>(companyWalletTransaction));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }
}