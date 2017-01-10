using System.Collections.Generic;

namespace ParserHelper
{
    /// <summary>
    /// Generic container used to send documents to the cognitive services api
    /// </summary>
    public class CognitiveApiSendObject
    {
        // all public classes are lower case because they need to be serialized for the cognitive services api

        /// <summary>
        /// Applies to topic analysis, see GetTextTopicsAsync in TextProcessor
        /// </summary>
        public string[] stopWords { get; set; }

        /// <summary>
        /// Applies to topic analysis, see GetTextTopicsAsync in TextProcessor
        /// </summary>
        public string[] topicsToExclude { get; set; }

        /// <summary>
        /// A list of documents, CognitiveApiDocument is the base document, all other document types extend from this
        /// </summary>
        public IEnumerable<ICognitiveApiDocumentable> documents { get; set; }
        
    }
}