using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NusysServer.DocumentComparison
{
    public class Tfidf
    {
        public static void setTfidfVector(Document document, int documentCount)
        {
            // generate list ordering of megadictionary
            List<string> keysList = MegaDictionary.ReturnKeysList();
            
            Dictionary<int, double> TFIDFDict = new Dictionary<int, double>();

            // calculate TFDIF vector for document
            for (int i = 0; i < keysList.Count; i++)
            {
                string word = keysList[i];
                double tf = document.UniqueWordsFreq() == 0 ? 0 : (double)document.ReturnFrequency(word) / document.UniqueWordsFreq(); // if document has 0 terms it it, return 0
                double calc = documentCount / MegaDictionary.ReturnTermFrequency(word);
                double idf = Math.Log(calc);
                double tfidf = tf * idf;

                // only add to dictionary if tfidf is not 0
                if (tfidf != 0)
                {
                    TFIDFDict.Add(i, tfidf);
                }
            }

            // Normalize the vector
            NormalizeDictionary(TFIDFDict);

            // Add it to the document
            document.setTfidfVector(TFIDFDict);
        }

        public static void setTfidfVectors(List<Document> documents)
        {
            // generate list ordering of megadictionary
            List<string> keysList = MegaDictionary.ReturnKeysList();

            foreach (var document in documents)
            {
                Debug.WriteLine("TFIDF vector for document id: " + document.Id);
                Dictionary<int, double> TFIDFDict = new Dictionary<int, double>();

                // calculate TFDIF vector for document
                for (int i = 0; i < keysList.Count; i++)
                {
                    string word = keysList[i];
                    double tf = document.UniqueWordsFreq() == 0 ? 0 : (double)document.ReturnFrequency(word) / document.UniqueWordsFreq(); // if document has 0 terms it it, return 0
                    double calc = documents.Count / MegaDictionary.ReturnTermFrequency(word);
                    double idf = Math.Log(calc);
                    double tfidf = tf * idf;

                    // only add to dictionary if tfidf is not 0
                    if (tfidf != 0)
                    {
                        TFIDFDict.Add(i, tfidf);
                    }
                }

                // Normalize the vector
                NormalizeDictionary(TFIDFDict);

                // Add it to the document
                document.setTfidfVector(TFIDFDict);
            }
        } 
        
        public static Dictionary<int, double>[] ReturnTFIDFDicts(List<Document> documents)
        {
            // generate list ordering of megadictionary
            List<string> keysList = MegaDictionary.ReturnKeysList();

            List<Dictionary<int, double>> TFIDFDictionaryList = new List<Dictionary<int, double>>();
            int counter = 1;

            foreach (var document in documents)
            {
                Debug.WriteLine("TFIDF vector for document #: " + counter);
                Dictionary<int, double> TFIDFDict = new Dictionary<int, double>();

                // calculate TFDIF vector for document
                for (int i = 0; i < keysList.Count; i++)
                {
                    string word = keysList[i];
                    double tf = document.UniqueWordsFreq() == 0 ? 0 : (double)document.ReturnFrequency(word) / document.UniqueWordsFreq(); // if document has 0 terms it it, return 0
                    double calc = documents.Count / MegaDictionary.ReturnTermFrequency(word);
                    double idf = Math.Log(calc);
                    double tfidf = tf * idf;

                    // only add to dictionary if tfidf is not 0
                    if (tfidf != 0)
                    {
                        TFIDFDict.Add(i, tfidf);
                    }
                }

                TFIDFDictionaryList.Add(TFIDFDict);
                counter++;
            }

            // change into array and normalize
            Dictionary<int, double>[] listOfDictionaries = TFIDFDictionaryList.ToArray();
            NormalizeDictionaryArray(listOfDictionaries);
            return listOfDictionaries;
        }

        private static void NormalizeDictionary(Dictionary<int, double> input)
        {
            double sumSquare = 0;
            foreach (double d in input.Values)
            {
                sumSquare += (d * d);
            }
            double sqrtSumSquare = Math.Sqrt(sumSquare);

            if (sqrtSumSquare != 0)
            {
                List<int> keysList = new List<int>(input.Keys);

                foreach (var key in keysList)
                {
                    input[key] = input[key] / sqrtSumSquare;
                }
            }
        }

        private static void NormalizeDictionaryArray(Dictionary<int, double>[] inputs)
        {
            foreach (Dictionary<int, double> dictionary in inputs)
            {
                double sumSquare = 0;
                foreach (double d in dictionary.Values)
                {
                    sumSquare += (d * d);
                }
                double sqrtSumSquare = Math.Sqrt(sumSquare);

                if (sqrtSumSquare != 0)
                {
                    List<int> keysList = new List<int>(dictionary.Keys);

                    foreach (var key in keysList)
                    {
                        dictionary[key] = dictionary[key] / sqrtSumSquare;
                    }
                }
            }
        }
    }
}