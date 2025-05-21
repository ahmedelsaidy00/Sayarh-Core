using Abp.Application.Services;
using Sayarah.Application.Helpers.Dto;

namespace Sayarah.Application.Helpers.SendingMails
{
    public interface ISendingMailsAppService : IApplicationService
    {
        Task<bool> SendEmail(SendEmailRequest input);
    }

}
