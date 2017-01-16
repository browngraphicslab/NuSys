using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NusysIntermediate;
using Newtonsoft.Json;
using GemBox.Document;

namespace NusysServer
{
    public class FileHelper
    {

        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Open(byte[] data, int length);
        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ActivateDocument(IntPtr document);
        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderPage(int width, int height);
        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTextBytes(byte[] sb);
        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetBuffer();
        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPageWidth();
        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetPageHeight();
        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumComponents();
        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumPages();

        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GotoPage(int page);
        [DllImport("mupdfapit2", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Dispose(IntPtr pointer);
        /// <summary>
        /// the encoding when writing bytes to a file.
        /// </summary>
        private static UnicodeEncoding Encoding = new UnicodeEncoding();

        /// <summary>
        /// the pdf lock object
        /// </summary>
        public static object MuPdfLock = new object();

        /// <summary>
        /// returns the correct data for a ContentDataModel based on the contentUrl from the database. 
        /// Will return the passed in Url for Images, Videos, and Audio contentTypes
        /// </summary>
        /// <param name="contentUrl"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string GetDataFromContentURL(string contentUrl, NusysConstants.ContentType contentType)
        {
            try
            {
                switch (contentType)
                {
                    case NusysConstants.ContentType.Audio:
                    case NusysConstants.ContentType.Image:
                    case NusysConstants.ContentType.Video:
                    case NusysConstants.ContentType.PDF:
                    case NusysConstants.ContentType.Word:
                    case NusysConstants.ContentType.Text:
                    case NusysConstants.ContentType.Collection:
                        return contentUrl;
                }
                throw new Exception("the requested contentType is not supported yet for url-to-data conversion");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "  FileHelper Method: GetDataFromContentURL");
            }
        }

        /// <summary>
        /// Creates a thumbnail file for a given library element.  
        /// The byte string should be a base-64 string that represents a byte array
        /// </summary>
        /// <param name="libraryElementId"></param>
        /// <param name="size"></param>
        /// <param name="byteString"></param>
        /// <returns></returns>
        public static string CreateThumbnailFile(string libraryElementId, NusysConstants.ThumbnailSize size, string byteString)
        {
            try
            {
                if (byteString == null)
                {
                    return null;
                }
                if (libraryElementId == null)
                {
                    throw new Exception("the libraryElementModelId Id cannot be null when creating a new thumbnail File");
                }
                var fileName = NusysConstants.GetDefaultThumbnailFileName(libraryElementId, size) +
                               NusysConstants.DEFAULT_THUMBNAIL_FILE_EXTENSION;
                var fileStream = File.Create(Constants.WWW_ROOT + fileName);
                fileStream.Dispose();

                using (var fstream = File.OpenWrite(Constants.WWW_ROOT + fileName))
                {
                    var bytes = Convert.FromBase64String(byteString);
                    fstream.Write(bytes, 0, bytes.Length);
                }


                return Constants.SERVER_ADDRESS + fileName;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "  FileHelper Method: CreateThumbnailFile");
            }
        }

