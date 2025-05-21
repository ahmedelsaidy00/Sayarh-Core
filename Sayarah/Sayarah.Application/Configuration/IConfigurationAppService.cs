using Abp.Application.Services;
using Sayarah.Application.Configuration.Dto;

namespace Sayarah.Application.Configuration
{
    public interface IConfigurationAppService: IApplicationService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}