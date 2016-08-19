using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NusysServer.DocumentComparison;

namespace NusysServer.Static_Classes.DocumentComparison
{
    public class DocSave
    {
        /// <summary>
        /// Cleansed, i.e. with terms that appear in less than k documents removed.
        /// For now, will work with k = 1 until # of documents is > 50 (switch to k = 3).
        /// Tfidf vectors should always work with the cleansed dictionary.
        /// </summary>
        [JsonProperty("CleansedMegaDictionary")]
        public static Dictionary<string, int> CleansedMegaDictionary { get; set; }
        
        /// <summary>
        /// Dictionary mapping each word in the entire corpus to how many documents it is in 
        /// </summary>
        [JsonProperty("MegaDictionary")]
        public static Dictionary<string, int> MegaDictionary { get; set; }

        /// <summary>
        /// Maps from id to Document
        /// </summary>
        [JsonProperty("IdToDocumentDictionary")]
        public static Dictionary<string, Document> IdToDocumentDictionary { get; set; }

        /// <summary>
        /// Maps from id to document title, only for testing purposes
        /// </summary>
        [JsonProperty("IdToTitleDictionary")]
        public static Dictionary<string, string> IdToTitleDictionary { get; set; }

        public DocSave()
        {
            MegaDictionary = new Dictionary<string, int>();
            IdToDocumentDictionary = new Dictionary<string, Document>();
            IdToTitleDictionary = new Dictionary<string, string>();
            CleansedMegaDictionary = new Dictionary<string, int>();
        }

        /// <summary>
        /// Adds a term to the MegaDictionary
        /// </summary>
        /// <param name="key"></param>
        public static void AddToMegaDictionary(string key)
        {
            if (MegaDictionary.ContainsKey(key))
            {
                MegaDictionary[key] = MegaDictionary[key] += 1;
            }
            else
            {
                MegaDictionary[key] = 1;
            }
        }

        /// <summary>
        /// Returns all the keys of the cleansed megadictionary as a list
        /// </summary>
        /// <returns></returns>
        public static List<string> ReturnCleansedMegaKeysList()
        {
            return new List<string>(CleansedMegaDictionary.Keys);
        }

        /// <summary>
        /// Returns the # of documents a word is in, should never return 0
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static double ReturnMegaTermFrequency(string word)
        {
            int value;

            if (MegaDictionary.TryGetValue(word, out value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Adds a document to the IdToDocumentDictionary
        /// </summary>
        /// <param name="document"></param>
        /// <param name="title"></param>
        public static void addDocument(Document document, string title)
        {
            var id = document.Id;

            if (IdToDocumentDictionary.ContainsKey(id))
            {
                IdToDocumentDictionary[id] = document;
            }
            else
            {
                IdToDocumentDictionary.Add(id, document);
            }

            // TODO remove, for testing purposes
            if (IdToTitleDictionary.ContainsKey(id))
            {
                IdToTitleDictionary[id] = title;
            }
            else
            {
                IdToTitleDictionary.Add(id, title);
            }
        }

        /// <summary>
        /// Deletes a document from IdToDocumentDictionary
        /// </summary>
        public static void deleteDocument(string id)
        {
            // get the termfreq dictionary
            Document document = getDocumentFromId(id);

            if (document != null)
            {
                Dictionary<string, int> termFreqDictionary = document.GetDictionary();

                // loop through all of the keys and decrement/delete it from the megadictionary
                foreach (var word in termFreqDictionary.Keys)
                {
                    if (MegaDictionary[word] == 1) // delete, no longer in any document
                    {
                        MegaDictionary.Remove(word);
                    }
                    else
                    {
                        MegaDictionary[word] -= 1;
                    }
                }

                // remove from id to document dictionary
                if (IdToDocumentDictionary.ContainsKey(id))
                {
                    IdToDocumentDictionary.Remove(id);
                }

                // remove from title dict
                if (IdToTitleDictionary.ContainsKey(id))
                {
                    IdToTitleDictionary.Remove(id);
                }
            }
            else
            {
                Debug.WriteLine("Could not find the document, was not deleted");
            }
        }

        /// <summary>
        /// For testing, gets title given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string getTitle(string id)
        {
            return IdToTitleDictionary[id];
        }

        /// <summary>
        /// Retrives Document given an id, returns null if not found
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Document getDocumentFromId(string id)
        {
            if (IdToDocumentDictionary.ContainsKey(id))
            {
                return IdToDocumentDictionary[id];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// for testing
        /// </summary>
        /// <returns></returns>
        public static Document returnRandomDocument()
        {
            List<Document> documents = getDocumentList();

            Random random = new Random();
            int randomInt = random.Next(getDocumentCount() - 1);
            return documents[randomInt];
        }

        public static int getDocumentCount()
        {
            return IdToDocumentDictionary.Count;
        }

        public static List<Document> getDocumentList()
        {
            return IdToDocumentDictionary.Values.ToList();
        }

        /// <summary>
        /// Removes all entries in dictionary that have a value of k (removing all terms that show up in less than 
        /// k documents in the corpus, which dramatically reduces the number of entries and hence the dimensions of each
        /// TFIDF vector -- most of these entries are typos / gibberish and removing them doesn't seem change the output to 
        /// any large extent)
        /// </summary>
        public static void UpdateCleansedMegaDictionary(int k)
        {
            // copy over megadictionary to cleansedmegadictionary
            CleansedMegaDictionary = new Dictionary<string, int>(MegaDictionary);

            // creating list bc it's easy to check which words are removed
            List<string> removals = new List<string>();
            
            foreach (KeyValuePair<string, int> entry in MegaDictionary)
            {
                // do something with entry.Value or entry.Key
                if (entry.Value < k) // there exists only one document with this term - high probability it is a typo
                {
                    removals.Add(entry.Key);
                }
            }

            foreach (var word in removals)
            {
                CleansedMegaDictionary.Remove(word);
            }
        }
    }

}