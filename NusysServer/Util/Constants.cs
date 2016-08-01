using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysServer
{
    public class Constants
    {
        public static readonly string SERVER_SESSION_ID_STRING = "server_session_id";
        public static readonly string VALID_CREDENTIALS_BOOLEAN_STRING = "valid";
        private static readonly string user = "leandro";

        public static string WWW_ROOT {
            get
            {
                switch (user)
                {
                    case "leandro":
                        return Directory.Exists("C:/Users/Leandro Bengzon/Documents/NuSys Server/") ? "C:/Users/Leandro Bengzon/Documents/NuSys Server/" : "D:/home/site/wwwroot/";
                    case "trent":
                        return Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/") ? "C:/Users/graphics_lab/Documents/NuRepo_Test/" : "D:/home/site/wwwroot/";
                    default:
                        return "";
                }
            }
        }
        
        //public static readonly string WWW_ROOT = Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/")
        //    ? "C:/Users/graphics_lab/Documents/NuRepo_Test/"
        //    : "D:/home/site/wwwroot/";


        public static string FILE_FOLDER
        {
            get
            {
                switch (user)
                {
                    case "leandro":
                        return Directory.Exists("C:/Users/Leandro Bengzon/Documents/NuSys Server/") ? "C:/Users/Leandro Bengzon/Documents/NuSys Server/" : "D:/home/site/wwwroot/files/";
                    case "trent":
                        return Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/") ? "C:/Users/graphics_lab/Documents/NuRepo_Test/" : "D:/home/site/wwwroot/files/";
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// the folder on the server where all the content files will be saved.  
        /// Should only be storing .txt and .pdf files as of 7/31/16
        /// </summary>
        //public static readonly string FILE_FOLDER = Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/")
        //    ? "C:/Users/graphics_lab/Documents/NuRepo_Test/"
        //    : "D:/home/site/wwwroot/files/";


        public static string SERVER_ADDRESS {
            get
            {
                switch (user)
                {
                    case "leandro":
                        return Directory.Exists("C:/Users/Leandro Bengzon/Documents/NuSys Server/") ? "localhost:2685" : "http://nusysrepo.azurewebsites.net/";
                    case "trent":
                        return Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/") ? "localhost:2685" : "http://nusysrepo.azurewebsites.net/";
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// the address of the server so files can be stored in the database
        /// </summary>
        //public static readonly string SERVER_ADDRESS = Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/")
        //    ? "localhost:2685"
        //    : "http://nusysrepo.azurewebsites.net/";

        /// <summary>
        /// the file extension for saving pdf data files
        /// </summary>
        public static readonly string PDF_DATA_FILE_FILE_EXTENSION = ".pdf";

        /// <summary>
        /// the file extension for saving text data files
        /// </summary>
        public static readonly string TEXT_DATA_FILE_FILE_EXTENSION = ".txt";


        /// <summary>
        /// the list of possible SQL table types that we have
        /// </summary>
        public enum SQLTableType
        {
            Alias,
            LibrayElement,
            Metadata,
            Properties,
            Content
        }

        /// <summary>
        /// the operator used for SQl command making
        /// </summary>
        public enum Operator
        {
            And,
            Or
        }

        #region StaticMethods

        /// <summary>
        /// returns amn IEnumerable with all the column keys for a given sql table.  
        /// populated by string lists in the constants class
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAcceptedKeys(SQLTableType tableType)
        {
            switch (tableType)
            {
                case SQLTableType.Alias:
                    return NusysConstants.ALIAS_ACCEPTED_KEYS.Keys;
                case SQLTableType.LibrayElement:
                    return NusysConstants.LIBRARY_ELEMENT_MODEL_ACCEPTED_KEYS.Keys;
                case SQLTableType.Content:
                    return NusysConstants.ACCEPTED_CONTENT_TABLE_KEYS;
                case SQLTableType.Metadata:
                    return NusysConstants.ACCEPTED_METADATA_TABLE_KEYS;
                case SQLTableType.Properties:
                    return NusysConstants.ACCEPTED_PROPERTIES_TABLE_KEYS;
            }
            return new List<string>();
        }

        /// <summary>
        /// returns to you a a new message where all the keys/values in your given message 
        /// are present if they are valid column names in the speicified SQL table
        /// </summary>
        /// <param name="message"></param>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public static Message GetCleanedMessageForDatabase(Message message, SQLTableType tableType)
        {
            IEnumerable<string> acceptedKeys = GetAcceptedKeys(tableType);
            var cleanedMessage = new Message();
            foreach (var key in acceptedKeys)
            {
                if (message.ContainsKey(key))
                {
                    cleanedMessage[key] = message[key];
                }
            }
            return cleanedMessage;
        }

        /// <summary>
        /// to get a contentDataModel from a message taken directly from the ContentDataModel table
        /// </summary>
        /// <param name="m"></param>
        public static ContentDataModel ParseContentDataModelFromDatabaseMessage(Message m)
        {
            if (!m.ContainsKey(NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY))
            {
                throw new Exception("no content id was given to create a contetnDataModel from");
            }
            if (!m.ContainsKey(NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY))
            {
                throw new Exception("no content data was given to create a contetnDataModel from");
            }
            if (!m.ContainsKey(NusysConstants.CONTENT_TABLE_TYPE_KEY))
            {
                throw new Exception("no content type was given to create a contetnDataModel from");
            }
            var id = m.GetString(NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY);
            var contentUrl = m.GetString(NusysConstants.CONTENT_TABLE_CONTENT_URL_KEY);
            var type = m.GetEnum<NusysConstants.ContentType>(NusysConstants.CONTENT_TABLE_TYPE_KEY);
            var contentData = FileHelper.GetDataFromContentURL(contentUrl,type);

            var model = new ContentDataModel(id,contentData);
            model.ContentType = type;
            return model;
        }
        #endregion StaticMethods
    }
}
