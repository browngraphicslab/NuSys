using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Represents a container to hold information about a document returned by cognitive services sentiment analysis
    /// </summary>
    public class CognitiveApiReturnDocumentSentiment
    {
        /// <summary>
        /// The unique id of the document that this data refers to, matches the id mapped to the document wen the key phrase request was created
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// A double representing the sentiment of a document where 0.0 is negative and 1.0 is positive.
        /// </summary>
        public double score { get; set; }
    }
}