using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class SendTrent
    {
        public Document[] documents { get; set; }
        public string[] stopWords { get; set; }
        public string[] stopPhrases { get; set; }

    }
}