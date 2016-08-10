using System;
using System.Collections.Generic;

namespace NusysServer.DocumentComparison
{
    public class RetrieveSimilarDocs
    {
        /// <summary>
        /// Given a document, returns the top k similar matches
        /// </summary>
        /// <param name="document"></param>
        /// <param name="k"></param>
        public static Tuple<string, double>[] GetSimilarDocs(Document document, List<Document> documentList, int k)
        {
            // TODO Try with quickselect

            // document dictionary
            Dictionary<int, double> documentDictionary = document.GetTFIDFVector();
            
            // array that will hold the k documents
            Tuple<string, double>[] closestDocuments = new Tuple<string, double>[k];
            for (int i = 0; i < k; i++)
            {
                closestDocuments[i] = new Tuple<string, double>("", Double.MaxValue);
            }

            int farthestIndex = 0;

            foreach (var doc in documentList)
            {
                if (doc.Id != document.Id)
                {
                    double distance = VectorUtil.CosineDistance(doc.GetTFIDFVector(), documentDictionary);
                    if (distance < closestDocuments[farthestIndex].Item2)
                    {
                        closestDocuments[farthestIndex] = new Tuple<string, double>(doc.Id, distance);
                        farthestIndex = getFarthestIndex(closestDocuments);
                    }
                }    
            }

            return closestDocuments;
        }

        private static int getFarthestIndex(Tuple<string,double>[] array)
        {
            int counter = 0;
            double max = Double.MinValue;
            int index = -1;

            foreach (Tuple<string,double> tuple in array)
            {
                if (tuple.Item2 > max)
                {
                    max = tuple.Item2;
                    index = counter;
                }
                counter++;
            }
            return index;
        }
    }
}