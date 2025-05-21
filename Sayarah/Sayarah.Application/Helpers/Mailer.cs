using Microsoft.AspNet.Identity;
using Sayarah.Application.Contact.Dto;
using Sayarah.Application.Helpers.Dto;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Sayarah.Application.Helpers
{
    public static class Mailer
    {
        public static void SendMail(MailMessage mail)
        {
            try
            {
                using var smtpClient = new SmtpClient("al7osamcompany.com", 587)
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential("eslam.hamam@al7osamcompany.com", "123456"),
                    EnableSsl = true,
                    Timeout = 20000
                };
                mail.From = new MailAddress("eslam.hamam@al7osamcompany.com");
                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                // Consider logging the exception
                throw new MailerException("Failed to send mail.", ex);
            }
        }

        public static void SendRequestEmail(MailMessage mail, string Sender, string Host, string Port, string Password, string EnableSsl)
        {
            try
            {
                Sender = string.IsNullOrWhiteSpace(Sender) || Sender == "admin@bahr.com" || Sender == "info@al7osamcompany.com" ? "eslam.hamam@al7osamcompany.com" : Sender;
                Host = string.IsNullOrWhiteSpace(Host) ? "al7osamcompany.com" : Host;
                Port = string.IsNullOrWhiteSpace(Port) ? "587" : Port;
                Password = string.IsNullOrWhiteSpace(Password) ? "123456" : Password;

                mail.IsBodyHtml = true;
                mail.SubjectEncoding = Encoding.UTF8;
                var mailBuilder = new StringBuilder();
                mailBuilder.Append(
                                    @"<!DOCTYPE html>
                                        <html lang=""en"" xmlns=""http://www.w3.org/1999/xhtml"">
                                        <head>
                                            <meta charset=""utf-8"" />
                                            <title>{TEXT_HTML_TITLE}</title>
                                            <style>
                                                body {
                                                    font-family: Verdana, Geneva, 'DejaVu Sans', sans-serif;
                                                    font-size: 12px;
                                                }
                                            </style>
                                        </head>
                                        <body>
                                            <h3>{TEXT_HEADER}</h3>
                                            <p>{TEXT_DESCRIPTION}</p>
                                            <p>&nbsp;</p>
                                            <p><a href=""http://bahr.mict.pro/"">Bahr Company</a></p>
                                        </body>
                                        </html>");
                mailBuilder.Replace("{TEXT_HTML_TITLE}", mail.Subject);
                mailBuilder.Replace("{TEXT_HEADER}", mail.Subject);
                mailBuilder.Replace("{TEXT_DESCRIPTION}", mail.Body);

                mail.Body = mailBuilder.ToString();
                mail.BodyEncoding = Encoding.UTF8;

                using var smtpClient = new SmtpClient(Host, int.Parse(Port))
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(Sender, Password),
                    EnableSsl = bool.TryParse(EnableSsl, out var ssl) && ssl,
                    Timeout = 20000
                };
                mail.From = new MailAddress(Sender);
                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                // Consider logging the exception
                throw new MailerException("Failed to send request email.", ex);
            }
        }

        public static async Task SendEmailAsync(MailMessage mail, MailData data, bool? isBodyHtml)
        {
            try
            {
                using var client = new SmtpClient(data.Host, data.Port)
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(data.Sender, data.Password),
                    EnableSsl = true,
                    Timeout = 20000
                };
                mail.From = new MailAddress(data.Sender);
                mail.IsBodyHtml = isBodyHtml ?? false;
                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                throw new MailerException("Failed to send async mail.", ex);
            }
        }

        public static async Task SendEmailAsync(IdentityMessage message)
        {
            var credentialUserName = "a.zakzouk@al7osamcompany.com";
            var sentFrom = "a.zakzouk@al7osamcompany.com";
            var pwd = "I@123456";

            using var client = new SmtpClient("al7osamcompany.com", 587)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(credentialUserName, pwd),
                EnableSsl = true,
                Timeout = 20000
            };

            var mail = new MailMessage(sentFrom, message.Destination)
            {
                Subject = message.Subject,
                Body = message.Body
            };

            await client.SendMailAsync(mail);
        }

        public static async Task SendEmailAsync(IdentityMessage message, GetSettingDataDto data, bool isBodyHtml = false)
        {
            using var client = new SmtpClient(data.Host, data.Port)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(data.Sender, data.Password),
                EnableSsl = true,
                Timeout = 20000
            };

            var mail = new MailMessage(data.Sender, message.Destination)
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = isBodyHtml
            };

            await client.SendMailAsync(mail);
        }

        public static async Task SendEmailAsync(IdentityMessage message, GetSettingDataDto data, bool? isBodyHtml)
        {
            using var client = new SmtpClient(data.Host, data.Port)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(data.Sender, data.Password),
                EnableSsl = true,
                Timeout = 20000
            };

            var mail = new MailMessage(data.Sender, message.Destination)
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = isBodyHtml ?? false
            };

            await client.SendMailAsync(mail);
        }
    }

}
