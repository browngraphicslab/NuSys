using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace NusysServer
{
    public static class TextCognitiveRequestHelper
    {

        public static async Task<Dictionary<string, object>> MakeRequest(string jsonData)
        {
            var client = new HttpClient();

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CognitiveServicesConstants.TEXT_ANALYTICS);

            // Request parameters
            var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/topics";

            HttpResponseMessage response;

            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes(jsonData);

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);            
            }
            
            var client2 = new HttpClient();
            // Request headers
            client2.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CognitiveServicesConstants.TEXT_ANALYTICS);
            var uri2 = response.Headers.Location.AbsoluteUri;
            while (true) {
                var result = await client2.GetAsync(uri2);
                var s = await result.Content.ReadAsStringAsync();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(s);
                if (((string) dict["status"]).ToLower() == "succeeded")
                {
                    return dict;
                }
                await Task.Delay(60000);
            }




        }
    }
}


