using System.Collections.Generic;
using Newtonsoft.Json;

namespace MailjetApiClient.Models;

public class MailjetMailUser
{
    [JsonProperty(PropertyName = "Email")]
    public string Email { get; set; }

    [JsonProperty(PropertyName = "Name")]
    public string Name { get; set; }
}

public class MailjetAttachement
{
    [JsonProperty(PropertyName = "ContentType")]
    public string ContentType { get; set; }

    [JsonProperty(PropertyName = "Filename")]
    public string Filename { get; set; }

    [JsonProperty(PropertyName = "Base64Content")]
    public string Base64Content { get; set; }
}

public class MailjetMessage<T>
{
    [JsonProperty(PropertyName = "From", NullValueHandling = NullValueHandling.Ignore)]
    public MailjetMailUser From { get; set; }

    [JsonProperty(PropertyName = "To")]
    public List<MailjetMailUser> To { get; set; }

    [JsonProperty(PropertyName = "Cc", NullValueHandling = NullValueHandling.Ignore)]
    public List<MailjetMailUser> Cc { get; set; }

    [JsonProperty(PropertyName = "Bcc", NullValueHandling = NullValueHandling.Ignore)]
    public List<MailjetMailUser> Bcc { get; set; }

    [JsonProperty(PropertyName = "TemplateID")]
    public int TemplateId { get; set; }

    [JsonProperty(PropertyName = "TemplateLanguage")]
    public bool TemplateLanguage { get; } = true;

    [JsonProperty(PropertyName = "Variables", NullValueHandling = NullValueHandling.Ignore)]
    public T Variables { get; set; }

    [JsonProperty(PropertyName = "Attachments")]
    public List<MailjetAttachement> Attachements { get; set; }

    [JsonProperty(PropertyName = "TemplateErrorReporting")]
    public MailjetMailUser TemplateErrorReporting { get; set; }

    [JsonProperty(PropertyName = "TemplateErrorDeliver")]
    public bool TemplateErrorDeliver { get; } = true;
}
