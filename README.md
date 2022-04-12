# Mailjet client

![Nuget](https://img.shields.io/nuget/v/Unitee.MailjetApiClient.ApiClient?cachebuster=1)

Easily connect a .NET Core application with Mailjet

## Install

```bash
dotnet add package Unitee.MailjetApiClient.ApiClient
```

## Configuration

### Configure using `appSettings.json`

Define a `MailjetApi` section in appsettings.

```json5
{
    "MailjetApi": {
        // required
        "ApiKeyPublic": "xxxxxxxxxxxxxxxxxxx", // mailjet public key (https://app.mailjet.com/account/api_keys)
        "ApiKeyPrivate": "xxxxxxxxxxxxxxxxxx", // mailjet private key (https://app.mailjet.com/account/api_keys)
       
        // optional (if null, use sender defined in the template) 
        "SenderEmail": "sendermail@unitee.io", // email address used to send mails
        "SenderName": "unitee.io", // displayed name
       
         // optional (used in development)
        "IsSendingMailAllowed": "false", // disallow sending mails
        "TestingRedirectionMail": "john.doo@unitee.io" // redirect all mails
    }
}
```

### Configure using env

Also, the configuration can be defined as env, for example:

```
MailjetApi__ApiKeyPublic=xxxxxxx
...
```


### Add Extensions in `Program.cs`

```cs
builder.Services.AddMailjetApiClient(builder.Configuration);
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

## Features

### Send an email

You can use the `SendMail` method by following the example below to send an email via a Mailjet Template.

:information_source: Some parameters are optionals (attachementFiles, variables, Cc mails)

```cs
var mailjetMail = new MailjetMail
{
    // required properties
    Users = new List<User>() { new User { Email = "mailTo@unitee.io" } },
    TemplateId = MailjetTemplateId,

    // optionnal properties
    Variables = new { hello = "world" },

    UsersInCc = new List<User>() { new User { Email = "mailCc@unitee.io" } },
    UsersInBcc = new List<User>() { new User { Email = "mailBcc@unitee.io" } },
    AttachmentFiles = new List<MailAttachmentFile>() 
    {
         new MailAttachmentFile
         {
             Filename = filename,
             ContentType = contentType,
             Base64Content = base64Content,
         }
    }
};

await _iMailjetApiClient.SendMail(mailjetMail);
```

You can also use the shorthand methods:

```cs
var email = "mailTo@unitee.io";
var templateId = 1337;
var variables = new { foo = "foo", bar = "bar" };

var usersInCc = new List<User>() { new User { Email = "mailCc@unitee.io" } };
var usersInBcc = new List<User>() { new User { Email = "mailBcc@unitee.io" } };

await _iMailjetApiClient.SendMail(email, templateId);
await _iMailjetApiClient.SendMail(email, templatedId, variables);
await _iMailjetApiClient.SendMail(email, templatedId, variables, usersInCc);
await _iMailjetApiClient.SendMail(email, templatedId, variables, usersInCc, usersInBcc);
```
    
### Create or update a contact 

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

### Get contact id by mail

You can use the `GetContactId` to get id contact with mail.

```cs
var id = await _iMailjetApiClient.GetContactId("contact@unitee.io");
```
### Remove contact from mailings list

You can use the `DeleteContactFromContactList` to remove a contact from mailing list.

```cs
await _iMailjetApiClient.DeleteContactFromContactList("contact@unitee.io", "idMailingList");
```


# Development

## Environment

A dev container can be started at Docker/MailjetApiClient/ with docker-compose up -d for use with vscode remote.

Run `dotnet test`.

