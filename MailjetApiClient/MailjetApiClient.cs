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
using Flurl.Http;

namespace MailjetApiClient;

public class MailjetApiClient : IMailjetApiClient
{
    private readonly MailjetClient _clientV3_1;
    private readonly MailjetClient _clientV3;

    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly string _testingRedirectionMail;
    private readonly bool _isSendingMailAllowed;

    private readonly FlurlClient _httpClient;

    public MailjetApiClient(string publicKey, string privateKey, string senderEmail, string senderName, string testingRedirectionMail, bool? isSendingMailAllowed = null)
    {
        _clientV3_1 = new MailjetClient(publicKey, privateKey) { Version = ApiVersion.V3_1 };
        _clientV3 = new MailjetClient(publicKey, privateKey) { Version = ApiVersion.V3 };
        _senderEmail = senderEmail;
        _senderName = senderName;
        _testingRedirectionMail = testingRedirectionMail;
        _isSendingMailAllowed = isSendingMailAllowed ?? true;

        _httpClient = new FlurlClient("https://api.mailjet.com/v4/").WithBasicAuth(publicKey, privateKey);
    }

    private bool IsInTestMode()
    {
        return !string.IsNullOrEmpty(_testingRedirectionMail);
    }

    public async Task SendMail(string email, int templateId, object variables = null, List<Models.User> usersInCc = null, List<Models.User> usersInBcc = null)
    {
        var mailjetMail = new MailjetMail
        {
            Users = new List<Models.User> { new Models.User() { Email = email } },
            TemplateId = templateId,
            Variables = variables,
            UsersInCc = usersInCc,
            UsersInBcc = usersInBcc,
        };

        await SendMail(mailjetMail);
    }

    public MailjetMessage<object> ConvertToMailjetMessage(MailjetMail mailjetMail)
    {
        var mailjetMessage = new MailjetMessage<object>
        {
            Variables = mailjetMail.Variables,
            Attachements = mailjetMail?.AttachmentFiles?.Select(x => new MailjetAttachement
            {
                ContentType = x.ContentType,
                Filename = x.Filename,
                Base64Content = x.Base64Content
            })?.ToList(),
            TemplateId = mailjetMail.TemplateId,
            To = mailjetMail.Users?.Select(x => new MailjetMailUser { Email = x.Email, Name = x.Email })?.ToList(),
            Bcc = mailjetMail.UsersInBcc?.Select(x => new MailjetMailUser { Email = x.Email, Name = x.Email })?.ToList(),
            Cc = mailjetMail.UsersInCc?.Select(x => new MailjetMailUser { Email = x.Email, Name = x.Email })?.ToList(),
        };

        if (!string.IsNullOrEmpty(_senderEmail) && !string.IsNullOrEmpty(_senderName))
        {
            mailjetMessage.From = new MailjetMailUser { Email = _senderEmail, Name = _senderName };
        }

        if (IsInTestMode())
        {
            mailjetMessage.Bcc = null;
            mailjetMessage.Cc = null;
            mailjetMessage.To = new List<MailjetMailUser>() { new MailjetMailUser
            {
                Email = _testingRedirectionMail,
                Name = _testingRedirectionMail,
            }};
        }

        return mailjetMessage;
    }

    public async Task SendMail(MailjetMail mailjetMail)
    {
        if (!_isSendingMailAllowed)
        {
            return;
        }

        var email = ConvertToMailjetMessage(mailjetMail);
        var emails = new List<MailjetMessage<object>> { email };

        try
        {
            var request = new MailjetRequest
            {
                Resource = Send.Resource
            }
            .Property(Send.Messages, JArray.FromObject(emails));

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

    public async Task DeleteContactFromContactList(string contactEmail, string contactListId)
    {
        var maybeContactId = await GetContactId(contactEmail);
        if (maybeContactId is null)
        {
            throw new MailjetApiClientException($"Contact {contactEmail} cannot be found.");
        }

        await _httpClient
            .Request("contacts", maybeContactId)
            .DeleteAsync();
    }
}