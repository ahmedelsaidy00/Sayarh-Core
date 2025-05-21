using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Configuration.Startup;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityHistory;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Sayarah.AbpZeroTemplate.Auditing.Dto;
using Sayarah.AbpZeroTemplate.Dto;
using Sayarah.AbpZeroTemplate.EntityHistory;
using Sayarah.Application.auditing.Dto;
using Sayarah.Application.auditing.Exporting;
using Sayarah.Application.DataTables.Dto;
using Sayarah.Application.EntityHistory;
using Sayarah.Authorization.Users;
using System.Linq.Dynamic.Core;

namespace Sayarah.Application.auditing;

[DisableAuditing]

[AbpAuthorize]
public class AuditLogAppService(
    IRepository<AuditLog, long> auditLogRepository,
    IRepository<User, long> userRepository,
    IAuditLogListExcelExporter auditLogListExcelExporter,
    INamespaceStripper namespaceStripper,
    IRepository<EntityChange, long> entityChangeRepository,
    IRepository<EntityChangeSet, long> entityChangeSetRepository,
    IRepository<EntityPropertyChange, long> entityPropertyChangeRepository,
    IAbpStartupConfiguration abpStartupConfiguration) : SayarahAppServiceBase, IAuditLogAppService
{
    private readonly IRepository<AuditLog, long> _auditLogRepository = auditLogRepository;
    private readonly IRepository<EntityChange, long> _entityChangeRepository = entityChangeRepository;
    private readonly IRepository<EntityChangeSet, long> _entityChangeSetRepository = entityChangeSetRepository;
    private readonly IRepository<EntityPropertyChange, long> _entityPropertyChangeRepository = entityPropertyChangeRepository;
    private readonly IRepository<User, long> _userRepository = userRepository;
    private readonly IAuditLogListExcelExporter _auditLogListExcelExporter = auditLogListExcelExporter;
    private readonly INamespaceStripper _namespaceStripper = namespaceStripper;
    private readonly IAbpStartupConfiguration _abpStartupConfiguration = abpStartupConfiguration;

    #region audit logs
    [AbpAuthorize]
    public async Task<DataTableOutputDto<AuditLogListDto>> GetPaged(GetAuditLogsInput2 input)
    {
        try
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        int auditLogId = Convert.ToInt32(input.ids[i]);
                        AuditLog auditLog = await _auditLogRepository.GetAsync(auditLogId);
                        if (auditLog != null)
                        {
                            if (input.action == "Delete")//Delete
                            {



                                //if (centersCount > 0)
                                //    throw new UserFriendlyException(L("Pages.AuditLogs.Error.HasCenters"));

                                await _auditLogRepository.DeleteAsync(auditLog);
                            }
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        int auditLogId = Convert.ToInt32(input.ids[0]);
                        AuditLog auditLog = await _auditLogRepository.GetAsync(auditLogId);
                        if (auditLog != null)
                        {
                            if (input.action == "Delete")//Delete
                            {

                                await _auditLogRepository.DeleteAsync(auditLog);
                            }

                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                var query = CreateAuditLogAndUsersQuery(new GetAuditLogsInput
                {
                    BrowserInfo = input.BrowserInfo,
                    EndDate = input.EndDate,
                    HasException = input.HasException,
                    MaxExecutionDuration = input.MaxExecutionDuration,
                    MethodName = input.MethodName,
                    MinExecutionDuration = input.MinExecutionDuration,
                    ServiceName = input.ServiceName,
                    StartDate = input.StartDate,
                    UserName = input.UserName,

                });

                var resultCount = await query.CountAsync();
                var results = await query
                    .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                    .Skip(input.start)
                    .Take(input.length)
                    .ToListAsync();




                var auditLogListDtos = ConvertToAuditLogListDtos(results);
                return new DataTableOutputDto<AuditLogListDto>
                {
                    iTotalDisplayRecords = resultCount,
                    iTotalRecords = resultCount,
                    aaData = auditLogListDtos
                };
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }


    }
    public async Task<PagedResultDto<AuditLogListDto>> GetAuditLogs(GetAuditLogsInput input)
    {
        var query = CreateAuditLogAndUsersQuery(input);

        var resultCount = await query.CountAsync();
        var results = await query
            .OrderBy(input.Sorting)
            .PageBy(input)
            .ToListAsync();

        var auditLogListDtos = ConvertToAuditLogListDtos(results);

        return new PagedResultDto<AuditLogListDto>(resultCount, auditLogListDtos);
    }
    public async Task<FileDto> GetAuditLogsToExcel(GetAuditLogsInput input)
    {
        var auditLogs = await CreateAuditLogAndUsersQuery(input)
            .AsNoTracking()
            .OrderByDescending(al => al.AuditLog.ExecutionTime)
            .ToListAsync();

        var auditLogListDtos = ConvertToAuditLogListDtos(auditLogs);

        return _auditLogListExcelExporter.ExportToFile(auditLogListDtos);
    }
    private List<AuditLogListDto> ConvertToAuditLogListDtos(List<AuditLogAndUser> results)
    {
        return results.Select(
            result =>
            {
                var auditLogListDto = ObjectMapper.Map<AuditLogListDto>(result.AuditLog);
                auditLogListDto.UserName = result.User?.UserName;
                auditLogListDto.ServiceName = _namespaceStripper.StripNameSpace(auditLogListDto.ServiceName);
                return auditLogListDto;
            }).ToList();
    }
    private IQueryable<AuditLogAndUser> CreateAuditLogAndUsersQuery(GetAuditLogsInput input)
    {
        var query = from auditLog in _auditLogRepository.GetAll()
                    join user in _userRepository.GetAll() on auditLog.UserId equals user.Id into userJoin
                    from joinedUser in userJoin.DefaultIfEmpty()
                    where auditLog.ExecutionTime >= input.StartDate && auditLog.ExecutionTime <= input.EndDate
                    select new AuditLogAndUser { AuditLog = auditLog, User = joinedUser };

        query = query
            .WhereIf(!input.UserName.IsNullOrWhiteSpace(), item => item.User.UserName.Contains(input.UserName))
            .WhereIf(!input.ServiceName.IsNullOrWhiteSpace(), item => item.AuditLog.ServiceName.Contains(input.ServiceName))
            .WhereIf(!input.MethodName.IsNullOrWhiteSpace(), item => item.AuditLog.MethodName.Contains(input.MethodName))
            .WhereIf(!input.BrowserInfo.IsNullOrWhiteSpace(), item => item.AuditLog.BrowserInfo.Contains(input.BrowserInfo))
            .WhereIf(input.MinExecutionDuration.HasValue && input.MinExecutionDuration > 0, item => item.AuditLog.ExecutionDuration >= input.MinExecutionDuration.Value)
            .WhereIf(input.MaxExecutionDuration.HasValue && input.MaxExecutionDuration < int.MaxValue, item => item.AuditLog.ExecutionDuration <= input.MaxExecutionDuration.Value)
            .WhereIf(input.HasException == true, item => item.AuditLog.Exception != null && item.AuditLog.Exception != "")
            .WhereIf(input.HasException == false, item => item.AuditLog.Exception == null || item.AuditLog.Exception == "");
        return query;
    }

    #endregion

    #region entity changes 
    public List<NameValueDto> GetEntityHistoryObjectTypes()
    {
        var entityHistoryObjectTypes = new List<NameValueDto>();
        var enabledEntities = (_abpStartupConfiguration.GetCustomConfig()
            .FirstOrDefault(x => x.Key == AbpZeroTemplate.EntityHistory.EntityHistoryHelper.EntityHistoryConfigurationName)
            .Value as AbpZeroTemplate.EntityHistory.EntityHistoryUiSetting)?.EnabledEntities ?? new List<string>();



        if (AbpSession.TenantId == null)
        {
            enabledEntities = AbpZeroTemplate.EntityHistory.EntityHistoryHelper.HostSideTrackedTypes.Select(t => t.FullName).Intersect(enabledEntities).ToList();
        }
        else
        {
            enabledEntities = AbpZeroTemplate.EntityHistory.EntityHistoryHelper.TenantSideTrackedTypes.Select(t => t.FullName).Intersect(enabledEntities).ToList();
        }

        foreach (var enabledEntity in enabledEntities)
        {
            entityHistoryObjectTypes.Add(new NameValueDto(L(enabledEntity), enabledEntity));
        }

        return entityHistoryObjectTypes;
    }

    [AbpAuthorize]
    public async Task<DataTableOutputDto<EntityChangeListDto>> GetEntityChangesPaged(CustomGetEntityChangeInput input)
    {
        try
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                if (input.actionType == "GroupAction")
                {
                    for (int i = 0; i < input.ids.Length; i++)
                    {
                        int entityChangeId = Convert.ToInt32(input.ids[i]);
                        EntityChange entityChange = await _entityChangeRepository.GetAsync(entityChangeId);
                        if (entityChange != null)
                        {
                            if (input.action == "Delete")//Delete
                            {



                                //if (centersCount > 0)
                                //    throw new UserFriendlyException(L("Pages.AuditLogs.Error.HasCenters"));

                                await _entityChangeRepository.DeleteAsync(entityChange);
                            }
                        }
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                else if (input.actionType == "SingleAction")
                {
                    if (input.ids.Length > 0)
                    {
                        int entityChangeId = Convert.ToInt32(input.ids[0]);
                        EntityChange entityChange = await _entityChangeRepository.GetAsync(entityChangeId);
                        if (entityChange != null)
                        {
                            if (input.action == "Delete")//Delete
                            {

                                await _entityChangeRepository.DeleteAsync(entityChange);
                            }

                        }
                        await CurrentUnitOfWork.SaveChangesAsync();
                    }
                }

                var query = CreateEntityChangesAndUsersQuery(new GetEntityChangeInput
                {
                    EndDate = input.EndDate,
                    EntityTypeFullName = input.EntityTypeFullName,
                    StartDate = input.StartDate,
                    UserName = input.UserName,
                    ChangeType = input.ChangeType,
                    EntityId = input.EntityId,
                });


                var resultCount = await query.CountAsync();



                var advertisements = new List<EntityChangeAndUser>();
                if (input.columns[input.order[0].column].name.Equals("EntityChange.EntityId"))
                {
                    if (input.order[0].dir.Equals("asc"))
                        advertisements = await query
                            .OrderBy(x => x.EntityChange.EntityId.Length).Skip(input.start).Take(input.length).ToListAsync();
                    else
                        advertisements = await query
                            .OrderByDescending(x => x.EntityChange.EntityId.Length).Skip(input.start).Take(input.length).ToListAsync();
                }
                else
                {
                    advertisements = await query
                    .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                    .Skip(input.start).Take(input.length)
                    .ToListAsync();
                }

                //var _advertisements = ObjectMapper.Map<List<PagedAdvertisementDto>>(advertisements);




                //var results = await query
                //    .OrderBy(string.Format("{0} {1}", input.columns[input.order[0].column].name, input.order[0].dir))
                //    //.OrderByDescending(a=>a.EntityChange.ChangeTime)
                //    .Skip(input.start)
                //    .Take(input.length)
                //    .ToListAsync();


                var entityChangeListDtos = ConvertToEntityChangeListDtos(advertisements);


                return new DataTableOutputDto<EntityChangeListDto>
                {
                    iTotalDisplayRecords = resultCount,
                    iTotalRecords = resultCount,
                    aaData = entityChangeListDtos
                };
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public async Task<PagedResultDto<EntityChangeListDto>> GetEntityChanges(GetEntityChangeInput input)
    {
        try
        {

            var query = CreateEntityChangesAndUsersQuery(input);

            var resultCount = await query.CountAsync();
            var results = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            var entityChangeListDtos = ConvertToEntityChangeListDtos(results);

            return new PagedResultDto<EntityChangeListDto>(resultCount, entityChangeListDtos);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public async Task<PagedResultDto<EntityChangeListDto>> GetEntityTypeChanges(GetEntityTypeChangeInput input)
    {
        // Fix for: https://github.com/aspnetzero/aspnet-zero-core/issues/2101
        var entityId = "\"" + input.EntityId + "\"";

        var query = from entityChangeSet in _entityChangeSetRepository.GetAll()
                    join entityChange in _entityChangeRepository.GetAll() on entityChangeSet.Id equals entityChange.EntityChangeSetId
                    join user in _userRepository.GetAll() on entityChangeSet.UserId equals user.Id
                    where entityChange.EntityTypeFullName == input.EntityTypeFullName &&
                          (entityChange.EntityId == input.EntityId || entityChange.EntityId == entityId)
                    select new EntityChangeAndUser
                    {
                        EntityChange = entityChange,
                        User = user
                    };

        var resultCount = await query.CountAsync();
        var results = await query
            .OrderBy(input.Sorting)
            .PageBy(input)
            .ToListAsync();

        var entityChangeListDtos = ConvertToEntityChangeListDtos(results);

        return new PagedResultDto<EntityChangeListDto>(resultCount, entityChangeListDtos);
    }
    public async Task<PagedResultDto<EntityChangeWithPropertiesListDto>> GetEntityTypeChangesWithPropertiesChanges(GetEntityTypeChangeInput input)
    {
        // Fix for: https://github.com/aspnetzero/aspnet-zero-core/issues/2101
        var entityId = "\"" + input.EntityId + "\"";

        var query = from entityChangeSet in _entityChangeSetRepository.GetAll()

                    join entityChange in _entityChangeRepository.GetAll().Include(a
     => a.PropertyChanges) on entityChangeSet.Id equals entityChange.EntityChangeSetId
                    join user in _userRepository.GetAll() on entityChangeSet.UserId equals user.Id
                    where entityChange.EntityTypeFullName
     == input.EntityTypeFullName &&
                          (entityChange.EntityId == input.EntityId || entityChange.EntityId == entityId)
                    select new EntityChangeAndUser
                    {
                        EntityChange = entityChange,
                        User = user
                    };
       
        var resultCount = await query.CountAsync();

        input.MaxResultCount = resultCount;

        var results = await query
            .OrderBy(input.Sorting)
            .PageBy(input)
            .ToListAsync();

        var entityChangeListDtos = ConvertToEntityChangeWithPropertiesListDtos(results);


        if(entityChangeListDtos != null)
        {
            foreach (var item in entityChangeListDtos.ToList())
            {
                item.PropertyChanges = await _entityPropertyChangeRepository.GetAll().Where(a => a.EntityChangeId == item.Id).ToListAsync();
            }
        }


        return new PagedResultDto<EntityChangeWithPropertiesListDto>(resultCount, entityChangeListDtos);
    }
    private List<EntityChangeWithPropertiesListDto> ConvertToEntityChangeWithPropertiesListDtos(List<EntityChangeAndUser> results)
    {
        return results.Select(
            result =>
            {
                var entityChangeListDto = ObjectMapper.Map<EntityChangeWithPropertiesListDto>(result.EntityChange);
                entityChangeListDto.UserName = result.User?.UserName;
                entityChangeListDto.PropertyChanges = result.EntityChange.PropertyChanges.ToList(); // Directly assign the PropertyChanges list
                return entityChangeListDto;
            }).ToList();
    }
    public async Task<FileDto> GetEntityChangesToExcel(GetEntityChangeInput input)
    {
        var entityChanges = await CreateEntityChangesAndUsersQuery(input)
            .AsNoTracking()
            .OrderByDescending(ec => ec.EntityChange.EntityChangeSetId)
            .ThenByDescending(ec => ec.EntityChange.ChangeTime)
            .ToListAsync();

        var entityChangeListDtos = ConvertToEntityChangeListDtos(entityChanges);

        return _auditLogListExcelExporter.ExportToFile(entityChangeListDtos);
    }
    public async Task<List<EntityPropertyChangeDto>> GetEntityPropertyChanges(long entityChangeId)
    {
        var entityPropertyChanges = await _entityPropertyChangeRepository.GetAll().Where(epc => epc.EntityChangeId == entityChangeId)
            .OrderByDescending(a => a.Id)
            .ToListAsync()
            ;

        return ObjectMapper.Map<List<EntityPropertyChangeDto>>(entityPropertyChanges);
    }
    private List<EntityChangeListDto> ConvertToEntityChangeListDtos(List<EntityChangeAndUser> results)
    {
        return [.. results.Select(
            result =>
            {
                var entityChangeListDto = ObjectMapper.Map<EntityChangeListDto>(result.EntityChange);
                entityChangeListDto.UserName = result.User?.UserName;
                return entityChangeListDto;
            })];
    }
    private IQueryable<EntityChangeAndUser> CreateEntityChangesAndUsersQuery(GetEntityChangeInput input)
    {
        var query = from entityChangeSet in _entityChangeSetRepository.GetAll()
                    join entityChange in _entityChangeRepository.GetAll() on entityChangeSet.Id equals entityChange.EntityChangeSetId
                    join user in _userRepository.GetAll() on entityChangeSet.UserId equals user.Id
                    select new EntityChangeAndUser
                    {
                        EntityChange = entityChange,
                        User = user
                    };

        query = query.WhereIf(!input.UserName.IsNullOrWhiteSpace(), item => item.User.UserName.Contains(input.UserName));
        query = query.WhereIf(!input.EntityId.IsNullOrWhiteSpace(), item => item.EntityChange.EntityId == input.EntityId);
        query = query.WhereIf(input.ChangeType.HasValue, item => item.EntityChange.ChangeType == input.ChangeType);
        query = query.WhereIf(input.StartDate.HasValue, item => item.EntityChange.ChangeTime >= input.StartDate);
        query = query.WhereIf(input.EndDate.HasValue, item => item.EntityChange.ChangeTime <= input.EndDate);
        query = query.WhereIf(!input.EntityTypeFullName.IsNullOrWhiteSpace(), item => item.EntityChange.EntityTypeFullName.Contains(input.EntityTypeFullName));
        query = query.WhereIf(input.Id.HasValue, item => item.EntityChange.Id == input.Id);

        return query;
    }
    #endregion
}
