using System;
using System.Collections.Generic;
using System.Diagnostics;
using NusysServer.Static_Classes.DocumentComparison;

namespace NusysServer.DocumentComparison
{
    public class Tfidf
    {
        public static void setTfidfVector(Document document)
        {
            // generate list ordering of megadictionary
            List<string> keysList = DocSave.ReturnCleansedMegaKeysList();

            Dictionary<int, double> TFIDFDict = new Dictionary<int, double>();

            // calculate TFDIF vector for document
            for (int i = 0; i < keysList.Count; i++)
            {
                string word = keysList[i];
                double tf = document.UniqueWordsFreq() == 0 ? 0 : (double)document.ReturnFrequency(word) / document.UniqueWordsFreq();
                double calc = DocSave.getDocumentCount() / DocSave.ReturnMegaTermFrequency(word);
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

        /// <summary>
        /// Updates all vectors
        /// </summary>
        /// <param name="documents"></param>
        public static void setTfidfVectors(List<Document> documents)
        {
            Debug.WriteLine("Resetting tfidf vectors for all documents");

            foreach (var document in documents)
            {
                setTfidfVector(document);
            }
        }

        /// <summary>
        /// Normalizes a vector (represented as a dictionary)
        /// </summary>
        /// <param name="input"></param>
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
    }
}