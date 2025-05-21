using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Application.Providers.Dto;
using Sayarah.Application.Users;
using Sayarah.Authorization.Users;
using Sayarah.Core.Helpers;
using Sayarah.Providers;
using System.Globalization;
using static Sayarah.SayarahConsts;
using System.Linq.Dynamic.Core;
using Sayarah.Application.DataTables.Dto;


namespace Sayarah.Application.Providers
{
    public class FuelPriceChangeRequestAppService : AsyncCrudAppService<FuelPriceChangeRequest, FuelPriceChangeRequestDto, long, GetFuelPriceChangeRequestsInput, CreateFuelPriceChangeRequestDto, UpdateFuelPriceChangeRequestDto>, IFuelPriceChangeRequestAppService
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly AbpNotificationHelper _abpNotificationHelper;
        public ICommonAppService _commonAppService { get; set; }

        public FuelPriceChangeRequestAppService(
            IRepository<FuelPriceChangeRequest, long> repository,
            IUserAppService userService,
            UserManager userManager,
            IRepository<User, long> userRepository,
            IRepository<Provider, long> providerRepository,
            AbpNotificationHelper abpNotificationHelper,
            ICommonAppService commonAppService
            )
            : base(repository)
        {
            LocalizationSourceName = SayarahConsts.LocalizationSourceName;
            _userRepository = userRepository;
            _providerRepository = providerRepository;
            _abpNotificationHelper = abpNotificationHelper;
            _commonAppService = commonAppService;
        }

        public override async Task<FuelPriceChangeRequestDto> GetAsync(EntityDto<long> input)
        {
            var FuelPriceChangeRequest = await Repository.GetAll()
                .Include(x => x.Provider.MainProvider)
                .Include(x => x.CreatorUser)
                .FirstOrDefaultAsync(x => x.Id == input.Id);
            return MapToEntityDto(FuelPriceChangeRequest);
        }

