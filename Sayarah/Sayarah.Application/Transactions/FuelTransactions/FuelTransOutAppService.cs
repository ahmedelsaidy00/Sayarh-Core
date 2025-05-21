using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Companies;
using Sayarah.Application.Companies.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.BackgroundJobs;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Helpers.StoredProcedures;
using Sayarah.Application.Helpers.StoredProcedures.Dto;
using Sayarah.Application.Transactions.FuelTransactions.Dto;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Application.Wallets;
using Sayarah.Authorization;
using Sayarah.Authorization.Users;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Drivers;
using Sayarah.Providers;
using Sayarah.Transactions;
using Sayarah.Veichles;
using System.Globalization;
using static Sayarah.SayarahConsts;
using System.Linq.Dynamic.Core;


namespace Sayarah.Application.Transactions.FuelTransactions
{
    public class FuelTransOutAppService : AsyncCrudAppService<FuelTransOut, FuelTransOutDto, long, GetFuelTransOutsInput, CreateFuelTransOutDto, UpdateFuelTransOutDto>, IFuelTransOutAppService
    {

        private readonly ICommonAppService _commonService;
        private readonly IBranchWalletTransactionAppService _branchWalletTransactionAppService;
        private readonly IBranchAppService _branchAppService;
        private readonly IStoredProcedureAppService _storedProcedureAppService;
        private readonly IRepository<Veichle, long> _veichleRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly IRepository<MainProvider, long> _mainProviderRepository;
        private readonly IRepository<Company, long> _companyRepository;
        private readonly IRepository<Worker, long> _workerRepository;
        private readonly IRepository<Driver, long> _driverRepository;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly IRepository<UserDevice, long> _userDevicesRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly UserManager _userManager;


        CultureInfo new_lang = new CultureInfo("ar");

        public FuelTransOutAppService(
            IRepository<FuelTransOut, long> repository,
             ICommonAppService commonService,
             IBranchWalletTransactionAppService branchWalletTransactionAppService,
             IBranchAppService branchAppService,
             IStoredProcedureAppService storedProcedureAppService,
             IRepository<Veichle, long> veichleRepository,
             IRepository<Provider, long> providerRepository,
             IRepository<MainProvider, long> mainProviderRepository,
             IRepository<Company, long> companyRepository,
             IRepository<Worker, long> workerRepository,
             IRepository<Driver, long> driverRepository,
              AbpNotificationHelper abpNotificationHelper,
               IRepository<UserDevice, long> userDevicesRepository,
               IRepository<User, long> userRepository,
               IBackgroundJobManager backgroundJobManager,
               UserManager userManager
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _commonService = commonService;
            _branchWalletTransactionAppService = branchWalletTransactionAppService;
            _branchAppService = branchAppService;
            _storedProcedureAppService = storedProcedureAppService;
            _veichleRepository = veichleRepository;
            _workerRepository = workerRepository;
            _providerRepository = providerRepository;
            _mainProviderRepository = mainProviderRepository;
            _companyRepository = companyRepository;
            _driverRepository = driverRepository;
            _abpNotificationHelper = abpNotificationHelper;
            _userDevicesRepository = userDevicesRepository;
            _userRepository = userRepository;
            _backgroundJobManager = backgroundJobManager;
            _userManager = userManager;
        }

        public override async Task<FuelTransOutDto> GetAsync(EntityDto<long> input)
        {
            var FuelTransIn = await Repository.GetAll()
                .Include(at => at.Branch.Company)
                .Include(at => at.Driver)
                .Include(at => at.Provider.MainProvider)
                .Include(at => at.Worker)
                .Include(at => at.Veichle)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(FuelTransIn);
        }

        [AbpAuthorize]
        public override async Task<FuelTransOutDto> CreateAsync(CreateFuelTransOutDto input)
        {
            try
            {
                //Check if fuelTransOut exists
                //int existingCount = await Repository.CountAsync(at => (at.NameAr == input.NameAr || at.NameEn == input.NameEn));
                //if (existingCount > 0)
                //    throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExist"));

                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "FuelTransOuts", CodeField = "Code" });
                var branch = await _branchAppService.GetAsync(new EntityDto<long> { Id = input.BranchId.Value });

                var veichle = await _veichleRepository
                    .GetAll()
                    .Include(a => a.FuelGroup)
                    .FirstOrDefaultAsync(x => x.Id == input.VeichleId);


                var _mappedVeichle = ObjectMapper.Map<VeichleDto>(veichle);
                decimal veichleBalanceInSar = 0;


                decimal fuelPrice = await GetFuelPrice(new GetFuelPriceInput { FuelType = veichle.FuelType.Value, VeichleId = veichle.Id });


                if (_mappedVeichle.ConsumptionType == ConsumptionType.Group && _mappedVeichle.FuelGroup != null)
                {

                    if (_mappedVeichle.FuelGroup.GroupType == GroupType.Litre)
                    {
                        veichleBalanceInSar = branch.FuelAmount;
                    }

                    else if (_mappedVeichle.FuelGroup.GroupType == GroupType.Period)
                    {
                        // check period end date 

                        if (_mappedVeichle.FuelGroup.PeriodConsumptionType == PeriodConsumptionType.Money)
                        {
                            if (DateTime.UtcNow <= _mappedVeichle.MoneyBalanceEndDate)
                                veichleBalanceInSar = _mappedVeichle.MoneyBalance;
                        }
                        else if (_mappedVeichle.FuelGroup.PeriodConsumptionType == PeriodConsumptionType.Litre)
                        {
                            if (DateTime.UtcNow <= _mappedVeichle.MoneyBalanceEndDate)
                                veichleBalanceInSar = _mappedVeichle.FuelLitreBalance * input.FuelPrice ;
                        }
                    }
                    else if (_mappedVeichle.FuelGroup.GroupType == GroupType.Open)
                    {
                        veichleBalanceInSar = _mappedVeichle.FuelGroup.MaximumRechargeAmount > branch.FuelAmount ? _mappedVeichle.FuelGroup.MaximumRechargeAmount : branch.FuelAmount;
                    }
                }

                input.BranchBalance = branch.FuelAmount - branch.Reserved;
                decimal veichleAvailableAmount = veichleBalanceInSar;

                input.Reserved = Math.Min(input.BranchBalance ?? 0, veichleAvailableAmount);

                var branchUpdateInput = new UpdateReservedBalanceBranchDto
                {
                    Id = input.BranchId.Value,
                    Reserved = input.Reserved.Value,
                    OperationType = OperationType.Begin
                };

                if (branch.FuelAmount - branch.Reserved >= input.Reserved)
                {
                    var branchUpdated = await _branchAppService.UpdateReservedAndBalance(branchUpdateInput);
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.Wallets.BranchAmountNotEnoughForCarAmount"));
                }

                var fuelTransOut = ObjectMapper.Map<FuelTransOut>(input);
                fuelTransOut = await Repository.InsertAsync(fuelTransOut);
                await CurrentUnitOfWork.SaveChangesAsync();

                //Update Veichle Quantities
                //await UpdateVeichleQuantities(new EntityDto<long> { Id = input.VeichleId.Value });

                // send notifications here 

                // notify branch - provider - driver

                //await NotifyUsers(new NotifyInputDto
                //{
                //    EntityId = fuelTransOut.Id,
                //    BranchId = fuelTransOut.BranchId ?? 0,
                //    VeichleId = fuelTransOut.VeichleId,
                //    WorkerId = fuelTransOut.WorkerId,
                //    DriverId = fuelTransOut.DriverId,
                //    ProviderId = fuelTransOut.ProviderId
                //});


