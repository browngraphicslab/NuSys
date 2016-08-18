using System.Collections.Generic;

namespace NusysIntermediate
{
    /// <summary>
    /// An array of objects, where each object represents a line of recognized text.
    /// </summary>
    public class CognitiveApiLineModel
    {
        /// <summary>
        /// An array of objects, where each object represents a recognized word.
        /// </summary>
        public List<CognitiveApiWordModel> Words { get; set; }

        /// <summary>
        /// Represents the Normalized bounding box of the Line
        /// </summary>
        public CognitiveApiRectangleModel Rectangle { get; set; }
    }
}