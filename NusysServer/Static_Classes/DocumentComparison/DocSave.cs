using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NusysServer.DocumentComparison;

namespace NusysServer.Static_Classes.DocumentComparison
{
    public class DocSave
    {
        [JsonProperty("MegaDictionary")]
        public static Dictionary<string, int> MegaDictionary { get; set; }
        [JsonProperty("IdToDocumentDictionary")]
        public static Dictionary<string, Document> IdToDocumentDictionary { get; set; }
        [JsonProperty("IdToTitleDictionary")]
        public static Dictionary<string, string> IdToTitleDictionary { get; set; }

        public DocSave()
        {
            MegaDictionary = new Dictionary<string, int>();
            IdToDocumentDictionary = new Dictionary<string, Document>();
            IdToTitleDictionary = new Dictionary<string, string>();
        }

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

        public static List<string> ReturnMegaKeysList()
        {
            return new List<string>(MegaDictionary.Keys);
        }

        public static double ReturnMegaTermFrequency(string word)
        {
            int value;

            if (MegaDictionary.TryGetValue(word, out value))
            {
                return value;
            }
            else
            {
                return -1;
            }
        }

        public static void addDocument(string id, Document document, string title)
        {
            if (IdToDocumentDictionary.ContainsKey(id))
            {
                IdToDocumentDictionary[id] = document;
            }
            else
            {
                IdToDocumentDictionary.Add(id, document);
            }

            // TODO remove
            if (IdToTitleDictionary.ContainsKey(id))
            {
                IdToTitleDictionary[id] = title;
            }
            else
            {
                IdToTitleDictionary.Add(id, title);
            }
        }

        public static string getTitle(string id)
        {
            return IdToTitleDictionary[id];
        }

        /// <summary>
        /// returns null if not found
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
        /// Removes all entries in dictionary that have a value of 1 (removing all terms that show up in only 
        /// one document in the corpus, which dramatically reduces the number of entries and hence the size of each
        /// TFIDF vector -- most of these entries are typos / gibberish and removing them doesn't change the output to 
        /// any large extent)
        /// </summary>
        public static void CleanseMegaDictionary()
        {
            List<string> removals = new List<string>();

            foreach (KeyValuePair<string, int> entry in MegaDictionary)
            {
                // do something with entry.Value or entry.Key
                if (entry.Value < 3) // there exists only one document with this term - high probability it is a typo
                {
                    removals.Add(entry.Key);
                }
            }

            foreach (var word in removals)
            {
                MegaDictionary.Remove(word);
            }
        }
    }

}
