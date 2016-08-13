using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;
using NusysServer.DocumentComparison;
using NusysServer.Static_Classes.DocumentComparison;
using Windows.Storage.Streams;

namespace NusysServer
{
    public class ComparisonController
    {
        private DocSave docSave;
        
        public ComparisonController()
        {
            // load in docsave file if it exists in system
            if (File.Exists(Constants.FILE_FOLDER + "docsave.txt"))
            {
                string json = ReadFromDocSaveFile();
                docSave = JsonConvert.DeserializeObject<DocSave>(json);
            }
            else
            {
                docSave = new DocSave();
            }
            
            //Timer t = new Timer(Recalculate, false, new TimeSpan(0), new TimeSpan(0,10,0));
        }


        public async void AddDocument(Tuple<string, string> contentIdTuple, string title)
        {
            Debug.WriteLine("Adding document with title: " + title);

            byte[] dataBytes = Convert.FromBase64String(contentIdTuple.Item1);
            
            using (var ms = new MemoryStream(dataBytes))
            {
                var sr = new StreamReader(ms);
                var myStr = sr.ReadToEnd();
            }

            string pdfContent = Encoding.UTF8.GetString(dataBytes, 0, dataBytes.Length);
            string pdfId = contentIdTuple.Item2;
            
            Document uploadedDocument = Parser.parseDocument(pdfContent, pdfId);

            // Add tfidf vector to the Document 
            Tfidf.setTfidfVector(uploadedDocument);               
            DocSave.addDocument(contentIdTuple.Item2, uploadedDocument, title); 
            
            UpdateDocSaveFile();

            // Every time it doubles, reset vectors -- for now if power of 2
            int documentcount = DocSave.getDocumentCount();
            if ((documentcount & (documentcount - 1)) == 0)
            {
                Recalculate();    
            }
            Debug.WriteLine("Done");
            Debug.WriteLine("new count: " + DocSave.getDocumentCount());
        }

        /// <summary>
        /// Testing purposes
        /// </summary>
        public void CompareRandomDoc()
        {
            Document doc = DocSave.returnRandomDocument();

            if (DocSave.getDocumentCount() > 10)
            {
                GetComparison(doc.Id, 5);
            }
        }

        public Tuple<string, double>[] GetComparison(string id, int k)
        {
            // locate document from id
            Document docToCompare = DocSave.getDocumentFromId(id);

            Debug.Assert(docToCompare != null);
            
            Tuple<string, double>[] closestKDocs = RetrieveSimilarDocs.GetSimilarDocs(docToCompare, DocSave.getDocumentList(), k);
            
            Debug.WriteLine("Generating " + k + " similar documents for document with title: " + DocSave.getTitle(id));
            testprint(closestKDocs);
            
            return closestKDocs;
        }

        private void testprint(Tuple<string, double>[] array)
        {
            foreach (Tuple<string, double> tuple in array)
            {
                Debug.Write(DocSave.getTitle(tuple.Item1) + " " + tuple.Item2 + ", ");
            }   
        }

        private void Recalculate()
        {
            Tfidf.setTfidfVectors(DocSave.getDocumentList());
        }

        private void UpdateDocSaveFile()
        {
            string json = JsonConvert.SerializeObject(docSave);            
            string filePath = "docsave.txt";
            var stream = File.Create(Constants.FILE_FOLDER + filePath);
            stream.Dispose();
            File.WriteAllText(Constants.FILE_FOLDER + filePath, json);
        }

        private string ReadFromDocSaveFile()
        {
            var filepath = Constants.FILE_FOLDER + "docsave.txt";
            return File.ReadAllText(filepath);
        }
    }
}