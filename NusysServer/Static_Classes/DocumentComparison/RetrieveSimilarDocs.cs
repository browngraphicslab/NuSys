using System;
using System.Collections.Generic;
using System.Linq;
using NusysServer.Static_Classes.DocumentComparison;

namespace NusysServer.DocumentComparison
{
    public class RetrieveSimilarDocs
    {
        /// <summary>
        /// Given a document, returns the top k similar matches in sorted order from closest to farthest
        /// </summary>
        /// <param name="document"></param>
        /// <param name="k"></param>
        public static List<Tuple<string, double>> GetSimilarDocs(Document document, List<Document> documentList, int k)
        {
            // TODO Rough implementation for now, quickselect probably more efficient. But k will always be ~5, so worth changing?
            
            // Tfidf vector for the document we are retrieving similar docs for
            Dictionary<int, double> documentDictionary = document.GetTFIDFVector();
            
            // array that will hold the k documents
            List<Tuple<string, double>> closestDocuments = new List<Tuple<string, double>>();

            // loop through the array and initialize values to 2 (0 <= cosine distance <= 1 in our case)
            for (int i = 0; i < k; i++)
            {
                closestDocuments[i] = new Tuple<string, double>("", 2);
            }

            int farthestIndex = 0;

            foreach (var doc in documentList)
            {
                if (doc.Id != document.Id)
                {
                    double distance = VectorUtil.CosineDistance(doc.GetTFIDFVector(), documentDictionary);
                    string title = DocSave.getTitle(doc.Id);
                    Tuple<string, double> tuple = new Tuple<string, double>(title, distance);

                    insertTupleIntoSortedList(closestDocuments, tuple, k);
                }    
            }

            return closestDocuments;
        }

        /// <summary>
        /// Given a sorted list and a tuple, inserts the tuple into the correct location
        /// </summary>
        /// <param name="array"></param>
        /// <param name="tuple"></param>
        private static void insertTupleIntoSortedList(List<Tuple<string, double>> list, Tuple<string, double> tuple, int k)
        {
            int markIndex = -1;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Item2 > tuple.Item2)
                {
                    // save index
                    markIndex = i;
                }                
            }
            
            // only if index != -1, we need to insert into list
            if (markIndex > 0)
            {
                list.Insert(markIndex, tuple);
                list.RemoveAt(k);
            }
        }
    }
}