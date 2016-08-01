using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using NusysIntermediate;

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
            switch (contentType)
            {
                case NusysConstants.ContentType.Audio:
                case NusysConstants.ContentType.Image:
                case NusysConstants.ContentType.Video:
                    return contentUrl;
                case NusysConstants.ContentType.PDF:
                case NusysConstants.ContentType.Text:
                    return FetchDataFromFile(contentUrl);
            }
            throw new Exception("the requested contentType is not supported yet for url-to-data conversion");
        }

        /// <summary>
        /// creates a content file based off the id of the file.  
        /// A file exstension is needed for Audio, Video, and Image contentTypes.  
        /// If the contentType isn't on of those, the extension string will be ignored.  
        /// 
        /// Returns the url to store in the database
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <param name="contentType"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string CreateDataFile(string contentDataModelId, NusysConstants.ContentType contentType, string contentData,
            string fileExtension = null)
        {
            if (contentDataModelId == null)
            {
                throw new Exception("the contentDataModel Id cannot be null when creating a new content File");
            }
            string filePath = null;
            switch (contentType)
            {
                case NusysConstants.ContentType.Audio:
                case NusysConstants.ContentType.Image:
                case NusysConstants.ContentType.Video:
                    if (fileExtension == null)
                    {
                        throw new Exception("the file extension cannot be null when creating a new data file with Audio, Video, or Image contentTypes");
                    }
                    filePath = Constants.WWW_ROOT + contentDataModelId + fileExtension;
                    File.Create(filePath);
                    File.WriteAllText(filePath, contentData);
                    break;
                case NusysConstants.ContentType.PDF:
                case NusysConstants.ContentType.Text:
                    var extension = contentType == NusysConstants.ContentType.PDF ? Constants.PDF_DATA_FILE_FILE_EXTENSION : Constants.TEXT_DATA_FILE_FILE_EXTENSION;
                    filePath = contentDataModelId + extension;
                    File.Create(Constants.FILE_FOLDER + filePath);
                    File.WriteAllText(Constants.FILE_FOLDER + filePath, contentData);
                    break;
            }
            if (filePath == null)
            {
                throw new Exception("this content type is not supported yet for creating Content Data Files");
            }
            return filePath;
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