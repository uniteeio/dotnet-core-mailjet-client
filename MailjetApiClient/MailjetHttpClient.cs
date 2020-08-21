using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailjetHttp {

    public class MailjetHttpClient {
        static readonly HttpClient client = new HttpClient();

        public async Task CallHttp()
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try	
            {
                var data = JsonConvert.SerializeObject(new JObject {
                    {"IsExcludedFromCampaigns", true},
                    {"Name", "New Contact"},
                    {"Email", "passenger@mailjet.com"}
                });
                var content = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("https://api.mailjet.com/v3/REST/contact/", content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
            }
            catch(HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");	
                Console.WriteLine("Message :{0} ",e.Message);
            }
        }
    }
}
