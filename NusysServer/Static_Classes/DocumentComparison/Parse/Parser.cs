using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NusysServer.Static_Classes.DocumentComparison;

namespace NusysServer.DocumentComparison
{
    public class Parser
    {
        private static Dictionary<string, int> termFreqDict;

        public List<Document> parseMultipleDocs(List<string> docs, List<string> ids)
        {
            List<Document> documentList = new List<Document>();

            for (int i = 0; i < docs.Count; i++)
            {
                documentList.Add(parseDocument(docs[i], ""));
                Debug.WriteLine("Done with document: " + i);
            }

            return documentList;
        }

        public static Document parseDocument(string line, string id)
        {
            termFreqDict = new Dictionary<string, int>();

            line = line.ToLower();
            line = line.TrimEnd(' ');
            line = Regex.Replace(line, @"\t|\n|\r", "");

            Regex rgx = new Regex("[^a-z0-9 ]"); // keep just alphanumeric characters
            line = rgx.Replace(line, " ");

            line = Regex.Replace(line, string.Format(@"(\p{{L}}{{{0}}})\p{{L}}+", 11), ""); // remove 12 >
            line = Regex.Replace(line, @"\b\w{1,3}\b", ""); // remove words that have three letters or fewer
            line = Regex.Replace(line, @"\s+", " ");  // remove extra whitespace

            var noSpaces = line.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            HashSet<string> uniqueWords = new HashSet<string>();

            Stemmer stemmer = new Stemmer();

            foreach (var s in noSpaces)
            {
                // stem words
                string word = stemmer.stem(s);
                if (!StopWords.stopWordsSet.Contains(word) && !word.Any(c => char.IsDigit(c)))
                {
                    addToLocalDict(word);

                    if (!uniqueWords.Contains(word))
                    {
                        DocSave.AddToMegaDictionary(word);
                        uniqueWords.Add(word);
                    }
                }
            }

            Debug.WriteLine("done with parsing document with id: " + id);
            return new Document(termFreqDict, id);
        }

        private static void addToLocalDict(string word)
        {
            // add words to dictionary
            if (termFreqDict.ContainsKey(word))
            {
                termFreqDict[word] = termFreqDict[word] += 1;
            }
            else
            {
                termFreqDict[word] = 1;
            }
        }
    }
}