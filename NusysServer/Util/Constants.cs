using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.DynamicData.ModelProviders;
using NusysIntermediate;

namespace NusysServer
{
    public class Constants
    {
        public static readonly string SERVER_SESSION_ID_STRING = "server_session_id";
        public static readonly string VALID_CREDENTIALS_BOOLEAN_STRING = "valid";

        public static readonly string user = "sahil"; //TODO: CHANGE TO PRIVATE LATER

        public static string WWW_ROOT {
            get
            {
                switch (user)
                {
                    case "leandro":
                        return Directory.Exists("C:/Users/Leandro Bengzon/Documents/NuSys Server/NuSys/NusysServer/") ? "C:/Users/Leandro Bengzon/Documents/NuSys Server/NuSys/NusysServer/" : "D:/home/site/wwwroot/";
                    case "trent":
                        return Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/") ? "C:/Users/graphics_lab/Documents/Trent_Nusys/nusys/NusysServer/" : "D:/home/site/wwwroot/";
                    case "harsh":
                        return Directory.Exists("C:/Users/Brown GFX/Documents/NuSys_Server") ? "C:/Users/Brown GFX/Documents/NuSys_Server" : "D:/home/site/wwwroot/";
                    case "miranda":
                        return Directory.Exists("C:/Users/miran_000/Documents/NuSys/NusysServer/") ? "C:/Users/miran_000/Documents/NuSys/NusysServer/" : "D:/home/site/wwwroot/";
                    case "sahil":
                        return Directory.Exists("C:/Users/nusys/Documents/Sahil7/NusysServer/") ? "C:/Users/nusys/Documents/Sahil7/NusysServer/" : "D:/home/site/wwwroot/";
                    case "zach":
                        return Directory.Exists("C:/Users/Zach/Documents/Visual Studio 2015/Projects/nusys/NusysServer/") ? "C:/Users/Zach/Documents/Visual Studio 2015/Projects/nusys/NusysServer/" : "D:/home/site/wwwroot/";
                    case "book":
                        return Directory.Exists("C:/Users/nusys/Desktop/Leandro NEW SIS/nusys/NusysServer/") ? "C:/Users/nusys/Desktop/Leandro NEW SIS/nusys/NusysServer/" : "D:/home/site/wwwroot/";
                    case "luke":
                        return Directory.Exists("C:/Users/luke murray/Documents/Visual Studio 2015/Projects/NewSys/NusysServer/") ? "C:/Users/luke murray/Documents/Visual Studio 2015/Projects/NewSys/NusysServer/" : "D:/home/site/wwwroot/";
                    default:
                        return "D:/home/site/wwwroot/";
                }
            }
        }

        private static bool Local = WWW_ROOT != "D:/home/site/wwwroot/";

        //public static readonly string WWW_ROOT = Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/")
        //    ? "C:/Users/graphics_lab/Documents/NuRepo_Test/"
        //    : "D:/home/site/wwwroot/";

