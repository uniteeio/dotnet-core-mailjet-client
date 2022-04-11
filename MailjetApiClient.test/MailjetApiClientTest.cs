using Xunit;
using MailjetApiClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MailjetApiClient.Models;
using static MailjetApiClient.MailjetApiClient;

namespace MailjetApiClient.test;

public class NestedClass
{
    public int I { get; set; }

    [JsonProperty(PropertyName = "field")]
    public string S { get; set; }
}

public class UnitTest1
{
    [Fact]
    public void ShoulConvertToCorrectMailjetEntity()
    {
        var options = new MailjetOptions
        {
            ApiKeyPublic = "",
            ApiKeyPrivate = "",
            IsSendingMailAllowed = true
        };

        var client = new MailjetService(options);

        var mailjetMail = new MailjetMail()
        {
            Users = new List<User> { new User { Email = "toto@toto.com" } },
        };

        var converted = client.ConvertToMailjetMessage(mailjetMail);

        Assert.Null(converted.From);
        Assert.IsType<List<MailjetMailUser>>(converted.To);
        Assert.Equal("toto@toto.com", converted.To[0].Email);
    }

    [Fact]
    public void ShouldConvertMailjetMessageToCorrectJson()
    {
        var options = new MailjetOptions
        {
            ApiKeyPublic = "",
            ApiKeyPrivate = "",
            IsSendingMailAllowed = true,
        };

        var client = new MailjetService(options);

        var mailjetMail = new MailjetMail()
        {
            Users = new List<User> { new User { Email = "toto@toto.com" } },
            Variables = new NestedClass
            {
                I = 42,
                S = "toto"
            }
        };

        var converted = client.ConvertToMailjetMessage(mailjetMail);

        var json = JObject.FromObject(converted);

        Assert.Equal("toto", json["Variables"]?["field"]?.Value<string>());
        Assert.Equal(42, json["Variables"]?["I"]?.Value<int>());
        Assert.Equal("toto@toto.com", json["To"]?[0]?["Email"]?.Value<string>());
        Assert.False(json.ContainsKey("From"));
    }
}