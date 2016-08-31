using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// the ENTIRE analysis model for a pdf.
    /// Encapsulates the pdf document text analysis and the image analysis OCR for each page. 
    /// </summary>
    public class NusysPdfAnalysisModel : AnalysisModel
    {
        /// <summary>
        /// the constrctor just takes in the ContentDataModelId of the contentDAtaModel this analysis model is analyzing
        /// </summary>
        /// <param name="contentDataModelId"></param>
        public NusysPdfAnalysisModel(string contentDataModelId) : base(contentDataModelId, NusysConstants.ContentType.PDF) { }

        /// <summary>
        /// the Document analysis that has analyzed this entire pdf.
        /// There is only a single one of these per pdf.
        /// </summary>
        public NusysPdfDocumentAnalysisModel DocumentAnalysisModel { get; set; }

        /// <summary>
        /// the list of OCR analysis models per page of pdf.  
        /// There should be as many of these as there are pages in the pdf.
        /// </summary>
        public List<NuSysOcrAnalysisModel> PageImageAnalysisModels { get; set; }

        /// <summary>
        /// The list of topic-modelling suggested topics for this entire pdf. 
        /// As of 8/25/16 this does not run a trained lda on this pdf, just a topic modelling based off of solely this pdf.
        /// </summary>
        public List<string> SuggestedTopics { get; set; }
    }
}
