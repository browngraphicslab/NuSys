using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace NusysServer
{
    public static class TextProcessor
    {
        public async static Task GetTextAnalytics(List<string> documentBodies)
        {
            Document[] documents = new Document[documentBodies.Count];

            var counter = 0;
            foreach(var body in documentBodies)
            {
                documents[counter] = new Document(counter.ToString(), body);
                counter++;
            }



            var jsonData = new SendTrent();
            jsonData.documents = documents;
            jsonData.stopWords = new string[] { };
            jsonData.stopPhrases = new string[] { };
            string json = JsonConvert.SerializeObject(jsonData);
            var resutl = await TextCognitiveRequestHelper.MakeRequest(json);
        }

    }
}