                return MapToEntityDto(fuelTransOut);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<FuelTransOutDto> UpdateTransaction(UpdateFuelTransOutDto input)
        {
            try
            {

                // get transactionFirst


                var fuelTransOut = await Repository.GetAll()
                    .Include(a => a.Branch.Company)
                        .Include(a => a.Driver)
                        .Include(a => a.Veichle.FuelGroup)
                        .Include(a => a.Provider)
                        .Include(a => a.Worker).FirstOrDefaultAsync(a => a.Id == input.Id);

                if (fuelTransOut == null || fuelTransOut.Branch == null)
                    throw new UserFriendlyException(L("MobileApi.Messages.ErrorOccurred"));


                if (fuelTransOut.Completed == true && fuelTransOut.BranchWalletTransactionId > 0)
                    throw new UserFriendlyException(L("MobileApi.Messages.AlreadyCreated"));


                //if (fuelTransOut.Branch.FuelAmount < fuelTransOut.Price)
                //    throw new UserFriendlyException(L("Pages.Wallets.BranchAmountNotEnoughForCarAmount"));

                // check veichles balance



                //if (fuelTransOut.Veichle.ConsumptionType == ConsumptionType.Group)
                //{
                //    fuelTransOut.GroupType = fuelTransOut.Veichle.FuelGroup.GroupType;

                //    if (fuelTransOut.Veichle.FuelGroup.GroupType == GroupType.Period)
                //    {
                //        fuelTransOut.PeriodConsumptionType = fuelTransOut.Veichle.FuelGroup.PeriodConsumptionType;

                //        if (fuelTransOut.Veichle.FuelGroup.PeriodConsumptionType == PeriodConsumptionType.Money)
                //        {
                //            if (fuelTransOut.Veichle.MoneyBalance < input.Price)
                //                throw new UserFriendlyException(L("Pages.Wallets.VeichleMoneyBalanceNotEnough"));
                //        }
                //        else if (fuelTransOut.Veichle.FuelGroup.PeriodConsumptionType == PeriodConsumptionType.Litre)
                //        {
                //            if (fuelTransOut.Veichle.FuelLitreBalance < input.Quantity)
                //                throw new UserFriendlyException(L("Pages.Wallets.VeichleLitreBalanceNotEnough"));
                //        }
                //    }

                //}
                //else
                if (fuelTransOut.Veichle.ConsumptionType == ConsumptionType.Trip)
                {
                    fuelTransOut.VeichleTripId = fuelTransOut.Veichle.VeichleTripId;
                }


                if (fuelTransOut.Veichle.FuelType == FuelType._91)
                    fuelTransOut.FuelPrice = fuelTransOut.Provider.FuelNinetyOnePrice;
                else if (fuelTransOut.Veichle.FuelType == FuelType._95)
                    fuelTransOut.FuelPrice = fuelTransOut.Provider.FuelNinetyFivePrice;
                else if (fuelTransOut.Veichle.FuelType == FuelType.diesel)
                    fuelTransOut.FuelPrice = fuelTransOut.Provider.SolarPrice;
                //else
                //    throw new UserFriendlyException("لم يتم تحديد نوع الوقود المستخدم للمركبة");





                //var wallet = await _branchWalletTransactionAppService
                //    .CreateAsync(new Wallets.Dto.CreateBranchWalletTransactionDto
                //    {
                //        BranchId = fuelTransOut.BranchId.Value,
                //        Amount = input.Price,
                //        IgnoreCompanyTransaction = true,
                //        TransactionType = TransactionType.Consumption,
                //        Note = L("Pages.Wallets.FuelTransNote"),
                //        TransId = input.Id,
                //        TransType = TransOutTypes.Fuel,
                //        VeichleId = fuelTransOut.VeichleId.Value,
                //        OperationType = OperationType.End,
                //        Reserved = fuelTransOut.Reserved.Value,
                //        Price = input.Price,
                //        IsTransOperation = true
                //    });

                var wallet = await _branchWalletTransactionAppService
                    .CreateFuelWalletTransaction(new Wallets.Dto.CreateBranchWalletTransactionDto
                    {
                        BranchId = fuelTransOut.BranchId.Value,
                        Amount = input.Price,
                        TransactionType = TransactionType.Consumption,
                        Note = L("Pages.Wallets.FuelTransNote"),
                        TransId = input.Id,
                        TransType = TransOutTypes.Fuel,
                        VeichleId = fuelTransOut.VeichleId.Value,
                        OperationType = OperationType.End,
                        Reserved = fuelTransOut.Reserved.Value,
                        Price = input.Price,
                        IsTransOperation = true
                    });



                ObjectMapper.Map(input, fuelTransOut);

                fuelTransOut.Completed = true;
                fuelTransOut.FuelType = fuelTransOut.Veichle.FuelType;
                fuelTransOut.ConsumptionType = fuelTransOut.Veichle.ConsumptionType;
                fuelTransOut.BranchWalletTransactionId = wallet.Id;
                if (fuelTransOut.Veichle.FuelGroup != null)
                {
                    fuelTransOut.GroupType = fuelTransOut.Veichle.FuelGroup.GroupType;
                    if (fuelTransOut.Veichle.FuelGroup.GroupType == GroupType.Period)
                        fuelTransOut.PeriodConsumptionType = fuelTransOut.Veichle.FuelGroup.PeriodConsumptionType;
                }

                await Repository.UpdateAsync(fuelTransOut);

                await CurrentUnitOfWork.SaveChangesAsync();

                //Update Veichle Quantities

                if (fuelTransOut.ConsumptionType == ConsumptionType.Group && fuelTransOut.GroupType.HasValue && fuelTransOut.GroupType == GroupType.Litre)
                {
                    await UpdateVeichleQuantities(new EntityDto<long> { Id = fuelTransOut.VeichleId.Value });
                }

                else if (fuelTransOut.ConsumptionType == ConsumptionType.Group && fuelTransOut.GroupType.HasValue && fuelTransOut.GroupType == GroupType.Period)
                {
                    if (fuelTransOut.PeriodConsumptionType == PeriodConsumptionType.Money)
                        await UpdateVeichleMoneyBalance(new UpdateVeichleMoneyBalanceInput { VeichleId = fuelTransOut.VeichleId.Value, Amount = fuelTransOut.Price });
                    else if (fuelTransOut.PeriodConsumptionType == PeriodConsumptionType.Litre)
                        await UpdateVeichleFuelLitreBalance(new UpdateVeichleMoneyBalanceInput { VeichleId = fuelTransOut.VeichleId.Value, Quantity = fuelTransOut.Quantity });
                }

                // notify branch - provider - driver


                await _backgroundJobManager.EnqueueAsync<NotifyUsersAfterFuelTransactionScheduleJob, NotifyInputDto>(new NotifyInputDto
                {
                    EntityId = fuelTransOut.Id,
                    BranchId = fuelTransOut.BranchId ?? 0,
                    VeichleId = fuelTransOut.VeichleId,
                    WorkerId = fuelTransOut.WorkerId,
                    DriverId = fuelTransOut.DriverId,
                    ProviderId = fuelTransOut.ProviderId
                }, BackgroundJobPriority.High, TimeSpan.FromSeconds(20));



                //await NotifyUsers(new NotifyInputDto
                //{
                //    EntityId = fuelTransOut.Id,
                //    BranchId = fuelTransOut.BranchId ?? 0,
                //    VeichleId = fuelTransOut.VeichleId,
                //    WorkerId = fuelTransOut.WorkerId,
                //    DriverId = fuelTransOut.DriverId,
                //    ProviderId = fuelTransOut.ProviderId
                //});

                return MapToEntityDto(fuelTransOut);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> NotifyUsers(NotifyInputDto input)
        {
            try
            {

                // get veichle 
                var veichle = await _veichleRepository.GetAll()
                    .Include(a => a.Branch.Company)
                    .FirstOrDefaultAsync(a => a.Id == input.VeichleId);


                if (veichle != null && veichle.Branch != null)
                {
                    input.BranchUserId = veichle.Branch.UserId;
                    input.BranchId = veichle.BranchId;
                    input.CompanyId = veichle.Branch.CompanyId;
                }

                var provider = await _providerRepository.GetAll()
                    .Include(a => a.MainProvider)
                    .FirstOrDefaultAsync(a => a.Id == input.ProviderId);

                if (provider != null && provider.MainProvider != null)
                {
                    input.ProviderUserId = provider.UserId;
                    input.ProviderId = provider.Id;
                    input.MainProviderId = provider.MainProviderId;
                }


                // get worker

                // get driver
                var driver = await _driverRepository.GetAll()
                  .FirstOrDefaultAsync(a => a.Id == input.DriverId);

                if (driver != null)
                {
                    input.DriverUserId = driver.UserId;
                    input.DriverName = driver.Name;
                }




                #region To Branch
                var notiProperties = new Dictionary<string, object>();
                List<UserIdentifier> targetUsersId = new List<UserIdentifier>();

                if (provider != null && provider.MainProvider != null)
                {

                    input.MessageAr = L("Pages.Notifications.BranchFuelTransactionMsg", new CultureInfo("ar"), veichle.FullPlateNumberAr, veichle.Code, provider.NameAr, provider.MainProvider.NameAr);
                    input.MessageEn = L("Pages.Notifications.BranchFuelTransactionMsg", new CultureInfo("en"), veichle.FullPlateNumber, veichle.Code, provider.NameEn, provider.MainProvider.NameEn);


                    // dictionary
                    notiProperties.Add("MessageAr", input.MessageAr);
                    notiProperties.Add("MessageEn", input.MessageEn);


                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.BranchUserId.Value));

                    // get branch employees here first 
                    // all employees that has permission   in this branch
                    var employees = await _userRepository.GetAll().Include(a => a.Permissions)
                        .Where(a => a.IsDeleted == false && a.IsActive == true && a.UserType == UserTypes.Employee && a.BranchId == input.BranchId && a.Permissions.Any(aa => aa.Name == PermissionNames.BranchData.BranchFuelTransOuts.Read && aa.IsGranted == true)).ToListAsync();

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
                        EntityType = Entity_Type.FuelTransaction,
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
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.FuelTransaction, _createUserNotificationData, targetUsersId.ToArray());

                }


                #endregion



                #region To Provider

                if (veichle != null && veichle.Branch != null && veichle.Branch.Company != null)
                {

                    input.MessageAr = L("Pages.Notifications.ProviderFuelTransactionMsg", new CultureInfo("ar"), veichle.FullPlateNumberAr, veichle.Code, veichle.Branch.NameAr, veichle.Branch.Company.NameAr);
                    input.MessageEn = L("Pages.Notifications.ProviderFuelTransactionMsg", new CultureInfo("en"), veichle.FullPlateNumber, veichle.Code, veichle.Branch.NameEn, veichle.Branch.Company.NameEn);


                    // dictionary
                    notiProperties = new Dictionary<string, object>();
                    notiProperties.Add("MessageAr", input.MessageAr);
                    notiProperties.Add("MessageEn", input.MessageEn);

                    targetUsersId = new List<UserIdentifier>();

                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.ProviderUserId.Value));

