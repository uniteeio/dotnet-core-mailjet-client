# mailjet-client

Client to easily connect a dotnet core application with Mailjet

## Configuration


### Set up Mailjet variables in appSettings.json

To use this extensions, you need to define in the `MailjetApi` section the following variables in your appSettings.json.

 `SenderEmail` : mail used to send mails 
 
 `SenderName` : name used to send mail
 
 `ApiKeyPublic` : mailjet public key (https://app.mailjet.com/account/api_keys)
   
 `ApiKeyPrivate` : mailjet private key (https://app.mailjet.com/account/api_keys) 
 
If you are in Staging or Development mode, you can send mail to a specific mail address setting the value of `EnableMailjetInDevEnv` to `true` and the value of `SendMailToInDevEnv` to your testing mail address.        

```
         "MailjetApi": {
           "EnableMailjetInDevEnv": true,
           "SendMailToInDevEnv": "john.doo@unitee.io",
           "SenderEmail": "sendermail@unitee.io",
           "SenderName": "unitee.io",
           "ApiKeyPublic": "xxxxxxxxxxxxxxxxxxx",
           "ApiKeyPrivate": "xxxxxxxxxxxxxxxxxx"
         },
``` 

### Add Extensions in Startup.cs

```
        private IHostingEnvironment _env;
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
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

## How to use it

### Inject service 

```
private readonly IMailjetApiClient _iMailjetApiClient;

public FooService(IMailjetApiClient iMailjetApiClient){

    _iMailjetApiClient = iMailjetApiClient;

}
``` 

### Use send mail method

You can use the send mail method following the example about to send a mail via a Mailjet Template.
Some parameters are optionals (attachementFile, variables, Cc mails)

```
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
            Base64Content = base64Content 
        },
        new []{new User{Email = "mailCc@unitee.io"}} 
    );
```


