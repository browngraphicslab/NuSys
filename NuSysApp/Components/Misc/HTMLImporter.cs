using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using HtmlAgilityPack;
using MyToolkit.Utilities;
using NAudio.Wave;
using Newtonsoft.Json;
using NusysIntermediate;
using WinRTXamlToolkit.Imaging;

namespace NuSysApp
{
    public class HTMLImporter
    {
        public HTMLImporter()
        {

        }
        public async Task<IEnumerable<LibraryElementModel>> Run(Uri url)
        {
            var doc = await GetDocumentFromUri(url);
            if (doc == null)
            {
                return null;
            }
            var models = new HashSet<LibraryElementModel>();
            var contentDataModels = new HashSet<ContentDataModel>();
            await RecursiveAdd(doc.DocumentNode, models, contentDataModels);
            var images = models.Where(m => m.Type == NusysConstants.ElementType.Image).ToList();
            var pdfs = models.Where(m => m.Type == NusysConstants.ElementType.PDF).ToList();
            var contentDict = contentDataModels.ToDictionary(k => k.ContentId, v => v);
            int i = 0;
            foreach (var model in models.Except(images).Except(pdfs))
            {
                if (i%10 == 0 && model.Type != NusysConstants.ElementType.Image)
                {
                    var args = new CreateNewContentRequestArgs();
                    args.LibraryElementArgs = new CreateNewLibraryElementRequestArgs();
                    args.LibraryElementArgs.Title = "Text Node Parsed, probably shit";
                    args.LibraryElementArgs.AccessType = NusysConstants.AccessType.Public;
                    args.LibraryElementArgs.LibraryElementType = NusysConstants.ElementType.Text;
                    args.LibraryElementArgs.ContentId = model.ContentDataModelId;
                    args.ContentId = model.ContentDataModelId;
                    var req = new CreateNewContentRequest(args);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(req);
                    if (req.WasSuccessful() == true)
                    {
                        req.AddReturnedLibraryElementToLibrary();
                        await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(model.ContentDataModelId);
                        var controller = SessionController.Instance.ContentController.GetContentDataController(model.ContentDataModelId);
                        controller?.SetData(contentDict[model.ContentDataModelId].Data);
                    }
                }
                i++;
            }
            foreach (var img in images)
            {
                var args = new CreateNewContentRequestArgs();
                args.LibraryElementArgs = new CreateNewImageLibraryElementRequestArgs();
                args.LibraryElementArgs.Title = img.Title;
                args.LibraryElementArgs.AccessType = NusysConstants.AccessType.Public;
                args.LibraryElementArgs.ContentId = img.ContentDataModelId;
                args.LibraryElementArgs.Large_Thumbnail_Url = contentDict[img.ContentDataModelId].Data;
                args.LibraryElementArgs.Small_Thumbnail_Url = contentDict[img.ContentDataModelId].Data;
                args.LibraryElementArgs.Medium_Thumbnail_Url = contentDict[img.ContentDataModelId].Data;
                args.LibraryElementArgs.LibraryElementType = NusysConstants.ElementType.Image;
                args.ContentId = img.ContentDataModelId;
                args.FileExtension = "hahaha";
                args.DataBytes = contentDict[img.ContentDataModelId].Data;
                var req = new CreateNewContentRequest(args);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(req);
                if (req.WasSuccessful() == true)
                {
                    req.AddReturnedLibraryElementToLibrary();
                }
            }

            return models;
        }

        public async Task RunWithSearch(string search)
        {
            await Run(new Uri("https://en.wikipedia.org/wiki/"+search));
        }

