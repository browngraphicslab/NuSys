using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class CognitiveApiTopicProcessingTopicAssignment
    {
        // All public properties are lowercase because they have to be serialized as lowercase in the cog services api
        public string topicId { get; set; }
        public string documentId { get; set; }
        public double distance { get; set; }
    }
}