                    // all employees that has permission   in this branch
                    var employees = await _userRepository.GetAll().Include(a => a.Permissions)
                        .Where(a => a.IsDeleted == false && a.IsActive == true && a.UserType == UserTypes.Employee && a.ProviderId == input.ProviderId && a.Permissions.Any(aa => aa.Name == PermissionNames.ProviderData.ProviderFuelTransactions.Read && aa.IsGranted == true)).ToListAsync();

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
                        EntityType = Entity_Type.FuelTransaction,
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
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.FuelTransaction, _createUserNotificationData, targetUsersId.ToArray());

                }

                #endregion

                #region To Driver

                if (veichle != null)
                {

                    input.MessageAr = L("Pages.Notifications.DriverFuelTransactionMsg", new CultureInfo("ar"), veichle.Code);
                    input.MessageEn = L("Pages.Notifications.DriverFuelTransactionMsg", new CultureInfo("en"), veichle.Code);


                    var targetDriverUsersId = new List<UserIdentifier>();

                    var _lang = await SettingManager.GetSettingValueForUserAsync("Abp.Localization.DefaultLanguageName", new UserIdentifier(AbpSession.TenantId, input.DriverUserId.Value));
                    if (_lang == null)
                        _lang = new_lang.ToString();

                    FCMPushNotification fcmPushClient = new FCMPushNotification();
                    var userDevices = await _userDevicesRepository.GetAll()
                        .Where(x => x.UserId == input.BranchUserId)
                        .ToListAsync();

                    if (userDevices != null && userDevices.Count > 0)
                    {
                        foreach (var item in userDevices)
                        {
                            FCMPushNotification fcmPushResultClient = await fcmPushClient.SendNotification(new FcmNotificationInput()
                            {
                                RegistrationToken = item.RegistrationToken,
                                Title = L("Common.SystemTitle"),
                                Body = _lang == "ar" ? input.MessageAr : input.MessageEn,
                                Type = FcmNotificationType.FuelTransaction,
                                PatternId = input.EntityId,
                                BranchId = input.BranchId,
                                DriverId = input.DriverId,
                                CompanyId = input.CompanyId,
                                MainProviderId = input.MainProviderId,
                                ProviderId = input.ProviderId,
                                WorkerId = input.WorkerId
                            });
                        }
                    }

                    // dictionary
                    notiProperties = new Dictionary<string, object>();
                    notiProperties.Add("MessageAr", input.MessageAr);
                    notiProperties.Add("MessageEn", input.MessageEn);


                    targetDriverUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: input.DriverUserId.Value));


                    var _createDriverUserNotificationData = new CreateNotificationDto
                    {
                        //SenderUserName = (await _userManager.GetUserByIdAsync(AbpSession.UserId.Value)).Name,
                        Message = input.MessageAr,
                        EntityType = Entity_Type.FuelTransaction,
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
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.FuelTransaction, _createDriverUserNotificationData, targetDriverUsersId.ToArray());

                }

                #endregion

                return true;
            }
            catch (Exception ex)
            {

                throw new UserFriendlyException(ex.Message);
            }
        }

        [AbpAuthorize]
        public async Task<bool> CancelFuelTransOut(CancelFuelTransOutDto input)
        {

            var fuelTransOut = await Repository.FirstOrDefaultAsync(x => x.Id == input.Id);
            if (fuelTransOut.Completed == true)
                return false;

            fuelTransOut.Completed = false;

            var veichle = _veichleRepository.FirstOrDefault(x => x.Id == fuelTransOut.VeichleId);


            var branchUpdateInput = new UpdateReservedBalanceBranchDto
            {
                Id = fuelTransOut.BranchId.Value,
                Reserved = fuelTransOut.Reserved.Value,
                OperationType = OperationType.Cancel
            };

            var branchUpdated = _branchAppService.UpdateReservedAndBalance(branchUpdateInput);

            ObjectMapper.Map(input, fuelTransOut);
            await Repository.UpdateAsync(fuelTransOut);

            return true;
        }

        [AbpAuthorize]
        public async Task<bool> UpdateVeichleQuantities(EntityDto<long> input)
        {
            try
            {
                // update Veichle fuel trans in 
                var _veichel = await _veichleRepository.FirstOrDefaultAsync(input.Id);
                _veichel.Fuel_Out = await Repository.GetAll().Where(a => a.VeichleId == input.Id && a.GroupType == GroupType.Litre).SumAsync(a => a.Quantity);
                _veichel.Fuel_Balance = _veichel.Fuel_In - _veichel.Fuel_Out;

                await _veichleRepository.UpdateAsync(_veichel);
                await UnitOfWorkManager.Current.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        [AbpAuthorize]
        public async Task<bool> UpdateVeichleMoneyBalance(UpdateVeichleMoneyBalanceInput input)
        {
            try
            {
                // update Veichle fuel trans in 
                var _veichel = await _veichleRepository.FirstOrDefaultAsync(input.VeichleId.Value);
                _veichel.MoneyBalance = _veichel.MoneyBalance - input.Amount;
                await _veichleRepository.UpdateAsync(_veichel);
                await UnitOfWorkManager.Current.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [AbpAuthorize]
        public async Task<bool> UpdateVeichleFuelLitreBalance(UpdateVeichleMoneyBalanceInput input)
        {
            try
            {
                // update Veichle fuel trans in 
                var _veichel = await _veichleRepository.FirstOrDefaultAsync(input.VeichleId.Value);
                decimal _fuel = _veichel.FuelLitreBalance - input.Quantity;
                _veichel.FuelLitreBalance = _fuel;
                await _veichleRepository.UpdateAsync(_veichel);
                await UnitOfWorkManager.Current.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        [AbpAuthorize]
        public async Task<decimal> GetFuelPrice(GetFuelPriceInput input)
        {
            try
            {
                decimal fuelPrice = 0m;
                // worker 
                var worker = await _workerRepository.FirstOrDefaultAsync(a => a.UserId == AbpSession.UserId);
                if (worker != null)
                {
                    var provider = await _providerRepository.FirstOrDefaultAsync(a => a.Id == worker.ProviderId);


                    if (string.IsNullOrEmpty(provider.FuelTypes))
                        throw new UserFriendlyException(L("Pages.Veichles.Messages.ProviderFuelTypeError"));

                    string[] _types = provider.FuelTypes.Split(';');

                    if (input.FuelType == FuelType._91 || input.FuelType == FuelType._95 || input.FuelType == FuelType.diesel)
                    {
                        if (input.FuelType == FuelType._91 && _types.Any(a => a == "91"))
                            return provider.FuelNinetyOnePrice;
                        if (input.FuelType == FuelType._95 && _types.Any(a => a == "95"))
                            return provider.FuelNinetyFivePrice;
                        if (input.FuelType == FuelType.diesel && _types.Any(a => a == "ديزل" || a == "Diezel"))
                            return provider.SolarPrice;
                        else
                            throw new UserFriendlyException(L("Pages.Veichles.Messages.FuelTypeNotAvailable"));
                    }
                    else
                        throw new UserFriendlyException(L("Pages.Veichles.Messages.FuelTypeError"));
                }

                return fuelPrice;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        [AbpAuthorize]
        public override async Task<FuelTransOutDto> UpdateAsync(UpdateFuelTransOutDto input)
        {

            var fuelTransOut = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
            ObjectMapper.Map(input, fuelTransOut);
            await Repository.UpdateAsync(fuelTransOut);
            return MapToEntityDto(fuelTransOut);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var fuelTransOut = await Repository.GetAsync(input.Id);
            if (fuelTransOut == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(fuelTransOut);
        }

        public override async Task<PagedResultDto<FuelTransOutDto>> GetAllAsync(GetFuelTransOutsInput input)
        {
            var query = Repository.GetAll()
                .Include(at => at.Branch.Company)
                .Include(at => at.Driver)
                .Include(at => at.Provider)
                .Include(at => at.Worker)
                .Include(at => at.Veichle).Where(a => a.Completed == true);
            query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
            query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
            query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
            query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
            query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);


            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var fuelTransIns = await query.OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount).Take(input.MaxResultCount)
                .ToListAsync();

            var _mappedList = ObjectMapper.Map<List<FuelTransOutDto>>(fuelTransIns);
            return new PagedResultDto<FuelTransOutDto>(count, _mappedList);
        }

        [AbpAuthorize]
        public async Task<DataTableOutputDto<FuelTransOutDto>> GetPaged(GetFuelTransOutsPagedInput input)
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
                            FuelTransOut fuelTransOut = await Repository.FirstOrDefaultAsync(id);
                            if (fuelTransOut != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _fuelTransInClinicRepository.CountAsync(a => a.FuelTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.FuelTransIns.HasClinics"));

                                    await Repository.DeleteAsync(fuelTransOut);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

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
                            FuelTransOut fuelTransOut = await Repository.FirstOrDefaultAsync(id);
                            if (fuelTransOut != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _fuelTransInClinicRepository.CountAsync(a => a.FuelTransInId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.FuelTransIns.HasClinics"));

                                    await Repository.DeleteAsync(fuelTransOut);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll()
                        .Include(a => a.Branch.Company)
                        .Include(a => a.Driver)
                        .Include(a => a.Veichle)
                        .Include(a => a.Provider.MainProvider)
                        .Include(a => a.Worker).Where(a => a.Completed == true)
                        ;
                    query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId);
                    query = query.WhereIf(input.BrandId.HasValue, at => at.Veichle.BrandId == input.BrandId);
                    query = query.WhereIf(input.ModelId.HasValue, at => at.Veichle.ModelId == input.ModelId);

                    if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                        }
                        else
                            return new DataTableOutputDto<FuelTransOutDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<FuelTransOutDto>()
                            };
                    }



                    if (input.IsCompanyEmployee.HasValue && input.IsCompanyEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                        }
                        else
                            return new DataTableOutputDto<FuelTransOutDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<FuelTransOutDto>()
                            };
                    }



                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);

                    query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                    query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
                    query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.InvoiceCode), at => at.InvoiceCode.Contains(input.InvoiceCode));

                    query = query.WhereIf(!string.IsNullOrEmpty(input.CompanyName), at => at.Branch.Company.NameAr.Contains(input.CompanyName) || at.Branch.Company.NameEn.Contains(input.CompanyName) || at.Branch.Company.Code.Contains(input.CompanyName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName) || at.Branch.Code.Contains(input.BranchName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.DriverName), at => at.Driver.Name.Contains(input.DriverName) || at.Driver.Code.Contains(input.DriverName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at =>
                    at.Veichle.NameAr.Contains(input.VeichleName) ||
                    at.Veichle.NameEn.Contains(input.VeichleName) ||
                    at.Veichle.FullPlateNumber.Contains(input.VeichleName) ||
                    at.Veichle.FullPlateNumberAr.Contains(input.VeichleName) ||
                    at.Driver.Code.Contains(input.VeichleName));


                    query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.Provider.MainProvider.NameAr.Contains(input.MainProviderName) || at.Provider.MainProvider.NameEn.Contains(input.MainProviderName) || at.Provider.MainProvider.Code.Contains(input.MainProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName) || at.Provider.Code.Contains(input.ProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.WorkerName), at => at.Worker.Name.Contains(input.WorkerName) || at.Worker.Code.Contains(input.WorkerName));

                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);
                    query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);

                    int filteredCount = await query.CountAsync();
                    var fuelTransIns = await query.Include(a => a.Branch.Company)
                        .Include(a => a.Driver)
                        .Include(a => a.Veichle)
                        .Include(a => a.Provider)
                        .Include(a => a.Worker)
                        .Include(x => x.CreatorUser).Include(x => x.LastModifierUser)
                        .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir)).Skip(input.start).Take(input.length).ToListAsync();
                    return new DataTableOutputDto<FuelTransOutDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<FuelTransOutDto>>(fuelTransIns)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<GetFuelTransoutOutput> GetAllPaged(GetFuelTransInsInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {


                    float totalRates = 0;

                    var query = Repository.GetAll()
                     .Include(a => a.Branch.Company)
                     .Include(a => a.Driver)
                     .Include(a => a.Veichle)
                     .Include(a => a.Provider.MainProvider)
                     .Include(a => a.Worker).Where(a => a.Completed == true);

                    query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId);

                    int count = query.Count();

                    query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                    query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
                    query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                    query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);

                    query = query.WhereIf(!string.IsNullOrEmpty(input.InvoiceCode), at => at.InvoiceCode.Contains(input.InvoiceCode));

                    query = query.WhereIf(!string.IsNullOrEmpty(input.CompanyName), at => at.Branch.Company.NameAr.Contains(input.CompanyName) || at.Branch.Company.NameEn.Contains(input.CompanyName) || at.Branch.Company.Code.Contains(input.CompanyName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName) || at.Branch.Code.Contains(input.BranchName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.DriverName), at => at.Driver.Name.Contains(input.DriverName) || at.Driver.Code.Contains(input.DriverName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at =>
                    at.Veichle.NameAr.Contains(input.VeichleName) ||
                    at.Veichle.NameEn.Contains(input.VeichleName) ||
                    at.Veichle.FullPlateNumber.Contains(input.VeichleName) ||
                    at.Veichle.FullPlateNumberAr.Contains(input.VeichleName) ||
                    at.Driver.Code.Contains(input.VeichleName));


                    query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.Provider.MainProvider.NameAr.Contains(input.MainProviderName) || at.Provider.MainProvider.NameEn.Contains(input.MainProviderName) || at.Provider.MainProvider.Code.Contains(input.MainProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName) || at.Provider.Code.Contains(input.ProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.WorkerName), at => at.Worker.Name.Contains(input.WorkerName) || at.Worker.Code.Contains(input.WorkerName));


                    if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                    {
                        DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodFromString) ?
                                            DateTime.ParseExact(input.FullPeriodFromString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                           : DateTime.UtcNow;

                        query = query.Where(at => at.CreationTime >= _reservationDate);

                    }

                    if (!string.IsNullOrEmpty(input.FullPeriodToString))
                    {
                        DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodToString) ?
                                            DateTime.ParseExact(input.FullPeriodToString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                           : DateTime.UtcNow;


                        query = query.Where(at => at.CreationTime <= _reservationDate);
                    }


                    if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }

                    if (input.IsCompanyEmployee.HasValue && input.IsCompanyEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }



                    int filterCount = query.Count();


                    if (input.MaxCount == true)
                    {
                        input.SkipCount = 0;
                        input.MaxResultCount = filterCount;
                    }


                    decimal totalPrice = 0;
                    decimal totalQuantity = 0;


                    if (filterCount > 0)
                    {
                        totalPrice = await query.SumAsync(a => a.Price);
                        totalQuantity = await query.SumAsync(a => a.Quantity);
                    }

                    var transactions = await query.OrderByDescending(x => x.CreationTime)
                        .Skip(input.SkipCount)
                        .Take(input.MaxResultCount).ToListAsync();

                    return new GetFuelTransoutOutput
                    {
                        Transactions = ObjectMapper.Map<List<FuelTransOutDto>>(transactions),
                        AllCount = count,
                        FilterCount = filterCount,
                        TotalRates = totalRates,
                        TotalQuantity = totalQuantity,
                        TotalPrice = totalPrice
                    };

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }



        [AbpAuthorize]
        public async Task<string> ExportExcel(RequestFuelExcelDtoInput input)
        {
            try
            {

                try
                {

                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {

                        CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

                        //var _transactions = await _storedProcedureAppService.GetTransactionsReport(input);

                        var query = Repository.GetAll()
               .Include(a => a.Branch.Company)
               .Include(a => a.Driver)
               .Include(a => a.Veichle)
               .Include(a => a.Provider.MainProvider)
               .Include(a => a.Worker).Where(a => a.Completed == true);

                        query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                        query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                        query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                        query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId);

                        int count = query.Count();

                        query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                        query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
                        query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                        query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
                        query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                        query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);

                        query = query.WhereIf(!string.IsNullOrEmpty(input.InvoiceCode), at => at.InvoiceCode.Contains(input.InvoiceCode));

                        query = query.WhereIf(!string.IsNullOrEmpty(input.CompanyName), at => at.Branch.Company.NameAr.Contains(input.CompanyName) || at.Branch.Company.NameEn.Contains(input.CompanyName) || at.Branch.Company.Code.Contains(input.CompanyName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName) || at.Branch.Code.Contains(input.BranchName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.DriverName), at => at.Driver.Name.Contains(input.DriverName) || at.Driver.Code.Contains(input.DriverName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at =>
                        at.Veichle.NameAr.Contains(input.VeichleName) ||
                        at.Veichle.NameEn.Contains(input.VeichleName) ||
                        at.Veichle.FullPlateNumber.Contains(input.VeichleName) ||
                        at.Veichle.FullPlateNumberAr.Contains(input.VeichleName) ||
                        at.Driver.Code.Contains(input.VeichleName));


                        query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.Provider.MainProvider.NameAr.Contains(input.MainProviderName) || at.Provider.MainProvider.NameEn.Contains(input.MainProviderName) || at.Provider.MainProvider.Code.Contains(input.MainProviderName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName) || at.Provider.Code.Contains(input.ProviderName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.WorkerName), at => at.Worker.Name.Contains(input.WorkerName) || at.Worker.Code.Contains(input.WorkerName));



                        if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                        {
                            DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodFromString) ?
                                                DateTime.ParseExact(input.FullPeriodFromString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                               : DateTime.UtcNow;

                            query = query.Where(at => at.CreationTime >= _reservationDate);

                        }

                        if (!string.IsNullOrEmpty(input.FullPeriodToString))
                        {
                            DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodToString) ?
                                                DateTime.ParseExact(input.FullPeriodToString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                               : DateTime.UtcNow;


                            query = query.Where(at => at.CreationTime <= _reservationDate);
                        }


                        if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                        {

                            if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                            {
                                query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                            }
                            else
                                throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                        }

                        if (input.IsCompanyEmployee.HasValue && input.IsCompanyEmployee == true)
                        {

                            if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                            {
                                query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                            }
                            else
                                throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                        }


                        //var lang = await SettingManager.GetSettingValueForUserAsync("Abp.Localization.DefaultLanguageName", new UserIdentifier(AbpSession.TenantId, AbpSession.UserId.Value));
                        //if (lang == null)
                        //    lang = new_lang.ToString();



                        var transactions = await query.OrderByDescending(x => x.CreationTime)
                 //.Skip(input.SkipCount)
                 //.Take(input.MaxResultCount)
                 .ToListAsync();

                        var courseBookingRequestsList = ObjectMapper.Map<List<NewFuelTransOutDto>>(transactions);

                        var cBList = courseBookingRequestsList.ToList();

                        if (cBList != null && cBList.Count > 0)
                        {

                            var excelData = new List<RequestFuelExcelDto>();

                            if (input.MainProviderId.HasValue)
                            {
                                excelData = ObjectMapper.Map<List<RequestFuelExcelDto>>(cBList.Select(m => new RequestFuelExcelDto
                                {
                                    Code = m.Code,
                                    InvoiceCode = m.CompanyInvoiceCode,
                                    Price = m.Price,
                                    Provider = m.Provider != null ? m.Provider.Name : "",
                                    FuelType = m.FuelType == FuelType._91 ? "91" : m.FuelType == FuelType._95 ? "95" : L("Pages.Providers.Diezel"),
                                    Quantity = m.Quantity,
                                    Veichle = m.Veichle != null ? m.MappedFullPlateNumber : "",
                                    Worker = m.Worker != null ? m.Worker.Name : "",
                                    CreationTime = m.CreationTime.ToString(),
                                }));
                            }
                            else if(input.IsBranch.HasValue && input.IsBranch == true)
                            {
                                {
                                    excelData = ObjectMapper.Map<List<RequestFuelExcelDto>>(cBList.Select(m => new RequestFuelExcelDto
                                    {
                                        Code = m.Code,
                                        InvoiceCode = m.CompanyInvoiceCode,
                                        Price = m.Price,
                                        Provider = m.Provider != null ? m.Provider.Name : "",
                                        FuelType = m.FuelType == FuelType._91 ? "91" : m.FuelType == FuelType._95 ? "95" : L("Pages.Providers.Diezel"),
                                        Quantity = m.Quantity,
                                        Veichle = m.Veichle != null ? m.MappedFullPlateNumber : "",
                                        VeichleTypeString = m.VeichleTypeString,
                                        Worker = m.Worker != null ? m.Worker.Name : "",
                                        Driver = m.Driver != null ? m.Driver.Name : "",
                                        MainProviderName = m.Provider.MainProvider.Name,
                                        CreationTime = m.CreationTime.ToString()
                                    }));
                                }
                            }
                            else
                            {
                                excelData = ObjectMapper.Map<List<RequestFuelExcelDto>>(cBList.Select(m => new RequestFuelExcelDto
                                {
                                    Code = m.Code,
                                    InvoiceCode = m.CompanyInvoiceCode,
                                    Branch = m.Branch != null ? m.Branch.Name : "",
                                    Price = m.Price,
                                    Provider = m.Provider != null ? m.Provider.Name : "",
                                    FuelType = m.FuelType == FuelType._91 ? "91" : m.FuelType == FuelType._95 ? "95" : L("Pages.Providers.Diezel"),
                                    Quantity = m.Quantity,
                                    Veichle = m.Veichle != null ? m.MappedFullPlateNumber : "",
                                    VeichleTypeString = m.VeichleTypeString,
                                    Worker = m.Worker != null ? m.Worker.Name : "",
                                    Driver = m.Driver != null ? m.Driver.Name : "",
                                    MainProviderName = m.Provider.MainProvider.Name,
                                    CreationTime = m.CreationTime.ToString()
                                }));
                            }

                            decimal _litresCount = excelData.Sum(a => a.Quantity);
                            decimal _priceAmount = excelData.Sum(a => a.Price);

                            excelData.Add(new RequestFuelExcelDto
                            {
                                Code = L("Pages.Report.TotalPrice"),
                                Quantity = _litresCount,
                                Price = _priceAmount
                            });

                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();


                            List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();

                            if (input.MainProviderId.HasValue)
                            {

                                var _mainProvider = await _mainProviderRepository.FirstOrDefaultAsync(input.MainProviderId.Value);


                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "نوع الملف",
                                    Value = L("Pages.StationTransactions.Title")
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "تاريخ الإصدار",
                                    Value = DateTime.Now.ToString()
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = L("Pages.Reports.Station"),
                                    Value = currentCulture.Name.Contains("ar")? _mainProvider.NameAr : _mainProvider.NameEn
                                });

                                _options = new RequestFuelExcelOptionsDto
                                {
                                    ExcelDate = DateTime.Now.ToString(),
                                    ProviderName = currentCulture.Name.Contains("ar") ? _mainProvider.NameAr : _mainProvider.NameEn,
                                    ExcelType = L("Pages.Reports.Operations"),
                                    KeyValues = _keyValues
                                };
                            }


                            else if (input.CompanyId.HasValue)
                            {

                                var _company = await _companyRepository.FirstOrDefaultAsync(input.CompanyId.Value);


                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "نوع الملف",
                                    Value = L("Pages.Reports.Operations")
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "تاريخ الإصدار",
                                    Value = DateTime.Now.ToString()
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "الشركة",
                                    Value = currentCulture.Name.Contains("ar") ? _company.NameAr : _company.NameEn
                                });

                                _options = new RequestFuelExcelOptionsDto
                                {
                                    ExcelDate = DateTime.Now.ToString(),
                                    ProviderName = currentCulture.Name.Contains("ar") ? _company.NameAr : _company.NameEn,
                                    ExcelType = L("Pages.Reports.Operations"),
                                    KeyValues = _keyValues
                                };
                            }
                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            //dataSet.Tables[0].TableName = L("Pages.StationTransactions.FuelTransactions.Header");
                            //dataSet.Tables[1].TableName = L("Pages.StationTransactions.FuelTransactions");
                            ExcelSource source = ExcelSource.FuelTransactions;
                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.StationTransactions.FuelTransactions") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                            // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);

                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<string> ExportExcelAdmin(RequestFuelExcelDtoInput input)
        {
            try
            {

                try
                {

                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {

                        var query = Repository.GetAll()
               .Include(a => a.Branch.Company)
               .Include(a => a.Driver)
               .Include(a => a.Veichle)
               .Include(a => a.Provider.MainProvider)
               .Include(a => a.Worker).Where(a => a.Completed == true);

                        query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                        query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                        query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                        query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId);

                        int count = query.Count();

                        query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                        query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
                        query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                        query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
                        query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                        query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);

                        query = query.WhereIf(!string.IsNullOrEmpty(input.InvoiceCode), at => at.InvoiceCode.Contains(input.InvoiceCode));

                        query = query.WhereIf(!string.IsNullOrEmpty(input.CompanyName), at => at.Branch.Company.NameAr.Contains(input.CompanyName) || at.Branch.Company.NameEn.Contains(input.CompanyName) || at.Branch.Company.Code.Contains(input.CompanyName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName) || at.Branch.Code.Contains(input.BranchName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.DriverName), at => at.Driver.Name.Contains(input.DriverName) || at.Driver.Code.Contains(input.DriverName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at =>
                        at.Veichle.NameAr.Contains(input.VeichleName) ||
                        at.Veichle.NameEn.Contains(input.VeichleName) ||
                        at.Veichle.FullPlateNumber.Contains(input.VeichleName) ||
                        at.Veichle.FullPlateNumberAr.Contains(input.VeichleName) ||
                        at.Driver.Code.Contains(input.VeichleName));


                        query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.Provider.MainProvider.NameAr.Contains(input.MainProviderName) || at.Provider.MainProvider.NameEn.Contains(input.MainProviderName) || at.Provider.MainProvider.Code.Contains(input.MainProviderName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName) || at.Provider.Code.Contains(input.ProviderName));
                        query = query.WhereIf(!string.IsNullOrEmpty(input.WorkerName), at => at.Worker.Name.Contains(input.WorkerName) || at.Worker.Code.Contains(input.WorkerName));



                        if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                        {
                            DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodFromString) ?
                                                DateTime.ParseExact(input.FullPeriodFromString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                               : DateTime.UtcNow;

                            query = query.Where(at => at.CreationTime >= _reservationDate);

                        }

                        if (!string.IsNullOrEmpty(input.FullPeriodToString))
                        {
                            DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodToString) ?
                                                DateTime.ParseExact(input.FullPeriodToString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                               : DateTime.UtcNow;


                            query = query.Where(at => at.CreationTime <= _reservationDate);
                        }



                        var transactions = await query.OrderByDescending(x => x.CreationTime)
                 .ToListAsync();

                        var courseBookingRequestsList = ObjectMapper.Map<List<NewFuelTransOutDto>>(transactions);

                        var cBList = courseBookingRequestsList.ToList();

                        if (cBList != null && cBList.Count > 0)
                        {

                            var excelData = new List<AdminRequestFuelExcelDto>();


                            excelData = ObjectMapper.Map<List<AdminRequestFuelExcelDto>>(cBList.Select(m => new AdminRequestFuelExcelDto
                            {
                                Code = m.Code,
                                InvoiceCode = m.CompanyInvoiceCode,
                                Company = m.Branch != null ? m.Branch.Company.Name : null,
                                Branch = m.Branch != null ? m.Branch.Name : "",
                                Price = m.Price,
                                Provider = m.Provider != null ? m.Provider.Name : "",
                                FuelType = m.FuelType == FuelType._91 ? "91" : m.FuelType == FuelType._95 ? "95" : "ديزل",
                                Quantity = m.Quantity,
                                //Veichle = m.Veichle != null ? m.Veichle.FullPlateNumber + " " + m.Veichle.FullPlateNumberAr : "",
                                Veichle = m.Veichle != null ? m.MappedFullPlateNumber : "",
                                Worker = m.Worker != null ? m.Worker.Name : "",
                                Driver = m.Driver != null ? m.Driver.Name : "",
                                CreationTime = m.CreationTime.ToString(),
                                MainProvider = m.Provider.MainProvider.Name
                            }));

                            decimal _litresCount = excelData.Sum(a => a.Quantity);
                            decimal _priceAmount = excelData.Sum(a => a.Price);

                            excelData.Add(new AdminRequestFuelExcelDto
                            {
                                Code = L("Pages.Report.TotalPrice"),
                                Quantity = _litresCount,
                                Price = _priceAmount
                            });

                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();


                            List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();



                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = "نوع الملف",
                                Value = L("Pages.Reports.Operations")
                            });

                            _keyValues.Add(new RequestFuelExcelOptionKeyValue
                            {
                                Key = "تاريخ الإصدار",
                                Value = DateTime.Now.ToString()
                            });

                            //_keyValues.Add(new RequestFuelExcelOptionKeyValue
                            //{
                            //    Key = "المحطة",
                            //    Value = _mainProvider.NameAr
                            //});

                            _options = new RequestFuelExcelOptionsDto
                            {
                                ExcelDate = DateTime.Now.ToString(),
                                //ProviderName = _mainProvider.NameAr,
                                ExcelType = L("Pages.Reports.Operations"),
                                KeyValues = _keyValues
                            };

                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            //dataSet.Tables[0].TableName = L("Pages.StationTransactions.FuelTransactions.Header");
                            //dataSet.Tables[1].TableName = L("Pages.StationTransactions.FuelTransactions");
                            ExcelSource source = ExcelSource.FuelTransactions;
                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.StationTransactions.FuelTransactions") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                            // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);

                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<string> ExportProviderExcel(RequestFuelExcelDtoInput input)
        {
            try
            {
                try
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {

                        var query = Repository.GetAll()
               .Include(a => a.Branch.Company)
               .Include(a => a.Driver)
               .Include(a => a.Veichle)
               .Include(a => a.Provider.MainProvider)
               .Include(a => a.Worker).Where(a => a.Completed == true);

                        query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                        query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                        query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                        query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId);

                        int count = query.Count();

                        query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                        query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
                        query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                        query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
                        query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                        query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);

                        if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                        {
                            DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodFromString) ?
                                                DateTime.ParseExact(input.FullPeriodFromString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                               : DateTime.UtcNow;

                            query = query.Where(at => at.CreationTime >= _reservationDate);

                        }

                        if (!string.IsNullOrEmpty(input.FullPeriodToString))
                        {
                            DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodToString) ?
                                                DateTime.ParseExact(input.FullPeriodToString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                               : DateTime.UtcNow;


                            query = query.Where(at => at.CreationTime <= _reservationDate);
                        }


                        if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                        {

                            if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                            {
                                query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                            }
                            else
                                throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                        }


                        string providerName = string.Empty;

                        if (input.ProviderId.HasValue)
                        {
                            var provider = await _providerRepository.FirstOrDefaultAsync(input.ProviderId.Value);
                            providerName = provider.NameAr;
                        }

                        else if (input.MainProviderId.HasValue)
                        {
                            var mainProvider = await _mainProviderRepository.FirstOrDefaultAsync(input.MainProviderId.Value);
                            providerName = mainProvider.NameAr;
                        }



                        var transactions = await query.OrderByDescending(x => x.CreationTime)
                 .ToListAsync();

                        var courseBookingRequestsList = ObjectMapper.Map<List<NewFuelTransOutDto>>(transactions);

                        var cBList = courseBookingRequestsList.ToList();

                        if (cBList != null && cBList.Count > 0)
                        {
                            var excelData = ObjectMapper.Map<List<RequestProviderFuelExcelDto>>(cBList.Select(m => new RequestProviderFuelExcelDto
                            {
                                Code = m.Code,
                                InvoiceCode = m.CompanyInvoiceCode,
                                Price = m.Price,
                                FuelType = m.FuelType == FuelType._91 ? "91" : m.FuelType == FuelType._95 ? "95" : "ديزل",
                                Quantity = m.Quantity,
                                Veichle = m.Veichle != null ? m.MappedFullPlateNumber : "",
                                Worker = m.Worker != null ? m.Worker.Name : "",
                                CreationTime = m.CreationTime.ToString(),
                            }));


                            decimal _litresCount = excelData.Sum(a => a.Quantity);
                            decimal _priceAmount = excelData.Sum(a => a.Price);

                            excelData.Add(new RequestProviderFuelExcelDto
                            {
                                Code = L("Pages.Report.TotalPrice"),
                                Quantity = _litresCount,
                                Price = _priceAmount
                            });

                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();


                            List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();

                            if (input.MainProviderId.HasValue)
                            {

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "نوع الملف",
                                    Value = L("Pages.Reports.Operations")
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "تاريخ الإصدار",
                                    Value = DateTime.Now.ToString()
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = input.ProviderId.HasValue ? L("Pages.Providers.ModalTitle") : L("Pages.Reports.ServiceProvider"),
                                    Value = providerName
                                });

                                _options = new RequestFuelExcelOptionsDto
                                {
                                    ExcelDate = DateTime.Now.ToString(),
                                    ProviderName = providerName,
                                    ExcelType = L("Pages.Reports.Operations"),
                                    KeyValues = _keyValues
                                };
                            }
                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            ExcelSource source = ExcelSource.FuelTransactions;
                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.StationTransactions.FuelTransactions") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }






        [AbpAuthorize]
        public async Task<string> ExportExcelForCompany(RequestFuelExcelDtoInput input)
        {
            try
            {

                try
                {

                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {
                        CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

                        //var _transactions = await _storedProcedureAppService.GetTransactionsReport(input);

                        var query = Repository.GetAll()
               .Include(a => a.Branch.Company)
               .Include(a => a.Driver)
               .Include(a => a.Veichle)
               .Include(a => a.Provider.MainProvider)
               .Include(a => a.Worker).Where(a => a.Completed == true);

                        query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                        query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                        query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                        query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId);

                        int count = query.Count();

                        query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                        query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
                        query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                        query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
                        query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                        query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);

                        if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                        {
                            DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodFromString) ?
                                                DateTime.ParseExact(input.FullPeriodFromString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                               : DateTime.UtcNow;

                            query = query.Where(at => at.CreationTime >= _reservationDate);

                        }

                        if (!string.IsNullOrEmpty(input.FullPeriodToString))
                        {
                            DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodToString) ?
                                                DateTime.ParseExact(input.FullPeriodToString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                               : DateTime.UtcNow;


                            query = query.Where(at => at.CreationTime <= _reservationDate);
                        }


                        if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                        {

                            if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                            {
                                query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                            }
                            else
                                throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                        }




                        //var lang = await SettingManager.GetSettingValueForUserAsync("Abp.Localization.DefaultLanguageName", new UserIdentifier(AbpSession.TenantId, AbpSession.UserId.Value));
                        //if (lang == null)
                        //    lang = new_lang.ToString();

                        var transactions = await query.OrderByDescending(x => x.CreationTime)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount).ToListAsync();

                        var courseBookingRequestsList = ObjectMapper.Map<List<FuelTransOutDto>>(transactions);

                        var cBList = courseBookingRequestsList.ToList();

                        if (cBList != null && cBList.Count > 0)
                        {
                            var excelData = ObjectMapper.Map<List<RequestFuelExcelCompanyDto>>(cBList.Select(m => new RequestFuelExcelCompanyDto
                            {
                                Code = m.Code,
                                InvoiceCode = m.CompanyInvoiceCode,
                                Price = m.Price,
                                Branch = m.Branch != null ? m.Branch.Name : "",
                                FuelType = m.FuelType == FuelType._91 ? "91" : m.FuelType == FuelType._95 ? "95" : L("Pages.Providers.Diezel"),
                                Quantity = m.Quantity,
                                Veichle = m.Veichle != null ? m.MappedFullPlateNumber : "",
                                CreationTime = m.CreationTime.ToString(),
                            }));


                            decimal _litresCount = excelData.Sum(a => a.Quantity);
                            decimal _priceAmount = excelData.Sum(a => a.Price);

                            excelData.Add(new RequestFuelExcelCompanyDto
                            {
                                Code = L("Pages.Report.TotalPrice"),
                                Quantity = _litresCount,
                                Price = _priceAmount
                            });

                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();


                            List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();
                            if (input.CompanyId.HasValue)
                            {

                                var _company = await _companyRepository.FirstOrDefaultAsync(input.CompanyId.Value);


                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "نوع الملف",
                                    Value = L("Pages.AdminNav.Transactions")
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = "تاريخ الإصدار",
                                    Value = DateTime.Now.ToString()
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Key = L("Pages.Companies.ModalTitle"),
                                    Value = currentCulture.Name.Contains("ar")? _company.NameAr : _company.NameEn
                                });

                                _options = new RequestFuelExcelOptionsDto
                                {
                                    ExcelDate = DateTime.Now.ToString(),
                                    ProviderName = _company.NameAr,
                                    ExcelType = L("Pages.StationTransactions.Title"),
                                    KeyValues = _keyValues
                                };
                            }

                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            //dataSet.Tables[0].TableName = L("Pages.StationTransactions.FuelTransactions.Header");
                            //dataSet.Tables[1].TableName = L("Pages.StationTransactions.FuelTransactions");
                            ExcelSource source = ExcelSource.FuelTransactions;
                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.StationTransactions.FuelTransactions") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                            // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);

                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task<GetBranchConsumptionReportOutput> GetBranchConsumptionReport(GetFuelTransInsInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    var query = Repository.GetAll()
                     .Include(a => a.Branch.Company)
                     .Include(a => a.Branch.User)
                     .Where(a => a.Completed == true);

                    query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId);
                    query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                    query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
                    query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                    query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.InvoiceCode), at => at.InvoiceCode.Contains(input.InvoiceCode));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.CompanyName), at => at.Branch.Company.NameAr.Contains(input.CompanyName) || at.Branch.Company.NameEn.Contains(input.CompanyName) || at.Branch.Company.Code.Contains(input.CompanyName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName) || at.Branch.Code.Contains(input.BranchName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.DriverName), at => at.Driver.Name.Contains(input.DriverName) || at.Driver.Code.Contains(input.DriverName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at =>
                    at.Veichle.NameAr.Contains(input.VeichleName) ||
                    at.Veichle.NameEn.Contains(input.VeichleName) ||
                    at.Veichle.FullPlateNumber.Contains(input.VeichleName) ||
                    at.Veichle.FullPlateNumberAr.Contains(input.VeichleName) ||
                    at.Driver.Code.Contains(input.VeichleName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.Provider.MainProvider.NameAr.Contains(input.MainProviderName) || at.Provider.MainProvider.NameEn.Contains(input.MainProviderName) || at.Provider.MainProvider.Code.Contains(input.MainProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName) || at.Provider.Code.Contains(input.ProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.WorkerName), at => at.Worker.Name.Contains(input.WorkerName) || at.Worker.Code.Contains(input.WorkerName));
                    if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                    {
                        DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodFromString) ?
                                            DateTime.ParseExact(input.FullPeriodFromString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                           : DateTime.UtcNow;

                        query = query.Where(at => at.CreationTime >= _reservationDate);

                    }

                    if (!string.IsNullOrEmpty(input.FullPeriodToString))
                    {
                        DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodToString) ?
                                            DateTime.ParseExact(input.FullPeriodToString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                           : DateTime.UtcNow;


                        query = query.Where(at => at.CreationTime <= _reservationDate);
                    }


                    if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }

                    if (input.IsCompanyEmployee.HasValue && input.IsCompanyEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }



                    int filterCount = query.Count();


                    if (input.MaxCount == true)
                    {
                        input.SkipCount = 0;
                        input.MaxResultCount = filterCount;
                    }


                    decimal totalPrice = 0;
                    decimal totalQuantity = 0;
                    decimal totalBranchBalance = 0;


                    if (filterCount > 0)
                    {
                        totalPrice = await query.SumAsync(a => a.Price);
                        totalQuantity = await query.SumAsync(a => a.Quantity);
                    }

                    CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

                    var groupedList = await query.GroupBy(a => new { a.BranchId, a.FuelType, a.Branch.NameAr , a.Branch.NameEn }).Select(at => new BranchConsumption
                    {
                        BranchId = at.Key.BranchId.Value,
                        TransFuelType = at.Key.FuelType.Value,
                        BranchName = currentCulture.Name.Contains("ar")? at.Key.NameAr : at.Key.NameEn,
                        BranchFuelBalance = at.ToList().FirstOrDefault().Branch.FuelAmount,
                        BranchStatus = at.ToList().FirstOrDefault().Branch.User.IsActive,
                        BranchIsDeleted = at.ToList().FirstOrDefault().Branch.User.IsDeleted,
                        TotalPrice = at.ToList().Sum(a => a.Price),
                        TotalQuantity = at.ToList().Sum(a => a.Quantity),
                    }).ToListAsync();

                    var groupedByBranchList = await query.GroupBy(a => new { a.BranchId }).Select(at => new BranchConsumption
                    {
                        BranchId = at.Key.BranchId.Value,
                        BranchFuelBalance = at.ToList().FirstOrDefault().Branch.FuelAmount,
                    }).ToListAsync();

                    totalBranchBalance = groupedByBranchList.Sum(a => a.BranchFuelBalance);

                    int count = groupedList.Count();

                    var transactions = groupedList.OrderByDescending(x => x.BranchName)
                        .Skip(input.SkipCount)
                        .Take(input.MaxResultCount).ToList();

                    return new GetBranchConsumptionReportOutput
                    {
                        Transactions = ObjectMapper.Map<List<BranchConsumption>>(transactions),
                        AllCount = count,
                        FilterCount = filterCount,
                        TotalQuantity = totalQuantity,
                        TotalPrice = totalPrice,
                        TotalBranchBalance = totalBranchBalance,
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [AbpAuthorize]
        public async Task<string> ExportBranchConsumption(RequestFuelExcelDtoInput input)
        {
            try
            {

                try
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {
                        CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;


                        var transactions = await GetBranchConsumptionReport(new GetFuelTransInsInput
                        {
                            BranchId = input.BranchId,
                            IsCompanyEmployee = input.IsCompanyEmployee,
                            BranchesIds = input.BranchesIds,
                            CompanyId = input.CompanyId,
                            FuelType = input.FuelType,
                            FullPeriodFromString = input.FullPeriodFromString,
                            FullPeriodToString = input.FullPeriodToString,
                            MaxCount = true
                        });

                        if (transactions.Transactions != null && transactions.Transactions.Count > 0)
                        {

                            var excelData = ObjectMapper.Map<List<BranchConsumptionExcel>>(transactions.Transactions.Select(m => new BranchConsumptionExcel
                            {
                                BranchName = m.BranchName,
                                TransFuelType = m.TransFuelType == FuelType._91 ? "91" : m.TransFuelType == FuelType._95 ? "95" :L("Pages.Providers.Diezel"),
                                TotalQuantity = m.TotalQuantity,
                                TotalPrice = m.TotalPrice,
                                BranchFuelBalance = m.BranchFuelBalance,
                                BranchStatus = m.BranchIsDeleted == true ? L("Common.Deleted") : m.BranchStatus == true ? L("Common.Active") : L("Common.NotActive")
                            }));


                            decimal _litresCount = transactions.TotalQuantity;
                            decimal _priceAmount = transactions.TotalPrice;
                            decimal _branchBalance = transactions.TotalBranchBalance;


                            excelData.Add(new BranchConsumptionExcel
                            {
                                BranchName = L("Pages.Report.TotalPrice"),
                                TotalQuantity = _litresCount,
                                TotalPrice = _priceAmount,
                                BranchFuelBalance = _branchBalance,
                            });

                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();


                            if (input.CompanyId.HasValue)
                            {

                                List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();
                                var _company = await _companyRepository.FirstOrDefaultAsync(input.CompanyId.Value);


                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = L("Pages.Reports.BranchesConsumption")
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = DateTime.Now.ToString()
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = currentCulture.Name.Contains("ar") ? _company.NameAr : _company.NameEn
                                });

                                _options = new RequestFuelExcelOptionsDto
                                {
                                    ExcelDate = DateTime.Now.ToString(),
                                    ProviderName = currentCulture.Name.Contains("ar") ? _company.NameAr : _company.NameEn,
                                    ExcelType = L("Pages.Reports.BranchesConsumption"),
                                    KeyValues = _keyValues
                                };
                            }

                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                            {
                                RequestFuelExcelOptionsDto _dateFrom = new RequestFuelExcelOptionsDto();

                                List<RequestFuelExcelOptionKeyValue> _dateFromkeyValues = new List<RequestFuelExcelOptionKeyValue>();


                                _dateFromkeyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = input.FullPeriodFromString
                                });

                                _dateFromkeyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = L("Pages.Invoices.PeriodFrom")
                                });
                                _dateFrom = new RequestFuelExcelOptionsDto
                                {
                                    KeyValues = _dateFromkeyValues
                                };
                                optionsList.Add(_dateFrom);
                            }

                            if (!string.IsNullOrEmpty(input.FullPeriodToString))
                            {
                                RequestFuelExcelOptionsDto _dateTo = new RequestFuelExcelOptionsDto();

                                List<RequestFuelExcelOptionKeyValue> _dateTokeyValues = new List<RequestFuelExcelOptionKeyValue>();


                                _dateTokeyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = input.FullPeriodToString
                                });

                                _dateTokeyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = L("Pages.Invoices.PeriodTo")
                                });
                                _dateTo = new RequestFuelExcelOptionsDto
                                {
                                    KeyValues = _dateTokeyValues
                                };
                                optionsList.Add(_dateTo);
                            }


                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            //dataSet.Tables[0].TableName = L("Pages.StationTransactions.FuelTransactions.Header");
                            //dataSet.Tables[1].TableName = L("Pages.StationTransactions.FuelTransactions");
                            ExcelSource source = ExcelSource.BranchConsumption;
                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.Reports.BranchesConsumption") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                            // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);

                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<bool> UpdateFuelandQuantityAsync(UpdateFuelAndQuantityDto input)
        {
            var user=await _userManager.GetUserByIdAsync(input.Id);

            if (user == null) return false;

            var userRole = await _userManager.GetRolesAsync(user);

            if (userRole != null && userRole.Contains("Admin"))
            {

                var fuelTransOut = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);
                if (string.IsNullOrEmpty(fuelTransOut.InvoiceCode) && string.IsNullOrEmpty(fuelTransOut.CompanyInvoiceCode))
                {
                    ObjectMapper.Map(input, fuelTransOut);
                    await Repository.UpdateAsync(fuelTransOut);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                return false;
            }
        }


        public async Task<GetProviderConsumptionReportOutput> GetProviderConsumptionReport(GetFuelTransInsInput input)
        {
            try
            {
                using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.SoftDelete))
                {
                    var query = Repository.GetAll()
                     .Include(a => a.Provider.MainProvider)
                     .Include(a => a.Provider.User)
                     .Where(a => a.Completed == true);

                    query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
                    query = query.WhereIf(input.MainProviderId.HasValue, at => at.Provider.MainProviderId == input.MainProviderId);
                    query = query.WhereIf(input.DriverId.HasValue, at => at.DriverId == input.DriverId);
                    query = query.WhereIf(input.WorkerId.HasValue, at => at.WorkerId == input.WorkerId);
                    query = query.WhereIf(input.VeichleId.HasValue, at => at.VeichleId == input.VeichleId);
                    query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.InvoiceCode), at => at.InvoiceCode.Contains(input.InvoiceCode));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.CompanyName), at => at.Branch.Company.NameAr.Contains(input.CompanyName) || at.Branch.Company.NameEn.Contains(input.CompanyName) || at.Branch.Company.Code.Contains(input.CompanyName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName) || at.Branch.Code.Contains(input.BranchName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.DriverName), at => at.Driver.Name.Contains(input.DriverName) || at.Driver.Code.Contains(input.DriverName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.VeichleName), at =>
                    at.Veichle.NameAr.Contains(input.VeichleName) ||
                    at.Veichle.NameEn.Contains(input.VeichleName) ||
                    at.Veichle.FullPlateNumber.Contains(input.VeichleName) ||
                    at.Veichle.FullPlateNumberAr.Contains(input.VeichleName) ||
                    at.Driver.Code.Contains(input.VeichleName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.Provider.MainProvider.NameAr.Contains(input.MainProviderName) || at.Provider.MainProvider.NameEn.Contains(input.MainProviderName) || at.Provider.MainProvider.Code.Contains(input.MainProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName) || at.Provider.Code.Contains(input.ProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.WorkerName), at => at.Worker.Name.Contains(input.WorkerName) || at.Worker.Code.Contains(input.WorkerName));
                    if (!string.IsNullOrEmpty(input.FullPeriodFromString))
                    {
                        DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodFromString) ?
                                            DateTime.ParseExact(input.FullPeriodFromString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                           : DateTime.UtcNow;

                        query = query.Where(at => at.CreationTime >= _reservationDate);

                    }

                    if (!string.IsNullOrEmpty(input.FullPeriodToString))
                    {
                        DateTime _reservationDate = !string.IsNullOrEmpty(input.FullPeriodToString) ?
                                            DateTime.ParseExact(input.FullPeriodToString, "MM/dd/yyyy HH:mm", new CultureInfo("en-US").DateTimeFormat, DateTimeStyles.AllowWhiteSpaces)
                                           : DateTime.UtcNow;


                        query = query.Where(at => at.CreationTime <= _reservationDate);
                    }


                    if (input.IsProviderEmployee.HasValue && input.IsProviderEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }

                    if (input.IsCompanyEmployee.HasValue && input.IsCompanyEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }



                    int filterCount = query.Count();


                    if (input.MaxCount == true)
                    {
                        input.SkipCount = 0;
                        input.MaxResultCount = filterCount;
                    }


                    decimal totalPrice = 0;
                    decimal totalQuantity = 0;
                    decimal totalBranchBalance = 0;


                    if (filterCount > 0)
                    {
                        totalPrice = await query.SumAsync(a => a.Price);
                        totalQuantity = await query.SumAsync(a => a.Quantity);
                    }

                    CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

                    var groupedList = await query.GroupBy(a => new { a.ProviderId, a.FuelType, a.Provider.NameAr, a.Provider.NameEn ,a.FuelPrice }).Select(at => new ProviderConsumption
                    {
                        ProviderId = at.Key.ProviderId.Value,
                        TransFuelType = at.Key.FuelType.Value,
                        ProviderName = currentCulture.Name.Contains("ar") ? at.Key.NameAr : at.Key.NameEn,
                        ProviderFuelBalance = at.ToList().FirstOrDefault().Branch.FuelAmount,
                        ProviderStatus = at.ToList().FirstOrDefault().Provider.User.IsActive,
                        ProviderIsDeleted = at.ToList().FirstOrDefault().Provider.User.IsDeleted,
                        TotalPrice = at.ToList().Sum(a => a.Price),
                        TotalQuantity = at.ToList().Sum(a => a.Quantity),
                        FuelPrice = at.Key.FuelPrice
                    }).ToListAsync();

                    //var groupedByBranchList = await query.GroupBy(a => new { a.ProviderId }).Select(at => new ProviderConsumption
                    //{
                    //    ProviderId = at.Key.ProviderId.Value,
                    //    ProviderFuelBalance = at.ToList().FirstOrDefault().Provider.FuelAmount,
                    //}).ToListAsync();

                    //totalBranchBalance = groupedByBranchList.Sum(a => a.ProviderFuelBalance);

                    int count = groupedList.Count();

                    var transactions = groupedList.OrderByDescending(x => x.ProviderName)
                        .Skip(input.SkipCount)
                        .Take(input.MaxResultCount).ToList();

                    return new GetProviderConsumptionReportOutput
                    {
                        Transactions = ObjectMapper.Map<List<ProviderConsumption>>(transactions),
                        AllCount = count,
                        FilterCount = filterCount,
                        TotalQuantity = totalQuantity,
                        TotalPrice = totalPrice,
                        //TotalProviderBalance = totalBranchBalance,
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [AbpAuthorize]
        public async Task<string> ExportProviderConsumption(RequestFuelExcelDtoInput input)
        {
            try
            {

                try
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
                    {
                        CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;


                        var transactions = await GetProviderConsumptionReport(new GetFuelTransInsInput
                        {
                            ProviderId = input.ProviderId,
                            IsProviderEmployee = false,
                            BranchesIds = input.BranchesIds,
                            MainProviderId = input.MainProviderId,
                            FuelType = input.FuelType,
                            FullPeriodFromString = input.FullPeriodFromString,
                            FullPeriodToString = input.FullPeriodToString,
                            MaxCount = true
                        });

                        if (transactions.Transactions != null && transactions.Transactions.Count > 0)
                        {
                            var excelData = ObjectMapper.Map<List<ProviderConsumptionExcel>>(transactions.Transactions.Select(m => new ProviderConsumptionExcel
                            {
                                ProviderName = m.ProviderName,
                                TransFuelType = m.TransFuelType == FuelType._91 ? "91" : m.TransFuelType == FuelType._95 ? "95" : L("Pages.Providers.Diezel"),
                                FuelPrice = m.FuelPrice.ToString("N2"),
                                TotalQuantity = m.TotalQuantity.ToString("N2"),
                                TotalPrice = m.TotalPrice.ToString("N2"),
                                BranchStatus = m.ProviderIsDeleted == true ? L("Common.Deleted") : m.ProviderStatus == true ? L("Common.Active") : L("Common.NotActive")
                            }));

                            decimal _litresCount = transactions.TotalQuantity;
                            decimal _priceAmount = transactions.TotalPrice;


                            excelData.Add(new ProviderConsumptionExcel
                            {
                                ProviderName = L("Pages.Invoices.Total"),
                                TotalQuantity = Math.Round(_litresCount, 2, MidpointRounding.AwayFromZero).ToString("F2"),
                                TotalPrice = Math.Round(_priceAmount, 2, MidpointRounding.AwayFromZero).ToString("F2"),
                            });

                            RequestFuelExcelOptionsDto _options = new RequestFuelExcelOptionsDto();


                            if (input.MainProviderId.HasValue)
                            {

                                List<RequestFuelExcelOptionKeyValue> _keyValues = new List<RequestFuelExcelOptionKeyValue>();
                                var _company = await _mainProviderRepository.FirstOrDefaultAsync(input.MainProviderId.Value);


                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = L("Pages.Reports.ProvidersConsumption")
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = DateTime.Now.ToString()
                                });

                                _keyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = currentCulture.Name.Contains("ar") ? _company.NameAr : _company.NameEn
                                });

                                _options = new RequestFuelExcelOptionsDto
                                {
                                    ExcelDate = DateTime.Now.ToString(),
                                    ProviderName = currentCulture.Name.Contains("ar") ? _company.NameAr : _company.NameEn,
                                    ExcelType = L("Pages.Reports.ProvidersConsumption"),
                                    KeyValues = _keyValues
                                };
                            }

                            List<RequestFuelExcelOptionsDto> optionsList = new List<RequestFuelExcelOptionsDto>();
                            optionsList.Add(_options);

                            //if (!string.IsNullOrEmpty(input.FullPeriodFromString) && !string.IsNullOrEmpty(input.FullPeriodToString))
                            //{
                            //    RequestFuelExcelOptionsDto _dateFrom = new RequestFuelExcelOptionsDto();

                            //    List<RequestFuelExcelOptionKeyValue> _dateFromkeyValues = new List<RequestFuelExcelOptionKeyValue>();

                            //    _dateFromkeyValues.Add(new RequestFuelExcelOptionKeyValue
                            //    {
                            //        Value = input.FullPeriodFromString
                            //    });

                            //    _dateFromkeyValues.Add(new RequestFuelExcelOptionKeyValue
                            //    {
                            //        Value = L("Pages.Invoices.PeriodFrom")
                            //    });
                            //    _dateFrom = new RequestFuelExcelOptionsDto
                            //    {
                            //        KeyValues = _dateFromkeyValues
                            //    };
                            //    optionsList.Add(_dateFrom);
                            //}

                            //if (!string.IsNullOrEmpty(input.FullPeriodToString))
                            //{
                            //    RequestFuelExcelOptionsDto _dateTo = new RequestFuelExcelOptionsDto();

                            //    List<RequestFuelExcelOptionKeyValue> _dateTokeyValues = new List<RequestFuelExcelOptionKeyValue>();


                            //    _dateTokeyValues.Add(new RequestFuelExcelOptionKeyValue
                            //    {
                            //        Value = input.FullPeriodToString
                            //    });

                            //    _dateTokeyValues.Add(new RequestFuelExcelOptionKeyValue
                            //    {
                            //        Value = L("Pages.Invoices.PeriodTo")
                            //    });
                            //    _dateTo = new RequestFuelExcelOptionsDto
                            //    {
                            //        KeyValues = _dateTokeyValues
                            //    };
                            //    optionsList.Add(_dateTo);
                            //}

                            if (!string.IsNullOrEmpty(input.FullPeriodFromString) && !string.IsNullOrEmpty(input.FullPeriodToString))
                            {
                                RequestFuelExcelOptionsDto _datePeriod = new RequestFuelExcelOptionsDto();

                                List<RequestFuelExcelOptionKeyValue> _datePeriodKeyValues = new List<RequestFuelExcelOptionKeyValue>();
                               
                                if (currentCulture.Name.Contains("ar") == false)
                                {
                                    _datePeriodKeyValues.Add(new RequestFuelExcelOptionKeyValue
                                    {
                                        Value = L("Pages.Invoices.FullPeriod")
                                    });
                                }

                                if (currentCulture.Name.Contains("ar"))
                                {
                                    _datePeriodKeyValues.Add(new RequestFuelExcelOptionKeyValue
                                    {
                                        Value = L("Pages.Invoices.FullPeriod")
                                    });
                                }

                                _datePeriodKeyValues.Add(new RequestFuelExcelOptionKeyValue
                                {
                                    Value = currentCulture.Name.Contains("ar")? $"من : {input.FullPeriodFromString} إلى : {input.FullPeriodToString}" : $"From : {input.FullPeriodFromString} - To :{input.FullPeriodToString}"
                                });
                                

                                _datePeriod = new RequestFuelExcelOptionsDto
                                {
                                    KeyValues = _datePeriodKeyValues
                                };


                               

                                optionsList.Add(_datePeriod);
                            }

                            var dataSet = Utilities.ToDataTableForExcelV2(excelData, input.DisplayedColumns, LocalizationManager, optionsList);
                            if (dataSet == null)
                                return string.Empty;

                            //dataSet.Tables[0].TableName = L("Pages.StationTransactions.FuelTransactions.Header");
                            //dataSet.Tables[1].TableName = L("Pages.StationTransactions.FuelTransactions");
                            ExcelSource source = ExcelSource.BranchConsumption;
                            return Utilities.CreateExcelEpPlusV2(dataSet, @"Files\\Excel\\", L("Pages.Reports.ProvidersConsumption") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle, source);

                            // return Utilities.SaveExcel(dt, @"Files\\Excel\\", L("Pages.Request.Title") + "_" + Abp.Timing.Clock.Now.ToString("yyyyMMddHHmmss"), input.ExcelTitle);

                        }
                        else
                            throw new UserFriendlyException(L("Common.EmptyListToBeExported"));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

}