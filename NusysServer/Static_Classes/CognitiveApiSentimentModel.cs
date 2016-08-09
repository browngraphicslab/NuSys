using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class CognitiveApiSentimentModel
    {
        public List<CognitiveApiReturnDocumentSentiment> documents { get; set; }
        public List<CognitiveApiReturnError> errors { get; set; }
    }
}