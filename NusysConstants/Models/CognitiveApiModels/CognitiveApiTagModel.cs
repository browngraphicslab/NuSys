using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class CognitiveApiTagModel
    {
        /// <summary>
        /// The name or title of the tag.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The level of confidence that the tag is represented in the image
        /// ranges from 0 being not confident to 1 being very confident
        /// </summary>
        public double? Confidence { get; set; }
        /// <summary>
        /// When the name or title of the tag is ambiguous this can provide context.
        /// Does not always have a value however
        /// </summary>
        public string Hint { get; set; }
    }
}
