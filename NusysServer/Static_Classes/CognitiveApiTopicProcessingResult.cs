using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class CognitiveApiTopicProcessingResult
    {
        // All public properties are lowercase because they have to be serialized as lowercase in the cog services api
        public List<CognitiveApiTopicProcessingTopic> topics { get; set; }
        public List<CognitiveApiTopicProcessingTopicAssignment> topicAssignments { get; set; }

    }
}

