using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class CognitiveApiTopicProcessingTopic
    {
        // All public properties are lowercase because they have to be serialized as lowercase in the cog services api
        public string id { get; set; }
        public double score { get; set; }
        public string keyPhrase { get; set; }
    }
}



