using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Models the return results for a cognitive services sentiment text analysis
    /// </summary>
    public class CognitiveApiSentimentModel
    {

        // all public properties are lower case because they need to be serialized for json

        /// <summary>
        /// An array of documents, use this to get the data from the model
        /// </summary>
        public List<CognitiveApiReturnDocumentSentiment> documents { get; set; }

        /// <summary>
        /// An array of errors, use this to see if any errors occured during analysis
        /// </summary>
        public List<CognitiveApiReturnError> errors { get; set; }
    }
}
