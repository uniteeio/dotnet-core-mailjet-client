using System;
using System.Linq;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using MailjetApiClient.Models;
using MailjetHttp;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace MailjetApiClient
{
    public class MailjetService: IMailjetApiClient
    {
        private readonly MailjetClient _clientV3_1;
        private readonly MailjetClient _clientV3;
        private readonly MailjetHttpClient _MailJetHttpClient;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _testingRedirectionMail;
        private readonly bool _isSendingMailAllowed;


        public MailjetService(MailjetOptions options)
        {
            _env = env;
            _clientV3_1 = new MailjetClient(options.ApiKeyPublic, options.ApiKeyPrivate)
            _client = new MailjetClient(options.ApiKeyPublic, options.ApiKeyPrivate)
            {
                Version = ApiVersion.V3_1
            };
            _clientV3 = new MailjetClient(options.ApiKeyPublic, options.ApiKeyPrivate)
            {
                Version = ApiVersion.V3
            };
            _MailJetHttpClient = new MailjetHttpClient();
            _senderEmail = options.SenderEmail;
            _senderName = options.SenderName;

            _testingRedirectionMail = options.TestingRedirectionMail;
            _isSendingMailAllowed = options.IsSendingMailAllowed ?? true;
        }
        
        private bool IsInTestMode()
        {
            return !string.IsNullOrEmpty(_testingRedirectionMail);
        }
        public async Task TestHttp()
        {
            //test client http
            await _MailJetHttpClient.CallHttp();
        }
        public async Task<int?> AddContact(bool isExcluded, string contactName, string contactEmail, string contactListID = "") 
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
                Log.Information($"Total: {response.GetTotal()}, Count: {response.GetCount()}\n");
                Log.Information(response.GetData().ToString());
                var responseData = response.GetData();
                int ID = (int)responseData[0]["ID"];
                //if the contact is successfully created, push it to a contact list, if it's id is given
                if (contactListID != "")
                {
                    try
                    {
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
                            Log.Information($"Total: {responseContactList.GetTotal()}, Count: {responseContactList.GetCount()}\n");
                            Log.Information(responseContactList.GetData().ToString());
                            return ID;
                        }
                        else
                        {
                            Log.Error($"StatusCode: {responseContactList.StatusCode}\n");
                            Log.Error($"ErrorInfo: {responseContactList.GetErrorInfo()}\n");
                            Log.Error(response.GetData().ToString());
                            Log.Error($"ErrorMessage: {responseContactList.GetErrorMessage()}\n");
                            return ID;
                        }
                    } catch (Exception e)
                    {
                        Log.Error(e.Message);
                        Log.Error(e.StackTrace);
                        Log.Error("InnerException", e.InnerException);
                        return ID;
                    }
                } else {
                    return ID;
                }    
            }
            else
            {
                Log.Error($"StatusCode: {response.StatusCode}\n");
                Log.Error($"ErrorInfo: {response.GetErrorInfo()}\n");
                Log.Error(response.GetData().ToString());
                Log.Error($"ErrorMessage: {response.GetErrorMessage()}\n");
                return null;
            }
        }

        //TODO upgrade the addcontact method to add custom property then another method to update a contact
        public async Task<int?> GetContactID(string contactEmail)
        {
            MailjetRequest request = new MailjetRequest
            {
                Resource = new ResourceInfo("contact/"+contactEmail),
            };
            MailjetResponse response = await _clientV3.GetAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Log.Information($"Total: {response.GetTotal()}, Count: {response.GetCount()}\n");
                Log.Information(response.GetData().ToString());
                var responseData = response.GetData();
                int ID = (int)responseData[0]["ID"];
                return ID;
            }
            else
            {
                Log.Error($"StatusCode: {response.StatusCode}\n");
                Log.Error($"ErrorInfo: {response.GetErrorInfo()}\n");
                Log.Error(response.GetData().ToString());
                Log.Error($"ErrorMessage: {response.GetErrorMessage()}\n");
                return null;
            }
        }
        
        // Mailjet doesn't allow deleting a contact with their API (except in V4), you still need to delete it manually, but at least it won't recieve any mail from this list
        //TODO: add a method using HTTP client to delete the contact (V4 API is only accepting http requests). So you need to create the HTTP client too
        public async Task<bool> DeleteContactFromContactList(string contactEmail, string contactListID)
        {
            var IDint =  GetContactID(contactEmail);
            if (IDint != null) {
                var ID = Convert.ToInt64(IDint.Result);
                MailjetRequest request = new MailjetRequest
                {
                    Resource = ContactManagecontactslists.Resource,
                    ResourceId = ResourceId.Numeric(ID)
                }.Property(ContactManagecontactslists.ContactsLists, new JArray {
                    new JObject {
                        {"Action", "remove"},
                        {"ListID", contactListID}
                    }
                });
                MailjetResponse response = await _clientV3.PostAsync(request);
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
            } else {
                Log.Error($"ErrorInfo: contact doesn't exist in mailjet\n");
                return false;
            }
        }
        public async Task<bool> SendMail(IEnumerable<User> users, int templateId, JObject variables = null, MailAttachmentFile attachmentFile = null, List<User> usersInCc = null)
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
                
                if (!IsProduction() && !_enableMailjetInDevEnv)
                    return true;
                
                var response = await _clientV3_1.PostAsync(request);
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