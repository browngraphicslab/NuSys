using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// The languages currently supported by microsoft cognitive services for sentiment analysis
    /// </summary>
    public enum SentimentLanguages  {English, Spanish, French, Portuguese}

    /// <summary>
    /// Helper object to be serialized and sent with a request to microsoft cognitive services for sentiment analysis
    /// </summary>
    public class CognitiveApiDocumentSentiment : CognitiveApiDocument
    {

        public string language { get; set; }

        /// <summary>
        /// Create a new document for sentiment analysis the default language is english. sorry world.
        /// </summary>
        /// <param name="id">a unique id for the document</param>
        /// <param name="text">the text contents of the document</param>
        /// <param name="language">one of the languages supported for sentiment analysis</param>
        public CognitiveApiDocumentSentiment(string id, string text, SentimentLanguages language=SentimentLanguages.English ) : base(id, text)
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

        /// <summary>
        /// Creates a new document for sentiment analysis from a base document. default language is english. sorry world.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="language"></param>
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
