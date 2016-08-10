using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// A container for the data returned by a cognitive services topic analysis request
    /// </summary>
    public class CognitiveApiTopicProcessingResult
    {
        // All public properties are lowercase because they have to be serialized as lowercase in the cog services api

        /// <summary>
        /// A list of the topics returned by topic analysis
        /// </summary>
        public List<CognitiveApiTopicProcessingTopic> topics { get; set; }

        /// <summary>
        /// A mapping of topics to documents
        /// </summary>
        public List<CognitiveApiTopicProcessingTopicAssignment> topicAssignments { get; set; }

    }
}

