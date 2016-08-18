namespace NusysIntermediate
{
    /// <summary>
    /// An array of objects, where each object represents a recognized word.
    /// </summary>
    public class CognitiveApiWordModel
    {
        /// <summary>
        /// String value of a recognized word.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Represents the normalized bounding box of the word
        /// </summary>
        public CognitiveApiRectangleModel Rectangle { get; set; }
    }
}