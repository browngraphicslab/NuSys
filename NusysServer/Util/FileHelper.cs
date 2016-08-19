using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using NusysIntermediate;
using Newtonsoft.Json;
using NusysServer.Util;

namespace NusysServer
{
    public class FileHelper
    {
        /// <summary>
        /// the encoding when writing bytes to a file.
        /// </summary>
        private static UnicodeEncoding Encoding = new UnicodeEncoding();

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
                if (contentUrl.Length <= Constants.SERVER_ADDRESS.Length)
                {
                    throw new Exception("the suggested content URL is too short");
                }
                switch (contentType)
                {
                    case NusysConstants.ContentType.Audio:
                    case NusysConstants.ContentType.Image:
                    case NusysConstants.ContentType.Video:
                    case NusysConstants.ContentType.PDF:
                    case NusysConstants.ContentType.Word:
                        return contentUrl;
                    case NusysConstants.ContentType.Text:
                        return FetchDataFromFile(contentUrl.Substring(Constants.SERVER_ADDRESS.Length));
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
        /// Returns the url to store in the database
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
                        //var pdfUrl = CreateDataFile(contentDataModelId, NusysConstants.ContentType.PDF, contentData, fileExtension);

                        break;
                    case NusysConstants.ContentType.PDF:
                        var tempPath = Constants.WWW_ROOT + contentDataModelId + ".pdf";//create a temporary .pdf path
                        File.WriteAllBytes(tempPath, Convert.FromBase64String(contentData));

                        //creates a file and url for each page image and returns a serialized list of urls
                        var listOfBytes = PdfUtil.GetPdfPageBytes(tempPath);

                        

                        List<string> listOfUrls = new List<string>();
                        int i = 0;
                        foreach (var bytesOfImage in listOfBytes)
                        {
                            filePath = Constants.WWW_ROOT + contentDataModelId + "_" + i + NusysConstants.DEFAULT_PDF_PAGE_IMAGE_EXTENSION;
                            var stream1 = File.Create(filePath);
                            stream1.Dispose();

                            using (var fstream = File.OpenWrite(filePath))
                            {
                                fstream.Write(bytesOfImage, 0, bytesOfImage.Length);
                            }

                            listOfUrls.Add(Constants.SERVER_ADDRESS + contentDataModelId + "_" + i +
                                           NusysConstants.DEFAULT_PDF_PAGE_IMAGE_EXTENSION);
                            i++;
                        }

                        return JsonConvert.SerializeObject(listOfUrls);
                        break;
                    case NusysConstants.ContentType.Text:
                        var extension = Constants.TEXT_DATA_FILE_FILE_EXTENSION;
                        filePath = contentDataModelId + extension;
                        var stream = File.Create(Constants.FILE_FOLDER + filePath);
                        stream.Dispose();
                        File.WriteAllText(Constants.FILE_FOLDER + filePath, contentData);
                        fileUrl = Constants.SERVER_ADDRESS + filePath;
                        break;
                }
                if (filePath == null || fileUrl == null)
                {
                    throw new Exception("this content type is not supported yet for creating Content Data Files");
                }
                return fileUrl;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "  FileHelper Method: CreateDataFile");
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
                        throw new Exception("only text content can be updated");
                    case NusysConstants.ContentType.Text:
                        var filePath = Constants.FILE_FOLDER + contentId + Constants.TEXT_DATA_FILE_FILE_EXTENSION;
                        if (File.Exists(filePath))
                        {
                            File.WriteAllText(filePath, updatedContentData);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
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
                throw new Exception("The content file you requests does not exist. file: " + fileName);
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
    }
}