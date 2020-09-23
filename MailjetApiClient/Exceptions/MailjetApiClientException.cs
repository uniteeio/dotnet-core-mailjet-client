using System;

namespace MailjetApiClient.Exceptions
{
    public class MailjetApiClientException : Exception
    {
        public MailjetApiClientException(string message) : base(message) {}
        public MailjetApiClientException(string message, Exception eInnerException) : base(message, eInnerException) {}
    }
}