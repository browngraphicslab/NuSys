using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class ContentController { 
        private static ContentController _instance = null;
        public static ContentController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ContentController();
                }
                return _instance;
            }
        }

        private ContentController()
        {
            SqlConnector = new SQLConnector();
        }
    
    }
}