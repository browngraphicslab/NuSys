using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class CognitiveApiFaceModel
    {
        /// <summary>
        /// The estimated age of the face
        /// </summary>
        public int? Age { get; set; }
        /// <summary>
        /// The predicted gender of the face
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Contains normalized coordinates for a rectangle forming a box around the face
        /// </summary>
        public CognitiveApiFaceRectangleModel FaceRectangle { get; set; }
    }
}
