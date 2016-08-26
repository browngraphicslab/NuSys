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
        public const string COMPUTER_VISION = "7ba8a277262f4410aca39c1947a262de";

        /// <summary>
        /// API Key used to submit requests to cognitive services text analytics api
        /// </summary>
        public const string TEXT_ANALYTICS = "dc8098cc7b864c798382438960169565";
    }
}