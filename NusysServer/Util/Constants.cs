using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysServer
{
    public class Constants
    {
        public static readonly string IGNORE_SERVER_REQUEST_STRING = "server_ignore_request";
        public static readonly string SERVER_REQUEST_TYPE_STRING = "server_request_type";
        public static readonly string SERVER_REQUEST_ITEM_TYPE_STRING = "server_item_type";
        public static readonly string SERVER_REQUEST_ECHO_TYPE_STRING = "server_echo_type";
        public static readonly string FROM_SERVER_MESSAGE_INDICATOR_STRING = "server_indication_from_server";
        public static readonly string REQUEST_TYPE_STRING = "request_type";

        public static string LOGIN_REQUEST_STRING = "login_request";

        public static string SERVER_SESSION_ID_STRING = "server_session_id";
        public static string VALID_CREDENTIALS_BOOLEAN_STRING = "valid";

        public static string SUBSCRIBE_TO_COLLECTION_BOOL_STRING = "server_subscribe_to_collection_bool";
        public static string COLLECTION_TO_SUBSCRIBE_STRING = "server_collection_to_subscribe";


        public static string WWW_ROOT = Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/")
            ? "C:/Users/graphics_lab/Documents/NuRepo_Test/"
            : "D:/home/site/wwwroot/";

        public static string BASE_FOLDER = Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/")
            ? "C:/Users/graphics_lab/Documents/NuRepo_Test/"
            : "D:/home/site/wwwroot/files/";

        public static bool DELETE_ALL_BUT_ROSEMARY = false;

    }
}
