# Mailjet client

![Nuget](https://img.shields.io/nuget/v/Unitee.MailjetApiClient.ApiClient)

Easily connect a .NET Core application with Mailjet

## Configuration

### Set up Mailjet variables in `appSettings.json`

To use this extension, define a `MailjetApi` section containing the following informations in your configuration. 

```jsonc
//  appSettings.json
{
    "MailjetApi": {
        "SenderEmail": "sendermail@unitee.io", // email address used to send mails
        "SenderName": "unitee.io", // displayed name
        "ApiKeyPublic": "xxxxxxxxxxxxxxxxxxx", // mailjet public key (https://app.mailjet.com/account/api_keys)
        "ApiKeyPrivate": "xxxxxxxxxxxxxxxxxx", // mailjet private key (https://app.mailjet.com/account/api_keys)
    },
}
```

:warning: By design, emails are not sent in `Development` or `Staging` environments.

You can still send emails to a specific email address by setting the value of `EnableMailjetInDevEnv` to `true` and the value of `SendMailToInDevEnv` to a testing email address. This will cause every email to be sent to the testing email address.

```json
// appSettings.json
{
    "MailjetApi": {
        "EnableMailjetInDevEnv": true,
        "SendMailToInDevEnv": "john.doo@unitee.io",
        "SenderEmail": "sendermail@unitee.io",
        "SenderName": "unitee.io",
        "ApiKeyPublic": "xxxxxxxxxxxxxxxxxxx",
        "ApiKeyPrivate": "xxxxxxxxxxxxxxxxxx"
    },
}
```

If you need to send emails in `Development` environments to the real recipients, you can bypass the defaults using:

```jsonc
// appSettings.json
{
    "MailjetApi": {
        "EmulateProduction": true
    }
}
```

### Add Extensions in `Startup.cs`

```cs
private IWebHostEnvironment _env;
private IConfiguration Configuration { get; }

public Startup(IConfiguration configuration, IWebHostEnvironment env)
{
    Configuration = configuration;
    _env = env;
}


public void ConfigureServices(IServiceCollection services)
{
    [...]
    services.AddMailjetApiClient(Configuration, _env);
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

:information_source: Some parameters are optionals (attachementFile, variables, Cc mails)

```cs
await _iMailjetApiClient.SendMail(
    new []{new User{Email = "mailTo@unitee.io"}},
    MailjetTemplateId,
    new JObject
    {
        new JProperty("fooTemplateVariableKey", "foovalue"),
        new JProperty("barTemplateVariableKey", "barvalue"),
    },
    new MailAttachmentFile()
    {
        Filename = filename,
        ContentType = contentType,
        Base64Content = base64Content,
    },
    new []{new User{Email = "mailCc@unitee.io"}}
);
```


