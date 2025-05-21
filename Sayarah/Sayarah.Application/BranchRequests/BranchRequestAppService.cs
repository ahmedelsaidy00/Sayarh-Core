using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Sayarah.Application.BranchRequests.Dto;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Sayarah.Application.Helpers.NotificationService;
using Sayarah.Authorization.Roles;
using Sayarah.Authorization.Users;
using Sayarah.BranchRequests;
using Sayarah.Companies;
using Sayarah.Core.Helpers;
using System.Globalization;
using System.Linq.Dynamic.Core;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.BranchRequests;

public class BranchRequestAppService : AsyncCrudAppService<BranchRequest, BranchRequestDto, long, GetAllBranchRequest, CreateBranchRequestDto, UpdateBranchRequestDto>, IBranchRequestAppService
{
    private readonly IRepository<BranchRequest, long> _branchRequestRepository;
    private readonly IRepository<RequestOrder, long> _requestOrderRepository;
    private readonly IRepository<User, long> _userRepository;
    private readonly IRepository<Branch, long> _branchRepository;
    private readonly ICommonAppService _commonService;
    private readonly AbpNotificationHelper _abpNotificationHelper;
    private readonly UserManager _userManager;
    private readonly RoleManager _roleManager;

    public BranchRequestAppService(
           IRepository<BranchRequest, long> branchRequestRepository,
           IRepository<User, long> userRepository,
           IRepository<Branch, long> branchRepository,
           ICommonAppService commonService,
           AbpNotificationHelper abpNotificationHelper,
           UserManager userManager,
           RoleManager roleManager,
           IRepository<RequestOrder, long> requestOrderRepository)
           : base(branchRequestRepository)
    {
        LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        _branchRequestRepository = branchRequestRepository;
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _commonService = commonService;
        _abpNotificationHelper = abpNotificationHelper;
        _userManager = userManager;
        _roleManager = roleManager;
        _requestOrderRepository = requestOrderRepository;
    }

