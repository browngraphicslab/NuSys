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

namespace NusysServer
{
    public class ComparisonController
    {
        private DocSave docSave;

        public ComparisonController()
        {
            //TODO: figure out where to cleanse & where to recalculate vectors
            // right now cleansed dictionary is not initialized

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

            // Cleanse the dictionary on startup
            UpdateCleansedDictionary();
            
            //Timer t = new Timer(Recalculate, false, new TimeSpan(0), new TimeSpan(0,10,0));
        }


        public async void AddDocument(Tuple<string, string> contentIdTuple, string title)
        {
            Debug.WriteLine("Adding document with title: " + title);

            string pdfContent = contentIdTuple.Item1;
            string pdfId = contentIdTuple.Item2;

            Document uploadedDocument = Parser.parseDocument(pdfContent, pdfId);

            // Add tfidf vector to the Document 
            Tfidf.setTfidfVector(uploadedDocument);

            // Add document to DocSave
            DocSave.addDocument(uploadedDocument, title);
            
            UpdateDocSaveFile();

            // Every time count doubles, reset vectors -- for now if power of 2
            int documentcount = DocSave.getDocumentCount();
            if ((documentcount & (documentcount - 1)) == 0)
            {
                RecalculateTfidfVectors();
            }
            Debug.WriteLine("Done");
            Debug.WriteLine("new count: " + DocSave.getDocumentCount());
        }

        public void DeleteDocument(string id)
        {
            DocSave.deleteDocument(id);
            UpdateDocSaveFile();
        }

        /// <summary>
        /// Testing purposes
        /// </summary>
        public void CompareRandomDoc(string id)
        {
            Document doc = DocSave.getDocumentFromId(id);
            //if (DocSave.getDocumentCount() > 10)
            //{
            //    GetComparison(doc.Id, 5);
            //}
            GetComparison(doc.Id, 5);
        }

        /// <summary>
        /// Given a document's id, returns k closest (most similar) documents if it's able to locate the document in DocSave's dictionary. Else returns empty Tuple[].
        /// </summary>
        /// <param name="id"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public List<Tuple<string, double>> GetComparison(string id, int k)
        {
            // locate document from id
            Document docToCompare = DocSave.getDocumentFromId(id);

            // couldn't find, return empty tuple array
            if (docToCompare == null) 
            {
                Debug.WriteLine("Couldn't find the pdf in the dictionary, scanned pdf?");
                return new List<Tuple<string, double>>();                
            }
            
            List<Tuple<string, double>> closestKDocs = RetrieveSimilarDocs.GetSimilarDocs(docToCompare, DocSave.getDocumentList(), k);

            Debug.WriteLine("Generating " + k + " similar documents for document with title: " + DocSave.getTitle(id));
            testprint(closestKDocs);

            return closestKDocs;
        }

        /// <summary>
        /// Goes through all the documents on NuSys and updates all their tfidf vectors
        /// </summary>
        private void RecalculateTfidfVectors()
        {
            Tfidf.setTfidfVectors(DocSave.getDocumentList());
        }

        /// <summary>
        /// Updates the DocSave file saved on the drive
        /// </summary>
        private void UpdateDocSaveFile()
        {
            string json = JsonConvert.SerializeObject(docSave);
            string filePath = "docsave.txt";
            var stream = File.Create(Constants.FILE_FOLDER + filePath);
            stream.Dispose();
            File.WriteAllText(Constants.FILE_FOLDER + filePath, json);
        }

        /// <summary>
        /// Reads from the DocSave file saved on the drive
        /// </summary>
        /// <returns></returns>
        private string ReadFromDocSaveFile()
        {
            var filepath = Constants.FILE_FOLDER + "docsave.txt";
            return File.ReadAllText(filepath);
        }

        /// <summary>
        /// For testing purposes, prints the tuple array
        /// </summary>
        /// <param name="array"></param>
        private void testprint(List<Tuple<string, double>> list)
        {
            foreach (Tuple<string, double> tuple in list)
            {
                Debug.Write(DocSave.getTitle(tuple.Item1) + " " + tuple.Item2 + ", ");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void UpdateCleansedDictionary()
        {
            if (DocSave.getDocumentCount() > 10)
            {
                if (DocSave.getDocumentCount() < 100)
                {
                    DocSave.UpdateCleansedMegaDictionary(1);
                }
                else
                {
                    DocSave.UpdateCleansedMegaDictionary(3);
                }
            }
        }
    }
}