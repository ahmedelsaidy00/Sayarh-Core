using Abp.Authorization;
using Abp.Configuration;
using Abp.Runtime.Session;
using Sayarah.Application.Configuration.Dto;

namespace Sayarah.Application.Configuration;
public class SettingAppService(ISettingDefinitionManager settingDefinitionManager, ISettingManager settingManager) : ISettingAppService
{
    private readonly ISettingDefinitionManager _settingDefinitionManager = settingDefinitionManager;
    private readonly ISettingManager _settingManager = settingManager;
    public IAbpSession Session { get; set; }

    [AbpAuthorize]
    public async Task<GetSettingDefinitionsOutput> GetSettingDefinitions(GetSettingDefinitionsInput input)
    {
        //get definitions
        List<SettingDefinition> definitions = [.. _settingDefinitionManager.GetAllSettingDefinitions().Where(x => x.Scopes.HasFlag((SettingScopes)input.Scope))];
        IReadOnlyList<ISettingValue> values = null;
        List<ISettingValue> explicitValues = null;
        if ((SettingScopes)input.Scope == SettingScopes.User)
        {
            values = await _settingManager.GetAllSettingValuesAsync(SettingScopes.Application | SettingScopes.Tenant | SettingScopes.User);
        }
        else if ((SettingScopes)input.Scope == SettingScopes.Tenant)
        {
            if (Session.TenantId.HasValue) // it's tenant
            {
                values = await _settingManager.GetAllSettingValuesAsync(SettingScopes.Application | SettingScopes.Tenant);
                //Get Explicit values for tenant email settings
                explicitValues = (await _settingManager.GetAllSettingValuesAsync(SettingScopes.Tenant)).Where(sv => sv.Name.Contains("Abp.Net.Mail")).ToList();
            }
            else // it's Tenancy Owner
            {
                values = await _settingManager.GetAllSettingValuesAsync(SettingScopes.Application);
            }
        }
        return new GetSettingDefinitionsOutput() { Items = definitions, Values = values.ToList(), ExplicitValues = explicitValues };
    }

    [AbpAuthorize]
    public async Task SaveSettings(SaveSettingsInput input)
    {
        foreach (SettingDto setting in input.Settings)
        {
                if (Session.TenantId.HasValue) // it's tenant
                    await _settingManager.ChangeSettingForTenantAsync(Session.TenantId.Value, setting.Name, setting.Value);
                else // it's Tenancy Owner
                    await _settingManager.ChangeSettingForApplicationAsync(setting.Name, setting.Value);
        }
    }

    public string VersionNum()
    {
        var currentVersionNum = "0001";
        return currentVersionNum;
    }
    public async Task UpdateSettingVersion(UpdateSettingVersionInput input)
    {
        var _setting = await _settingManager.GetSettingValueAsync(input.SettingName);
        if (!string.IsNullOrEmpty(_setting))
        {
            long _version = Convert.ToInt64(_setting);
            _version += 1;
            await _settingManager.ChangeSettingForApplicationAsync(input.SettingName, _version.ToString());
        }
        else
        {
            await _settingManager.ChangeSettingForApplicationAsync(input.SettingName, "1");
        }
    }
}