    [DisableAuditing]
    [AbpAuthorize]
    public async Task<DataTableOutputDto<BranchRequestDto>> GetPaged(GetBranchRequestInput input)
    {
        using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
        {
            // Handle GroupAction and SingleAction
            if (!string.IsNullOrWhiteSpace(input.actionType))
            {
                var ids = input.ids?.Select(id => Convert.ToInt64(id)).ToList() ?? [];

                if (input.actionType == "GroupAction" && input.action == "Delete")
                {
                    foreach (var id in ids)
                    {
                        await _branchRequestRepository.DeleteAsync(id);
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }

                if (input.actionType == "SingleAction" && ids.Any())
                {
                    var id = ids.First();
                    if (input.action == "Delete")
                    {
                        await _branchRequestRepository.DeleteAsync(id);
                    }
                    else if (input.action == "Reject")
                    {
                        await Reject(new EntityDto<long> { Id = id });
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }

            var query = _branchRequestRepository
                .GetAll()
                .Include(x => x.Branch)
                    .ThenInclude(b => b.Company)
                .Include(x => x.CreatorUser)
                .Include(x => x.LastModifierUser)
                .Where(x => x.BranchId.HasValue);

            // Apply filters
            if (input.IsEmployee == true)
            {
                if (input.BranchesIds != null && input.BranchesIds.Count > 0)
                {
                    query = query.Where(x => input.BranchesIds.Contains(x.BranchId.Value));
                }
                else
                {
                    return new DataTableOutputDto<BranchRequestDto>
                    {
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = []
                    };
                }
            }

            query = query
                .WhereIf(input.BranchId.HasValue, x => x.BranchId == input.BranchId)
                .WhereIf(input.CompanyId.HasValue, x => x.Branch.CompanyId == input.CompanyId)
                .WhereIf(!string.IsNullOrEmpty(input.Quantity), x => x.Quantity == input.Quantity)
                .WhereIf(!string.IsNullOrEmpty(input.Code), x => x.Code == input.Code)
                .WhereIf(!string.IsNullOrEmpty(input.BranchName), x =>
                    x.Branch.NameAr.Contains(input.BranchName) || x.Branch.NameEn.Contains(input.BranchName))
                .WhereIf(!string.IsNullOrEmpty(input.CompanyName), x =>
                    x.Branch.Company.NameAr.Contains(input.CompanyName) || x.Branch.Company.NameEn.Contains(input.CompanyName))
                .WhereIf(input.DeliveyWay.HasValue, x => x.DeliveyWay == input.DeliveyWay)
                .WhereIf(input.DeliveyDate.HasValue, x => x.DeliveyDate.HasValue && x.DeliveyDate.Value.Date == input.DeliveyDate.Value.Date)
                .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                .WhereIf(input.FuelType.HasValue, x => x.FuelType == input.FuelType)
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id);

            var totalCount = await query.CountAsync();

            // Sorting
            string sortColumn = input.columns[input.order[0].column].name;
            string sortDirection = input.order[0].dir;
            string orderByString = sortColumn.Equals("Code", StringComparison.OrdinalIgnoreCase)
                ? sortDirection == "asc"
                    ? "Code.Length, Code"
                    : "Code.Length DESC, Code DESC"
                : $"{sortColumn} {sortDirection}";

            var pagedData = await query
                .OrderBy(orderByString)
                .Skip(input.start)
                .Take(input.length)
                .ToListAsync();

            var result = new DataTableOutputDto<BranchRequestDto>
            {
                iTotalRecords = totalCount,
                iTotalDisplayRecords = totalCount,
                aaData = ObjectMapper.Map<List<BranchRequestDto>>(pagedData)
            };

            return result;
        }
    }
    public override async Task<BranchRequestDto> GetAsync(EntityDto<long> input)
    {
        var branchRequest = _branchRequestRepository.GetAll().Include(x => x.Branch).FirstOrDefault(x => x.Id == input.Id);
        return await Task.FromResult(ObjectMapper.Map<BranchRequestDto>(branchRequest));
    }

    [AbpAuthorize]
    public override async Task<BranchRequestDto> CreateAsync(CreateBranchRequestDto input)
    {
        try
        {
            input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto { TableName = "BranchRequests", CodeField = "Code", AddWhere = "" });
            var branchRequest = ObjectMapper.Map<BranchRequest>(input);
            var branchRequestId = await _branchRequestRepository.InsertAndGetIdAsync(branchRequest);
            var branche = await _branchRepository.FirstOrDefaultAsync(x => x.Id == input.BranchId);

            #region ///////  Send Abp Notifications from Branch To Admin ///////
            List<UserIdentifier> adminTargetUsersId = [];
            var role = await _roleManager.GetRoleByNameAsync(RolesNames.Admin);
            if (role != null)
            {
                var admins = _userManager.Users.Where(x => x.Roles.Any(y => y.RoleId == role.Id));
                foreach (var usr in admins)
                {
                    adminTargetUsersId.Add(new UserIdentifier(tenantId: AbpSession.TenantId, userId: usr.Id));
                }

                CreateNotificationDto CreateAdminNotificationData = new CreateNotificationDto
                {
                    SenderUserName = branche.NameAr,
                    Message = L("Pages.Notifications.NewBranchRequest"),
                    EntityType = Entity_Type.BrancheRequest,
                    EntityId = branchRequest.Id
                };
                //Publish Notification Data
                await _abpNotificationHelper.Publish_CreateNotification(NotificationsNames.BrancheRequest, CreateAdminNotificationData, adminTargetUsersId.ToArray());
            }
            #endregion
            return MapToEntityDto(branchRequest);
        }
        catch (Exception ex)
        {
            throw;
        }

    }

    [AbpAuthorize]
    public override async Task<BranchRequestDto> UpdateAsync(UpdateBranchRequestDto input)
    {
        var branchRequest = await _branchRequestRepository.GetAsync(input.Id);
        ObjectMapper.Map(input, branchRequest);
        await _branchRequestRepository.UpdateAsync(branchRequest);
        return MapToEntityDto(branchRequest);
    }
    public override async Task<PagedResultDto<BranchRequestDto>> GetAllAsync(GetAllBranchRequest input)
    {
        var query = _branchRequestRepository.GetAll();

        query = query.WhereIf(input.BranchId.HasValue, x => x.BranchId == input.BranchId)
                     .WhereIf(input.DeliveyWay.HasValue, x => x.DeliveyWay == input.DeliveyWay)
                     .WhereIf(input.DeliveyDate.HasValue, x => x.DeliveyDate.HasValue && x.DeliveyDate.Value.Date == input.DeliveyDate.Value.Date)
                     .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
                     .WhereIf(input.FuelType.HasValue, x => x.FuelType == input.FuelType);

        if (input.MaxCount == true)
        {
            input.SkipCount = 0;
            input.MaxResultCount = await query.CountAsync();
        }

        var result = await query
            .OrderBy(x => x.Id)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToListAsync();

        var total = await query.CountAsync();
        return new PagedResultDto<BranchRequestDto>(total, ObjectMapper.Map<List<BranchRequestDto>>(result));
    }
    public async Task<BranchRequestDto> Reject(EntityDto<long> input)
    {
        var branchRequest = await _branchRequestRepository.GetAll()
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Id == input.Id);

        branchRequest.Status = RequestStatus.Rejected;
        await _branchRequestRepository.UpdateAsync(branchRequest);
        await CurrentUnitOfWork.SaveChangesAsync();

        var userId = branchRequest.Branch.UserId;
        var lang = await SettingManager.GetSettingValueForUserAsync("Abp.Localization.DefaultLanguageName", new UserIdentifier(AbpSession.TenantId, userId.Value)) ?? new CultureInfo("ar").ToString();

        var notification = new CreateNotificationDto
        {
            Message = L("Pages.BranchRequests.Rejected", new CultureInfo(lang), userId),
            EntityType = Entity_Type.RejectBrancheRequest,
            EntityId = branchRequest.Id
        };

        await _abpNotificationHelper.Publish_CreateNotification(
            NotificationsNames.RejectBrancheRequest,
            notification,
            new[] { new UserIdentifier(AbpSession.TenantId.Value, userId.Value) }
        );

        return MapToEntityDto(branchRequest);
    }
}
