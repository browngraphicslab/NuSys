using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class CognitiveApiTopicModel
    {
        // All public properties are lowercase because they have to be serialized as lowercase in the cog services api
        public CognitiveApiTopicProcessingResult operationProcessingResult { get; set; }
        public string status { get; set; }
        public string createdDateTime { get; set; }
        public string operationType { get; set; }

    }
}
