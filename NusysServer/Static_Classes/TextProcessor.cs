using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the text analytics requests that we currently support
    /// </summary>
    public enum TextAnalyticsRequestType { Topic, KeyPhrases, Sentiment}

    public static class TextProcessor
    {
        /// <summary>
        /// Gets the topics for the passed in list of documents.
        /// Takes optional parameters stopWords and topicsToExclude.
        /// 
        /// CAUTION IF ANY OF THE FOLLOWING INVARIANTS ARE BROKEN FUNCTION WILL NOT WORK
        /// Max file size per doc is 30kb. 
        /// Max file size per request is 30 mb
        /// Max files per request is 1000
        /// Takes a minimum of 100 documents
        /// A Document With an Empty String will Throw an error
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="stopWords">These words and their close forms (e.g. plurals) will be excluded from the entire topic detection pipeline. Use this for common words.</param>
        /// <param name="topicsToExclude">These will be excluded from the list of returned topics. Use this to exclude generic topics that you don’t want to see in the results. </param>
        /// <returns>CognitigeApiTopicModel</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="HttpResponseException"></exception>
        private static async Task<CognitiveApiTopicModel> GetTextTopicsAsync(List<CognitiveApiDocument> documents, List<string> stopWords = null, List<string> topicsToExclude = null)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents), "The passed in list of documents cannot be null");
            }
            if (documents.Count < 100)
            {
                throw new ArgumentOutOfRangeException(nameof(documents), $"The document list passed in contained {documents.Count} items, but requires at least 100.");
            }
            if (documents.Count > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(documents), $"The document list passed in contained {documents.Count} items, but requires fewer than 1000.");
            }

            // This is a slow safety check
            CheckDocumentsAndRequest(documents, TextAnalyticsRequestType.Topic);

            var jsonData = new CognitiveApiSendObject
            {
                documents = documents.ToArray(),
                // stopwords and topicstoexclude default to empty arrays
                stopWords = stopWords?.ToArray() ?? new string[] {},
                topicsToExclude = topicsToExclude?.ToArray() ?? new string[] {}
            };

            // serialize the data for a text analytics request
            string json = JsonConvert.SerializeObject(jsonData);

            // get the http response message from a text analytics request
            var response = await MakeTextAnalyticsRequestAsync(json, TextAnalyticsRequestType.Topic);          
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpResponseException(response);
            }


            // get the url for the topic modeling process from the text analytics request http response message
            var processUrl = response.Headers.Location.AbsoluteUri;

            // get the http response message from the topic modeling process upon process finishing - note if this is null the process failed
            response = await GetProcessDataAsync(processUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpResponseException(response);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CognitiveApiTopicModel>(responseContent);
        }

        /// <summary>
        /// Gets the sentiment for the passed in list of documents
        /// 
        /// CAUTION IF ANY OF THE FOLLOWING INVARIANTS ARE BROKEN FUNCTION WILL NOT WORK
        /// Max file size per doc is 10kb. 
        /// Max file size per request is 1 mb
        /// Max files per request is 1000
        /// Takes a minimum of 1 document
        /// A Document With an Empty String will Throw an error
        /// </summary>
        /// <param name="documents"></param>
        /// <returns>CognitiveApiSentimentModel</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="HttpResponseException"></exception>
        private static async Task<CognitiveApiSentimentModel> GetTextSentimentAsync(List<CognitiveApiDocumentSentiment> documents)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents), "The passed in list of documents cannot be null");
            }
            if (!documents.Any())
            {
                throw new ArgumentException("There must be at least one document in the list of documents for sentiment analysis to work.");
            }
            if (documents.Count > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(documents), $"The document list passed in contained {documents.Count} items, but requires fewer than 1000.");
            }

            // This is a slow safety check
            CheckDocumentsAndRequest(documents, TextAnalyticsRequestType.Sentiment);

            // serialize the data for a text analytics request
            var jsonData = new CognitiveApiSendObject {documents = documents};
            string json = JsonConvert.SerializeObject(jsonData);

            // get the http respose message from a text analytics request
            var response = await MakeTextAnalyticsRequestAsync(json, TextAnalyticsRequestType.Sentiment);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpResponseException(response);
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CognitiveApiSentimentModel>(responseContent);
        }

        /// <summary>
        /// Gets the sentiment for the passed in list of documents.
        /// Takes optional parameter language, which is the language of the documents you are finding the sentiment for.
        /// 
        /// CAUTION IF ANY OF THE FOLLOWING INVARIANTS ARE BROKEN FUNCTION WILL NOT WORK
        /// Max file size per doc is 10kb. 
        /// Max file size per request is 1 mb
        /// Max files per request is 1000
        /// Takes a minimum of 1 document
        /// A Document With an Empty String will Throw an error
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="language">The language of the documents you are getting the sentiment for</param>
        /// <returns>CognitiveApiDocumentSentiment</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static async Task<CognitiveApiSentimentModel> GetTextSentimentAsync(List<CognitiveApiDocument> documents, SentimentLanguages language = SentimentLanguages.English)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents), "The passed in list of documents cannot be null");
            }
            if (!documents.Any())
            {
                throw new ArgumentException("There must be at least one document in the list of documents for sentiment analysis to work.");
            }
            if (documents.Count > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(documents), $"The document list passed in contained {documents.Count} items, but requires fewer than 1000.");
            }

            // This is a slow safety check
            CheckDocumentsAndRequest(documents, TextAnalyticsRequestType.Sentiment);

            var sentimentDocs = documents.Select(document => new CognitiveApiDocumentSentiment(document, language)).ToList();
            return await GetTextSentimentAsync(sentimentDocs);
        }

        /// <summary>
        /// Gets the key phrases for the passed in list of documents.
        /// 
        /// CAUTION IF ANY OF THE FOLLOWING INVARIANTS ARE BROKEN FUNCTION WILL NOT WORK
        /// Max file size per doc is 10kb. 
        /// Max file size per request is 1 mb
        /// Max files per request is 1000
        /// Takes a minimum of 1 document
        /// A Document With an Empty String will Throw an error
        /// </summary>
        /// <param name="documents"></param>
        /// <returns>CognitiveApiDocumentKeyPhrases</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="HttpResponseException"></exception>
        private static async Task<CognitiveApiKeyPhrasesModel> GetTextKeyPhrasesAsync(List<CognitiveApiDocumentKeyPhrases> documents)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents), "The passed in list of documents cannot be null");
            }
            if (!documents.Any())
            {
                throw new ArgumentException("There must be at least one document in the list of documents for keyphrase analysis to work.");
            }
            if (documents.Count > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(documents), $"The document list passed in contained {documents.Count} items, but requires fewer than 1000.");
            }

            // This is a slow safety check
            CheckDocumentsAndRequest(documents, TextAnalyticsRequestType.KeyPhrases);

            // serialize the data for a text analytics request
            var jsonData = new CognitiveApiSendObject();
            jsonData.documents = documents.ToArray();
            string json = JsonConvert.SerializeObject(jsonData);

            // get the http respose message from a text analytics request
            var response = await MakeTextAnalyticsRequestAsync(json, TextAnalyticsRequestType.KeyPhrases);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpResponseException(response);
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CognitiveApiKeyPhrasesModel>(responseContent);
        }

        /// <summary>
        /// Gets the key phrases for the passed in list of documents
        /// Takes optional parameter language, which is the language of the documents you are finding the key phrases for.
        /// 
        /// CAUTION IF ANY OF THE FOLLOWING INVARIANTS ARE BROKEN FUNCTION WILL NOT WORK
        /// Max file size per doc is 10kb. 
        /// Max file size per request is 1 mb
        /// Max files per request is 1000
        /// Takes a minimum of 1 document
        /// A Document With an Empty String will Throw an error
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="language">The language of the documents you are getting the key phrases for</param>
        /// <returns>CognitiveApiDocumentKeyPhrases</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static async Task<CognitiveApiKeyPhrasesModel> GetTextKeyPhrasesAsync(List<CognitiveApiDocument> documents, KeyPhrasesLanguages language = KeyPhrasesLanguages.English)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents), "The passed in list of documents cannot be null");
            }
            if (!documents.Any())
            {
                throw new ArgumentException("There must be at least one document in the list of documents for keyphrase analysis to work.");
            }
            if (documents.Count > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(documents), $"The document list passed in contained {documents.Count} items, but requires fewer than 1000.");
            }

            // This is a slow safety check
            CheckDocumentsAndRequest(documents, TextAnalyticsRequestType.KeyPhrases);

            var keyPhraseDocs = documents.Select(document => new CognitiveApiDocumentKeyPhrases(document, language)).ToList();
            return await GetTextKeyPhrasesAsync(keyPhraseDocs);
        }

        /// <summary>
        /// Returns a NusysPdfDocumentAnalysisModel for the text passed into it
        /// </summary>
        /// <param name="text">the text body of the pdf document to be analysed</param>
        /// <param name="contentDataModelId">the string Id of the contentDataModel that this analysis model is analyzing</param>
        /// <returns>NusysPdfDocumentAnalysisModel</returns>
        public static async Task<NusysPdfDocumentAnalysisModel> GetNusysPdfAnalysisModelFromTextAsync(string text, string contentDataModelId)
        {
            // create a list of cognitiveApiDocuments where each document represents a single sentence
            var id = 0;
            var sentences = Regex.Split(text, @"(?<=[\.!\?])\s+");
            var combinedRequests = sentences.Where(item => !string.IsNullOrEmpty(item)).Select(sentence => new CognitiveApiDocument(id++.ToString(), sentence)).ToList();

            // split the total list of cognitiveApiDocuments into requests containing 1000 docunments each (due to api restrictions)
            var requestList = new List<List<CognitiveApiDocument>>();
            for (int i = 0; i < combinedRequests.Count; i++)
            {
                if (i%1000 == 0 && i != 0)
                {
                    requestList.Add(combinedRequests.GetRange(i - 1000, 1000));
                }
            }
            requestList.Add(combinedRequests.GetRange(combinedRequests.Count - combinedRequests.Count % 1000,combinedRequests.Count % 1000) );

            // create a list of nusysPdfDocumentSegmentModels by iterating through each of the requests
            var segmentList = new List<NusysPdfSegmentAnalysisModel>();
            foreach (var request in requestList)
            {
                var phrasesModel = await GetTextKeyPhrasesAsync(request);
                var sentimentModel = await GetTextSentimentAsync(request);
                segmentList.AddRange(GetNuSysPdfSegments(request, sentimentModel, phrasesModel));
            }

            // return a new nusysPdfDocumentModel with the passed in segmentList and the contentDatamModelId set
            return new NusysPdfDocumentAnalysisModel(contentDataModelId) {Segments = segmentList};
        }

        /// <summary>
        /// Given a list of requests, a phrasesModel, and a sentiment model, returns an IEnumberable of NusysPdfSegmentModel
        /// </summary>
        /// <param name="request"></param>
        /// <param name="sentimentModel"></param>
        /// <param name="keyPhrasesModel"></param>
        /// <returns></returns>
        private static IEnumerable<NusysPdfSegmentAnalysisModel> GetNuSysPdfSegments(IEnumerable<CognitiveApiDocument> request, CognitiveApiSentimentModel sentimentModel,
            CognitiveApiKeyPhrasesModel keyPhrasesModel)
        {
            var segmentDictionary = new Dictionary<string, NusysPdfSegmentAnalysisModel>();
            foreach (var document in request)
            {
                segmentDictionary.Add(document.id, new NusysPdfSegmentAnalysisModel {Text = document.text});           
            }
            foreach (var document in sentimentModel.documents)
            {
                segmentDictionary[document.id].SentimentRating = document.score;
            }
            foreach (var document in keyPhrasesModel.documents)
            {
                segmentDictionary[document.id].KeyPhrases = document.keyPhrases;
            }

            return segmentDictionary.Values;

        }


        /// <summary>
        /// Makes a post request to microsoft cognitive services for a given request type, should only be called from inside text processor
        /// </summary>
        /// <param name="jsonData"></param>
        /// <param name="requestType"></param>
        /// <returns>An http response message for the post request that was passed in</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static async Task<HttpResponseMessage> MakeTextAnalyticsRequestAsync(string jsonData, TextAnalyticsRequestType requestType)
        {
            if (jsonData == null)
            {
                throw new ArgumentNullException(nameof(jsonData),
                    "the passed in jsonData string was null, this exception was caught to avoid throwing more later on.");
            }


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
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestType), requestType, "The method does not support the passed in request type, please add support for this if you need to use it");
            }

            // Build the content of the post request
            byte[] byteData = Encoding.UTF8.GetBytes(jsonData);
            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return await client.PostAsync(url, content);
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
        private static async Task<HttpResponseMessage> GetProcessDataAsync(string processURI, int numQueries = 30)
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
                if (((string) dict["status"]).ToLower() == "succeeded")
                {
                    return result;
                }
                await Task.Delay(60000);
                queries += 1;
            }
            // return null if max number of queries exceeeded
            return null;
        }

        #region testing helper code

        /// <summary>
        /// This is a highly inefficient please only call in debug.asserts
        /// 
        /// Foreach document check document less than max document size
        /// Foreach document does not contain empty string
        /// For request checks not greater than maximum size
        /// </summary>
        /// <param name="documents">The </param>
        /// <param name="requestType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static bool CheckDocumentsAndRequest(IEnumerable<ICognitiveApiDocumentable> documents, TextAnalyticsRequestType requestType)
        {
            int maxBytesPerDoc;
            int maxBytesPerRequest;
            var totalByteLength = 0; // used to keep track of total request size
            var idHash = new HashSet<string>();

            // set byte limits based on the request type
            switch (requestType)
            {
                case TextAnalyticsRequestType.KeyPhrases:
                case TextAnalyticsRequestType.Sentiment:
                    maxBytesPerDoc = (int) ConvertKilobytesToBytes(10); // 10kb
                    maxBytesPerRequest = (int) ConvertMegaBytesToBytes(1); // 1mb
                    break;
                case TextAnalyticsRequestType.Topic:
                    maxBytesPerDoc = (int) ConvertKilobytesToBytes(30); // 30kb
                    maxBytesPerRequest = (int) ConvertMegaBytesToBytes(30); // 30mb
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestType), requestType, "The method does not support the passed in request type, please add support for this if you need to use it");
            }

            // make sure none of the documents are greater than maxBytesPerDoc, and that none of them are empty strings
            foreach (var document in documents)
            {
                var docByteLength = document.text.Length*sizeof(char);
                totalByteLength += docByteLength;
                if (docByteLength > maxBytesPerDoc)
                {
                    throw new ArgumentOutOfRangeException(nameof(document), $"The document byte length {docByteLength} exceeds the maximum allowed for {requestType} of {maxBytesPerDoc}");
                }
                if (string.IsNullOrEmpty(document.text))
                {
                    throw new ArgumentException(nameof(document.text), $"The document's text field cannot be empty. Please check for and remove empty text fields before calling a cognitive services request.");
                }
                if (idHash.Contains(document.id))
                {
                    throw new ArgumentException(nameof(document.id), $"The request contained two instances of the id {document.id}. Please make sure the invariant, one instance of an id per request is maintained.");
                }
                idHash.Add(document.id);
            }

            // make sure that the total size of the request is less than maxBytesPerRequest
            if (totalByteLength > maxBytesPerRequest)
            {
                throw new ArgumentOutOfRangeException(nameof(documents), $"The request byte length {totalByteLength} exceeds the maximum allowed for {requestType} of {maxBytesPerRequest}");
            }
            return totalByteLength >= maxBytesPerRequest;
        }

        /// <summary>
        /// Converts Megabytes to Bytes
        /// </summary>
        /// <param name="megaBytes"></param>
        /// <returns></returns>
        private static double ConvertMegaBytesToBytes(int megaBytes)
        {
            return (megaBytes*1024f)*1024f;
        }

        /// <summary>
        /// Converts Kilobytes to bytes
        /// </summary>
        /// <param name="kiloBytes"></param>
        /// <returns></returns>
        private static double ConvertKilobytesToBytes(long kiloBytes)
        {
            return kiloBytes*1024f;
        }

        #endregion testing helper code
    }
}