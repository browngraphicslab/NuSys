using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{ 
    public class CognitiveApiReturnDocumentKeyPhrases
    {
        public string id { get; set; }
        public List<string> keyPhrases { get; set; }
    }
}