        /// <summary>
        /// creates a content file based off the content data model id of the file.  
        /// A file exstension is needed for Audio, Video, and Image contentTypes.  
        /// If the contentType isn't on of those, the extension string will be ignored.  
        /// 
        /// Returns the string to store in the database
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <param name="contentType"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string CreateDataFile(string contentDataModelId, NusysConstants.ContentType contentType, string contentData, string fileExtension = null)
        {
            try
            {
                if (contentDataModelId == null)
                {
                    throw new Exception("the libraryElementModelId Id cannot be null when creating a new content File");
                }
                string filePath = null;
                string fileUrl = null;
                switch (contentType)
                {
                    case NusysConstants.ContentType.Audio:
                    case NusysConstants.ContentType.Image:
                    case NusysConstants.ContentType.Video:
                        if (fileExtension == null)
                        {
                            return contentData;
                            throw new Exception(
                                "the file extension cannot be null when creating a new data file with Audio, Video, or Image contentTypes");
                        }
                        filePath = Constants.WWW_ROOT + contentDataModelId + fileExtension;
                        var fileStream = File.Create(filePath);
                        fileStream.Dispose();

                        using (var fstream = File.OpenWrite(filePath))//write the bytes to file
                        {
                            var bytes = Convert.FromBase64String(contentData);
                            fstream.Write(bytes, 0, bytes.Length);
                        }

                        fileUrl = Constants.SERVER_ADDRESS + contentDataModelId + fileExtension;
                        break;
                    case NusysConstants.ContentType.Word:
                        var wordPath = Constants.GetWordDocumentFilePath(contentDataModelId);

                        //convert from word to pdf, and save word doc elsewhere
                        var pdfByteData = GetWordBytes(contentData, contentDataModelId, wordPath);
                        
                        MakeWordThumbnails(pdfByteData, contentDataModelId);

                        var pdfUrl = CreateDataFile(contentDataModelId, NusysConstants.ContentType.PDF, Convert.ToBase64String(pdfByteData), fileExtension);
                        return pdfUrl;
                        break;

                    case NusysConstants.ContentType.PDF:
                        lock (MuPdfLock)
                        {
                            var pdfBytes = Convert.FromBase64String(contentData);
                            var doc = Open(pdfBytes, pdfBytes.Length);

                            // Active the pdf document
                            ActivateDocument(doc);
                            var listOfUrls = new List<string>();
                            for (int page = 0; page < GetNumPages(); page++)
                            {
                                // Goto a page
                                GotoPage(page);

                                // Get aspect ratio of the page
                                var aspectRatio = GetPageWidth()/(double) GetPageHeight();

                                // Render the Page
                                var size = 2000;
                                var numBytes = RenderPage((int) (size*aspectRatio), size);

                                // Get a reference to the buffer that contains the rendererd page
                                var buffer = GetBuffer();

                                // Copy the buffer from unmanaged to managed memory (mngdArray contains the bytes of the pdf page rendered as PNG)
                                byte[] mngdArray = new byte[numBytes];
                                try
                                {
                                    Marshal.Copy(buffer, mngdArray, 0, numBytes);
                                    filePath = Constants.WWW_ROOT + contentDataModelId + "_" + page +
                                               NusysConstants.DEFAULT_PDF_PAGE_IMAGE_EXTENSION;

                                    var stream1 = File.Create(filePath);
                                    stream1.Dispose();

                                    using (var fstream = File.OpenWrite(filePath))
                                    {
                                        var bytes = mngdArray;
                                        fstream.Write(bytes, 0, bytes.Length);
                                    }
                                    listOfUrls.Add(Constants.SERVER_ADDRESS + contentDataModelId + "_" + page +
                                                   NusysConstants.DEFAULT_PDF_PAGE_IMAGE_EXTENSION);
                                }
                                catch (Exception e)
                                {
                                    var e2 =
                                        new Exception(e.Message + ".  Error creating pdf and copying bytes for image");
                                    ErrorLog.AddError(e2);
                                    throw e2;
                                }
                            }
                            try
                            {
                                Dispose(doc);
                            }
                            catch (Exception e)
                            {
                                ErrorLog.AddError(e);
                                return JsonConvert.SerializeObject(listOfUrls);
                            }

                            return JsonConvert.SerializeObject(listOfUrls);
                        }
                        break;
                    case NusysConstants.ContentType.Text:
                    case NusysConstants.ContentType.Collection:
                        filePath = "";
                        fileUrl = contentData ?? "";
                        break;
                }
                if (filePath == null || fileUrl == null)
                {
                    var e = new Exception("this content type is not supported yet for creating Content Data Files");
                    ErrorLog.AddError(e);
                    throw e;
                }
                return fileUrl;
            }
            catch (Exception e)
            {
                var e2 = new Exception(e.Message + "  FileHelper Method: CreateDataFile");
                ErrorLog.AddError(e2);
                throw e2;
            }
        }

        /// <summary>
        /// Method that can be used to save any file to the WWWRoot.
        /// Pass in the file byte array, the name of the file, and an extension if you want.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileExtension"></param>
        /// <param name="fileBytes"></param>
        public static void SaveFileToRoot(byte[] fileBytes, string fileName, string fileExtension = null)
        {
            Debug.Assert(fileBytes != null && fileName != null);
            var fp = Constants.WWW_ROOT + fileName + (fileExtension ?? "");

            using (var fstream = File.OpenWrite(fp))
            {
                fstream.Write(fileBytes, 0, fileBytes.Length);
            }
        }

