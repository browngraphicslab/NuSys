﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LDAUser;
using Newtonsoft.Json;
using NusysIntermediate;
using Image = System.Drawing.Image;

namespace NusysServer
{
    /// <summary>
    /// this class will be used to process media asynchronously and alert the clients when the analysis models have been created.
    /// </summary>
    public class MediaProcessor
    {
        //Dll imports and things for MuPdf
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Open(byte[] data, int length);
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ActivateDocument(IntPtr document);
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderPage(int width, int height);
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTextBytes(byte[] sb);
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetBuffer();
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPageWidth();
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPageHeight();
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumComponents();
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumPages();
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GotoPage(int page);
        [DllImport("mupdfapit1", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Dispose(IntPtr pointer);

        /// <summary>
        /// this method will process media from a CreateContentDataModelRequest's request message.  
        /// This will also take in the URL of the content uploaded.
        /// The sender handler is the NuWebSocketHandler of the original message sender.   
        /// </summary>
        /// <param name="contentDataModelMessage"></param>
        /// <param name="contentUrl"></param>
        /// <param name="elementType"></param>
        /// <param name="senderHandler"></param>
        public static void ProcessCreateContentDataModelRequestMedia(Message contentDataModelMessage, string contentDataModelId, string contentUrl, NusysConstants.ElementType elementType, NuWebSocketHandler senderHandler)
        {
            var title = contentDataModelMessage.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY); //get the current title of tje library element model used for this

            //create a new async Task so we don't slow down the request
            Task.Run(async delegate
            {
                var contentType = NusysConstants.ElementTypeToContentType(elementType);

                //create an empty analysis model, default to null
                AnalysisModel analysisModel = null;
                switch (contentType)
                {
                    case NusysConstants.ContentType.PDF:
                        //store the pdf text as local variable
                        var pdfText = new List<string>();

                        var pdfBytes = Convert.FromBase64String(contentDataModelMessage.GetString(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES));

                        // Open the pdf with MuPdf a store a reference to it (doc)
                        var doc = Open(pdfBytes, pdfBytes.Length);

                        // Active the pdf document
                        ActivateDocument(doc);

                        for (int i = 0; i < GetNumPages(); i++)
                        {
                            // Goto a page
                            GotoPage(i);

                            // Allocate a buffer which the text will be written to
                            byte[] textBuffer = new byte[10000];

                            // Write the text on the pdf page to textBuffer
                            var numTextBytes = GetTextBytes(textBuffer);

                            // Convert the extracted text bytes to a utf8 string
                            var textString = Encoding.UTF8.GetString(textBuffer.Take(numTextBytes).ToArray());
                            pdfText.Add(textString);

                        }
                        // Free up memory
                        Dispose(doc);

                        if (pdfText != null && pdfText.Any())
                        {
                            try
                            {
                                var tup = new Tuple<string, string>(string.Join("",pdfText), contentDataModelId);
                                //ContentController.Instance.ComparisonController.AddDocument(tup, title);
                            }
                            catch (Exception e)
                            {
                                ErrorLog.AddError(e);
                                senderHandler.SendError(e);
                            }
                            try
                            {
                                var pdfDocModel = await TextProcessor.GetNusysPdfAnalysisModelFromTextAsync(pdfText);//get the document analysis

                                var pageUrls = JsonConvert.DeserializeObject<List<string>>(contentUrl);//get the list of urls for the pdf pages as images

                                var OCRModels = new NuSysOcrAnalysisModel[pageUrls.Count()];//create empty list for the page model analyses

                                int returned = 0;
                                var i = 0;
                                foreach (var pageUrl in pageUrls)
                                {
                                    Task.Run(async delegate {
                                        RunPageOcr(i, OCRModels, pageUrl, senderHandler);
                                    });
                                    i++;
                                }

                                var millisecondDelay = 200;
                                var maxSecondsToRetrieve = 90;
                                var totalDelay = 0;
                                while (OCRModels.Any(model => model == null))
                                {
                                    totalDelay += millisecondDelay;
                                    await Task.Delay(millisecondDelay); //wait until all the pages return
                                    if (totalDelay >= maxSecondsToRetrieve * 1000)
                                    {
                                        break;
                                    }
                                }

                                for (int k = 0; k < OCRModels.Length; k++) //for every index, if its null just create an empty model
                                {
                                    if (OCRModels[k] == null)
                                    {
                                        OCRModels[k] = new NuSysOcrAnalysisModel();
                                    }
                                }

                                var topics = await GetTopicsOfText(string.Join(" ", pdfText ?? new List<string>()), title ?? "") ?? new List<string>(); //get the topics of this pdf

                                var pdfModel = new NusysPdfAnalysisModel(contentDataModelId)//create the actual important pdf model
                                {
                                    DocumentAnalysisModel = pdfDocModel,
                                    PageImageAnalysisModels = new List<NuSysOcrAnalysisModel>(OCRModels),
                                    SuggestedTopics = topics?.Where(t => !string.IsNullOrEmpty(t))?.ToList() ?? new List<string>()
                                };
                                analysisModel = pdfModel; //set the analysis model

                                var regions = pdfModel?.PageImageAnalysisModels?.SelectMany(page => page?.Regions ?? new List<CognitiveApiRegionModel>());//get all the ocr regions
  
                                var sortedList = new SortedList<double, CognitiveApiRegionModel>(); //create a sorted list to get the most important topics

                                foreach (var region in regions ?? new List<CognitiveApiRegionModel>())//for each region in all the ocr regins
                                {
                                    var words = region?.Lines?.SelectMany(line => line?.Words?.Select(word => word?.Text ?? "") ?? new List<string>()) ?? new List<string>();
                                    var regionText = string.Join(" ", words ?? new List<string>());//create a text from all the words
                                    var matches = topics?.Sum(topic => Regex.Matches(regionText ?? "", topic ?? "")?.Count ?? 0) ?? 0;
                                    var key = (double)((double)matches/(Math.Max(words?.Count() ?? 1, 1)));
                                    while (sortedList.ContainsKey(key))
                                    {
                                        key += .00000001;
                                    }
                                    if (region != null)
                                    {
                                        sortedList.Add(key, region); //add to the sorted list the number of matches of topics and the region  
                                    }
                                }
                                var topCount = Math.Min(10, sortedList.Count);//get the count of how many we can mark important

                                for (int k = 0; k < topCount; k++)//for the number of important regions
                                {
                                    sortedList[sortedList.Max(r => r.Key)].MarkedImportant = true;//mark the top region as important
                                    sortedList.Remove(sortedList.Max(r => r.Key));
                                }
                            }
                            catch (Exception e)
                            {
                                ErrorLog.AddError(e);
                                senderHandler.SendError(e);
                            }
                        }
                        
                        break;
                    case NusysConstants.ContentType.Image:
                        try
                        {
                            analysisModel = await ImageProcessor.GetNusysImageAnalysisModelFromUrlAsync(contentUrl, contentDataModelId);
                        }
                        catch (Exception e)
                        {
                            ErrorLog.AddError(e);
                            senderHandler.SendError(e);
                        }
                        break;
                }
                //if the model was created because the content type had an async model to create
                if (analysisModel != null)
                {
                    //serialze the model and add the json to the sql tables
                    var json = JsonConvert.SerializeObject(analysisModel);
                    ContentController.Instance.SqlConnector.AddAnalysisModel(contentDataModelId, json);
                    senderHandler?.Notify(new AnalysisModelMadeNotification(new AnalysisModelMadeNotificationArgs() {ContentId = contentDataModelId})); //notify the sender of the new analysis model
                }
            });
        }

        /// <summary>
        /// method to asynchronous get the ocr model from the Cognitive services api of a pdf page.
        /// Will add the returned model to the array passed in.  
        /// If it fails to get the model, it adds an empty ocr model instead
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="array"></param>
        /// <param name="pageUrl"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        private static async Task RunPageOcr(int pageNumber, NuSysOcrAnalysisModel[] array, string pageUrl, NuWebSocketHandler senderHandler)
        {
            try
            {
                var image = Image.FromFile(FileHelper.FilePathFromUrl(pageUrl));
                var ocrModel = await ImageProcessor.GetNusysOcrAnalysisModelFromUrlAsync(pageUrl, image.Width, image.Height);
                foreach (var r in ocrModel?.Regions ?? new List<CognitiveApiRegionModel>())
                {
                    if (r != null)
                    {
                        r.PageNumber = pageNumber;
                    }
                }
                array[pageNumber] = ocrModel;
            }
            catch (Exception e)
            {
                array[pageNumber] = new NuSysOcrAnalysisModel();
                ErrorLog.AddError(e);
            }
        }

        /// <summary>
        /// gets the topics of an arbitrary text with a title;
        /// Returns the ienumerable of the topics
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<string>> GetTopicsOfText(string text, string title = "")
        {
            var test = new List<string>();

            // parameters for our LDA algorithm
            test.Add(title ?? "");
            test.Add("niters 8");
            test.Add("ntopics 5");
            test.Add("twords 10");
            test.Add("dir ");
            test.Add("est true");
            test.Add("alpha 12.5");
            test.Add("beta .1");
            test.Add("model model-final");

            DieStopWords ds = new DieStopWords();
            var data = ds.removeStopWords(text);
            var topics = await new TagExtractor().launch(test, new List<string>() { data });
            var returnTopics = new HashSet<string>();
            foreach (TopicWordPercent topic in topics)
            {
                if (topic != null)
                {
                    returnTopics.Add(topic.Word);
                }
            }
            return returnTopics;
        }
    }
}