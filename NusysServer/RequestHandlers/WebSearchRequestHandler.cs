using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// Class used to handle the web search request
    /// </summary>
    public class WebSearchRequestHandler : FullArgsRequestHandler<WebSearchRequestArgs, ServerReturnArgsBase>
    {
        /// <summary>
        /// This method should not forward the request to anybody, but should rather just 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected override ServerReturnArgsBase HandleArgsRequest(WebSearchRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var searchString = args.SearchString;
            Task.Run(async delegate
            {
                try
                {
                    await RunParser(searchString, senderHandler);
                }
                catch (Exception e)
                {
                    senderHandler.SendError(e);
                    ErrorLog.AddError(e);
                }
            });
            return new ServerReturnArgsBase() {WasSuccessful =  true};
        }

        /// <summary>
        /// Method to acutally instanitate and run the parser with the given searchString
        /// </summary>
        /// <param name="searchString"></param>
        private async Task RunParser(string searchString, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(senderHandler != null);
            var userId = NusysClient.IDtoUsers[senderHandler].UserID;
            var parsed = await HtmlImporter.RunWithSearch(searchString);
            parsed.RemoveAt(0);
            var docs =
                parsed.SelectMany(i => i)
                    .Where(
                        dh =>
                            dh?.Content != null &&
                            (dh.Content.ContentType == NusysConstants.ContentType.Text ||
                             dh.Content.ContentType == NusysConstants.ContentType.Image));
            if (docs.Count() > 50)
            {
                docs = docs.Take(50);
            }
            var requests = new List<Request>();
            foreach (var doc in docs)
            {
                var m = await MakeContentMessage(doc, userId,  searchString);
                if (m != null)
                {
                    requests.Add(m);
                }
            }
            foreach (var request in requests.Where(i => i != null))
            {
                try
                {
                    var handler = new CreateNewContentRequestHandler();
                    var m = handler.HandleRequest(request, senderHandler);
                    m[NusysConstants.REQUEST_TYPE_STRING_KEY] = NusysConstants.RequestType.CreateNewLibraryElementRequest;
                    senderHandler.Send(m.GetSerialized());
                }
                catch (Exception e)
                {
                    var f = new Exception(e.Message + ".  FAILED IN RUNPARSER" );
                    ErrorLog.AddError(f);
                }
            }

            var notification = new WebSearchCompletedNotification(new WebSearchCompletedNotificationArgs()
            {
                OriginalSearch = searchString,
                LibraryElementIds = new List<string>(docs.Select(doc => doc?.LibraryElement?.LibraryElementId).Where(i => i != null))
            });
            senderHandler?.Notify(notification);

            Task.Run(async delegate
            {
                await RunTopicModelling(docs.Where(doc => doc.Content.ContentType == NusysConstants.ContentType.Text),
                    senderHandler, searchString);
            });

        }

        /// <summary>
        /// private method to run topic modelling asynchronously on tect documents 
        /// </summary>
        /// <param name="docs"></param>
        /// <returns></returns>
        private async Task RunTopicModelling(IEnumerable<DataHolder> docs, NuWebSocketHandler senderHandler, string searchString)
        {
            try
            {
                var tags = await new BingCognitiveServiceSender().RunTextAnalysis(new List<DataHolder>(docs));
                foreach (var kvp in tags)
                {
                    var message = new Message();
                    message[NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID] = kvp.Key;

                    var kvpTags = new List<Keyword>(kvp.Value.Concat(new List<Keyword>() {new Keyword("Search for: " + searchString)}));

                    //var currentKeywordsQuery = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.LibraryElement,Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY)),new SqlQueryEquals(Constants.SQLTableType.LibraryElement,NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY,kvp.Key));
                    //var currentKeywords = JsonConvert.DeserializeObject<List<Keyword>>(currentKeywordsQuery.ExecuteCommand().FirstOrDefault().GetString(Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY).FirstOrDefault()));

                    message[NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY] = JsonConvert.SerializeObject(kvpTags);
                    message[NusysConstants.REQUEST_TYPE_STRING_KEY] = NusysConstants.RequestType.UpdateLibraryElementModelRequest;
                    var request = new Request(NusysConstants.RequestType.UpdateLibraryElementModelRequest, message);
                    var handler = new UpdateLibraryElementRequestHandler();

                    var m = handler.HandleRequest(request, senderHandler);
                    message[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = m.GetBool(NusysConstants.REQUEST_SUCCESS_BOOL_KEY);
                    message[NusysConstants.REQUEST_TYPE_STRING_KEY] = NusysConstants.RequestType.UpdateLibraryElementModelRequest;
                    senderHandler.Send(message.GetSerialized());
                }
            }
            catch (Exception e)
            {
                ErrorLog.AddError(e);
                senderHandler?.SendError(e);
            }
        }

        /// <summary>
        /// Method to pass in a dataholder and get back request ready to be executed as create content and library element models. 
        /// </summary>
        /// <param name="dataHolder"></param>
        /// <returns></returns>
        private async Task<Request> MakeContentMessage(DataHolder dataHolder, string userId, string searchString)
        {
            Debug.Assert(dataHolder?.Content != null && dataHolder?.LibraryElement != null);
            var message = new Message();

            Debug.Assert(dataHolder.Content.ContentType == NusysConstants.ElementTypeToContentType(dataHolder.LibraryElement.Type));

            switch (dataHolder.Content.ContentType)
            {
                case NusysConstants.ContentType.PDF:
                    try
                    {
                        var webRequest = HttpWebRequest.Create(dataHolder.Content.Data);
                        HttpWebResponse response = (HttpWebResponse) (await webRequest.GetResponseAsync());
                        Stream stream = response.GetResponseStream();
                        byte[] bytes;
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            bytes = ms.ToArray();
                        }
                        dataHolder.Content.Data = Convert.ToBase64String(bytes);
                    }
                    catch (Exception e)
                    {
                        ErrorLog.AddError(e);
                        return null;
                    }
                    break;
                case NusysConstants.ContentType.Image:
                    message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_EXISTING_SMALL_ICON_URL] =
                        dataHolder.Content.Data;
                    message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_EXISTING_MEDIUM_ICON_URL] =
                        dataHolder.Content.Data;
                    message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_EXISTING_LARGE_ICON_URL] =
                        dataHolder.Content.Data;
                    break;
            }

            message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = dataHolder.Content.Data;
            var contentId = dataHolder.Content.ContentId ?? NusysConstants.GenerateId();
            message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY] = contentId;
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY] = contentId;
            message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = dataHolder.Content.ContentType.ToString();

            switch (dataHolder.LibraryElement.Type)
            {
                case NusysConstants.ElementType.Image:
                    message[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_HEIGHT] = 1;
                    message[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_WIDTH] = 1;
                    break;
                case NusysConstants.ElementType.Link:
                    Debug.Assert(dataHolder.LibraryElement is LinkLibraryElementModel);
                    message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_IN_KEY] = (dataHolder.LibraryElement as LinkLibraryElementModel).InAtomId;
                    message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_OUT_KEY] = (dataHolder.LibraryElement as LinkLibraryElementModel).OutAtomId;
                    break;
                case NusysConstants.ElementType.PDF:
                    message[NusysConstants.NEW_PDF_LIBRARY_ELEMENT_REQUEST_PAGE_END_KEY] = 10000;
                    message[NusysConstants.NEW_PDF_LIBRARY_ELEMENT_REQUEST_PAGE_START_KEY] = 0;
                    message[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_HEIGHT] = 1;
                    message[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_WIDTH] = 1;
                    break;

            }
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_ACCESS_KEY] = NusysConstants.AccessType.Public.ToString();
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATOR_USER_ID_KEY] = userId;
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] =
                dataHolder.LibraryElement.LibraryElementId;
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = dataHolder.LibraryElement.Type;
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = dataHolder.LibraryElement.Title;

            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_KEYWORDS_KEY] =
                JsonConvert.SerializeObject(new List<Keyword>()
                {
                    new Keyword("Search for: " + searchString, Keyword.KeywordSource.TagExtraction)
                });

            var request = new Request(NusysConstants.RequestType.CreateNewContentRequest, message);
            return request;
        }
    }
}