        private static void MakeWordThumbnails(byte[] pdfBytes, string contentDataModelId)
        {
            if (contentDataModelId == null)
            {
                throw new Exception("the contentDataModelId cannot be null when creating a word thumbnail");
            }
            lock (MuPdfLock)
            {
                try
                {
                    var doc = Open(pdfBytes, pdfBytes.Length);
                    ActivateDocument(doc);
                    GotoPage(0);
                    var aspectRatio = GetPageWidth()/(double) GetPageHeight();

                    foreach (var s in Enum.GetValues(typeof(NusysConstants.ThumbnailSize)))
                    {
                        var size = (NusysConstants.ThumbnailSize) s;
                        var fileName = NusysConstants.GetDefaultThumbnailFileName(contentDataModelId, size) +
                                       NusysConstants.DEFAULT_THUMBNAIL_FILE_EXTENSION;

                        //hard to read but just switches on size and sets the width accordingly
                        var height = size == NusysConstants.ThumbnailSize.Small
                            ? 100
                            : (size == NusysConstants.ThumbnailSize.Medium ? 250 : 500);

                        var numBytes = RenderPage((int) (height*aspectRatio), height);
                        var buffer = GetBuffer();

                        byte[] mngdArray = new byte[numBytes];

                        var fileStream = File.Create(Constants.WWW_ROOT + fileName);
                        fileStream.Dispose();

                        Marshal.Copy(buffer, mngdArray, 0, numBytes);
                        using (var fstream = File.OpenWrite(Constants.WWW_ROOT + fileName))
                        {
                            fstream.Write(mngdArray, 0, mngdArray.Length);
                        }
                    }

                }
                catch (Exception e)
                {
                    ErrorLog.AddError(new Exception(e.Message + "  ADDING WORD THUMBNAILS FAILURE."));
                }
            }
        }

