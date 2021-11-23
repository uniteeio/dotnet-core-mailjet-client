# Mailjet client

![Nuget](https://img.shields.io/nuget/v/Unitee.MailjetApiClient.ApiClient)

Easily connect a .NET Core application with Mailjet

## Install

```bash
dotnet add package Unitee.MailjetApiClient.ApiClient
```

## Configuration

### Set up Mailjet variables in `appSettings.json`

To use this extension, define a `MailjetApi` section containing the following informations in your configuration. 

```
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

For Dotnet core >= 6:

```cs
builder.Services.AddMailjetApiClient(builder.Configuration);
```

in `Program.cs`.

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
var mailjetMail = new MailjetMail()
{
    // Required properties
    Users = new List<User>() { new User { Email = "mailTo@unitee.io" } },
    TemplateId = MailjetTemplateId,

    // Optionnal properties
    Variables = new Dictionary<string, object>
    {
        { "fooTemplateVariableKey", "foovalue" },
        { "barTemplateVariableKey", "barvalue" }
    };

    UsersInCc = new List<User>() { new User() { Email = "mailCc@unitee.io" } },
    UsersInBcc = new List<User>() { new User() { Email = "mailBcc@unitee.io" } },
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

You can also use the following overload (note that 'variables' is no longer a Dictionary)

```cs
var email = "mailTo@unitee.io";
var templateId = 1337;
var variables = new { fooTemplateVariableKey = "foovalue", barTemplateVariableKey = "barvalue" };

var usersInCc = new List<User>() { new User() { Email = "mailCc@unitee.io" } };
var usersInBcc = new List<User>() { new User() { Email = "mailBcc@unitee.io" } };

await _iMailjetApiClient.SendMail(email, templateId);
await _iMailjetApiClient.SendMail(email, templatedId, variables);
await _iMailjetApiClient.SendMail(email, templatedId, variables, usersInCc);
await _iMailjetApiClient.SendMail(email, templatedId, variables, usersInCc, usersInBcc);
```
    
### Contacts Features
#### Create or update a contact 

You can use the `AddOrUpdateContact` method by following the example below to add or update a contact in mailjet and mailings lists.

```cs
var mailjetContact = new MailjetContact()
{
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


# Development

## Environment

A dev container can be started at Docker/MailjetApiClient/ with docker-compose up -d for use with vscode remote.

## Testing

Edit the MailjetApiClientTest.cs and replace the empty field of the options with your mailjet configuration as well as replace the templateId.

👏 Don't 👏 Push 👏 Your 👏 Api 👏 Key

Run `dotnet test` and expect to receive a mail at the specified mail address.

