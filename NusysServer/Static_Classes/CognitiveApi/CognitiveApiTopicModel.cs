using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Models the return results for a cognitive services key phrases topic analysis
    /// </summary>
    public class CognitiveApiTopicModel
    {
        // All public properties are lowercase because they have to be serialized as lowercase in the cog services api

        /// <summary>
        /// The processing result, use this to get data from the model. Only exists if status = succeeded
        /// </summary>
        public CognitiveApiTopicProcessingResult operationProcessingResult { get; set; }

        /// <summary>
        /// The status of the operation. Takes 3 possible values
        /// 
        ///     notstarted  - the process has not started
        ///     running     - the process is running
        ///     succeeded   - the process has succeeded
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// The date time when the operation was started
        /// </summary>
        public string createdDateTime { get; set; }

        /// <summary>
        /// The type of the operation being processed
        /// </summary>
        public string operationType { get; set; }

    }
}
