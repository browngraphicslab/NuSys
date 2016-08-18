using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class CognitiveApiRectangleModel
    {
        /// <summary>
        /// The normalized width of the face rectangle
        /// </summary>
        public double? Width { get; set; }
        /// <summary>
        /// The normalized height of the face rectangle
        /// </summary>
        public double? Height { get; set; }
        /// <summary>
        /// The normalized distance of the face rectangle from the left side of the image
        /// </summary>
        public double? Left { get; set; }
        /// <summary>
        /// The normalized distance of the face rectangle from the top of the image
        /// </summary>
        public double? Top { get; set; }
    }
}
