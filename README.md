# Mailjet client

![Nuget](https://img.shields.io/nuget/v/Unitee.MailjetApiClient.ApiClient)

Easily connect a .NET Core application with Mailjet

## Configuration

### Set up Mailjet variables in `appSettings.json`

To use this extension, define a `MailjetApi` section containing the following informations in your configuration. 

```json
//  appSettings.json
{
    "MailjetApi": {
        
        // Mandatory parameters
        "SenderEmail": "sendermail@unitee.io", // email address used to send mails
        "SenderName": "unitee.io", // displayed name
        "ApiKeyPublic": "xxxxxxxxxxxxxxxxxxx", // mailjet public key (https://app.mailjet.com/account/api_keys)
        "ApiKeyPrivate": "xxxxxxxxxxxxxxxxxx", // mailjet private key (https://app.mailjet.com/account/api_keys)
        
        // Optionnal parameters
        "IsSendingMailAllowed": "false", // disallow sending mails
        "TestingRedirectionMail": "john.doo@unitee.io" // mail used to redirect all mails. Useful for testing 
    }
}
```

### Add Extensions in `Startup.cs`

```cs
private IConfiguration Configuration { get; }

public Startup(IConfiguration configuration)
{
    Configuration = configuration;
}


public void ConfigureServices(IServiceCollection services)
{
    [...]

    services.AddMailjetApiClient(Configuration);

    [...]
}
``` 

## How to use

### Inject the service

Use the dependency injection to inject the service into your class.

```cs
private readonly IMailjetApiClient _iMailjetApiClient;

public FooService(IMailjetApiClient iMailjetApiClient)
{
    _iMailjetApiClient = iMailjetApiClient;
}
``` 

### Mailing Features
#### Send an email

You can use the `SendMail` method by following the example below to send an email via a Mailjet Template.

:information_source: Some parameters are optionals (attachementFiles, variables, Cc mails)

```cs
var mailjetMail = new MailjetMail(){
    // Required properties
    Users = new List<User>(){new User{Email = "mailTo@unitee.io"}}, 
    TemplateId = MailjetTemplateId, 
    
    // Optionnal properties
    Variables = new JObject
    {
        new JProperty("fooTemplateVariableKey", "foovalue"),
        new JProperty("barTemplateVariableKey", "barvalue"),
    },
    UsersInCc = new List<User>(){new User(){Email = "mailCc@unitee.io"}},
    AttachmentFiles = new List<MailAttachmentFile>(){
         new MailAttachmentFile()
         {
             Filename = filename,
             ContentType = contentType,
             Base64Content = base64Content,
         }
    }
};

await _iMailjetApiClient.SendMail(mailjetMail);
```
    
### Contacts Features
#### Create or update a contact 

You can use the `AddOrUpdateContact` method by following the example below to add or update a contact in mailjet and mailings lists.

```cs
var mailjetContact = new MailjetContact(){
    
    // Required properties
    ContactEmail = "contact@unitee.io",
    
    // Optionnal properties
    ContactName = "John Doe",
    ContactListId = 1, 
    IsExcluded = false,
    CustomProperties = new Dictionary<string, string>
       {
           { "customProperties1", "value1" },
           { "customProperties2", "value2" }
       };


};
await _iMailjetApiClient.AddContact(mailjetContact);
```

#### Get contact id by mail

You can use the `GetContactId` to get id contact with mail.

```cs
await _iMailjetApiClient.GetContactId("contact@unitee.io");
```
#### Remove contact from mailings list

You can use the `DeleteContactFromContactList` to remove a contact from mailing list.

```cs
await _iMailjetApiClient.DeleteContactFromContactList("contact@unitee.io", "idMailingList");
```
