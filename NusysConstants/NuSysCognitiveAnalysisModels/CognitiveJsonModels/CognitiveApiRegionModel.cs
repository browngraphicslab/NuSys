using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// a region of recognized text. 
    /// A region consists of multiple lines (e.g. a column of text in a multi-column document).
    /// </summary>
    public class CognitiveApiRegionModel
    {
        /// <summary>
        /// An array of objects, where each object represents a line of recognized text.
        /// </summary>
        public List<CognitiveApiLineModel> Lines { get; set; }

        /// <summary>
        /// Represents the normalized bounding box of the region
        /// </summary>
        public CognitiveApiRectangleModel Rectangle { get; set; }

        /// <summary>
        /// the boolean representing whether the server has marked this specific region to be particularly important.  
        /// NOT FROM THE COGNITIVE SERVICES API. 
        /// </summary>
        public bool MarkedImportant { get; set; }
    }
}
