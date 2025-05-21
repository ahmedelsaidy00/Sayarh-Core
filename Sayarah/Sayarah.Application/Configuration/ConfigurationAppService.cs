using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Sayarah.Application;
using Sayarah.Application.Configuration.Dto;
using Sayarah.Configuration;

namespace Sayarah.Application.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : SayarahAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
