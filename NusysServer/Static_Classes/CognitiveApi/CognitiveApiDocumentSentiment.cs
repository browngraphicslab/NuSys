using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// The languages currently supported by microsoft cognitive services for sentiment analysis
    /// </summary>
    public enum SentimentLanguages
    {
        /// <summary>
        /// The english language
        /// </summary>
        English,
        /// <summary>
        /// The spanish language
        /// </summary>
        Spanish,
        /// <summary>
        /// The french language
        /// </summary>
        French,
        /// <summary>
        /// The portuguese language
        /// </summary>
        Portuguese
    }

    /// <summary>
    /// Helper object to be serialized and sent with a request to microsoft cognitive services for sentiment analysis
    /// </summary>
    public class CognitiveApiDocumentSentiment : CognitiveApiDocument
    {
        /// <summary>
        /// The language of the text being sent for key phrase analysis. Should only be set by the constructor.
        /// </summary>
        public string language { get; set; }

        /// <summary>
        /// Create a new document for sentiment analysis. The default language is english
        /// The id for each document in a cognitive services request must be unique.
        /// </summary>
        /// <param name="id">a unique id for the document. There should only be one instance of each id in a cognitive services request</param>
        /// <param name="text">the text contents of the document</param>
        /// <param name="language">one of the languages supported for sentiment analysis</param>
        public CognitiveApiDocumentSentiment(string id, string text, SentimentLanguages language=SentimentLanguages.English ) : base(id, text)
        {
            // Assign a short code to language based on the language passed in
            switch (language)
            {
                case SentimentLanguages.English:
                    this.language = "en";
                    break;
                case SentimentLanguages.Spanish:
                    this.language = "es";
                    break;
                case SentimentLanguages.French:
                    this.language = "fr";
                    break;
                case SentimentLanguages.Portuguese:
                    this.language = "pt";
                    break;
                default:
                    this.language = "";
                    break;
            }
        }

        /// <summary>
        /// Creates a new document for sentiment analysis from a base document. default language is english.
        /// </summary>
        /// <param name="document">The base document class for sending seralized json objects to microsoft cognitive services</param>
        /// <param name="language">The language of the text being sent for sentiment analysis</param>
        public CognitiveApiDocumentSentiment(CognitiveApiDocument document, SentimentLanguages language=SentimentLanguages.English) : base(document.id, document.text)
        {
            switch (language)
            {
                case SentimentLanguages.English:
                    this.language = "en";
                    break;
                case SentimentLanguages.Spanish:
                    this.language = "es";
                    break;
                case SentimentLanguages.French:
                    this.language = "fr";
                    break;
                case SentimentLanguages.Portuguese:
                    this.language = "pt";
                    break;
                default:
                    this.language = "";
                    break;
            }
        }
    }
}
