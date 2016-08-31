using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// a super class for all the analysis models.  
    /// This abstract class simply requires the Analysis model to have a Content Data Model Id that points back to the content that this model will be the analysis of.
    /// </summary>
    public class AnalysisModel
    {
        /// <summary>
        /// the Content Data Model ID of the content that his Analysis models analyzes. 
        /// </summary>
        public string ContentDataModelId { get; private set; }

        /// <summary>
        /// the type of analysis model this is
        /// </summary>
        public NusysConstants.ContentType Type { get; set; }

        /// <summary>
        /// the constructor for the abstract class.  
        /// Just sets content Data Model ID of the analysis model and the type of content data model this is an analysis model of
        /// </summary>
        /// <param name="contentDataModelId"></param>
        public AnalysisModel(string contentDataModelId, NusysConstants.ContentType type)
        {
            ContentDataModelId = contentDataModelId;
            Type = type;
            //if we have been given a null or empty Id
            if (string.IsNullOrEmpty(ContentDataModelId))
            {
                throw new Exception("You can't have a null or empty Id when creating an AnalysisModel");
            }
        }

    }
}
