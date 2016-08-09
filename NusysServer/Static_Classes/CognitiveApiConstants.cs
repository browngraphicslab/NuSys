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
        public const string COMPUTER_VISION = "d670935ac1b94221be9288c5a2b3f307";

        /// <summary>
        /// API Key used to submit requests to cognitive services text analytics api
        /// </summary>
        public const string TEXT_ANALYTICS = "06ae6c84d15140a7a39d62527d72ec88";
    }
}