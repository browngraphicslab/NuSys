using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using NusysServer.DocumentComparison;

namespace NusysServer
{
    public class ComparisonController
    {
        //hard code filepath for megajson

        
        public ComparisonController()
        {
            Timer t = new Timer(Recalculate, false, new TimeSpan(0), new TimeSpan(0,10,0));
        }


        public void AddDocument(object sometheing)
        {
            Document uploadedDocument = Parser.parseDocument("text string", "3");
            Tfidf.setTfidfVector(uploadedDocument, 3);
        }

        public void CompareRandonDoc()
        {
            
        }

        public object GetComparison(object docToCompare)
        {
            //Document randomDoc = documents[0];
            //Tuple<string, double>[] closestKDocs = RetrieveSimilarDocs.GetSimilarDocs(randomDoc, documents, 5);
            return null;
        }

        private void Recalculate(object state)
        {
            //Tfidf.Tfidf.setTfidfVectors(documents);
        }
    }
}