using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.BackgroundJobs;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNet.Identity;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Drivers.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.BackgroundJobs;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Subscriptions.Dto;
using Sayarah.Application.Users;
using Sayarah.Application.Veichles.Dto;
using Sayarah.Authorization;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.Chips;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using Sayarah.Drivers;
using Sayarah.Lookups;
using Sayarah.Packages;
using Sayarah.Veichles;
using System.Globalization;
using static Sayarah.SayarahConsts;
using System.Linq.Dynamic.Core;
using Sayarah.Application.Users.Dto;

namespace Sayarah.Application.Veichles
{
    public class VeichleAppService : AsyncCrudAppService<Veichle, VeichleDto, long, GetVeichlesInput, CreateVeichleDto, UpdateVeichleDto>, IVeichleAppService
    {

        private readonly IUserAppService _userService;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly ICommonAppService _commonService;

        private readonly IRepository<Driver, long> _driverRepository;
        private readonly IRepository<VeichlePic, long> _veichlePicRepository;
        private readonly IRepository<VeichleTrip, long> _veichleTripRepository;
        private readonly IRepository<DriverVeichle, long> _driverVeichleRepository;
        private readonly IRepository<ChipNumber, long> _chipNumberRepository;
        private readonly IRepository<FuelGroup, long> _fuelGroupRepository;
        private readonly IRepository<Brand, long> _brandRepository;
        private readonly IRepository<Model, long> _modelRepository;
        private readonly IRepository<Subscription, long> _subscriptionRepository;
        private readonly IRepository<Branch, long> _branchRepository;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly RoleManager _roleManager;

        public VeichleAppService(
            IRepository<Veichle, long> repository,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
             ICommonAppService commonService,
             IRepository<Driver, long> driverRepository,
             IRepository<VeichlePic, long> veichlePicRepository,
             AbpNotificationHelper abpNotificationHelper,
             IRepository<DriverVeichle, long> driverVeichleRepository,
             IRepository<ChipNumber, long> chipNumberRepository,
             IRepository<VeichleTrip, long> veichleTripRepository,
             IRepository<Brand, long> brandRepository,
             IRepository<Model, long> modelRepository,
             IBackgroundJobManager backgroundJobManager,
             IRepository<FuelGroup, long> fuelGroupRepository,
             IRepository<Branch, long> branchRepository,
             IRepository<Subscription, long> subscriptionRepository,
             RoleManager roleManager
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userService = userService;
            _userManager = userManager;
            _userRepository = userRepository;
            _commonService = commonService;
            _driverRepository = driverRepository;
            _veichlePicRepository = veichlePicRepository;
            _veichleTripRepository = veichleTripRepository;
            _driverVeichleRepository = driverVeichleRepository;
            _chipNumberRepository = chipNumberRepository;
            _abpNotificationHelper = abpNotificationHelper;
            _fuelGroupRepository = fuelGroupRepository;
            _brandRepository = brandRepository;
            _modelRepository = modelRepository;
            _backgroundJobManager = backgroundJobManager;
            _branchRepository = branchRepository;
            _subscriptionRepository = subscriptionRepository;
            _roleManager = roleManager;
        }

        public override async Task<VeichleDto> GetAsync(EntityDto<long> input)
        {
            var Veichle = await Repository.GetAll()
                .Include(x => x.Branch.Company)
                .Include(x => x.Brand)
                .Include(x => x.Model)
                .Include(x => x.FuelGroup)
                .Include(x => x.Driver.User)
                .Include(x => x.VeichleTrips)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(Veichle);
        }



        public async Task<GetVeichleBySimOutput> GetVeichle(GetVeichlesInput input)
        {

            var Veichle = await Repository.GetAll()
           .Include(x => x.Branch.Company)
           .Include(x => x.Brand)
           .Include(x => x.Model)
           .Include(x => x.FuelGroup)
           .Include(x => x.Driver)
           .Include(x => x.VeichleTrips)
           .FirstOrDefaultAsync(x => x.Id == input.Id);


            var _mappedVeichle = ObjectMapper.Map<VeichleDto>(Veichle);

            decimal veichleBalanceInLitre = 0;
            decimal veichleBalanceInSar = 0;

            // check usage type 
            if (_mappedVeichle.ConsumptionType == ConsumptionType.Group && _mappedVeichle.FuelGroup != null)
            {

                if (_mappedVeichle.FuelGroup.GroupType == GroupType.Litre)
                {
                    veichleBalanceInLitre = _mappedVeichle.Fuel_Balance;
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
                            veichleBalanceInLitre = _mappedVeichle.FuelLitreBalance;
                    }
                }

            }


            return new GetVeichleBySimOutput
            {
                Veichle = _mappedVeichle,
                VeichleBalanceInLitre = veichleBalanceInLitre,
                VeichleBalanceInSar = veichleBalanceInSar,
                GroupType = _mappedVeichle.FuelGroup != null ? _mappedVeichle.FuelGroup.GroupType : GroupType.None,
            };
        }



        public async Task<GetVeichleBySimOutput> GetBySim(GetVeichlesInput input)
        {

            // get sim details 
            var chipNumber = await _chipNumberRepository.GetAll()
                .Include(x => x.Veichle.Branch.Company)
                .Include(x => x.Veichle.Brand)
                .Include(x => x.Veichle.Model)
                .Include(x => x.Veichle.FuelGroup)
                .Include(x => x.Veichle.VeichleTrips)
                .FirstOrDefaultAsync(a => a.Code == input.SimNumber);

            if (chipNumber != null)
            {

                if (chipNumber.Veichle != null)
                {
                    var _mappedVeichle = ObjectMapper.Map<VeichleDto>(chipNumber.Veichle);
                    decimal veichleBalanceInLitre = 0;
                    decimal veichleBalanceInSar = 0;
                    // check usage type 
                    if (_mappedVeichle.ConsumptionType == ConsumptionType.Group && _mappedVeichle.FuelGroup != null)
                    {
                        if (_mappedVeichle.FuelGroup.GroupType == GroupType.Litre)
                        {
                            veichleBalanceInLitre = _mappedVeichle.Fuel_Balance;
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
                                    veichleBalanceInLitre = _mappedVeichle.FuelLitreBalance;
                            }
                        }
                    }


                    return new GetVeichleBySimOutput
                    {
                        Veichle = _mappedVeichle,
                        VeichleBalanceInLitre = veichleBalanceInLitre,
                        VeichleBalanceInSar = veichleBalanceInSar,
                        GroupType = _mappedVeichle.FuelGroup != null ? _mappedVeichle.FuelGroup.GroupType : GroupType.None,
                        PeriodConsumptionType = _mappedVeichle.FuelGroup != null ? _mappedVeichle.FuelGroup.PeriodConsumptionType : PeriodConsumptionType.Money,
                        MaximumRechargeAmount = _mappedVeichle.FuelGroup != null ? _mappedVeichle.FuelGroup.MaximumRechargeAmount : 0,
                        CounterPicIsRequired = _mappedVeichle.Branch != null && _mappedVeichle.Branch.Company != null ? _mappedVeichle.Branch.Company.CounterPicIsRequired : false,
                        MaximumRechargeAmountForOnce = _mappedVeichle.FuelGroup != null ? _mappedVeichle.FuelGroup.MaximumRechargeAmountForOnce : 0,
                        Success = true,
                    };
                }
                else return new GetVeichleBySimOutput { NotFoundVeichle = true };

            }

