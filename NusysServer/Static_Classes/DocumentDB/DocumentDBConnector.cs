using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

// Additional Services Needed to Support DocumentDB
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer.Static_Classes
{
    /// <summary>
    /// Serves as the connector for the DocumentDB database
    /// </summary>
    public static class DocumentDBConnector
    {
        /// <summary>
        /// Set the endpoints to local endpoints or server side endpoitns. If Local you must have DocumentDB emulator installed
        /// </summary>
        private const bool DEVELOP_LOCALLY = true;

        /// <summary>
        /// The endpoint that we connect to in order to find the DocumentDB
        /// endpoints found on azure portal under documentDB -> keys
        /// endpointURL is the URI field, 
        /// </summary>
        private static readonly string EndpointUrl = DEVELOP_LOCALLY ? NusysConstants.LocalEndpointUrl : NusysConstants.ServerEndpointUrl;

        /// <summary>
        /// The key authorizing the DocumentDB to trust us
        /// primarykey found on azure portal under documentDB -> keys
        /// primarykey is the primary key field 
        /// </summary>
        private static readonly string PrimaryKey = DEVELOP_LOCALLY ? NusysConstants.LocalPrimaryKey :  // this local key is always the same
                                                                      NusysConstants.ServerPrimaryKey; // this secret key can be refreshed on the azure portal and might change
        /// <summary>
        /// Clientside representation of DocumentDB service used to communicate with the database
        /// </summary>
        private static DocumentClient client;

        /// <summary>
        /// The documentDB database we are connecting to, only exists after a call to initialize
        /// </summary>
        private static Database _db;

        /// <summary>
        /// The documentDB collection we are connecting to, only exists after a call to initialize
        /// </summary>
        private static DocumentCollection _collection;

        public static async void Initialize()
        {
            client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            //CreateDatabaseIfNotExistsAsync().Wait();
            //CreateCollectionIfNotExistsAsync().Wait();

            // Creates the database with the passed in id if it does not exist, otherwise returns the database with the passed in id
            _db = await client.CreateDatabaseIfNotExistsAsync(new Database { Id = NusysConstants.DocDB_Database_ID });

            // Creates the collection 
            _collection = await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(NusysConstants.DocDB_Database_ID), new DocumentCollection { Id = NusysConstants.DocDB_Collection_ID });

            //QueryAllPresentationLinks(_collection.SelfLink);
            QueryAllInkStrokes(_collection.SelfLink);
        }

        private static void QueryAllInkStrokes(string collectionSelfLink)
        {
            //// LINQ Query
            var result =
                from item in client.CreateDocumentQuery<InkModel>(collectionSelfLink)
                where item.DocType == NusysConstants.DocDB_DocumentType.Ink.ToString()
                select item;

            var ink_models = result.ToList();

            //// LINQ Lambda
            result = client.CreateDocumentQuery<InkModel>(collectionSelfLink).Where(e => e.DocType == NusysConstants.DocDB_DocumentType.Ink.ToString());

            ink_models = result.ToList();

            // SQL
            result = client.CreateDocumentQuery<InkModel>(collectionSelfLink, $"SELECT * FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.Ink}'");

            ink_models = result.ToList();
        }

        private static void QueryAllPresentationLinks(string collectionSelfLink)
        {
            //// LINQ Query
            var result =
                from item in client.CreateDocumentQuery<PresentationLinkModel>(collectionSelfLink)
                where item.DocType == NusysConstants.DocDB_DocumentType.Presentation_Link.ToString()
                select item;

            var presentation_links = result.ToList();

            //// LINQ Lambda
            result = client.CreateDocumentQuery<PresentationLinkModel>(collectionSelfLink).Where(e => e.DocType == NusysConstants.DocDB_DocumentType.Presentation_Link.ToString());

            presentation_links = result.ToList();

            // SQL
            result = client.CreateDocumentQuery<PresentationLinkModel>(collectionSelfLink, $"SELECT * FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.Presentation_Link}'");

            presentation_links = result.ToList();
        }
    }
}