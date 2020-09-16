using System;
using System.Linq;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using MailjetApiClient.Models;
using Newtonsoft.Json.Linq;
using Serilog;

namespace MailjetApiClient
{
    public class MailjetService: IMailjetApiClient
    {
        private readonly MailjetClient _client;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _testingRedirectionMail;
        private readonly bool _isSendingMailAllowed;


        public MailjetService(MailjetOptions options)
        {
            _client = new MailjetClient(options.ApiKeyPublic, options.ApiKeyPrivate)
            {
                Version = ApiVersion.V3_1
            };
            _senderEmail = options.SenderEmail;
            _senderName = options.SenderName;

            _testingRedirectionMail = options.TestingRedirectionMail;
            _isSendingMailAllowed = options.IsSendingMailAllowed ?? true;
        }
        
        private bool IsInTestMode()
        {
            return !string.IsNullOrEmpty(_testingRedirectionMail);
        }

        public async Task<bool> SendMail(MailjetMail mailJetMail)
        {
            if (!_isSendingMailAllowed)
            {
                return true;
            }
            try
            {
                var mailTo = IsInTestMode() ? 
                    new JArray{ new JObject( new JProperty("Email", _testingRedirectionMail), new JProperty("Name", " ") )} : 
                    new JArray { from user in mailJetMail.Users select new JObject( new JProperty("Email", user.Email), new JProperty("Name", user.Email) ) };


                var mailCc = new JArray ();
                    
                if (mailJetMail.UsersInCc != null && mailJetMail.UsersInCc.Any())
                {
                    mailCc = IsInTestMode() ? 
                        new JArray { new JObject( new JProperty("Email", _testingRedirectionMail), new JProperty("Name", "TESTING") )} : 
                        new JArray { from userCc in mailJetMail.UsersInCc select new JObject( new JProperty("Email", userCc.Email), new JProperty("Name", userCc.Email) ) };
                }
                    
                // Mail
                var request = new MailjetRequest
                {
                    Resource = Send.Resource
                }
                .Property(Send.Messages, new JArray
                {
                    new JObject
                    {
                        {"From", new JObject { new JProperty("Email", _senderEmail), new JProperty("Name", _senderName) }},
                        {"To", mailTo},
                        {"Bcc", mailCc},
                        {"TemplateID", mailJetMail.TemplateId},
                        {"TemplateLanguage", true},
                        {"Variables", mailJetMail.Variables},
                        {"Attachments", !mailJetMail.AttachmentFiles.Any() ? null : 
                                new JArray { from file in mailJetMail.AttachmentFiles select 
                                new JObject
                                {
                                    {"ContentType", file.ContentType},
                                    {"Filename", file.Filename},                                
                                    {"Base64Content", file.Base64Content}                                
                                }
                            }
                        }, 
                        {"TemplateErrorReporting", IsInTestMode() ? 
                            new JObject {
                                new JProperty("Email", _testingRedirectionMail),
                                new JProperty("Name", _testingRedirectionMail) 
                            }
                            : null  
                        },
                        {"TemplateErrorDeliver", true}
                    }
                });
                
                var response = await _client.PostAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Log.Information($"Total: {response.GetTotal()}, Count: {response.GetCount()}\n");
                    Log.Information(response.GetData().ToString());
                    return true;
                }
                else
                {
                    Log.Error($"StatusCode: {response.StatusCode}\n");
                    Log.Error($"ErrorInfo: {response.GetErrorInfo()}\n");
                    Log.Error(response.GetData().ToString());
                    Log.Error($"ErrorMessage: {response.GetErrorMessage()}\n");
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
                Log.Error("InnerException", e.InnerException);
                return false;
            }     
        }
    }
}