        /// <summary>
        /// This tries to update a content data file.  As of right now, it can only update text type content.
        /// </summary>
        /// <returns></returns>
        public static bool UpdateContentDataFile(string contentId, NusysConstants.ContentType contentType, string updatedContentData)
        {
            try
            {
                if (contentId == null)
                {
                    throw new Exception("the contentId cannot be null when updating a content file");
                }
                updatedContentData = updatedContentData ?? "";
                switch (contentType)
                {
                    case NusysConstants.ContentType.Audio:
                    case NusysConstants.ContentType.Image:
                    case NusysConstants.ContentType.Video:
                    case NusysConstants.ContentType.PDF:
                    case NusysConstants.ContentType.Word:
                        throw new Exception("only text and collections content can be updated in this method");
                        break;
                    case NusysConstants.ContentType.Collection:
                    case NusysConstants.ContentType.Text:
                        var sqlQuery = new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.Content),
                            new List<SqlQueryEquals>() {new SqlQueryEquals(Constants.SQLTableType.Content, NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY, updatedContentData)},
                            new SqlQueryEquals(Constants.SQLTableType.Content, NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY,contentId));
                        return sqlQuery.ExecuteCommand();
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "  FileHelper Method: UpdateContentDataFile");
            }
        }

        /// <summary>
        /// gets the bytes from the public encoding before saving to files;
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static byte[] GetBytesForWritingToFile(string text)
        {
            return Encoding.GetBytes(text);
        }

        /// <summary>
        /// returns the string contents from a file.  Should only be used on text and pdf content types. 
        /// Will throw an exception if the file doesn't exist
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static string FetchDataFromFile(string fileName)
        {
            var filepath = Constants.FILE_FOLDER + fileName;
            if (!File.Exists(filepath))
            {
                throw new Exception("The content file you requests does not exist. file: "+fileName);
            }
            try
            {
                return File.ReadAllText(filepath);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "  FileHelper Method: FetchDataFromFile");
            }
        }

        /// <summary>
        /// a public static methd that allows you to input the contentDataModel Id and get the file path of the word document for that Id;
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <returns></returns>
        public static string GetWordDocumentFIlePathFromContentId(string contentDataModelId)
        {
            return Constants.FILE_FOLDER + contentDataModelId + ".docx";//TODO extract this out to actual constant
        }

        /// <summary>
        /// Gets the file path for a certain url if that url points to our server.
        /// Will return gibberish if it's not for our server.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string FilePathFromUrl(string url)
        {
            return Constants.WWW_ROOT + url.Substring(Constants.SERVER_ADDRESS.Length);
        }

        /// <summary>
        /// method called from the UploadWordDocController to update an existing word document. 
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string UpdateWordDoc(string wordByteDataString, string customContentIdPropertyKey = "contentDataModelId")
        {
            ComponentInfo.SetLicense("DORJ-JFGF-MSBP-2XUV");
            var bytes = Convert.FromBase64String(wordByteDataString);
            Stream stream = new MemoryStream(bytes);
            var doc = DocumentModel.Load(stream, LoadOptions.DocxDefault);
            if (!doc.DocumentProperties.Custom.ContainsKey(customContentIdPropertyKey))
            {
                return "document did not have the correct custom property id";
            }
            var contentDataModelId = doc.DocumentProperties.Custom[customContentIdPropertyKey].ToString();
            try
            {
                var updatedContentData = CreateDataFile(contentDataModelId, NusysConstants.ContentType.Word, wordByteDataString);
                var sqlQuery = new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.Content),
                            new List<SqlQueryEquals>() { new SqlQueryEquals(Constants.SQLTableType.Content, NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY, updatedContentData) },
                            new SqlQueryEquals(Constants.SQLTableType.Content, NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY, contentDataModelId));
                var success = sqlQuery.ExecuteCommand();

                var notification = new WordChangedNotification(new WordChangedNotificationArgs() { ContentDataModelId = contentDataModelId});
                NuWebSocketHandler.NotifyAll(notification);
                return "Success!";
            }
            catch(Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Method to get the word document bytes from a word file using only the contentId;
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static byte[] GetWordBytesFromContentId(string contentId)
        {
            var filePath = Constants.GetWordDocumentFilePath(contentId);
            var bytes = File.ReadAllBytes(filePath);
            return bytes;
        }

        /// <summary>
        /// This method will save the word document in the correct location, and then return the pdf bytes for the document.
        /// You must use a word file path ending in '.docx'.
        /// This will also give the save word document a special property.  
        /// The special property will be named the 'customContentIdPropertyKey' parameter and its value will be the 'contentDataModelId' parameter.
        /// </summary>
        /// <param name="wordByteData"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static byte[] GetWordBytes(string wordByteData, string contentDataModelId, string wordPath, string customContentIdPropertyKey = "contentDataModelId")
        {
            if (!wordPath.EndsWith(".docx"))
            {
                throw new Exception("path to save word document to must end in '.docx'");
            }
            ComponentInfo.SetLicense("DORJ-JFGF-MSBP-2XUV");
            DocumentModel doc = null;
            try
            {
                var bytes = Convert.FromBase64String(wordByteData);
                Stream stream = new MemoryStream(bytes);
                doc = DocumentModel.Load(stream, LoadOptions.DocxDefault);
                doc.DocumentProperties.Custom[customContentIdPropertyKey] = contentDataModelId;
                var pdfPath = wordPath.Substring(0, wordPath.Length - 4) + "pdf";
                doc.Save(wordPath);
                doc.Save(pdfPath);
                var returnBytes = File.ReadAllBytes(pdfPath);
                File.Delete(pdfPath);

                return returnBytes;
            }
            catch (Exception e)
            {
                throw new Exception("docx too large. "+e.Message);
            }
        }


        //TODO fix, but dont delete this
        /*
        public static string UpdateWordDoc(byte[] bytes)
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
            try
            {
                string retString = "";
                Stream stream = new MemoryStream(bytes);
                var doc = DocumentModel.Load(stream, LoadOptions.DocxDefault);
                if (!doc.DocumentProperties.Custom.ContainsKey("libraryId"))
                {
                    retString = "bytes sucessfully parsed to document but couldn't find library Id in document metadata";
                }
                var id = doc.DocumentProperties.Custom["libraryId"];
                var docPath = NusysContent.BaseFolder + id + ".docx";
                var pdfPath = NusysContent.BaseFolder + id + ".pdf";
                var dataPath = NusysContent.BaseFolder + id + ".data";
                doc.Save(docPath);
                doc.Save(pdfPath);//must be done with .pdf extension so gembox knows what file type to save as
                var pdfBytes = File.ReadAllBytes(pdfPath);
                var base64String = Convert.ToBase64String(pdfBytes);
                var writableBytes = Encoding.UTF8.GetBytes(base64String);
                using (FileStream pdfstream = new FileStream(dataPath, FileMode.Create, FileAccess.Write))
                {
                    pdfstream.Write(writableBytes, 0, writableBytes.Length);
                    pdfstream.Close();
                }
                File.Delete(pdfPath);
                if (id is string && ContentsHolder.Instance.Contents.ContainsKey(id as string))
                {
                    var content = ContentsHolder.Instance.Contents[id as string];
                    if (content != null)
                    {
                        NuWebSocketHandler.BroadcastContentDataUpdate(content);
                    }
                }
                return "success!";
            }
            catch (Exception e)
            {
                return "Could not convert bytes to gem box word doc: ERROR MESSAGE: " + e.Message + "   ";
            }
            return null;
        }
        */
    }
}