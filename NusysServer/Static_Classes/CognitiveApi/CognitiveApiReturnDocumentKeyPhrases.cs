using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{ 
    /// <summary>
    /// Represents a container to hold information about a document returned by cognitive services key phrases analysis
    /// </summary>
    public class CognitiveApiReturnDocumentKeyPhrases
    {
        /// <summary>
        /// The unique id of the document that this data refers to, matches the id mapped to the document wen the key phrase request was created
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// A string list of key phrases extracted from the document by cognitive services
        /// </summary>
        public List<string> keyPhrases { get; set; }
    }
}