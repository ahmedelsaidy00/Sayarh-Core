using Abp.Domain.Services;
using System.Net.Mail;

namespace Sayarah.Application.Helpers
{
    public interface IEmailService : IDomainService
    {
        void SendEmail(MailMessage mail);
    }

}
