using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using NusysIntermediate;
using Newtonsoft.Json;

namespace NusysServer
{
    public class FileHelper
    {
        /// <summary>
        /// returns the correct data for a ContentDataModel based on the contentUrl from the database. 
        /// Will return the passed in Url for Images, Videos, and Audio contentTypes
        /// </summary>
        /// <param name="contentUrl"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string GetDataFromContentURL(string contentUrl, NusysConstants.ContentType contentType)
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
                    return contentUrl;
                case NusysConstants.ContentType.Text:
                    return FetchDataFromFile(contentUrl.Substring(Constants.SERVER_ADDRESS.Length));
            }
            throw new Exception("the requested contentType is not supported yet for url-to-data conversion");
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
            if (byteString == null)
            {
                return null;
            }
            if (libraryElementId == null)
            {
                throw new Exception("the libraryElementModelId Id cannot be null when creating a new thumbnail File");
            }
            var fileName = NusysConstants.GetDefaultThumbnailFileName(libraryElementId, size) + NusysConstants.DEFAULT_THUMBNAIL_FILE_EXTENSION;
            var fileStream = File.Create(Constants.WWW_ROOT + fileName);
            fileStream.Dispose();
            File.WriteAllBytes(Constants.WWW_ROOT + fileName,Convert.FromBase64String(byteString));
            return Constants.SERVER_ADDRESS + fileName;
        }

        /// <summary>
        /// creates a content file based off the id of the file.  
        /// A file exstension is needed for Audio, Video, and Image contentTypes.  
        /// If the contentType isn't on of those, the extension string will be ignored.  
        /// 
        /// Returns the url to store in the database
        /// </summary>
        /// <param name="libraryElementModelId"></param>
        /// <param name="contentType"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string CreateDataFile(string libraryElementModelId, NusysConstants.ContentType contentType, string contentData,
            string fileExtension = null)
        {
            if (libraryElementModelId == null)
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
                        throw new Exception("the file extension cannot be null when creating a new data file with Audio, Video, or Image contentTypes");
                    }
                    filePath = Constants.WWW_ROOT + libraryElementModelId + fileExtension;
                    var fileStream = File.Create(filePath);
                    fileStream.Dispose();
                    File.WriteAllBytes(filePath, Convert.FromBase64String(contentData));
                    fileUrl = Constants.SERVER_ADDRESS + libraryElementModelId + fileExtension;
                    break;
                case NusysConstants.ContentType.PDF:
                    //creates a file and url for each page image and returns a serialized list of urls
                    var listOfBytes = JsonConvert.DeserializeObject<List<string>>(contentData);
                    List<string> listOfUrls = new List<string>();
                    int i = 0;
                    foreach(var bytesOfImage in listOfBytes)
                    {
                        filePath = Constants.WWW_ROOT + libraryElementModelId + "_" + i + NusysConstants.DEFAULT_PDF_PAGE_IMAGE_EXTENSION;
                        var stream1 = File.Create(filePath);
                        stream1.Dispose();
                        File.WriteAllBytes(filePath, Convert.FromBase64String(bytesOfImage));
                        listOfUrls.Add(Constants.SERVER_ADDRESS + libraryElementModelId + "_" + i + NusysConstants.DEFAULT_PDF_PAGE_IMAGE_EXTENSION);
                        i++;
                    }
                    return JsonConvert.SerializeObject(listOfUrls);
                    break;
                case NusysConstants.ContentType.Text:
                    var extension = Constants.TEXT_DATA_FILE_FILE_EXTENSION;
                    filePath = libraryElementModelId + extension;
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
            return File.ReadAllText(filepath);
        }

    }
}