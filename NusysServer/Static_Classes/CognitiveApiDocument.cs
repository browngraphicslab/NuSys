using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Helper object that is serialized and sent in json format to microsoft cognitive services API
    /// </summary>
    public class CognitiveApiDocument
    {
        /// <summary>
        /// All public properties are lowercase because they have to be serialized as lowercase in the cog services api
        /// </summary>
        public string id { get; set; }
        public string text { get; set; }

        public CognitiveApiDocument(string id, string text)
        {
            this.id = id;
            this.text = text;
        }

    }
}