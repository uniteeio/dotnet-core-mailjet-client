using Xunit;
using MailjetApiClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

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
    async public Task SendMail()
    {
        var options = new MailjetOptions
        {
            ApiKeyPublic = "",
            ApiKeyPrivate = "",
            TestingRedirectionMail = "turbo.test@yopmail.com",
            SenderEmail = "",
            SenderName = "",
            IsSendingMailAllowed = true,
        };

        var client = new MailjetService(options);
        await client.SendMail("t.dolley@unitee.io", 0);
    }

    [Fact]
    public async Task Accept_NestedVariables()
    {

        var nestedInstance = new NestedClass
        {
            I = 42,
            S = "Hello"
        };

        var x = new Dictionary<string, object>
        {
            { "hello", "world" },
            { "nested", nestedInstance }
        };

        var serialized = JObject.FromObject(x);

        Assert.Equal("world", serialized["hello"].ToString());
        Assert.Equal("Hello", serialized["nested"]["field"].ToString());
    }
}