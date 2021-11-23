using System;
using System.Linq;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using MailjetApiClient.Exceptions;
using MailjetApiClient.Extensions;
using MailjetApiClient.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MailjetApiClient
{
    public class MailjetService : MailjetApiClient
    {
        public MailjetService(MailjetOptions options) : base(options.ApiKeyPublic, options.ApiKeyPrivate, options.SenderEmail, options.SenderName, options.TestingRedirectionMail, options.IsSendingMailAllowed) {}
    }
}