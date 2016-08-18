using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Represents a portion of a pdf, normally used with a single sentence
    /// </summary>
    public class NusysPdfSegmentAnalysisModel
    {
        /// <summary>
        /// The text of the segment
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The sentiment of the segment, from 0 negative to 1 positive
        /// </summary>
        public double SentimentRating { get; set; }

        /// <summary>
        /// A string list of key phrases extracted from the segment by cognitive services
        /// </summary>
        public List<string> KeyPhrases { get; set; }

        /// <summary>
        /// The page number of the segment in the pdf document
        /// </summary>
        public int pageNumber { get; set; }
    }
}
