using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Base Document class for sending serialized documents to the cognitive services api
    /// requires only the document text and an identifying id
    /// </summary>
    public class CognitiveApiDocument
    {
        // All public properties are lowercase because they have to be serialized as lowercase in the cog services api

        /// <summary>
        /// A uniquely identifying id for the document. You cannot have two instances of the same id
        /// in a cognitive services api request
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The text contents of the document,
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// Creates a new instance of the base document class, used for sending seralized documents to the cognitive services api
        /// </summary>
        /// <param name="id">A uniquely identifying string id, there cannot be more than one instance of a given id in a cognitive services api request</param>
        /// <param name="text">The text contents of the document to be created</param>
        public CognitiveApiDocument(string id, string text)
        {
            Debug.Assert(id != null && text != null);

            this.id = id;
            this.text = text;
        }

    }
}