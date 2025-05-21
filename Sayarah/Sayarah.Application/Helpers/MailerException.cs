using System.Runtime.Serialization;

namespace Sayarah.Application.Helpers
{
    [Serializable]
    public class MailerException : Exception
    {
        public MailerException() : base() { }
        public MailerException(string message) : base(message) { }
        public MailerException(string format, params object[] args) : base(string.Format(format, args)) { }
        public MailerException(string message, Exception innerException) : base(message, innerException) { }
        public MailerException(string format, Exception innerException, params object[] args) : base(string.Format(format, args), innerException) { }
        protected MailerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

}