        [AbpAuthorize]
        public override async Task<FuelPriceChangeRequestDto> CreateAsync(CreateFuelPriceChangeRequestDto input)
        {
            try
            {

                input.Code = await _commonAppService.GetNextCode(new GetNextCodeInputDto { TableName = "FuelPriceChangeRequests", CodeField = "Code" });

                var fuelPriceChangeRequest = ObjectMapper.Map<FuelPriceChangeRequest>(input);
                fuelPriceChangeRequest = await Repository.InsertAsync(fuelPriceChangeRequest);
                await CurrentUnitOfWork.SaveChangesAsync();

                // send to admin 
                await SendNotificationToEmployees(new UpdateProviderFuelPrice
                {
                    RequestId = fuelPriceChangeRequest.Id,
                    FuelType = fuelPriceChangeRequest.FuelType,
                    NewPrice = fuelPriceChangeRequest.NewPrice,
                    ProviderId = fuelPriceChangeRequest.ProviderId
                });

                return MapToEntityDto(fuelPriceChangeRequest);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [AbpAuthorize]
        public async Task<bool> SendNotificationToEmployees(UpdateProviderFuelPrice input)
        {
            try
            {

                var provider = await _providerRepository.FirstOrDefaultAsync(input.ProviderId.Value);
                if (provider == null)
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
                            SenderUserName = provider.NameAr,
                            EntityType = Entity_Type.NewFuelPriceChangeRequest,
                            EntityId = input.RequestId,
                            Message = L("Pages.FuelPriceChangeRequests.Messages.New")
                        };
                        //Publish Notification Data
                        await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.NewFuelPriceChangeRequest, _createNotificationData, targetAdminUsersId.ToArray());

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
        public override async Task<FuelPriceChangeRequestDto> UpdateAsync(UpdateFuelPriceChangeRequestDto input)
        {

            var fuelPriceChangeRequest = await Repository.GetAllIncluding(x => x.Provider.MainProvider)
                .FirstOrDefaultAsync(x => x.Id == input.Id);

            if (!string.IsNullOrEmpty(fuelPriceChangeRequest.FilePath) && (string.IsNullOrEmpty(input.FilePath) || !fuelPriceChangeRequest.FilePath.Equals(input.FilePath)))
                Utilities.DeleteImage(10, fuelPriceChangeRequest.FilePath, new string[] { });

            ObjectMapper.Map(input, fuelPriceChangeRequest);
            await Repository.UpdateAsync(fuelPriceChangeRequest);
            return MapToEntityDto(fuelPriceChangeRequest);
        }

        [AbpAuthorize]
        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var fuelPriceChangeRequest = await Repository.GetAsync(input.Id);
            if (fuelPriceChangeRequest == null)
                throw new UserFriendlyException("Common.Message.ElementNotFound");
            await Repository.DeleteAsync(fuelPriceChangeRequest);
        }

        public override async Task<PagedResultDto<FuelPriceChangeRequestDto>> GetAllAsync(GetFuelPriceChangeRequestsInput input)
        {
            var query = Repository.GetAll()
                .Include(x => x.CreatorUser)
                .Include(at => at.Provider.MainProvider).AsQueryable();

            query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
            query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName) || at.Provider.Code.Contains(input.ProviderName));
            query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
            query = query.WhereIf(input.MainProviderId.HasValue, a => a.Provider.MainProviderId == input.MainProviderId);
            query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);


            int count = query.Count();
            if (input.MaxCount == true)
            {
                input.SkipCount = 0;
                input.MaxResultCount = query.Count();
            }

            var fuelPriceChangeRequests = await query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
            var _mappedList = ObjectMapper.Map<List<FuelPriceChangeRequestDto>>(fuelPriceChangeRequests);
            return new PagedResultDto<FuelPriceChangeRequestDto>(count, _mappedList);
        }


        [AbpAuthorize]
        public async Task<bool> UpdateProviderFuelPrice(UpdateProviderFuelPrice input)
        {

            var provider = await _providerRepository.FirstOrDefaultAsync(input.ProviderId.Value);
            if (provider == null)
                return false;


            if (input.FuelType == FuelType._91)
                provider.FuelNinetyOnePrice = input.NewPrice;

            if (input.FuelType == FuelType._95)
                provider.FuelNinetyFivePrice = input.NewPrice;

            if (input.FuelType == FuelType.diesel)
                provider.SolarPrice = input.NewPrice;

            await _providerRepository.UpdateAsync(provider);
            await UnitOfWorkManager.Current.SaveChangesAsync();

            // send notification to provider here 

            List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
            string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");

            targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: provider.UserId.Value));

            CreateNotificationDto CreateNotificationData = new CreateNotificationDto
            {
                SenderUserId = AbpSession.UserId.Value,
                Message = L("Pages.FuelPriceChangeRequests.Messages.Approved"),
                EntityType = Entity_Type.AcceptFuelPriceChangeRequest,
                EntityId = input.RequestId
            };
            //Publish Notification Data
            await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.AcceptFuelPriceChangeRequest, CreateNotificationData, targetUsersId.ToArray());

            return true;
        }



        [AbpAuthorize]
        public async Task<bool> SendRefuseNotification(UpdateProviderFuelPrice input)
        {

            var provider = await _providerRepository.FirstOrDefaultAsync(input.ProviderId.Value);
            if (provider == null)
                return false;

            // send notification to provider here 

            List<UserIdentifier> targetUsersId = new List<UserIdentifier>();
            string lang = await SettingManager.GetSettingValueAsync("Abp.Localization.DefaultLanguageName");

            targetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: provider.UserId.Value));

            CreateNotificationDto CreateNotificationData = new CreateNotificationDto
            {
                SenderUserId = AbpSession.UserId.Value,
                Message = L("Pages.FuelPriceChangeRequests.Messages.Refused"),
                EntityType = Entity_Type.RefuseFuelPriceChangeRequest,
                EntityId = input.RequestId
            };
            //Publish Notification Data
            await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.RefuseFuelPriceChangeRequest, CreateNotificationData, targetUsersId.ToArray());

            return true;
        }


        [AbpAuthorize]
        public async Task<DataTableOutputDto<FuelPriceChangeRequestDto>> GetPaged(GetFuelPriceChangeRequestsPagedInput input)
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
                            FuelPriceChangeRequest fuelPriceChangeRequest = await Repository.GetAllIncluding(a => a.Provider.MainProvider).FirstOrDefaultAsync(a => a.Id == id);
                            if (fuelPriceChangeRequest != null)
                            {
                                if (input.action == "Delete")
                                {


                                    await Repository.DeleteAsync(fuelPriceChangeRequest);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }

                                else if (input.action == "Accept")
                                {
                                    fuelPriceChangeRequest.Status = ChangeRequestStatus.Accepted;

                                    // get provider and update price then send notification 
                                    await UpdateProviderFuelPrice(new UpdateProviderFuelPrice
                                    {
                                        ProviderId = fuelPriceChangeRequest.ProviderId,
                                        FuelType = fuelPriceChangeRequest.FuelType,
                                        NewPrice = fuelPriceChangeRequest.NewPrice,
                                        RequestId = fuelPriceChangeRequest.Id
                                    });


                                }
                                else if (input.action == "Refuse")
                                {
                                    fuelPriceChangeRequest.Status = ChangeRequestStatus.Refused;

                                    // send refuse notification 

                                    // get provider and update price then send notification 
                                    await SendRefuseNotification(new UpdateProviderFuelPrice
                                    {
                                        ProviderId = fuelPriceChangeRequest.ProviderId,
                                        FuelType = fuelPriceChangeRequest.FuelType,
                                        NewPrice = fuelPriceChangeRequest.NewPrice,
                                        RequestId = fuelPriceChangeRequest.Id
                                    });


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
                            FuelPriceChangeRequest fuelPriceChangeRequest = await Repository.GetAllIncluding(a => a.Provider.MainProvider).FirstOrDefaultAsync(a => a.Id == id);
                            if (fuelPriceChangeRequest != null)
                            {
                                if (input.action == "Delete")
                                {

                                    await Repository.DeleteAsync(fuelPriceChangeRequest);
                                    await UnitOfWorkManager.Current.SaveChangesAsync();

                                }
                                else if (input.action == "Accept")
                                {
                                    fuelPriceChangeRequest.Status = ChangeRequestStatus.Accepted;

                                    // get provider and update price then send notification 
                                    await UpdateProviderFuelPrice(new UpdateProviderFuelPrice
                                    {
                                        ProviderId = fuelPriceChangeRequest.ProviderId,
                                        FuelType = fuelPriceChangeRequest.FuelType,
                                        NewPrice = fuelPriceChangeRequest.NewPrice,
                                        RequestId = fuelPriceChangeRequest.Id
                                    });


                                }
                                else if (input.action == "Refuse")
                                {
                                    fuelPriceChangeRequest.Status = ChangeRequestStatus.Refused;

                                    // get provider and update price then send notification 
                                    await SendRefuseNotification(new UpdateProviderFuelPrice
                                    {
                                        ProviderId = fuelPriceChangeRequest.ProviderId,
                                        FuelType = fuelPriceChangeRequest.FuelType,
                                        NewPrice = fuelPriceChangeRequest.NewPrice,
                                        RequestId = fuelPriceChangeRequest.Id
                                    });

                                }

                            }
                        }
                        await UnitOfWorkManager.Current.SaveChangesAsync();
                    }

                    var query = Repository.GetAll()
                        .Include(x => x.CreatorUser)
                        .Include(a => a.Provider.MainProvider).AsQueryable();


                    query = query.WhereIf(input.ProviderId.HasValue, at => at.ProviderId == input.ProviderId);
                    query = query.WhereIf(input.MainProviderId.HasValue, a => a.Provider.MainProviderId == input.MainProviderId);


                    if (input.IsEmployee.HasValue && input.IsEmployee == true)
                    {

                        if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                        {
                            query = query.WhereIf(input.BranchesIds != null && input.BranchesIds.Count > 0, a => input.BranchesIds.Any(_id => _id == a.ProviderId));
                        }
                        else
                            return new DataTableOutputDto<FuelPriceChangeRequestDto>
                            {
                                iTotalDisplayRecords = 0,
                                iTotalRecords = 0,
                                aaData = new List<FuelPriceChangeRequestDto>()
                            };
                    }



                    int count = await query.CountAsync();
                    query = query.FilterDataTable((DataTableInputDto)input);
                    query = query.WhereIf(!string.IsNullOrEmpty(input.Code), at => at.Code.Contains(input.Code));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.ProviderName), at => at.Provider.NameAr.Contains(input.ProviderName) || at.Provider.NameEn.Contains(input.ProviderName) || at.Provider.Code.Contains(input.ProviderName));
                    query = query.WhereIf(!string.IsNullOrEmpty(input.MainProviderName), at => at.Provider.MainProvider.NameAr.Contains(input.MainProviderName) || at.Provider.MainProvider.NameEn.Contains(input.MainProviderName) || at.Provider.MainProvider.Code.Contains(input.MainProviderName));
                    query = query.WhereIf(input.FuelType.HasValue, at => at.FuelType == input.FuelType);
                    query = query.WhereIf(input.Status.HasValue, at => at.Status == input.Status);
                    query = query.WhereIf(input.Id.HasValue, a => a.Id == input.Id);
                    int filteredCount = await query.CountAsync();

                    var fuelPriceChangeRequests = await query
                        .Include(x => x.Provider.MainProvider)
                        .Include(x => x.CreatorUser)
                        .Include(x => x.LastModifierUser)
                        .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                        .Skip(input.start).Take(input.length)
                        .ToListAsync();


                    return new DataTableOutputDto<FuelPriceChangeRequestDto>
                    {
                        iTotalDisplayRecords = filteredCount,
                        iTotalRecords = count,
                        aaData = ObjectMapper.Map<List<FuelPriceChangeRequestDto>>(fuelPriceChangeRequests)
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
