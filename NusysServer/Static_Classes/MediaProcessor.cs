using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// this class will be used to process media asynchronously and alert the clients when the analysis models have been created.
    /// </summary>
    public class MediaProcessor
    {
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
                        var pdfText = contentDataModelMessage.GetList<string>(NusysConstants.CREATE_NEW_PDF_CONTENT_REQUEST_PDF_TEXT_KEY);

                        if (pdfText != null && pdfText.Any())
                        {
                            if (Constants.user == "junsu") //TODO remove after junsu tests  AND not make it only use the first page
                            {
                                var tup = new Tuple<string, string>(pdfText.First(), contentDataModelMessage.GetString(NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY));
                                ContentController.Instance.ComparisonController.AddDocument(tup);
                                ContentController.Instance.ComparisonController.CompareRandonDoc();
                            }
                            else
                            {
                                try
                                {
                                    var pdfDocModel = await TextProcessor.GetNusysPdfAnalysisModelFromTextAsync(pdfText);//get the document analysis

                                    var pageUrls = JsonConvert.DeserializeObject<List<string>>(contentUrl); //get the list of urls for the pdf pages as images

                                    var OCRModels = new NuSysOcrAnalysisModel[pageUrls.Count];//create empty list for the page model analyses

                                    int returned = 0;
                                    var i = 0;
                                    foreach (var pageUrl in pageUrls)
                                    {
                                        Task.Run(async delegate
                                        {
                                            int index = Convert.ToInt32(i);
                                            var image = Image.FromFile(FileHelper.FilePathFromUrl(pageUrl));
                                            var ocrModel = await ImageProcessor.GetNusysOcrAnalysisModelFromUrlAsync(pageUrl, contentDataModelId, image.Width, image.Height);
                                            OCRModels[index] = ocrModel;
                                            returned++;
                                        });
                                        i++;
                                    }

                                    while (returned < pageUrls.Count())
                                    {
                                        Task.Delay(50);//wait until all the pages return
                                    }

                                    analysisModel = new NusysPdfAnalysisModel(contentDataModelId)
                                    {
                                        DocumentAnalysisModel = pdfDocModel,
                                        PageImageAnalysisModels = new List<NuSysOcrAnalysisModel>(OCRModels)
                                    };
                                }
                                catch (Exception e)
                                {
                                    ErrorLog.AddError(e);
                                    senderHandler.SendError(e);
                                }
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
                }
            });
        }
    }
}