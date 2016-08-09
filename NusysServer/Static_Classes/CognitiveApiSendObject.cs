using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class CognitiveApiSendObject
    {
        public string[] stopWords { get; set; }
        public string[] topicsToExclude { get; set; }
        public CognitiveApiDocument[] documents { get; set; }
        
    }
}