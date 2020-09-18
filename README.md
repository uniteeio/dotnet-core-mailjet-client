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

### Send an email

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
    


