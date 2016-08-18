using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class CognitiveApiCaptionModel
    {
        /// <summary>
        /// The human readable sentence for the given image
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// The confidence ranging from 0 not confident to 1 very confident
        /// that the text describes the given image
        /// </summary>
        public double? Confidence { get; set; }
    }
}