        /// <summary>
        /// the folder on the server where all the content files will be saved.  
        /// Should only be storing .txt and .pdf files as of 7/31/16
        /// </summary>
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
                    case "harsh":
                        return Directory.Exists("C:/Users/Brown GFX/Documents/NuSys_Server") ? "C:/ Users/Brown GFX/Documents/NuSys_Server" : "D:/home/site/wwwroot/files/";
                    case "miranda":
                        return Directory.Exists("C:/Users/miran_000/Documents/NuSys/NusysServer/")
                            ? "C:/Users/miran_000/Documents/NuSys/NusysServer/"
                            : "D:/home/site/wwwroot/files/";
                    case "sahil":
                        return Directory.Exists("C:/Users/nusys/Documents/Sahil7/NusysServer/") ? "C:/Users/nusys/Documents/Sahil7/NusysServer/" : "D:/home/site/wwwroot/files/";
                    case "zach":
                        return Directory.Exists("C:/Users/Zach/Documents/Visual Studio 2015/Projects/nusys/NusysServer/") ? "C:/Users/Zach/Documents/Visual Studio 2015/Projects/nusys/NusysServer/" : "D:/home/site/wwwroot/files/";
                    case "book":
                        return Directory.Exists("C:/Users/nusys/Desktop/Leandro NEW SIS/nusys/NusysServer/") ? "C:/Users/nusys/Desktop/Leandro NEW SIS/nusys/NusysServer/" : "D:/home/site/wwwroot/files/";
                    case "luke":
                        return Directory.Exists("C:/Users/luke murray/Documents/Visual Studio 2015/Projects/NewSys/NusysServer/") ? "C:/Users/luke murray/Documents/Visual Studio 2015/Projects/NewSys/NusysServer/" : "D:/home/site/wwwroot/files/";
                    default:
                        return "D:/home/site/wwwroot/files/";
                }
            }
        }

        //public static readonly string FILE_FOLDER = Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/")
        //    ? "C:/Users/graphics_lab/Documents/NuRepo_Test/"
        //    : "D:/home/site/wwwroot/files/";


        public static string SERVER_ADDRESS {
            get
            {
                switch (user)
                {
                    case "leandro":
                        return Directory.Exists("C:/Users/Leandro Bengzon/Documents/NuSys Server/") ? "http://localhost:2685/" : "http://nusysrepo.azurewebsites.net/";
                    case "trent":
                        return Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/") ? "http://localhost:2685/" : "http://nusysrepo.azurewebsites.net/";
                    case "harsh":
                        return Directory.Exists("C:/Users/Brown GFX/Documents/NuSys_Server") ? "http://localhost:2685/" : "http://nusysrepo.azurewebsites.net/";
                    case "miranda":
                        return Directory.Exists("C:/Users/miran_000/Documents/NuSys/NusysServer/") ? "http://localhost:2685/" : "http://nusysrepo.azurewebsites.net/";
                    case "sahil":
                        return Directory.Exists("C:/Users/nusys/Documents/Sahil7/NusysServer/") ? "http://localhost:2685/" : "http://nusysrepo.azurewebsites.net/";
                    case "zach":
                        return Directory.Exists("C:/Users/Zach/Documents/Visual Studio 2015/Projects/nusys/NusysServer/") ? "http://localhost:2685/" : "D:/home/site/wwwroot/";
                    case "book":
                        return Directory.Exists("C:/Users/nusys/Desktop/Leandro NEW SIS/nusys/NusysServer/") ? "http://localhost:2685/" : "http://nusysrepo.azurewebsites.net/";
                    case "luke":
                        return Directory.Exists("C:/Users/luke murray/Documents/Visual Studio 2015/Projects/NewSys/NusysServer/") ? "http://localhost:2685/" : "http://nusysrepo.azurewebsites.net/";
                    default:
                        return "http://nusysrepo.azurewebsites.net/";
                }
            }
        }

        /// <summary>
        /// Collection of all the column names in all of the tables
        /// </summary>
        public static readonly IEnumerable<string> SqlColumns =
            GetFullColumnTitles(SQLTableType.Alias, GetAcceptedKeys(SQLTableType.Alias)).Concat(
            GetFullColumnTitles(SQLTableType.LibraryElement, GetAcceptedKeys(SQLTableType.LibraryElement)).Concat(
            GetFullColumnTitles(SQLTableType.Content, GetAcceptedKeys(SQLTableType.Content)).Concat(
            GetFullColumnTitles(SQLTableType.Metadata, GetAcceptedKeys(SQLTableType.Metadata)).Concat(
            GetFullColumnTitles(SQLTableType.Properties, GetAcceptedKeys(SQLTableType.Properties))))));

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
            LibraryElement,
            Metadata,
            Properties,
            Content,
            PresentationLink,
            Users,
            AnalysisModels
        }

        /// <summary>
        /// the operator used for SQl command making
        /// </summary>
        public enum Operator
        {
            And,
            Or
        }
        
        /// <summary>
        /// The possible join operators for tables
        /// </summary>
        public enum JoinedType
        {
            LeftJoin,
            InnerJoin,
            RightJoin
        }

        #region StaticMethods

        /// <summary>
        /// returns amn IEnumerable with all the column keys for a given sql table.  
        /// populated by string lists in the constants class
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAcceptedKeys(SQLTableType tableType, bool fullColumnNames = true)
        {
            IEnumerable<string> keys;
            switch (tableType)
            {
                case SQLTableType.Alias:
                    keys =  NusysConstants.ALIAS_ACCEPTED_KEYS.Keys;
                    break;
                case SQLTableType.LibraryElement:
                    keys = NusysConstants.LIBRARY_ELEMENT_MODEL_ACCEPTED_KEYS.Keys;
                    break;
                case SQLTableType.Content:
                    keys = NusysConstants.ACCEPTED_CONTENT_TABLE_KEYS;
                    break;
                case SQLTableType.Metadata:
                    keys = NusysConstants.ACCEPTED_METADATA_TABLE_KEYS;
                    break;
                case SQLTableType.Properties:
                    keys = NusysConstants.ACCEPTED_PROPERTIES_TABLE_KEYS;
                    break;
                case SQLTableType.PresentationLink:
                    keys = NusysConstants.ACCEPTED_PRESENTATION_LINKS_TABLE_KEYS;
                    break;
                case SQLTableType.AnalysisModels:
                    keys = NusysConstants.ACCEPTED_ANALYSIS_MODELS_TABLE_KEYS;
                    break;
                default:
                    return new List<string>();
            }
            if (!fullColumnNames)
            {
                return keys;
            }
            keys = keys.Select(key => GetTableName(tableType) + "." + key);
            return keys;
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
            IEnumerable<string> acceptedKeys = GetAcceptedKeys(tableType,false);
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
        /// Takes a list of possible column keys and returns the same list for every valid key.  
        /// Used to clean key lists before database entry
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetCleanIEnumerableForDatabase(IEnumerable<string> keys, SQLTableType tableType)
        {
            return GetAcceptedKeys(tableType).Intersect(keys);
        }

        /// <summary>
        /// Simply assigns the sql table name to the column titles for complex queries
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetFullColumnTitles(SQLTableType tableType, IEnumerable<string> columnNames)
        {
            return columnNames.Select(name => GetTableName(tableType) + "." + name);
        }

        /// <summary>
        /// Simply assigns the sql table name to the column title for complex queries
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetFullColumnTitle(SQLTableType tableType, string columnName)
        {
            return new List<string>() {GetTableName(tableType) + "." + columnName};
        }

        /// <summary>
        /// will take in a list of table properties and strip the table name off of it. 
        /// Works whether or not the table name is present
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static Message StripTableNames(Message properties)
        {
            return new Message(properties.ToDictionary(kvp => kvp.Key.Contains(".") ? kvp.Key.Substring(kvp.Key.IndexOf(".") + 1) : kvp.Key, kvp => kvp.Value));
        }


        /// <summary>
        /// method to return the name of the sql table
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Constants.SQLTableType type)
        {
            string name;
            switch (type)
            {
                case Constants.SQLTableType.Alias:
                    name = NusysConstants.ALIASES_SQL_TABLE_NAME;
                    break;
                case Constants.SQLTableType.LibraryElement:
                    name = NusysConstants.LIBRARY_ELEMENTS_SQL_TABLE_NAME;
                    break;
                case Constants.SQLTableType.Metadata:
                    name = NusysConstants.METADATA_SQL_TABLE_NAME;
                    break;
                case Constants.SQLTableType.Properties:
                    name = NusysConstants.PROPERTIES_SQL_TABLE_NAME;
                    break;
                case Constants.SQLTableType.Content:
                    name = NusysConstants.CONTENTS_SQL_TABLE_NAME;
                    break;
                case Constants.SQLTableType.PresentationLink:
                    name = NusysConstants.PRESENTATION_LINKS_SQL_TABLE_NAME;
                    break;
                case Constants.SQLTableType.AnalysisModels:
                    name = NusysConstants.ANALYSIS_MODELS_SQL_TABLE_NAME;
                    break;
                case Constants.SQLTableType.Users:
                    name = NusysConstants.USERS_SQL_TABLE_NAME;
                    break;
                default:
                    throw new Exception("type not supported yet for getting the table name");
            }
            if (Local)
            {
                return name + "_" + user;//TODO remove all the '  +"_"+user   ', it's only for testing
            }
            return name;
        }

        #endregion StaticMethods
    }
}
