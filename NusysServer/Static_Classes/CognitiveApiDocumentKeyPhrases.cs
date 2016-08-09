using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// The languages currently supported by microsoft cognitive services for key phrases analysis
    /// </summary>
    public enum KeyPhrasesLanguages { English, Spanish, German, Japanese }

    /// <summary>
    /// Helper object to be serialized and sent with a request to microsoft cognitive services for key phrases analysis
    /// </summary>
    public class CognitiveApiDocumentKeyPhrases : CognitiveApiDocument
    {

        public string language { get; set; }

        /// <summary>
        /// Create a new document for sentiment analysis the default language is english. sorry world.
        /// </summary>
        /// <param name="id">a unique id for the document</param>
        /// <param name="text">the text contents of the document</param>
        /// <param name="language">one of the languages supported for sentiment analysis</param>
        public CognitiveApiDocumentKeyPhrases(string id, string text, KeyPhrasesLanguages language = KeyPhrasesLanguages.English) : base(id, text)
        {
            switch (language)
            {
                case KeyPhrasesLanguages.English:
                    this.language = "en";
                    break;
                case KeyPhrasesLanguages.Spanish:
                    this.language = "es";
                    break;
                case KeyPhrasesLanguages.German:
                    this.language = "de";
                    break;
                case KeyPhrasesLanguages.Japanese:
                    this.language = "ja";
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
        public CognitiveApiDocumentKeyPhrases(CognitiveApiDocument document, KeyPhrasesLanguages language = KeyPhrasesLanguages.English) : base(document.id, document.text)
        {
            switch (language)
            {
                case KeyPhrasesLanguages.English:
                    this.language = "en";
                    break;
                case KeyPhrasesLanguages.Spanish:
                    this.language = "es";
                    break;
                case KeyPhrasesLanguages.German:
                    this.language = "de";
                    break;
                case KeyPhrasesLanguages.Japanese:
                    this.language = "ja";
                    break;
                default:
                    this.language = "";
                    break;
            }
        }
    }
}