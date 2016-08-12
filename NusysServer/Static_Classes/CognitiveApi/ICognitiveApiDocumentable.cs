using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysServer
{
    /// <summary>
    /// Interface for creating document models that can be sent to the Cognitive Services API for Text Analysis
    /// </summary>
    public interface ICognitiveApiDocumentable
    {
        /// <summary>
        /// A unique identifier for the document, can only be one instance of each id in a congnitive services request
        /// </summary>
        string id { get; set; }

        /// <summary>
        /// The text contents of the document. 
        /// </summary>
        string text { get; set; }

    }
}
