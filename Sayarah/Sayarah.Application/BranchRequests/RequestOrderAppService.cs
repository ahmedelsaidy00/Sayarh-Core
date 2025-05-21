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
using Sayarah.Core.Helpers;
using System.Globalization;
using System.Linq.Dynamic.Core;
using static Sayarah.SayarahConsts;

namespace Sayarah.Application.BranchRequests;

[DisableAuditing]
public class RequestOrderAppService : AsyncCrudAppService<RequestOrder, RequestOrderDto, long, GetAllRequestOrder, CreateRequestOrderDto, UpdateRequestOrderDto>, IRequestOrderAppService
{
    private readonly IRepository<BranchRequest, long> _branchRequestRepository;
    private readonly ICommonAppService _commonService;
    private readonly RoleManager _roleManager;
    private readonly UserManager _userManager;
    private readonly AbpNotificationHelper _abpNotificationHelper;
    private readonly CultureInfo _defaultCulture = new("ar");

    public RequestOrderAppService(
        IRepository<RequestOrder, long> repository,
        IRepository<BranchRequest, long> branchRequestRepository,
        ICommonAppService commonService,
        RoleManager roleManager,
        UserManager userManager,
        AbpNotificationHelper abpNotificationHelper)
        : base(repository)
    {
        LocalizationSourceName = SayarahConsts.LocalizationSourceName;
        _branchRequestRepository = branchRequestRepository;
        _commonService = commonService;
        _roleManager = roleManager;
        _userManager = userManager;
        _abpNotificationHelper = abpNotificationHelper;
    }

    [AbpAuthorize]
    public async Task<DataTableOutputDto<RequestOrderDto>> GetPaged(GetRequestOrderInput input)
    {
        using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
        {
            // Actions
            var ids = input.ids?.Select(long.Parse).ToList() ?? new List<long>();

            if (input.actionType == "GroupAction" && input.action == "Delete")
            {
                foreach (var id in ids)
                    await Repository.DeleteAsync(id);

                await CurrentUnitOfWork.SaveChangesAsync();
            }
            else if (input.actionType == "SingleAction" && ids.Any() && input.action == "Delete")
            {
                await Repository.DeleteAsync(ids.First());
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            // Query + filtering
            var query = Repository.GetAll()
                .Include(x => x.BranchRequest)
                    .ThenInclude(br => br.Branch)
                        .ThenInclude(b => b.Company)
                .WhereIf(input.BranchRequestId.HasValue, x => x.BranchRequestId == input.BranchRequestId)
                .WhereIf(input.FuelType.HasValue, x => x.FuelType == input.FuelType)
                .WhereIf(input.Discount.HasValue, x => x.Discount == input.Discount)
                .WhereIf(input.PayMethod.HasValue, x => x.PayMethod == input.PayMethod)
                .WhereIf(input.Price.HasValue, x => x.Price == input.Price)
                .WhereIf(input.BranchId.HasValue, x => x.BranchRequest.BranchId == input.BranchId)
                .WhereIf(input.Id.HasValue, x => x.Id == input.Id)
                .FilterDataTable(input);

            var totalCount = await Repository.CountAsync();
            var filteredCount = await query.CountAsync();

            var orderBy = $"{input.columns[input.order[0].column].name} {input.order[0].dir}";
            var result = await query
                .OrderBy(orderBy)
                .Skip(input.start)
                .Take(input.length)
                .ToListAsync();

            return new DataTableOutputDto<RequestOrderDto>
            {
                iTotalRecords = totalCount,
                iTotalDisplayRecords = filteredCount,
                aaData = ObjectMapper.Map<List<RequestOrderDto>>(result)
            };
        }
    }
    public override async Task<RequestOrderDto> GetAsync(EntityDto<long> input)
    {
        var requestOrder = await Repository.GetAll()
            .Include(x => x.BranchRequest)
            .ThenInclude(b => b.Branch)
            .FirstOrDefaultAsync(x => x.Id == input.Id);

        return ObjectMapper.Map<RequestOrderDto>(requestOrder);
    }

    [AbpAuthorize]
    public override async Task<RequestOrderDto> CreateAsync(CreateRequestOrderDto input)
    {
        try
        {
            input.Code = await _commonService.GetNextCode(new GetNextCodeInputDto
            {
                TableName = "RequestOrders",
                CodeField = "Code"
            });

            var requestOrder = ObjectMapper.Map<RequestOrder>(input);
            var id = await Repository.InsertAndGetIdAsync(requestOrder);

            var fullOrder = await Repository.GetAll()
                .Include(x => x.BranchRequest)
                    .ThenInclude(br => br.Branch)
                .FirstOrDefaultAsync(x => x.Id == id);

            var userId = fullOrder?.BranchRequest?.Branch?.UserId;
            if (userId.HasValue)
            {
                var lang = await SettingManager.GetSettingValueForUserAsync(
                    "Abp.Localization.DefaultLanguageName",
                    new UserIdentifier(AbpSession.TenantId, userId.Value)
                ) ?? _defaultCulture.Name;

                var notification = new CreateNotificationDto
                {
                    Message = L("Pages.OrderRequest.NewRequestOrder", new CultureInfo(lang), id),
                    EntityType = Entity_Type.NewRequestOrder,
                    EntityId = id
                };

                await _abpNotificationHelper.Publish_CreateNotification(
                    NotificationsNames.NewRequestOrder,
                    notification,
                    new[] { new UserIdentifier(AbpSession.TenantId.Value, userId.Value) }
                );
            }

            // Update related branch request status
            var branchRequest = await _branchRequestRepository.FirstOrDefaultAsync(x => x.Id == input.BranchRequestId);
            if (branchRequest != null)
            {
                branchRequest.Status = RequestStatus.Accepted;
                await _branchRequestRepository.UpdateAsync(branchRequest);
            }

            await CurrentUnitOfWork.SaveChangesAsync();
            return MapToEntityDto(requestOrder);
        }
        catch
        {
            throw;
        }
    }

    [AbpAuthorize]
    public override async Task<RequestOrderDto> UpdateAsync(UpdateRequestOrderDto input)
    {
        try
        {
            var requestOrder = await Repository.GetAsync(input.Id);
            ObjectMapper.Map(input, requestOrder);
            await Repository.UpdateAsync(requestOrder);
            return MapToEntityDto(requestOrder);
        }
        catch
        {
            throw;
        }
    }
    public override async Task<PagedResultDto<RequestOrderDto>> GetAllAsync(GetAllRequestOrder input)
    {
        try
        {
            var query = Repository.GetAll()
                .WhereIf(input.BranchRequestId.HasValue, x => x.BranchRequestId == input.BranchRequestId)
                .WhereIf(input.FuelType.HasValue, x => x.FuelType == input.FuelType)
                .WhereIf(input.Discount.HasValue, x => x.Discount == input.Discount)
                .WhereIf(input.PayMethod.HasValue, x => x.PayMethod == input.PayMethod)
                .WhereIf(input.Price.HasValue, x => x.Price == input.Price);

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

            return new PagedResultDto<RequestOrderDto>(total, ObjectMapper.Map<List<RequestOrderDto>>(result));
        }
        catch
        {
            throw;
        }
    }
}
