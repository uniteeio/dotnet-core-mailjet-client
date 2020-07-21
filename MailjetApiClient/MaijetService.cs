using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using MailjetApiClient.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;
using Serilog;
using Microsoft.Extensions.Hosting;
using User = MailjetApiClient.Models.User;

namespace MailjetApiClient
{
    public class MailjetService: IMailjetApiClient
    {
        private readonly MailjetClient _client;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly bool _enableMailjetInDevEnv;
        private readonly string _sendMailToInDevEnv;
        private readonly bool _emulateProduction;
        
        private readonly IWebHostEnvironment _env;
        
        public MailjetService(MailjetOptions options, IWebHostEnvironment env)
        {
            _env = env;
            _client = new MailjetClient(options.ApiKeyPublic, options.ApiKeyPrivate)
            {
                Version = ApiVersion.V3_1
            };
            _senderEmail = options.SenderEmail;
            _senderName = options.SenderName;

            // Used in dev environnement only
            _enableMailjetInDevEnv = options.EnableMailjetInDevEnv;
            _sendMailToInDevEnv = options.SendMailToInDevEnv;
            _emulateProduction = options.EmulateProduction;
        }

        private bool IsProduction() 
        {
            return _env.EnvironmentName == Environments.Production || _emulateProduction;
        }

        public async Task<bool> SendMail(IEnumerable<User> users, int templateId, JObject variables = null, MailAttachmentFile attachmentFile = null, List<User> usersInCc = null)
        {
            try
            {
                var mailTo = !IsProduction() ? 
                    new JArray{ new JObject( new JProperty("Email", _sendMailToInDevEnv), new JProperty("Name", " ") )} : 
                    new JArray { from m in users select new JObject( new JProperty("Email", m.Email), new JProperty("Name", m.Email) ) };


                var mailCc = new JArray ();
                if (IsProduction())
                {
                    if (usersInCc != null)
                    {
                        foreach (var user in usersInCc)
                        {
                            mailCc.Add( new JObject( new JProperty("Email", user.Email), new JProperty("Name", user.Email) ));
                        }
                    }
                }
                else
                {
                    if (usersInCc == null || !usersInCc.Any())
                    {
                        mailCc = new JArray { new JObject( new JProperty("Email", _sendMailToInDevEnv), new JProperty("Name", " ") )};
                    }
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
                        {"TemplateID", templateId},
                        {"TemplateLanguage", true},
                        {"Variables", variables},
                        {"Attachments", attachmentFile == null ? null : new JArray {
                                new JObject
                                {
                                    {"ContentType", attachmentFile.ContentType},
                                    {"Filename", attachmentFile.Filename},                                
                                    {"Base64Content", attachmentFile.Base64Content}                                
                                }
                            }
                        }, 
                        {"TemplateErrorReporting", IsProduction() ? null : new JObject {
                                new JProperty("Email", _sendMailToInDevEnv),
                                new JProperty("Name", _sendMailToInDevEnv)
                            }
                        },
                        {"TemplateErrorDeliver", true}
                    }
                });
                
                if (!IsProduction() && !_enableMailjetInDevEnv)
                    return true;
                
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