using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// A wrapper for information returned about a single topic from a cognitive services topic analysis request
    /// </summary>
    public class CognitiveApiTopicProcessingTopic
    {
        // All public properties are lowercase because they have to be serialized as lowercase in the cog services api

        /// <summary>
        /// The unique id for the topic
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The number of documents which are mapped to this document
        /// </summary>
        public double score { get; set; }

        /// <summary>
        /// A summarizing word or phrase for the topic
        /// </summary>
        public string keyPhrase { get; set; }
    }
}



