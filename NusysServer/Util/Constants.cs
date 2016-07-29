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
        public static readonly string SERVER_SESSION_ID_STRING = "server_session_id";
        public static readonly string VALID_CREDENTIALS_BOOLEAN_STRING = "valid";

        public static readonly string WWW_ROOT = Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/")
            ? "C:/Users/graphics_lab/Documents/NuRepo_Test/"
            : "D:/home/site/wwwroot/";

        public static readonly string BASE_FOLDER = Directory.Exists("C:/Users/graphics_lab/Documents/NuRepo_Test/")
            ? "C:/Users/graphics_lab/Documents/NuRepo_Test/"
            : "D:/home/site/wwwroot/files/";

        public static readonly bool DELETE_ALL_BUT_ROSEMARY = false;

    }
}
