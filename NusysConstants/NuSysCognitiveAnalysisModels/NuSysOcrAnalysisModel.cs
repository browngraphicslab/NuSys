using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class NuSysOcrAnalysisModel
    {

        /// <summary>
        /// The identified Language of the object character recognition results.
        /// BCP-47 language code. Converters exist for this if you want.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The angle in degrees that the image was rotated so that the identified text would be horizontal
        /// The angle measure is positive clockwise and the transform occurs at the image center.
        /// </summary>
        public double? TextAngle { get; set; }

        /// <summary>
        /// Orientation of the text recognized in the image. The value (up,down,left, or right) refers to 
        /// the direction that the top of the recognized text is facing, 
        /// after the image has been rotated around its center according to the detected text angle
        /// </summary>
        public string Orientation { get; set; }

        /// <summary>
        /// An array of objects, where each object represents a region of recognized text. 
        /// A region consists of multiple lines (e.g. a column of text in a multi-column document).
        /// </summary>
        public List<CognitiveApiRegionModel> Regions { get; set; }

    }
}