            return new GetVeichleBySimOutput { NotFoundSim = true, Message = L("Pages.Veichles.Messages.NotRegisteredChipNumber") };
        }


        public async Task<VeichleDto> GetByPlateNum(UpdateVeichleSimPicDto input)
        {
            var Veichle = await Repository.GetAll()
                .Include(x => x.Branch)
                .Include(x => x.Brand)
                        .Include(x => x.Model)
                .Include(x => x.FuelGroup)
                .FirstOrDefaultAsync(x => x.PlateNumber == input.Number);
            return MapToEntityDto(Veichle);
        }




        public async Task<List<ShortVeichleDto>> GetVeichlesListByIds(GetListByIdsInput input)
        {

            if (input.Ids == null || input.Ids.Length == 0) return new List<ShortVeichleDto>();

            var veichles = await Repository.GetAll()
                .Include(x => x.Branch.Company)
                //.Include(x => x.Brand)
                //.Include(x => x.Model)
                //.Include(x => x.FuelGroup)
                //.Include(x => x.Driver.User)
                //.Include(x => x.VeichleTrips)
                .Where(x => input.Ids.Any(a => a == x.Id)).ToListAsync();

            return ObjectMapper.Map<List<ShortVeichleDto>>(veichles);
        }


        [AbpAuthorize]
        public override async Task<VeichleDto> CreateAsync(CreateVeichleDto input)
        {
            try
            {

                // check company package 
                await GetCurrentSubscription(input.BranchId.Value, 1);

                int count = await Repository.CountAsync(a => a.PlateLetters == input.PlateLetters && a.PlateLettersEn == input.PlateLettersEn && a.PlateNumber == input.PlateNumber && a.PlateNumberEn == input.PlateNumberEn);
                if (count > 0)
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.PlateLettersExists"));


                input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "Veichles", CodeField = "Code" });
                input.FullPlateNumber = input.PlateLettersEn + " " + input.PlateNumberEn;
                input.FullPlateNumberAr = input.PlateLetters + " " + input.PlateNumber;
                var veichle = ObjectMapper.Map<Veichle>(input);
                veichle = await Repository.InsertAsync(veichle);
                await CurrentUnitOfWork.SaveChangesAsync();

                if (input.Trip != null && input.Trip.StartDate.HasValue)
                {
                    if (input.Trip.Id > 0)
                    {
                        // update 

                        var trip = await _veichleTripRepository.FirstOrDefaultAsync(x => x.Id == input.VeichleTripId);
                        ObjectMapper.Map(input.Trip, trip);
                        trip.VeichleId = veichle.Id;
                        trip.BranchId = veichle.BranchId;
                        await _veichleTripRepository.UpdateAsync(trip);
                        await CurrentUnitOfWork.SaveChangesAsync();

                        veichle.VeichleTripId = trip.Id;

                    }
                    else
                    {
                        // create 

                        input.Trip.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "VeichleTrips", CodeField = "Code" });
                        var trip = ObjectMapper.Map<VeichleTrip>(input.Trip);
                        trip.VeichleId = veichle.Id;
                        trip.BranchId = veichle.BranchId;
                        trip = await _veichleTripRepository.InsertAsync(trip);
                        await CurrentUnitOfWork.SaveChangesAsync();

                        veichle.VeichleTripId = trip.Id;
                    }

                    await Repository.UpdateAsync(veichle);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }


                if (input.ConsumptionType == ConsumptionType.Group)
                {
                    // get group
                    var _fuelGroup = await _fuelGroupRepository.FirstOrDefaultAsync(a => a.Id == input.FuelGroupId);
                    if (_fuelGroup != null)
                    {

                        if (_fuelGroup.GroupType == GroupType.Period && veichle.MoneyBalanceEndDate.HasValue)
                        {
                            // create background job for end date 

                            veichle.PeriodScheduleCount = veichle.PeriodScheduleCount + 1;
                            await CreateBackGroundJobs(new EndFuelGroupPeriodScheduleJobArgs
                            {
                                VeichleId = veichle.Id,
                                PeriodScheduleCount = veichle.PeriodScheduleCount,
                                MoneyBalanceEndDate = veichle.MoneyBalanceEndDate.Value
                            });

                            await Repository.UpdateAsync(veichle);
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                }



                if (input.AddDriver == true)
                {
                    input.Driver.BranchId = input.BranchId;
                    input.Driver.User.BranchId = input.BranchId;
                    input.Driver.User.CompanyId = input.CompanyId;
                    input.Driver.CompanyId = input.CompanyId;
                    input.Driver.User.IsActive = true;

                    var driver = await CreateDriver(input.Driver);
                    veichle.DriverId = driver.Id;
                    await Repository.UpdateAsync(veichle);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    // create driver veichle
                    await _driverVeichleRepository.InsertAsync(new DriverVeichle
                    {
                        IsCurrent = true,
                        DriverId = driver.Id,
                        VeichleId = veichle.Id
                    });
                }

                return MapToEntityDto(veichle);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [AbpAuthorize]
        public override async Task<VeichleDto> UpdateAsync(UpdateVeichleDto input)
        {
            try
            {

                int count = await Repository.CountAsync(a => a.Id != input.Id && a.PlateLetters == input.PlateLetters && a.PlateLettersEn == input.PlateLettersEn && a.PlateNumber == input.PlateNumber && a.PlateNumberEn == input.PlateNumberEn);
                if (count > 0)
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.PlateLettersExists"));


                var veichle = await Repository.GetAllIncluding(x => x.Branch).FirstOrDefaultAsync(x => x.Id == input.Id);

                input.FullPlateNumber = input.PlateLettersEn + " " + input.PlateNumberEn;
                input.FullPlateNumberAr = input.PlateLetters + " " + input.PlateNumber;


                if (input.ConsumptionType == ConsumptionType.Group)
                {
                    // check if end date changed or group changed 

                    if (veichle.FuelGroupId != input.FuelGroupId || veichle.MoneyBalanceEndDate != input.MoneyBalanceEndDate)
                    {
                        // get group
                        var _fuelGroup = await _fuelGroupRepository.FirstOrDefaultAsync(a => a.Id == input.FuelGroupId);
                        if (_fuelGroup != null)
                        {

                            if (_fuelGroup.GroupType == GroupType.Period && input.MoneyBalanceEndDate.HasValue)
                            {
                                // create background job for end date 

                                veichle.PeriodScheduleCount = veichle.PeriodScheduleCount + 1;
                                await CreateBackGroundJobs(new EndFuelGroupPeriodScheduleJobArgs
                                {
                                    VeichleId = veichle.Id,
                                    PeriodScheduleCount = veichle.PeriodScheduleCount,
                                    MoneyBalanceEndDate = input.MoneyBalanceEndDate.Value
                                });
                            }
                        }
                    }
                }




                ObjectMapper.Map(input, veichle);
                await Repository.UpdateAsync(veichle);
                await CurrentUnitOfWork.SaveChangesAsync();

                if (input.Trip != null && input.Trip.StartDate.HasValue)
                {
                    if (input.Trip.Id > 0)
                    {
                        // update 

                        var trip = await _veichleTripRepository.FirstOrDefaultAsync(x => x.Id == input.VeichleTripId);
                        ObjectMapper.Map(input.Trip, trip);
                        trip.VeichleId = veichle.Id;
                        trip.BranchId = veichle.BranchId;
                        await _veichleTripRepository.UpdateAsync(trip);
                        await CurrentUnitOfWork.SaveChangesAsync();

                        veichle.VeichleTripId = trip.Id;
                        await Repository.UpdateAsync(veichle);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                    else
                    {
                        // create 

                        input.Trip.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "VeichleTrips", CodeField = "Code" });
                        var trip = ObjectMapper.Map<VeichleTrip>(input.Trip);
                        trip.VeichleId = veichle.Id;
                        trip.BranchId = veichle.BranchId;
                        trip = await _veichleTripRepository.InsertAsync(trip);
                        await CurrentUnitOfWork.SaveChangesAsync();

                        veichle.VeichleTripId = trip.Id;
                        await Repository.UpdateAsync(veichle);
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }

                }

                if (input.AddDriver == true)
                {


                    if (veichle.DriverId.HasValue == true)
                    {
                        input.Driver.BranchId = input.BranchId;
                        input.Driver.User.BranchId = input.BranchId;
                        input.Driver.User.CompanyId = input.CompanyId;

                        // create driver 
                        await UpdateDriver(input.Driver);
                    }

                    else
                    {
                        input.Driver.BranchId = input.BranchId;
                        input.Driver.User.BranchId = input.BranchId;
                        input.Driver.User.CompanyId = input.CompanyId;
                        input.Driver.CompanyId = input.CompanyId;
                        input.Driver.User.IsActive = true;

                        var driver = await CreateDriver(input.Driver);
                        veichle.DriverId = driver.Id;
                        await Repository.UpdateAsync(veichle);
                        await CurrentUnitOfWork.SaveChangesAsync();

                        // create driver veichle
                        await _driverVeichleRepository.InsertAsync(new DriverVeichle
                        {
                            IsCurrent = true,
                            DriverId = driver.Id,
                            VeichleId = veichle.Id
                        });
                    }

                }


                return MapToEntityDto(veichle);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }

        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var veichle = await Repository.GetAsync(input.Id);
            if (veichle == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(veichle);
        }

        [AbpAuthorize]
        public override async Task<PagedResultDto<VeichleDto>> GetAllAsync(GetVeichlesInput input)
        {
            var query = Repository.GetAll()
                .Include(x => x.FuelGroup)
                .Include(x => x.Brand)
                        .Include(x => x.Model)
                .Include(at => at.Branch).AsQueryable();
            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
            query = query.WhereIf(!string.IsNullOrEmpty(input.PlateNumber), at => at.PlateNumber.Contains(input.PlateNumber));
            query = query.WhereIf(!string.IsNullOrEmpty(input.BodyNumber), at => at.BodyNumber.Contains(input.BodyNumber));
            //query = query.WhereIf(!string.IsNullOrEmpty(input.Mark), at => at.Mark.Contains(input.Mark));
            //query = query.WhereIf(!string.IsNullOrEmpty(input.Model), at => at.Model.Contains(input.Model));
            query = query.WhereIf(!string.IsNullOrEmpty(input.SimNumber), at => at.SimNumber.Contains(input.SimNumber));
            query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
            query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
            query = query.WhereIf(input.TankSize.HasValue, at => at.TankSize == input.TankSize);
            query = query.WhereIf(input.FuelAverageUsage.HasValue, at => at.FuelAverageUsage == input.FuelAverageUsage);
            query = query.WhereIf(input.KiloCount.HasValue, at => at.KiloCount == input.KiloCount);
            query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
            query = query.WhereIf(!string.IsNullOrEmpty(input.FullPlateNumber), at => at.FullPlateNumber.Contains(input.FullPlateNumber) || at.FullPlateNumberAr.Contains(input.FullPlateNumber));

            if (input.IsEmployee.HasValue && input.IsEmployee == true)
            {
                if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                {
                    query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                }
                else
                    return new PagedResultDto<VeichleDto>(0, new List<VeichleDto>());
            }



            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var veichles = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<VeichleDto>>(veichles);
            return new PagedResultDto<VeichleDto>(count, _mappedList);
        }




        [AbpAuthorize]
        public async Task<VeichleDto> UpdateVeichleDetails(UpdateVeichleSimPicDto input)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
                var veichle = await Repository
                    .GetAll()
                    .Include(a => a.Branch)
                    .FirstOrDefaultAsync(x => x.FullPlateNumber == input.Number || x.PlateNumber == input.Number || x.PlateNumberEn == input.Number);

                if (veichle == null)
                {
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.NotRegisteredVeichle"));
                }

                var chipNumber = await _chipNumberRepository.FirstOrDefaultAsync(a => a.Code == input.SimNumber);
                if (chipNumber == null)
                {
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.NotRegisteredChipNumber"));
                }

                if (chipNumber.Status == ChipStatus.Archived || chipNumber.Status == ChipStatus.Blocked)
                {
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.NotRegisteredChipNumber"));
                }

                chipNumber.BranchId = veichle.BranchId;
                chipNumber.CompanyId = veichle.Branch.CompanyId;
                chipNumber.VeichleId = veichle.Id;
                chipNumber.Status = ChipStatus.Linked;
                chipNumber.ActivationUserId = AbpSession.UserId;
                chipNumber.ActivationDate = DateTime.Now;

                await _chipNumberRepository.UpdateAsync(chipNumber);
                await UnitOfWorkManager.Current.SaveChangesAsync();


                ObjectMapper.Map(input, veichle);
                await Repository.UpdateAsync(veichle);

                foreach (var item in input.VeichleMedias.ToList())
                {
                    await _veichlePicRepository.InsertAsync(new VeichlePic
                    {
                        VeichleId = veichle.Id,
                        FilePath = item.FilePath,
                    });
                }

                await CurrentUnitOfWork.SaveChangesAsync();


                // notify branch here that veichle data updated 
                if (veichle != null && veichle.Branch != null)
                {

                    List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
                    string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");

                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: veichle.Branch.UserId.Value));

                    // all employees that has permission veichles in this branch
                    var employees = await _userRepository.GetAll().Include(a => a.Permissions)
                        .Where(a => a.IsDeleted == false && a.IsActive == true && a.UserType == UserTypes.Employee && a.BranchId == veichle.BranchId && a.Permissions.Any(aa => aa.Name == PermissionNames.BranchData.BranchVeichles.Write && aa.IsGranted == true)).ToListAsync();

                    if (employees != null)
                    {
                        foreach (var employee in employees)
                        {
                            targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: employee.Id));
                        }
                    }

                    CreateNotificationDto CreateNotificationData = new CreateNotificationDto
                    {
                        SenderUserId = AbpSession.UserId.Value,
                        //Message = L("Pages.Veichles.Messages.VeichleUpdatedNotification", veichle.NameAr, veichle.Code),
                        Message = L("Pages.Veichles.Messages.VeichleUpdatedNotification", currentCulture.Name.Contains("ar") ? veichle.FullPlateNumberAr : veichle.FullPlateNumber, veichle.Code),
                        EntityType = Entity_Type.VeichleUpdated,
                        EntityId = veichle.Id
                    };

                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.VeichleUpdated, CreateNotificationData, targetUsersId.ToArray());


                }

                return MapToEntityDto(veichle);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task CheckVeichleExists(UpdateVeichleSimPicDto input)
        {
            try
            {
                var veichle = await Repository.FirstOrDefaultAsync(x => x.FullPlateNumber == input.Number || x.PlateNumber == input.Number || x.PlateNumberEn == input.Number);
                if (veichle == null)
                {
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.NotRegisteredVeichle"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<DataTableOutputDto<VeichleDto>> GetPaged(GetVeichlesPagedInput input)
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
                            Veichle veichle = await Repository.FirstOrDefaultAsync(id);
                            if (veichle != null)
                            {
                                if (input.action == "Delete")
                                {

                                    //// check before delete 
                                    //int clinicsCount = await _veichleClinicRepository.CountAsync(a => a.VeichleId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.Veichles.HasClinics"));

                                    await Repository.DeleteAsync(veichle);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }

                                else if (input.action == "ManageActive")
                                {
                                    veichle.IsActive = !veichle.IsActive;
                                    await Repository.UpdateAsync(veichle);
                                }

                                else if (input.action == "Restore")
                                {
                                    await GetCurrentSubscription(veichle.BranchId.Value, 1);

                                    veichle.UnDelete();
                                    await Repository.UpdateAsync(veichle);
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
                            Veichle veichle = await Repository.FirstOrDefaultAsync(id);
                            if (veichle != null)
                            {
                                if (input.action == "Delete")
                                {

                                    // check before delete 
                                    //int clinicsCount = await _veichleClinicRepository.CountAsync(a => a.VeichleId == id);
                                    //if (clinicsCount > 0)
                                    //    throw new UserFriendlyException(L("Pages.Veichles.HasClinics"));
                                    await Repository.DeleteAsync(veichle);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }

                                else if (input.action == "ManageActive")
                                {
                                    veichle.IsActive = !veichle.IsActive;
                                    await Repository.UpdateAsync(veichle);
                                }
                                else if (input.action == "Restore")
                                {
                                    await GetCurrentSubscription(veichle.BranchId.Value, 1);

                                    veichle.UnDelete();
                                    await Repository.UpdateAsync(veichle);
                                }

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                    var query = Repository.GetAll()
                        .Include(x => x.FuelGroup)
                        .Include(x => x.Brand)
                        .Include(x => x.Driver)
                        .Include(x => x.Model)
                        .Include(a => a.Branch).AsQueryable();

                    query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
                    query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);

                    if (input.IsEmployee.HasValue && input.IsEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                        }
                        else
                            return new DataTableOutputDto<VeichleDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<VeichleDto>()
                            };
                    }


                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchName), at => at.Branch.NameAr.Contains(input.BranchName) || at.Branch.NameEn.Contains(input.BranchName) || at.Branch.Code.Contains(input.BranchName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BranchCode), at => at.Branch.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BrandName), at => at.Brand.NameAr.Contains(input.BrandName) || at.Brand.NameEn.Contains(input.BrandName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ModelName), at => at.Model.NameAr.Contains(input.ModelName) || at.Model.NameEn.Contains(input.ModelName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.PlateNumber), at => at.PlateNumber.Contains(input.PlateNumber));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.BodyNumber), at => at.BodyNumber.Contains(input.BodyNumber));
                    //query = query.WhereIf(!string.IsNullOrEmpty(input.Mark), at => at.Mark.Contains(input.Mark));
                    //query = query.WhereIf(!string.IsNullOrEmpty(input.Model), at => at.Model.Contains(input.Model));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.SimNumber), at => at.SimNumber.Contains(input.SimNumber));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.FullPlateNumber), at => at.FullPlateNumber.Contains(input.FullPlateNumber) || at.FullPlateNumberAr.Contains(input.FullPlateNumber));
                    query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
                    query = query.WhereIf(input.TankSize.HasValue, at => at.TankSize == input.TankSize);
                    query = query.WhereIf(input.FuelAverageUsage.HasValue, at => at.FuelAverageUsage == input.FuelAverageUsage);
                    query = query.WhereIf(input.KiloCount.HasValue, at => at.KiloCount == input.KiloCount);
                    query = query.WhereIf(input.ConsumptionType.HasValue, at => at.ConsumptionType == input.ConsumptionType);
                    query = query.WhereIf(input.Id.HasValue, at => at.Id == input.Id);
                    query = query.WhereIf(input.VeichleId.HasValue, at => at.Id == input.VeichleId);
                    query = query.WhereIf(input.IsActive.HasValue, at => at.IsActive == input.IsActive);

                    query = query.WhereIf(input.Fuel_BalanceFrom.HasValue, at => at.Fuel_Balance >= input.Fuel_BalanceFrom);
                    query = query.WhereIf(input.Fuel_BalanceTo.HasValue, at => at.Fuel_Balance <= input.Fuel_BalanceTo);

                    query = query.WhereIf(input.Maintain_BalanceFrom.HasValue, at => at.Maintain_Balance >= input.Maintain_BalanceFrom);
                    query = query.WhereIf(input.Maintain_BalanceTo.HasValue, at => at.Maintain_Balance <= input.Maintain_BalanceTo);

                    query = query.WhereIf(input.Oil_BalanceFrom.HasValue, at => at.Oil_Balance >= input.Oil_BalanceFrom);
                    query = query.WhereIf(input.Oil_BalanceTo.HasValue, at => at.Oil_Balance <= input.Oil_BalanceTo);

                    query = query.WhereIf(input.Wash_BalanceFrom.HasValue, at => at.Wash_Balance >= input.Wash_BalanceFrom);
                    query = query.WhereIf(input.Wash_BalanceTo.HasValue, at => at.Wash_Balance <= input.Wash_BalanceTo);

                    int filteredCount = await query.CountAsync();
                    var veichles = await query.Include(x => x.Branch)
                        .Include(x => x.CreatorUser)
                        .Include(x => x.LastModifierUser)
                        .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start).Take(input.length)
                        .ToListAsync();

                    var _mappedList = ObjectMapper.Map<List<VeichleDto>>(veichles);

                    foreach (var item in _mappedList)
                    {
                        item.DriversCount = await _driverVeichleRepository.CountAsync(a => a.VeichleId == item.Id && a.Driver.IsDeleted == false);
                    }


                    return new DataTableOutputDto<VeichleDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = _mappedList
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [AbpAuthorize]
        public async Task<UpdateDriverCodeOutput> ConfirmDriverCode(UpdateDriverCodeInput input)
        {
            try
            {
                //var driverVeichle = await _driverVeichleRepository.GetAll()
                //    .Include(x => x.Driver)
                //    .FirstOrDefaultAsync(x => x.VeichleId == input.VeichleId && x.IsCurrent == true);


                var driver = await _driverRepository
                   .FirstOrDefaultAsync(x => x.Code == input.Code);


                if (driver != null)
                {
                    var viechle = await UpdateVeichleByDriverCode(new UpdateVeichleSimPicDto
                    {
                        DriverId = driver.Id,
                        SimNumber = input.SimNumber
                    });



                    return new UpdateDriverCodeOutput
                    {
                        Success = true,
                        Message = L("Pages.Veichles.Messages.CodeIsCorrect"),
                        Veichle = ObjectMapper.Map<ApiVeichleDto>(viechle)
                    };
                }
                else
                {
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.CodeNotCorrect"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<VeichleDto> UpdateVeichleByDriverCode(UpdateVeichleSimPicDto input)
        {
            try
            {
                CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

                var veichle = await Repository
                    .GetAll()
                    .Include(a => a.Branch)
                    .FirstOrDefaultAsync(x => x.DriverId == input.DriverId);


                if (veichle == null)
                {
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.DriverDonnotHaveCar"));
                }

                var currentVeichleChip = await _chipNumberRepository.FirstOrDefaultAsync(a => a.VeichleId == veichle.Id);
                if (currentVeichleChip != null)
                {
                    if (currentVeichleChip.Code == input.SimNumber)
                        return MapToEntityDto(veichle);
                    else
                        throw new UserFriendlyException(L("Pages.Veichles.Messages.CarRegisteredWithAnotherChip"));
                }

                var chipNumber = await _chipNumberRepository.FirstOrDefaultAsync(a => a.Code == input.SimNumber);
                if (chipNumber == null)
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.NotRegisteredChipNumber"));

                if (chipNumber.Status == ChipStatus.Archived || chipNumber.Status == ChipStatus.Blocked)
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.NotRegisteredChipNumber"));

                if (chipNumber.VeichleId.HasValue)
                    throw new UserFriendlyException(L("Pages.Veichles.Messages.ChipNumberAlreadyLinked"));


                if (veichle != null)
                {
                    if (chipNumber.CompanyId.HasValue && veichle.Branch.CompanyId != chipNumber.CompanyId)
                    {
                        throw new UserFriendlyException(L("Pages.ChipsNumbers.ItsRelatedToOtherCompany"));
                    }
                    if (chipNumber.BranchId.HasValue && veichle.BranchId != chipNumber.BranchId)
                    {
                        throw new UserFriendlyException(L("Pages.ChipsNumbers.ItsRelatedToOtherBranch"));
                    }
                }

                chipNumber.BranchId = veichle.BranchId;
                chipNumber.CompanyId = veichle.Branch.CompanyId;
                chipNumber.VeichleId = veichle.Id;
                chipNumber.Status = ChipStatus.Linked;
                chipNumber.ActivationUserId = AbpSession.UserId;
                chipNumber.ActivationDate = DateTime.Now;

                await _chipNumberRepository.UpdateAsync(chipNumber);

                ObjectMapper.Map(input, veichle);

                veichle.SimNumber = input.SimNumber;
                await Repository.UpdateAsync(veichle);

                await CurrentUnitOfWork.SaveChangesAsync();


                // notify branch here that veichle data updated 
                if (veichle != null && veichle.Branch != null)
                {

                    List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
                    string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");

                    targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: veichle.Branch.UserId.Value));

                    // all employees that has permission veichles in this branch
                    var employees = await _userRepository.GetAll().Include(a => a.Permissions)
                        .Where(a => a.IsDeleted == false && a.IsActive == true && a.UserType == UserTypes.Employee && a.BranchId == veichle.BranchId && a.Permissions.Any(aa => aa.Name == PermissionNames.BranchData.BranchVeichles.Write && aa.IsGranted == true)).ToListAsync();

                    if (employees != null)
                    {
                        foreach (var employee in employees)
                        {
                            targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: employee.Id));
                        }
                    }

                    CreateNotificationDto CreateNotificationData = new CreateNotificationDto
                    {
                        SenderUserId = AbpSession.UserId.Value,
                        Message = L("Pages.Veichles.Messages.VeichleUpdatedNotification", currentCulture.Name.Contains("ar") ? veichle.FullPlateNumberAr : veichle.FullPlateNumber, veichle.Code),
                        EntityType = Entity_Type.VeichleUpdated,
                        EntityId = veichle.Id
                    };

                    //Publish Notification Data
                    await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.VeichleUpdated, CreateNotificationData, targetUsersId.ToArray());


                }

                return MapToEntityDto(veichle);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        public async Task CreateBackGroundJobs(EndFuelGroupPeriodScheduleJobArgs input)
        {
            var currentTime = Abp.Timing.Clock.Now;
            var resDateTime = input.MoneyBalanceEndDate;
            var remainingMinutes = resDateTime.Subtract(Abp.Timing.Clock.Now).TotalMinutes;

            await _backgroundJobManager.EnqueueAsync<EndFuelGroupPeriodScheduleJob, EndFuelGroupPeriodScheduleJobArgs>(input,
                BackgroundJobPriority.High, TimeSpan.FromMinutes(remainingMinutes));

        }



        public async Task<bool> HandleEndFuelGroupPeriod(EndFuelGroupPeriodScheduleJobArgs input)
        {
            try
            {
                var veichle = await Repository.GetAll()
                    .Include(x => x.FuelGroup)
                    .FirstOrDefaultAsync(x => x.Id == input.VeichleId);

                if (veichle != null && veichle.PeriodScheduleCount == input.PeriodScheduleCount && veichle.ConsumptionType == ConsumptionType.Group)
                {

                    if (veichle.FuelGroup != null && veichle.FuelGroup.GroupType == GroupType.Period)
                    {

                        if (veichle.FuelGroup.PeriodConsumptionType == PeriodConsumptionType.Money)
                        {
                            if (veichle.FuelGroup.Transferable == true)
                                veichle.MoneyBalance += veichle.FuelGroup.Amount;
                            else
                                veichle.MoneyBalance = veichle.FuelGroup.Amount;
                        }
                        else if (veichle.FuelGroup.PeriodConsumptionType == PeriodConsumptionType.Litre)
                        {
                            if (veichle.FuelGroup.Transferable == true)
                                veichle.FuelLitreBalance += veichle.FuelGroup.LitersCount;
                            else
                                veichle.FuelLitreBalance = veichle.FuelGroup.LitersCount;
                        }


                        if (veichle.FuelGroup.PeriodType == PeriodType.Daily)
                            veichle.MoneyBalanceEndDate = DateTime.UtcNow.AddDays(1);
                        else if (veichle.FuelGroup.PeriodType == PeriodType.Weekly)
                            veichle.MoneyBalanceEndDate = DateTime.UtcNow.AddDays(7);
                        else if (veichle.FuelGroup.PeriodType == PeriodType.Monthly)
                            veichle.MoneyBalanceEndDate = DateTime.UtcNow.AddDays(30);

                        else
                        {
                            veichle.MoneyBalance = 0;
                            veichle.FuelLitreBalance = 0;
                            veichle.MoneyBalanceEndDate = null;
                            await Repository.UpdateAsync(veichle);
                            await UnitOfWorkManager.Current.SaveChangesAsync();

                            return true;
                        }

                        veichle.PeriodScheduleCount = veichle.PeriodScheduleCount + 1;
                        await Repository.UpdateAsync(veichle);
                        await UnitOfWorkManager.Current.SaveChangesAsync();

                        await CreateBackGroundJobs(new EndFuelGroupPeriodScheduleJobArgs
                        {
                            VeichleId = input.VeichleId,
                            PeriodScheduleCount = veichle.PeriodScheduleCount,
                            MoneyBalanceEndDate = veichle.MoneyBalanceEndDate.Value
                        });
                    }
                    else
                    {
                        veichle.MoneyBalance = 0;
                        veichle.FuelLitreBalance = 0;
                        veichle.MoneyBalanceEndDate = null;
                        await Repository.UpdateAsync(veichle);
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [AbpAuthorize]
        public async Task<PagedResultDto<VeichleNumbersDto>> GetAllVeichlesNumbers(GetVeichlesInput input)
        {
            var query = Repository.GetAll();
            query = query.WhereIf(!string.IsNullOrEmpty(input.Name), at => at.NameAr.Contains(input.Name) || at.NameEn.Contains(input.Name));
            query = query.WhereIf(!string.IsNullOrEmpty(input.PlateNumber), at => at.PlateNumber.Contains(input.PlateNumber));
            query = query.WhereIf(!string.IsNullOrEmpty(input.BodyNumber), at => at.BodyNumber.Contains(input.BodyNumber));
            query = query.WhereIf(!string.IsNullOrEmpty(input.SimNumber), at => at.SimNumber.Contains(input.SimNumber));
            query = query.WhereIf(!string.IsNullOrEmpty(input.FullPlateNumber), at => at.FullPlateNumber.Contains(input.FullPlateNumber) || at.FullPlateNumberAr.Contains(input.FullPlateNumber));
            query = query.WhereIf(input.BranchId.HasValue, at => at.BranchId == input.BranchId);
            query = query.WhereIf(input.CompanyId.HasValue, at => at.Branch.CompanyId == input.CompanyId);
            query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
            query = query.WhereIf(input.TankSize.HasValue, at => at.TankSize == input.TankSize);
            query = query.WhereIf(input.FuelAverageUsage.HasValue, at => at.FuelAverageUsage == input.FuelAverageUsage);
            query = query.WhereIf(input.KiloCount.HasValue, at => at.KiloCount == input.KiloCount);


            if (input.IsEmployee.HasValue && input.IsEmployee == true)
            {
                if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                {
                    query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.BranchId));
                }
                else
                    return new PagedResultDto<VeichleNumbersDto>(0, new List<VeichleNumbersDto>());
            }



            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var veichles = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<VeichleNumbersDto>>(veichles);
            return new PagedResultDto<VeichleNumbersDto>(count, _mappedList);
        }




        // driver 

        [AbpAuthorize]
        public async Task<DriverDto> CreateDriver(UpdateDriverDto input)
        {
            try
            {
                var user = new UserDto();
                if (input.User != null)
                {
                    input.User.Name = input.Name;
                    input.User.Surname = input.Name;
                    input.User.PhoneNumber = input.PhoneNumber;
                    input.User.UserName = string.IsNullOrEmpty(input.User.UserName) ? input.User.EmailAddress : input.User.UserName;
                    input.User.UserType = UserTypes.Driver;
                    input.User.Avatar = input.Avatar;

                    input.User.EmailAddress = input.EmailAddress;
                    input.User.RoleName = RolesNames.Driver;
                    user = await _userService.CreateNewUser(input.User);
                }
                if (user.Id > 0)
                {
                    input.UserId = user.Id;
                    var driver = ObjectMapper.Map<Driver>(input);
                    driver.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "Drivers", CodeField = "Code" });
                    driver = await _driverRepository.InsertAsync(driver);

                    await CurrentUnitOfWork.SaveChangesAsync();


                    // update user with comapny id 
                    var _user = await _userRepository.FirstOrDefaultAsync(user.Id);
                    _user.BranchId = driver.BranchId;
                    _user.CompanyId = input.CompanyId;
                    await _userRepository.UpdateAsync(_user);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    return ObjectMapper.Map<DriverDto>(driver);

                }
                else
                {
                    throw new UserFriendlyException(L("Pages.Drivers.Error.CantCreateUser"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<DriverDto> UpdateDriver(UpdateDriverDto input)
        {

            var driver = await _driverRepository.GetAllIncluding(x => x.User).FirstOrDefaultAsync(x => x.Id == input.Id);

            if (!string.IsNullOrEmpty(driver.Avatar) && (string.IsNullOrEmpty(input.Avatar) || !driver.Avatar.Equals(input.Avatar)))
                Utilities.DeleteImage(5, driver.Avatar, new string[] { "600x600_" });

            if (!string.IsNullOrEmpty(driver.Licence) && (string.IsNullOrEmpty(input.Licence) || !driver.Licence.Equals(input.Licence)))
                Utilities.DeleteImage(5, driver.Licence, new string[] { "600x600_" });


            if (input.User != null)
            {
                driver.User.Name = input.Name;
                input.User.Name = input.Name;

                driver.User.UserName = string.IsNullOrEmpty(input.User.UserName) ? input.User.EmailAddress : input.User.UserName;
                driver.User.Surname = input.Name;
                input.User.Surname = input.Name;

                driver.User.PhoneNumber = input.PhoneNumber;
                input.User.PhoneNumber = input.PhoneNumber;

                driver.User.Avatar = input.Avatar;
                input.User.Avatar = input.Avatar;

                driver.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                                       driver.User.Password : _userManager.PasswordHasher.HashPassword(driver.User, input.User.Password);

                input.User.Password = string.IsNullOrEmpty(input.User.Password) || input.User.Password.Equals("***************") ?
                                   driver.User.Password : _userManager.PasswordHasher.HashPassword(driver.User, input.User.Password);



                driver.User.EmailAddress = string.IsNullOrEmpty(input.EmailAddress) ? input.User.EmailAddress : input.EmailAddress;


                if (!string.IsNullOrEmpty(driver.User.UserName))
                {
                    var existUserName = await _userRepository.FirstOrDefaultAsync(x => x.UserName == driver.User.UserName && x.Id != driver.UserId && !string.IsNullOrEmpty(x.UserName) && x.IsDeleted == false);
                    if (existUserName != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistUserName"));
                }


                if (!string.IsNullOrEmpty(driver.User.EmailAddress))
                {
                    var existEmail = await _userRepository.FirstOrDefaultAsync(x => x.EmailAddress == driver.User.EmailAddress && x.Id != driver.UserId && !string.IsNullOrEmpty(x.EmailAddress) && x.IsDeleted == false);
                    if (existEmail != null)
                        throw new UserFriendlyException(L("Pages.Users.Error.AlreadyExistEmail"));
                }


                await _userManager.UpdateAsync(driver.User);

            }
            ObjectMapper.Map(input, driver);
            await _driverRepository.UpdateAsync(driver);
            return ObjectMapper.Map<DriverDto>(driver);
        }

        public string RemoveSpaces(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Remove all spaces first
            text = text.Replace(" ", "");

            // Return the updated string
            return text;
        }

        [AbpAuthorize]
        public async Task<List<ImportExcelDataOutput>> ImportExcel(ImportExcelDataInput input)
        {
            try
            {
                List<ImportExcelDataOutput> resultlst = new List<ImportExcelDataOutput>();
                var brands = await _brandRepository.GetAll().ToListAsync();
                var models = await _modelRepository.GetAll().ToListAsync();

                var role = await _roleManager.GetRoleByNameAsync(RolesNames.Driver);

                await GetCurrentSubscription(input.BranchId, input.lst.Count);

                foreach (var item in input.lst)
                {
                    // map veichleType
                    item.BranchId = input.BranchId;
                    item.CompanyId = input.CompanyId;

                    #region _veichleType    
                    VeichleType _veichleType = VeichleType.Bus;
                    if (item.VeichleType == "حافلة" || item.VeichleType == "حافله")
                        _veichleType = VeichleType.Bus;
                    else if (item.VeichleType == "خصوصي" || item.VeichleType == "خصوصى")
                        _veichleType = VeichleType.Private;
                    else if (item.VeichleType == "نقل")
                        _veichleType = VeichleType.Transport;
                    else if (item.VeichleType == "أجرة" || item.VeichleType == "تاكسي")
                        _veichleType = VeichleType.Taxi;
                    else if (item.VeichleType == "شاحنة" || item.VeichleType == "شاحنه")
                        _veichleType = VeichleType.Truck;
                    else if (item.VeichleType == "فان")
                        _veichleType = VeichleType.Van;
                    else if (item.VeichleType == "دراجة بخارية" || item.VeichleType == "دراجة")
                        _veichleType = VeichleType.Motorcycle;
                    else if (item.VeichleType == "معدات ثقيلة" || item.VeichleType == "معدات ثقيله")
                        _veichleType = VeichleType.HeavyEquipment;
                    else if (item.VeichleType == "نقل خاص")
                        _veichleType = VeichleType.PrivateTransfer;
                    else if (item.VeichleType == "دبلوماسي" || item.VeichleType == "دبلوماسى")
                        _veichleType = VeichleType.Diplomat;
                    #endregion

                    #region _fuelType    
                    FuelType _fuelType = FuelType._91;
                    if (item.FuelType == "91")
                        _fuelType = FuelType._91;
                    else if (item.FuelType == "95")
                        _fuelType = FuelType._95;
                    else if (item.FuelType == "ديزل")
                        _fuelType = FuelType.diesel;
                    else
                    {
                        resultlst.Add(new ImportExcelDataOutput
                        {
                            Veichle = item,
                            Message = L("Pages.Veichles.Messages.FuelTypeError")
                        });
                        continue;
                    }
                    #endregion

                    //#region _consumptionType

                    //ConsumptionType _consumptionType = ConsumptionType.Trip;
                    //if (item.ConsumptionType == "رحلة" || item.ConsumptionType == "رحله")
                    //    _consumptionType = ConsumptionType.Trip;
                    //else if (item.ConsumptionType.Contains("مجموع"))
                    //    _consumptionType = ConsumptionType.Group;
                    //else
                    //{
                    //    item.Message = L("Pages.Veichles.Messages.FuelTypeError");
                    //    resultlst.Add(item);
                    //    continue;
                    //}

                    //#endregion

                    #region brand and model

                    // select brand
                    var brand = brands.FirstOrDefault(a => a.NameAr == item.Brand || a.NameEn == item.Brand);
                    if (brand != null)
                        item.BrandId = brand.Id;

                    var model = models.FirstOrDefault(a => a.NameAr == item.Model || a.NameEn == item.Model);
                    if (model != null)
                        item.ModelId = model.Id;
                    #endregion

                    #region Plate

                    if (string.IsNullOrEmpty(item.PlateLetters) || string.IsNullOrEmpty(item.PlateLettersEn) || string.IsNullOrEmpty(item.PlateNumber) || string.IsNullOrEmpty(item.PlateNumberEn))
                    {

                        resultlst.Add(new ImportExcelDataOutput
                        {
                            Veichle = item,
                            Message = L("Pages.Veichles.Messages.PlateLettersError")
                        });
                        continue;
                    }

                    // check exist 

                    // remove spaces from veichles 
                    item.PlateLetters = RemoveSpaces(item.PlateLetters);
                    item.PlateLettersEn = RemoveSpaces(item.PlateLettersEn);
                    item.PlateNumber = RemoveSpaces(item.PlateNumber);
                    item.PlateNumberEn = RemoveSpaces(item.PlateNumberEn);

                    item.FullPlateNumber = item.PlateLettersEn + " " + item.PlateNumberEn;
                    item.FullPlateNumberAr = item.PlateLetters + " " + item.PlateNumber;

                    int count = await Repository.CountAsync(a => a.PlateLetters == item.PlateLetters && a.PlateLettersEn == item.PlateLettersEn && a.PlateNumber == item.PlateNumber && a.PlateNumberEn == item.PlateNumberEn);
                    if (count > 0)
                    {
                        resultlst.Add(new ImportExcelDataOutput
                        {
                            Veichle = item,
                            Message = L("Pages.Veichles.Messages.PlateLettersExists")
                        });
                        continue;
                    }
                    #endregion

                    if (!string.IsNullOrEmpty(item.WorkingDays))
                    {
                        item.WorkingDays = RemoveSpaces(item.WorkingDays);

                        var days = item.WorkingDays.Split(',');
                        if (days.Length == 7)
                            item.WorkingDays += ",7";

                        item.WorkingDays = item.WorkingDays.Replace(',', ';');
                    }

                    string code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "Veichles", CodeField = "Code" });
                    Veichle _veichle = new Veichle
                    {
                        BranchId = item.BranchId,
                        Code = code,
                        PlateLetters = item.PlateLetters,
                        PlateLettersEn = item.PlateLettersEn,
                        PlateNumber = item.PlateNumber,
                        PlateNumberEn = item.PlateNumberEn,
                        FullPlateNumber = item.FullPlateNumber,
                        FullPlateNumberAr = item.FullPlateNumberAr,
                        BrandId = item.BrandId,
                        ModelId = item.ModelId,
                        BodyNumber = item.BodyNumber,
                        FuelAverageUsage = item.FuelAverageUsage,
                        InternalNumber = item.InternalNumber,
                        IsActive = true,
                        TankSize = item.TankSize,
                        YearOfIndustry = item.YearOfIndustry,
                        VeichleType = _veichleType,
                        KiloCount = item.KiloCount,
                        FuelType = _fuelType,
                        //WorkingDays = "0;1;2;3;4;5;6;7",
                        WorkingDays = item.WorkingDays
                    };

                    _veichle = await Repository.InsertAsync(_veichle);
                    await CurrentUnitOfWork.SaveChangesAsync();


                    if (!string.IsNullOrEmpty(item.DriverName))
                    {
                        Driver driver = new Driver
                        {
                            BranchId = item.BranchId,
                            EmailAddress = item.DriverEmailAddress,
                            PhoneNumber = item.DriverPhoneNumber,
                            Name = item.DriverName
                        };

                        var _createNewUser = new CreateNewUserInput
                        {
                            BranchId = input.BranchId,
                            CompanyId = input.CompanyId,
                            IsActive = true,
                            EmailAddress = item.DriverEmailAddress,
                            Password = item.DriverPassword,
                            Name = item.DriverName,
                            Surname = item.DriverName,
                            UserName = item.DriverUserName,
                            RoleName = RolesNames.Driver,
                            UserType = UserTypes.Driver,
                            PhoneNumber = item.DriverPhoneNumber,
                        };

                        var _createDriverFromExcel = await CreateDriverFromExcel(new CreateDriverFromExcelInput
                        {
                            Driver = driver,
                            Veichle = _veichle,
                            User = _createNewUser,
                            RoleId = role.Id
                        });

                        if (_createDriverFromExcel.Success == false)
                        {
                            resultlst.Add(new ImportExcelDataOutput
                            {
                                Veichle = item,
                                Message = _createDriverFromExcel.Message
                            });
                            continue;
                        }
                    }
                }
                return resultlst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        [AbpAuthorize]
        public async Task<CreateDriverFromExcelOutput> CreateDriverFromExcel(CreateDriverFromExcelInput input)
        {
            try
            {

                if (!string.IsNullOrEmpty(input.User.UserName))
                {
                    var existUser = await _userRepository.FirstOrDefaultAsync(x => x.UserName == input.User.UserName && x.IsDeleted == false);
                    if (existUser != null)
                    {
                        return new CreateDriverFromExcelOutput { Message = L("Pages.Users.Error.AlreadyExistUserName") };
                    }
                }

                Random random = new Random();
                string random_text = LocalizationSourceName + random.Next(100000000).ToString();

                if (!string.IsNullOrEmpty(input.User.EmailAddress))
                {
                    var existEmail = await _userRepository.FirstOrDefaultAsync(x => x.EmailAddress == input.User.EmailAddress && x.IsDeleted == false);
                    if (existEmail != null)
                    {
                        return new CreateDriverFromExcelOutput { Message = L("Pages.Users.Error.AlreadyExistEmail") };
                    }
                }
                else
                {
                    input.User.EmailAddress = random_text + "@Sayarah.com";
                }

                if (!string.IsNullOrEmpty(input.User.PhoneNumber))
                {
                    var existUserPhone = await _userRepository.FirstOrDefaultAsync(x => x.PhoneNumber == input.User.PhoneNumber && !string.IsNullOrEmpty(x.PhoneNumber) && x.IsDeleted == false);
                    if (existUserPhone != null)
                    {
                        return new CreateDriverFromExcelOutput { Message = L("Pages.Users.Error.AlreadyExistPhone") };
                    }
                }
                else
                {
                    input.User.PhoneNumber = random_text;
                }
                input.User.UserName = string.IsNullOrEmpty(input.User.UserName) ? random_text : input.User.UserName;
                input.User.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "AbpUsers", CodeField = "Code" });
                var user = ObjectMapper.Map<User>(input.User);

                if (string.IsNullOrEmpty(input.User.Name))
                    user.Name = random_text;
                if (string.IsNullOrEmpty(input.User.Surname))
                    user.Surname = input.User.Name;

                //
                user.Password = new PasswordHasher().HashPassword(input.User.Password);
                user.IsEmailConfirmed = true;


                //Assign roles
                if (!string.IsNullOrEmpty(input.User.RoleName))
                {
                    user.Roles = new List<UserRole>();
                    if (input.RoleId > 0)
                        user.Roles.Add(new UserRole(AbpSession.TenantId, user.Id, input.RoleId));
                }
                await _userManager.CreateAsync(user);
                await CurrentUnitOfWork.SaveChangesAsync();

                input.Driver.UserId = user.Id;
                input.Driver.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "Drivers", CodeField = "Code" });
                input.Driver = await _driverRepository.InsertAsync(input.Driver);
                await CurrentUnitOfWork.SaveChangesAsync();

                input.Veichle.DriverId = input.Driver.Id;
                await Repository.UpdateAsync(input.Veichle);
                await CurrentUnitOfWork.SaveChangesAsync();

                // create driver veichle
                await _driverVeichleRepository.InsertAsync(new DriverVeichle
                {
                    IsCurrent = true,
                    DriverId = input.Driver.Id,
                    VeichleId = input.Veichle.Id
                });

                return new CreateDriverFromExcelOutput { Success = true };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public async Task GetCurrentSubscription(long branchId, int newCount)
        {
            try
            {
                // get current user 
                var branch = await _branchRepository.GetAll()
                    .Include(a => a.Company).FirstOrDefaultAsync(a => a.Id == branchId);
                if (branch == null || branch.Company == null)
                    throw new UserFriendlyException(L("Pages.Packages.CompanyNoSubscribe"));

                long companyUserId = branch.Company.UserId.Value;

                var query = _subscriptionRepository.GetAll()
               .Include(a => a.Company)
               .Include(x => x.Package)
               .Where(at => at.Company.UserId == companyUserId && at.Status == DepositStatus.Accepted && at.IsExpired == false);

                var subscriptions = await query.OrderByDescending(a => a.CreationTime).ToListAsync();
                var subscription = subscriptions.FirstOrDefault();


                var mappedSubscription = ObjectMapper.Map<SubscriptionDto>(subscription);

                if (mappedSubscription != null)
                {
                    var veichlesCount = await Repository.CountAsync(a => a.Branch.CompanyId == branch.CompanyId && a.IsDeleted == false);
                    if (veichlesCount + newCount > mappedSubscription.VeichlesCount)
                    {
                        throw new UserFriendlyException(L("Pages.Packages.VeichlesCountExceeded"));
                    }
                }
                else
                    throw new UserFriendlyException(L("Pages.Packages.CompanyNoSubscribe"));

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }





        //[AbpAuthorize]
        public async Task CreateBackGroundJobs(CreateVeichleDto input)
        {
            try
            {

                // get veichles that (MoneyBalanceEndDate = null)
                var veichles = await Repository.GetAll()
                    .Include(a => a.FuelGroup)
                    .Where(a => a.ConsumptionType == ConsumptionType.Group && a.FuelGroupId.HasValue == true && a.FuelGroup.GroupType == GroupType.Period && a.MoneyBalanceEndDate == null && a.IsDeleted == false)
                    .ToListAsync();


                if (veichles != null)
                {

                    DateTime _endDate;

                    foreach (var veichle in veichles)
                    {
                        // update end date 
                        if (veichle.FuelGroup.PeriodType == PeriodType.Daily)
                            _endDate = new DateTime(2025, 1, 14);
                        else if (veichle.FuelGroup.PeriodType == PeriodType.Weekly)
                            _endDate = new DateTime(2025, 1, 21);
                        else if (veichle.FuelGroup.PeriodType == PeriodType.Monthly)
                            _endDate = new DateTime(2025, 1, 31);
                        else
                            _endDate = new DateTime(2025, 1, 31);
                        var currentTime = Abp.Timing.Clock.Now;
                        var resDateTime = _endDate;
                        var remainingMinutes = resDateTime.Subtract(Abp.Timing.Clock.Now).TotalMinutes;

                        veichle.MoneyBalanceEndDate = _endDate;
                        veichle.PeriodScheduleCount = veichle.PeriodScheduleCount + 1;
                        await Repository.UpdateAsync(veichle);

                        await _backgroundJobManager.EnqueueAsync<EndFuelGroupPeriodScheduleJob, EndFuelGroupPeriodScheduleJobArgs>(
                            new EndFuelGroupPeriodScheduleJobArgs
                            {
                                VeichleId = veichle.Id,
                                PeriodScheduleCount = veichle.PeriodScheduleCount,
                                MoneyBalanceEndDate = veichle.MoneyBalanceEndDate.Value
                            },
                    BackgroundJobPriority.High, TimeSpan.FromMinutes(remainingMinutes));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}