        private async Task<HtmlDocument> GetDocumentFromUri(Uri url)
        {
            try
            {
                url = url ?? new Uri("https://en.wikipedia.org/wiki/Computer_science");
                var doc = new HtmlDocument();
                var webRequest = HttpWebRequest.Create(url.AbsoluteUri);
                HttpWebResponse response = (HttpWebResponse) (await webRequest.GetResponseAsync());
                Stream stream = response.GetResponseStream();
                doc.Load(stream);
                stream.Dispose();
                return doc;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private async Task RecursiveAdd(HtmlNode node, HashSet<LibraryElementModel> models, HashSet<ContentDataModel> contentDataModels, bool ignoreTexts = false)
        {
            if (node.Name.ToLower() == "script")
            {
                return;
            }
            if (!ignoreTexts && node.ChildNodes.Count(c => c.NodeType == HtmlNodeType.Text) > node.ChildNodes.Count()/2)
            {
                var text = StripText(RecursiveSpan(node));
                if (IsValidText(text) && (text.Length < 2000 || !node.ChildNodes.Any()))
                {
                    ignoreTexts = true;
                    var content = new ContentDataModel(SessionController.Instance.GenerateId(), text);
                    var m = new LibraryElementModel(SessionController.Instance.GenerateId(), NusysConstants.ElementType.Text) { ContentDataModelId = content.ContentId };
                    models.Add(m);
                    contentDataModels.Add(content);
                }
            }
            foreach (var child in node.ChildNodes)
            {
                if (child.Name.ToLower() != "script")
                {
                    await RecursiveAdd(child, models, contentDataModels, ignoreTexts);
                }
            }
            if (node.Name == "img")
            {
                var src = FormatSource(node.GetAttributeValue("src", null));
                if (src == null || src.Contains(".svg")) 
                {
                    return;
                }
                var height = node.GetAttributeValue("height", 1000);
                var width = node.GetAttributeValue("height", 1000);
                if (height > 75 && width > 75)
                {
                    var title = SearchForTitle(node);
                    var content = new ContentDataModel(SessionController.Instance.GenerateId(), src);
                    var m = new ImageLibraryElementModel(SessionController.Instance.GenerateId())
                    {
                        ContentDataModelId = content.ContentId,
                        Title = title == null ? "Image" : StripText(title),
                        SmallIconUrl = src,
                        MediumIconUrl = src,
                        LargeIconUrl = src,
                        AccessType = NusysConstants.AccessType.Public
                    };
                    models.Add(m);
                    contentDataModels.Add(content);
                }
            }
            if (node.Name == "a")
            {
                var href = node.GetAttributeValue("href", null);
                if (href != null && href.Contains(".pdf"))
                {
                    return;
                    byte[] bytes;
                    try
                    {
                        var webRequest = HttpWebRequest.Create(FormatSource(href));
                        HttpWebResponse response = (HttpWebResponse) (await webRequest.GetResponseAsync());
                        Stream stream = response.GetResponseStream();
                        bytes = await stream.ReadToEndAsync();
                    }
                    catch (Exception e)
                    {
                        return;
                    }

                    UITask.Run(async delegate
                    {
                        var MuPdfDoc = await MediaUtil.DataToPDF(Convert.ToBase64String(bytes));

                        var pdfPageCount = MuPdfDoc.PageCount;
                        List<string> pdfTextByPage = new List<string>();
                        List<string> pdfPages = new List<string>();

                        var thumbnails = new Dictionary<NusysConstants.ThumbnailSize, string>();
                        thumbnails[NusysConstants.ThumbnailSize.Small] = string.Empty;
                        thumbnails[NusysConstants.ThumbnailSize.Medium] = string.Empty;
                        thumbnails[NusysConstants.ThumbnailSize.Large] = string.Empty;

                        // convert each page of the pdf into an image file, and store it in the pdfPages list
                        for (int pageNumber = 0; pageNumber < MuPdfDoc.PageCount; pageNumber++)
                        {
                            // set the pdf text by page for the current page number
                            pdfTextByPage.Add(MuPdfDoc.GetAllTexts(pageNumber));

                            // get variables for drawing the page
                            var pageSize = MuPdfDoc.GetPageSize(pageNumber);
                            var width = pageSize.X;
                            var height = pageSize.Y;

                            // create an image to use for converting
                            var image = new WriteableBitmap(width, height);

                            // create a buffer to draw the page on
                            IBuffer buf = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
                            buf.Length = image.PixelBuffer.Length;

                            // draw the page onto the buffer
                            MuPdfDoc.DrawPage(pageNumber, buf, 0, 0, width, height, false);
                            var ss = buf.AsStream();

                            // copy the buffer to the image
                            await ss.CopyToAsync(image.PixelBuffer.AsStream());
                            image.Invalidate();

                            // save the image as a file (temporarily)
                            var x = await image.SaveAsync(NuSysStorages.SaveFolder);

                            // use the system to convert the file to a byte array
                            pdfPages.Add(Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(x)));
                            if (pageNumber == 0)
                            {
                                // if we are on the first apge, get thumbnails of the file from the system
                                thumbnails = await MediaUtil.GetThumbnailDictionary(x);
                            }

                            // delete the image file that we saved
                            await x.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        }


                        var pdfArgs = new CreateNewPdfLibraryElementModelRequestArgs();
                        pdfArgs.PdfPageStart = 0;
                        pdfArgs.PdfPageEnd = pdfPageCount;

                        var args = new CreateNewContentRequestArgs();
                        args.LibraryElementArgs = pdfArgs;
                        args.LibraryElementArgs.Title = StripText(node.InnerText);
                        args.LibraryElementArgs.AccessType = NusysConstants.AccessType.Public;
                        args.LibraryElementArgs.Large_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Large];
                        args.LibraryElementArgs.Small_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Small];
                        args.LibraryElementArgs.Medium_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Medium];
                        args.LibraryElementArgs.LibraryElementType = NusysConstants.ElementType.PDF;
                        args.FileExtension = ".pdf";
                        args.DataBytes = JsonConvert.SerializeObject(pdfPages);
                        var req = new CreateNewContentRequest(args);
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(req);
                        if (req.WasSuccessful() == true)
                        {
                            req.AddReturnedLibraryElementToLibrary();
                        }
                    });
                }
            }
        }

        private string RecursiveSpan(HtmlNode node)
        {
            var s = "";
            if (!IsJS(node.InnerText) && node.NodeType == HtmlNodeType.Text)
            {
                s += node.InnerText;
            }
            foreach (var child in node.ChildNodes)
            {
                s += RecursiveSpan(child) + "  ";
            }
            return s;
        }

        private bool IsJS(string text)
        {
            var list = text.ToCharArray().ToList();
            if (list.Count(c => c == '{' || c == '}' || c == '$' || c == '&' || c == '.' || c == ';' || c == '!' || c == '=') > list.Count*.025)
            {
                return true;;
            }
            return false;
        }

        private string FormatSource(string src)
        {
            if (src == null)
            {
                return null;
            }
            if (src.StartsWith("//"))
            {
                return "http:" + src;
            }
            return src;
        }

        private bool IsValidText(string text)
        {
            return text.Length > 50;
        }

        private string StripText(string text)
        {
            return text.Replace("\n","").Replace("\t","");
        }

        private string SearchForTitle(HtmlNode node)
        {
            var visited = new HashSet<HtmlNode>(); 
            for (int i = 0; i < 4; i++) //depth to search set to 4
            {
                if(node == null)
                {
                    return null;
                }
                visited.Add(node);
                var title = RecursiveFindTitle(node, visited);
                if (title == null)
                {
                    node = node.ParentNode;
                }
                else
                {
                    return title;
                }
            }
            return null;
        }

        private static int r = 0;
        private string RecursiveFindTitle(HtmlNode node, HashSet<HtmlNode> visited)
        {
            r++;
            visited.Add(node);
            var title = GetTitle(node);
            if (title != null)
            {
                return title;
            }
            if (node.ChildNodes.Count() > 10)
            {
                return null;
            }
            foreach (var child in node.ChildNodes)
            {
                if (child != null && !visited.Contains(child))
                {
                    title = RecursiveFindTitle(child, visited);
                    if (title != null)
                    {
                        return title;
                    }
                }
            }
            return null;
        }

        private string GetTitle(HtmlNode node)
        {
            if (IsValidText(node.InnerText))
            {
                var text = node.InnerText;
                if (text.Length > 325)
                {
                    return null;
                }
                return StripText(text);
            }
            return null;
        }
    }
}
