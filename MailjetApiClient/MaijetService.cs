using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using MailjetApiClient.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using User = MailjetApiClient.Models.User;

namespace MailjetApiClient
{
    public class MailjetService: IMailjetApiClient
    {
        private readonly MailjetClient _clientV3_1;
        private readonly MailjetClient _clientV3;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly bool _enableMailjetInDevEnv;
        private readonly string _sendMailToInDevEnv;
        private readonly bool _emulateProduction;
        
        private readonly IHostingEnvironment _env;
        
        public MailjetService(MailjetOptions options, IHostingEnvironment env)
        {
            _env = env;
            _clientV3_1 = new MailjetClient(options.ApiKeyPublic, options.ApiKeyPrivate)
            {
                Version = ApiVersion.V3_1
            };
            _clientV3 = new MailjetClient(options.ApiKeyPublic, options.ApiKeyPrivate)
            {
                Version = ApiVersion.V3
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
            return _env.IsProduction() || _emulateProduction;
        }
        public async Task AddContact(bool isExcluded, string contactName, string contactEmail, string contactListID = "") 
        {
            MailjetRequest request = new MailjetRequest
            {
                Resource = Contact.Resource,
            }
                .Property(Contact.IsExcludedFromCampaigns, isExcluded)
                .Property(Contact.Name, contactName)
                .Property(Contact.Email, contactEmail);
            MailjetResponse response = await _clientV3.PostAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount()));
                Console.WriteLine(response.GetData());
                //if the contact is successfully created, push it to a contact list, if it's id is given
                if (contactListID != "")
                {
                    try
                    {
                        var responseData = response.GetData();
                        int ID = (int)responseData[0]["ID"];
                        MailjetRequest requestToContactList = new MailjetRequest
                        {
                            Resource = ContactManagecontactslists.Resource,
                            ResourceId = ResourceId.Numeric(ID)
                        }.Property(ContactManagecontactslists.ContactsLists, new JArray {
                            new JObject {
                                {"Action", "addnoforce"},
                                {"ListID", contactListID}
                            }
                        });
                        MailjetResponse responseContactList = await _clientV3.PostAsync(requestToContactList);
                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", responseContactList.GetTotal(), responseContactList.GetCount()));
                            Console.WriteLine(responseContactList.GetData());
                        }
                        else
                        {
                            Console.WriteLine(string.Format("StatusCode: {0}\n", responseContactList.StatusCode));
                            Console.WriteLine(string.Format("ErrorInfo: {0}\n", responseContactList.GetErrorInfo()));
                            Console.WriteLine(responseContactList.GetData());
                            Console.WriteLine(string.Format("ErrorMessage: {0}\n", responseContactList.GetErrorMessage()));
                        }
                    } catch (Exception e)
                    {
                        Log.Error(e.Message);
                        Log.Error(e.StackTrace);
                        Log.Error("InnerException", e.InnerException);
                    }
                }     
            }
            else
            {
                Console.WriteLine(string.Format("StatusCode: {0}\n", response.StatusCode));
                Console.WriteLine(string.Format("ErrorInfo: {0}\n", response.GetErrorInfo()));
                Console.WriteLine(response.GetData());
                Console.WriteLine(string.Format("ErrorMessage: {0}\n", response.GetErrorMessage()));
            }
        }

        //TODO add a method to delete a contact when the ContactID is known then upgrade the addcontact method to add custom property then another method to update a contact
        public async Task<int?> GetContactID(string contactEmail)
        {
            MailjetRequest request = new MailjetRequest
            {
                Resource = Contact.Resource,
            }.Property(Contact.Email, contactEmail);
            MailjetResponse response = await _clientV3.GetAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount()));
                Console.WriteLine(response.GetData());
                var responseData = response.GetData();
                int ID = (int)responseData[0]["ID"];
                return ID;
            }
            else
            {
                Console.WriteLine(string.Format("StatusCode: {0}\n", response.StatusCode));
                Console.WriteLine(string.Format("ErrorInfo: {0}\n", response.GetErrorInfo()));
                Console.WriteLine(response.GetData());
                Console.WriteLine(string.Format("ErrorMessage: {0}\n", response.GetErrorMessage()));
                return null;
            }
        }

        public async Task DeleteContact(string contactEmail)
        {
            var ID =  Convert.ToInt64(GetContactID(contactEmail));
            MailjetRequest request = new MailjetRequest
            {
                Resource = Contactdata.Resource,
                ResourceId = ResourceId.Numeric(ID)
            };
            MailjetResponse response = await _clientV3.DeleteAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount()));
                Console.WriteLine(response.GetData());
            }
            else
            {
                Console.WriteLine(string.Format("StatusCode: {0}\n", response.StatusCode));
                Console.WriteLine(string.Format("ErrorInfo: {0}\n", response.GetErrorInfo()));
                Console.WriteLine(response.GetData());
                Console.WriteLine(string.Format("ErrorMessage: {0}\n", response.GetErrorMessage()));
            }
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
                
                var response = await _clientV3_1.PostAsync(request);
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