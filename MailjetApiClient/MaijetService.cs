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
    public class MailjetService : IMailjetApiClient
    {
        private readonly MailjetClient _clientV3_1;
        private readonly MailjetClient _clientV3;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _testingRedirectionMail;
        private readonly bool _isSendingMailAllowed;


        public MailjetService(MailjetOptions options)
        {
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

            _testingRedirectionMail = options.TestingRedirectionMail;
            _isSendingMailAllowed = options.IsSendingMailAllowed ?? true;
        }

        private bool IsInTestMode()
        {
            return !string.IsNullOrEmpty(_testingRedirectionMail);
        }

        public async Task SendMail(string email, int templateId, object variables = null, List<Models.User> usersInCc = null, List<Models.User> usersInBcc = null)
        {
            var variablesAsDictionary = variables?.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(variables, null)); ;

            var mailjetMail = new MailjetMail
            {
                Users = new List<Models.User> { new Models.User() { Email = email } },
                TemplateId = templateId,
                Variables = variablesAsDictionary,
                UsersInCc = usersInCc,
                UsersInBcc = usersInBcc,
            };

            await SendMail(mailjetMail);
        }

        public async Task SendMail(MailjetMail mailJetMail)
        {
            if (!_isSendingMailAllowed)
            {
                return;
            }
            try
            {
                var mailTo = IsInTestMode() ?
                    new JArray { new JObject(new JProperty("Email", _testingRedirectionMail), new JProperty("Name", " ")) } :
                    new JArray { from user in mailJetMail.Users select new JObject(new JProperty("Email", user.Email), new JProperty("Name", user.Email)) };


                var mailCc = new JArray();

                if (mailJetMail.UsersInCc != null && mailJetMail.UsersInCc.Any())
                {
                    mailCc = IsInTestMode() ?
                        new JArray { new JObject(new JProperty("Email", _testingRedirectionMail), new JProperty("Name", "TESTING")) } :
                        new JArray { from userCc in mailJetMail.UsersInCc select new JObject(new JProperty("Email", userCc.Email), new JProperty("Name", userCc.Email)) };
                }

                var mailBcc = new JArray();

                if (mailJetMail.UsersInBcc != null && mailJetMail.UsersInBcc.Any())
                {
                    mailBcc = IsInTestMode() ?
                        new JArray { new JObject(new JProperty("Email", _testingRedirectionMail), new JProperty("Name", "TESTING")) } :
                        new JArray { from userBcc in mailJetMail.UsersInBcc select new JObject(new JProperty("Email", userBcc.Email), new JProperty("Name", userBcc.Email)) };
                }

                JObject variables = null;

                if (mailJetMail.Variables != null)
                {
                    variables = new JObject
                    {
                        from key in mailJetMail.Variables.Keys select new JProperty(key, mailJetMail.Variables[key])
                    };
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
                        {"Cc", mailCc},
                        {"Bcc", mailBcc},
                        {"TemplateID", mailJetMail.TemplateId},
                        {"TemplateLanguage", true},
                        {"Variables", variables },
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
                            } : null
                        },
                        {"TemplateErrorDeliver", true}
                    }
                });

                var response = await _clientV3_1.PostAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new MailjetApiClientException(response.FormatForLogs());
                }
            }
            catch (Exception e)
            {
                throw new MailjetApiClientException(e.Message, e.InnerException);
            }
        }

        public async Task AddOrUpdateContact(MailjetContact mailjetContact)
        {
            int? contactId = GetContactId(mailjetContact.ContactEmail).Result;
            if (contactId == null)
            {
                contactId = await CreateContact(mailjetContact);
            }

            //Add To a contact list
            if (!string.IsNullOrEmpty(mailjetContact.ContactListId))
            {
                await AddContactToAMailingList(mailjetContact, contactId.Value);
            }

            //update custom contact properties
            if (mailjetContact.CustomProperties.Keys.Any())
            {
                await UpdateContactCustomProperties(contactId.Value, mailjetContact);
            }
        }

        private async Task UpdateContactCustomProperties(int contactId, MailjetContact mailjetContact)
        {
            var enumerator = mailjetContact.CustomProperties.GetEnumerator();
            var request = new MailjetRequest
            {
                Resource = new ResourceInfo("contactdata/" + contactId),
            }
            .Property(Contactdata.Data,

                new JArray { from key in mailjetContact.CustomProperties.Keys select new JObject(new JProperty("Name", key), new JProperty("Value", mailjetContact.CustomProperties[key])) }
            );

            var response = await _clientV3.PutAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new MailjetApiClientException(response.FormatForLogs());
            }
        }

        private async Task<int> CreateContact(MailjetContact mailjetContact)
        {
            var request = new MailjetRequest
            {
                Resource = Contact.Resource,
            }
                .Property(Contact.IsExcludedFromCampaigns, mailjetContact.IsExcluded)
                .Property(Contact.Name, mailjetContact.ContactName)
                .Property(Contact.Email, mailjetContact.ContactEmail);

            var response = await _clientV3.PostAsync(request);
            if (!response.IsSuccessStatusCode)
            {

                throw new MailjetApiClientException(response.FormatForLogs());
            }
            var responseData = response.GetData();
            return (int)responseData[0]["ID"];
        }

        private async Task AddContactToAMailingList(MailjetContact mailjetContact, int id)
        {
            try
            {
                var requestToContactList = new MailjetRequest
                {
                    Resource = ContactManagecontactslists.Resource,
                    ResourceId = ResourceId.Numeric(id)
                }.Property(ContactManagecontactslists.ContactsLists, new JArray
                {
                    new JObject
                    {
                        {"Action", "addnoforce"},
                        {"ListID", mailjetContact.ContactListId}
                    }
                });
                var response = await _clientV3.PostAsync(requestToContactList);
                if (!response.IsSuccessStatusCode)
                {
                    throw new MailjetApiClientException(response.FormatForLogs());
                }
            }
            catch (Exception e)
            {
                throw new MailjetApiClientException(e.Message, e.InnerException);
            }
        }

        public async Task<int?> GetContactId(string contactEmail)
        {
            try
            {
                var request = new MailjetRequest
                {
                    Resource = new ResourceInfo("contact/" + contactEmail),
                };
                var response = await _clientV3.GetAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new MailjetApiClientException(response.FormatForLogs());
                }
                var responseData = response.GetData();
                return (int)responseData[0]["ID"];
            }
            catch (Exception)
            {
                return null;
            }
        }


        // Mailjet doesn't allow deleting a contact with their API (except in V4), you still need to delete it manually, but at least it won't recieve any mail from this list
        //TODO: add a method using HTTP client to delete the contact (V4 API is only accepting http requests). So you need to create the HTTP client too
        public async Task DeleteContactFromContactList(string contactEmail, string contactListId)
        {
            var id = Convert.ToInt64(GetContactId(contactEmail));
            var request = new MailjetRequest
            {
                Resource = Contactdata.Resource,
                ResourceId = ResourceId.Numeric(id)
            };
            var response = await _clientV3.DeleteAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new MailjetApiClientException(response.FormatForLogs());
            }
        }
    }
}