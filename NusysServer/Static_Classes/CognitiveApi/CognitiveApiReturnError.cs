using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Represents a container for any errors returned by the cogntive services request
    /// </summary>
    public class CognitiveApiReturnError
    {
        /// <summary>
        /// The id of the error.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The message the error contains.
        /// </summary>
        public string message { get; set; }
    }
}