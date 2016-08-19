using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NusysIntermediate
{
    /// <summary>
    /// this class is the factory class that should be the only place we instantiate Analysis Models in NusysApp.
    /// </summary>
    public class AnalysisModelFactory 
    {
        /// <summary>
        /// this Method will take in a serailized AnalysisModel and return to you the AnalysisModel from that. 
        ///  It also takes in a ContentType so it knows what type to deserialize to.
        /// </summary>
        /// <param name="analysisModelJson"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static AnalysisModel DeserializeFromString(string analysisModelJson, NusysConstants.ContentType contentType)
        {
            Debug.Assert(analysisModelJson != null);

            AnalysisModel model;

            //switch on the content type
            switch (contentType)
            {
                case NusysConstants.ContentType.Image:
                    model = JsonConvert.DeserializeObject<NusysImageAnalysisModel>(analysisModelJson);
                    break;
                case NusysConstants.ContentType.PDF:
                    model = JsonConvert.DeserializeObject<NusysPdfAnalysisModel>(analysisModelJson);
                    break;
                default:
                    throw new Exception(" this content type does not support analysis models yet.");
            }
            return model;
        }
    }
}
