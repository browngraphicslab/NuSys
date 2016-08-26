using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class NusysPdfDocumentAnalysisModel
    {

        /// <summary>
        /// A List of segments, representing single sentences within the pdf
        /// </summary>
        public List<NusysPdfSegmentAnalysisModel> Segments;

        
        /// <summary>
        /// The average of all the sentiments for all the segments in the pdf
        /// </summary>
        public double? AverageSentiment
        {
            get { return Segments?.Average(segment => segment.SentimentRating); }
        }

        /// <summary>
        /// The total text of the pdf
        /// </summary>
        public string TotalText => string.Join(" ", Segments.Select(item => item.Text));

    }
}
