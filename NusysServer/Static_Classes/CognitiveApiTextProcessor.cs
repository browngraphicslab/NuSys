using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// the text analytics requests that we currently support
    /// </summary>
    public enum TextAnalyticsRequestType { Topic, KeyPhrases, Sentiment}

    public static class CognitiveApiTextProcessor
    {
        /// <summary>
        /// Gets the topics for the passed in list of documents. There must be at least 100 documents.
        /// Takes optional parameters stopWords and topicsToExclude
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="stopWords">These words and their close forms (e.g. plurals) will be excluded from the entire topic detection pipeline. Use this for common words.</param>
        /// <param name="topicsToExclude">These will be excluded from the list of returned topics. Use this to exclude generic topics that you don’t want to see in the results. </param>
        /// <returns>CognitigeApiTopicModel</returns>
        public async static Task<CognitiveApiTopicModel> GetTextTopicsAsync(List<CognitiveApiDocument> documents, List<string> stopWords = null, List<string> topicsToExclude = null)
        {
            Debug.Assert(documents != null && documents.Count >= 100, "the cognitive services topic modeling requires that at least 100 documents are sent at a time");

            var jsonData = new CognitiveApiSendObject();
            jsonData.documents = documents.ToArray();

            // stop words and topics to exclude default to empty arrays
            jsonData.stopWords = stopWords?.ToArray() ?? new string[] { };
            jsonData.topicsToExclude = topicsToExclude?.ToArray() ?? new string[] { };

            // serialize the data for a text analytics request
            string json = JsonConvert.SerializeObject(jsonData);

            // get the http respose message from a text analytics request
            var response = await MakeTextAnalyticsRequestAsync(json, TextAnalyticsRequestType.Topic);

            // get the url for the topic modeling process from the text analytics request http response message
            var processURI = response.Headers.Location.AbsoluteUri;

            // get the http response message from the topic modeling process upon process finishing - note if this is null the process failed
            response = await GetProcessDataAsync(processURI);

            if(response != null)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                // test data for the model because waiting for this is too annoying
                //var responseContent = "{\"status\":\"Succeeded\",\"createdDateTime\":\"2016-08-09T16:00:12Z\",\"operationType\":\"topics\",\"operationProcessingResult\":{\"topics\":[{\"id\":\"c7532570-bb70-4342-bb04-c4a4663cdff7\",\"score\":1.0,\"keyPhrase\":\"pizza\"},{\"id\":\"4add87b6-eaef-4c7a-890a-d9da6ba21e79\",\"score\":1.0,\"keyPhrase\":\"floors\"}],\"topicAssignments\":[{\"documentId\":\"1\",\"topicId\":\"c7532570-bb70-4342-bb04-c4a4663cdff7\",\"distance\":0.0},{\"documentId\":\"1\",\"topicId\":\"4add87b6-eaef-4c7a-890a-d9da6ba21e79\",\"distance\":0.0}],\"errors\":[]}}";
                return JsonConvert.DeserializeObject<CognitiveApiTopicModel>(responseContent);
            }
            return null;
        }

        /// <summary>
        /// Gets the sentiment for the passed in list of documents. There must be at least 1 document.
        /// </summary>
        /// <param name="documents"></param>
        /// <returns>CognitiveApiSentimentModel</returns>
        public async static Task<CognitiveApiSentimentModel> GetTextSentimentAsync(List<CognitiveApiDocumentSentiment> documents)
        {
            Debug.Assert(documents != null && documents.Count >= 1, "the cognitive services sentiment analysis requires that at least 1 documents is sent at a time");

            // serialize the data for a text analytics request
            var jsonData = new CognitiveApiSendObject();
            jsonData.documents = documents.ToArray();
            string json = JsonConvert.SerializeObject(jsonData);

            // get the http respose message from a text analytics request
            var response = await MakeTextAnalyticsRequestAsync(json, TextAnalyticsRequestType.Sentiment);
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CognitiveApiSentimentModel>(responseContent);
        }

        /// <summary>
        /// Gets the sentiment for the passed in list of documents. There must be at least 1 document.
        /// Takes optional parameter language, which is the language of the documents you are finding the sentiment for.
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="language">The language of the documents you are getting the sentiment for</param>
        /// <returns>CognitiveApiDocumentSentiment</returns>
        public async static Task<CognitiveApiSentimentModel> GetTextSentimentAsync(List<CognitiveApiDocument> documents, SentimentLanguages language = SentimentLanguages.English)
        {
            Debug.Assert(documents != null && documents.Count >= 1, "the cognitive services sentiment analysis requires that at least 1 documents is sent at a time");
            List<CognitiveApiDocumentSentiment> sentimentDocs = new List<CognitiveApiDocumentSentiment>();
            foreach(var document in documents)
            {
                sentimentDocs.Add(new CognitiveApiDocumentSentiment(document, language));
            }
            return await GetTextSentimentAsync(sentimentDocs);
        }

        /// <summary>
        /// Gets the key phrases for the passed in list of documents. There must be at least 1 document.
        /// </summary>
        /// <param name="documents"></param>
        /// <returns>CognitiveApiDocumentKeyPhrases</returns>
        public async static Task<CognitiveApiKeyPhrasesModel> GetTextKeyPhrasesAsync(List<CognitiveApiDocumentKeyPhrases> documents)
        {
            Debug.Assert(documents != null && documents.Count >= 1, "the cognitive services keyphrases analysis requires that at least 1 documents is sent at a time");

            // serialize the data for a text analytics request
            var jsonData = new CognitiveApiSendObject();
            jsonData.documents = documents.ToArray();
            string json = JsonConvert.SerializeObject(jsonData);

            // get the http respose message from a text analytics request
            var response = await MakeTextAnalyticsRequestAsync(json, TextAnalyticsRequestType.KeyPhrases);
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CognitiveApiKeyPhrasesModel>(responseContent);
        }

        /// <summary>
        /// Gets the key phrases for the passed in list of documents. There must be at least 1 document.
        /// Takes optional parameter language, which is the language of the documents you are finding the key phrases for.
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="language">The language of the documents you are getting the key phrases for</param>
        /// <returns>CognitiveApiDocumentKeyPhrases</returns>
        public async static Task<CognitiveApiKeyPhrasesModel> GetTextKeyPhrasesAsync(List<CognitiveApiDocument> documents, KeyPhrasesLanguages language = KeyPhrasesLanguages.English)
        {
            Debug.Assert(documents != null && documents.Count >= 1, "the cognitive services sentiment analysis requires that at least 1 documents is sent at a time");
            List<CognitiveApiDocumentKeyPhrases> keyPhraseDocs = new List<CognitiveApiDocumentKeyPhrases>();
            foreach (var document in documents)
            {
                keyPhraseDocs.Add(new CognitiveApiDocumentKeyPhrases(document, language));
            }
            return await GetTextKeyPhrasesAsync(keyPhraseDocs);
        }

        /// <summary>
        /// Makes a post request to microsoft cognitive services for a given request type, should only be called from inside text processor
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="requestType"></param>
        /// <returns>An http response message for the post request that was passed in</returns>
        private static async Task<HttpResponseMessage> MakeTextAnalyticsRequestAsync(string jsonData, TextAnalyticsRequestType requestType)
        {
            var client = new HttpClient();

            // Add the subscription key to the request header
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CognitiveApiConstants.TEXT_ANALYTICS);

            // build the proper post request url based on the request type
            string url = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/";
            switch (requestType)
            {
                case TextAnalyticsRequestType.Topic:
                    url += "topics";
                    break;
                case TextAnalyticsRequestType.KeyPhrases:
                    url += "keyPhrases";
                    break;
                case TextAnalyticsRequestType.Sentiment:
                    url += "sentiment";
                    break;
            }

            HttpResponseMessage response;

            // Build the content of the post request
            byte[] byteData = Encoding.UTF8.GetBytes(jsonData);
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response = await client.PostAsync(url, content);
            }
        }

        /// <summary>
        /// Queries the microsoft cognitive services process until the process has finished.
        /// Returns the http response message when the process has finished
        /// 
        /// The delay between queries is one minute, so numQueries should be thought of as the maximum number of minutes
        /// the request should take
        /// </summary>
        /// <param name="processURI"></param>
        /// <param name="numQueries">Optional, limits the number of queries we send to the API, default is 30</param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> GetProcessDataAsync(string processURI, int numQueries=30)
        {
            var client = new HttpClient();
            
            // add the api key to the request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", CognitiveApiConstants.TEXT_ANALYTICS);

            var queries = 0;
            // query the api until status succeeded or max num queries exceeded
            while (queries < numQueries)
            {
                var result = await client.GetAsync(processURI);
                var s = await result.Content.ReadAsStringAsync();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(s);
                if (((string)dict["status"]).ToLower() == "succeeded")
                {
                    return result;
                }
                await Task.Delay(60000);
                Debug.WriteLine($"Query number {queries}");
                queries += 1;
            }
            // return null if max number of queries exceeeded
            return null;
        }

    }
}