using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// A mapping for each document submitted, of the document id to the topic id
    /// </summary>
    public class CognitiveApiTopicProcessingTopicAssignment
    {
        // All public properties are lowercase because they have to be serialized as lowercase in the cog services api

        /// <summary>
        /// The unique id of the topic
        /// </summary>
        public string topicId { get; set; }

        /// <summary>
        /// The unique id of the document
        /// </summary>
        public string documentId { get; set; }

        /// <summary>
        /// Document-to-topic affiliation score between 0 and 1. The lower a distance score the stronger the topic affiliation is.
        /// </summary>
        public double distance { get; set; }
    }
}
