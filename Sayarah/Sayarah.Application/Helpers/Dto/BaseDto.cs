using System.Collections.Generic;

namespace Sayarah.Application.Helpers.Dto
{
    public class BaseInputEntityDto
    {
        public long Id { get; set; }
        public EntityAction EntityAction { get; set; }
    }
    public class BaseInputDto
	{
        public EntityAction EntityAction { get; set; }
	}
	public enum EntityAction
	{
		Create = 1,
		Update = 2,
		Delete = 3
	}
    public class GetNextCodeInputDto
    {
        public string TableName { get; set; }
        public string CodeField { get; set; }
        public string AddWhere { get; set; }

    }

    public class MailData
    {

        public string Sender { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string DefaultFromDisplayName { get; set; }
    }

    public class SendEmailRequest
    {
        public string Subject { get; set; }
        public string Url { get; set; }
        public string UrlTitle { get; set; }
        public string[] Emails { get; set; }
        public string[] datalst { get; set; }
        public string ParamsHeader { get; set; }
        public Dictionary<string, string> Paramslst { get; set; }
        public bool IsBodyHtml { get; set; }
    }
}
