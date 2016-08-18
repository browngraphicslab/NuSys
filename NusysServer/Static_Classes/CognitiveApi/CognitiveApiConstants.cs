using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Contains constant API Keys which are used to in requests to microsoft cognitive services
    /// </summary>
    public class CognitiveApiConstants
    {
        /// <summary>
        /// API Key used to submit requests to cognitive services computer vision api
        /// </summary>
        public const string COMPUTER_VISION = "32ef911520a34ebc981154a75bb922e0";

        /// <summary>
        /// API Key used to submit requests to cognitive services text analytics api
        /// </summary>
        public const string TEXT_ANALYTICS = "7eb29bcf36b547e986427cb5f0076e41";
    }
}