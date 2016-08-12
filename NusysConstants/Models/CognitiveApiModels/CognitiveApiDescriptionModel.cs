using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class CognitiveApiDescriptionModel
    {
        /// <summary>
        /// The tags which the captions are generated from
        /// </summary>
        public string[] Tags { get; set; }
        /// <summary>
        /// A list of human readable sentences and corresponding confidence values
        /// </summary>
        public CognitiveApiCaptionModel[] Captions { get; set; }
    }
}
