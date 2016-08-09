using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// The languages currently supported by microsoft cognitive services for key phrase text analysis
    /// </summary>
    public enum KeyPhrasesLanguages
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
        /// The german language
        /// </summary>
        German,
        /// <summary>
        /// The japanese language
        /// </summary>
        Japanese
    }

    /// <summary>
    /// Helper object to be serialized and sent with a request to microsoft cognitive services for key phrases analysis
    /// </summary>
    public class CognitiveApiDocumentKeyPhrases : CognitiveApiDocument
    {
        // all the public properties are lower case because they will be serialized into json

        /// <summary>
        /// The language of the text being sent for key phrase analysis. Should only be set by the constructor.
        /// </summary>
        public string language { get; set; }

        /// <summary>
        /// Create a new document for key phrase analysis. The default language is english
        /// The id for each document in a cognitive services request must be unique.
        /// </summary>
        /// <param name="id">a unique id for the document. There should only be one instance of each id in a cognitive services request</param>
        /// <param name="text">the text contents of the document</param>
        /// <param name="language">one of the languages supported for key phrase analysis</param>
        public CognitiveApiDocumentKeyPhrases(string id, string text, KeyPhrasesLanguages language = KeyPhrasesLanguages.English) : base(id, text)
        {

            // sets the language to a short language code
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
        /// Creates a new document for key phrase analysis from a base document. default language is english.
        /// </summary>
        /// <param name="document">The base document class for sending seralized json objects to microsoft cognitive services</param>
        /// <param name="language">The language of the text being sent for key phrase analysis</param>
        public CognitiveApiDocumentKeyPhrases(CognitiveApiDocument document, KeyPhrasesLanguages language = KeyPhrasesLanguages.English) : base(document.id, document.text)
        {

            // sets the language to a short language code
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