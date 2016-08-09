using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{ 
    public class CognitiveApiKeyPhrasesModel
    {
        public List<CognitiveApiReturnDocumentKeyPhrases> documents { get; set; }
        public List<CognitiveApiReturnError> errors { get; set; }
    }
}