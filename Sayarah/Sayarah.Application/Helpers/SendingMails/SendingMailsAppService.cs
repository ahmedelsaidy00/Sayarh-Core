
using System;
using System.Threading.Tasks;
using Abp.Net.Mail;
using Abp.Configuration;
using System.Text;
using System.Net.Mail;
using System.IO;
using System.Web;
using System.Net.Mime;
using Sayarah.Application;
using Sayarah.Application.Helpers;
using Sayarah.Application.Helpers.Dto;
using Microsoft.AspNetCore.Http;

namespace Sayarah.Application.Helpers.SendingMails
{
    public class SendingMailsAppService : SayarahAppServiceBase, ISendingMailsAppService
    {
       
        public SendingMailsAppService()
        {
           
        }
        
        public async Task<bool> SendEmail(SendEmailRequest input)
        {
            try
            {
                StringBuilder content = new StringBuilder();
                #region Header&Logo
                string webRootPath = AppDomain.CurrentDomain.BaseDirectory;
                string logoPath = Path.Combine(webRootPath, SayarahConsts.MailLogoPath.TrimStart('~', '/').Replace('/', Path.DirectorySeparatorChar));
                LinkedResource inlineLogo = new LinkedResource(logoPath, MediaTypeNames.Image.Jpeg);
                inlineLogo.ContentId = Guid.NewGuid().ToString();
                content.Append("<div style=\"text-align:center;width:100%;background-color:#f5f5f5;padding:50px 0\">");
                content.Append("<div style=\"width:100%;max-width: 600px;display:inline-block;background: #fff;\">");
                content.Append(string.Format("<div style=\"width:100%;background:#0e2647;padding:15px 0\"><img style=\"width:80px\" src=\"cid:{0}\" id=\"img\" /></div>", inlineLogo.ContentId));
                #endregion
                 
                #region body
                content.Append(string.Format("<p style=\"font-size:16px;margin:30px 0 0;color:#333;\"><strong>{0}</strong></p>", input.Subject));
                if (input.datalst != null && input.datalst.Length > 0)
                {
                    for (int i = 0; i < input.datalst.Length; i++)
                    {
                        content.Append(string.Format("<p style=\"font-size:30px;margin:5px 0 30px;color:#333;\"><strong>{0}</strong></p>", input.datalst[i]));
                    }
                }
                content.Append("<div style=\"margin-top:30px\">");
                if (!string.IsNullOrEmpty(input.ParamsHeader))
                    content.Append(string.Format("<p style=\"font-size:14px;color:#333;\">{0}</p>", input.ParamsHeader));
                if (input.Paramslst != null && input.Paramslst.Count > 0)
                {
                    foreach (var item in input.Paramslst)
                    {
                        content.Append(string.Format("<div style=\"display:inline-block;width:95%;\"><p style=\"width:35%;float:right;background:#0e74bc;color:#fff;padding:5px;margin:0 0 0 5px;\"><b>{0}</b></p><p style=\"color:#333;width:57%;float:right;background: whitesmoke;padding:5px 20px 5px 5px;margin: 0 0 5px 0;text-align:right;\">{1}</p></div>", item.Key, item.Value));
                    }
                }
                if (!string.IsNullOrEmpty(input.Url))
                    content.Append(string.Format("<a href=\"{0}\" style=\"display:inline-block;text-decoration:none;border:0;padding:15px 20px;border-radius:6px;background-color:#e4bc76;font-size:20px;color:#ffffff;width:300px;margin-top:20px;\" class=\"button_link\">{1}</a>", input.Url, L(input.UrlTitle)));
                #endregion

                #region Footer
                content.Append(string.Format("</div><div><p style=\"padding:20px;background: #efefe9;margin: 30px 0 0;color:#333;\"><strong>{0} {1}</strong></p></div></div></div>", L("Common.CopyRights", DateTime.Now.Year), L("Common.Al7osam")));

                #endregion

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(content.ToString(), null, "text/html");
                htmlView.LinkedResources.Add(inlineLogo);
                MailMessage _mail = new MailMessage();
                _mail.Body = content.ToString();
                _mail.Subject = L("Common.SystemTitle");
                _mail.IsBodyHtml = input.IsBodyHtml;
                foreach (var mail in input.Emails)
                {
                    _mail.To.Add(mail);

                }

                _mail.AlternateViews.Add(htmlView);
                await Mailer.SendEmailAsync(_mail, await GetSettings(), true);
                return true;
            }


            catch (Exception ex)
            {
                return false;
                //throw ex;
            }
        }
        async Task<MailData> GetSettings()
        {
            var result = new MailData()
            {
                Host = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Host),
                Password = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Password),
                Port = await SettingManager.GetSettingValueAsync<int>(EmailSettingNames.Smtp.Port),
                Sender = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.UserName),
                DefaultFromDisplayName = await SettingManager.GetSettingValueAsync(EmailSettingNames.DefaultFromDisplayName)
            };
            return result;
        }

         
    }
}
