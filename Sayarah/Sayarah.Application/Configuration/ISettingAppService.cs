using Abp.Application.Services;
using Sayarah.Application.Configuration.Dto;

namespace Sayarah.Application.Configuration
{
    public interface ISettingAppService : IApplicationService
    {
        Task<GetSettingDefinitionsOutput> GetSettingDefinitions(GetSettingDefinitionsInput input);
        Task SaveSettings(SaveSettingsInput input);
        string VersionNum();
        Task UpdateSettingVersion(UpdateSettingVersionInput input);
    }
}
