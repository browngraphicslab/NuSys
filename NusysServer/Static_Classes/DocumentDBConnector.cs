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
using static NusysServer.Static_Classes.DocumentDBConstants;

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
        private static readonly string EndpointUrl = DEVELOP_LOCALLY ? LocalEndpointUrl : ServerEndpointUrl;

        /// <summary>
        /// The key authorizing the DocumentDB to trust us
        /// primarykey found on azure portal under documentDB -> keys
        /// primarykey is the primary key field 
        /// </summary>
        private static readonly string PrimaryKey = DEVELOP_LOCALLY ? LocalPrimaryKey :  // this local key is always the same
                                                                      ServerPrimaryKey; // this secret key can be refreshed on the azure portal and might change
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
            _db = await client.CreateDatabaseIfNotExistsAsync(new Database { Id = DocDB_Database_ID });

            // Creates the collection 
            _collection = await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DocDB_Database_ID), new DocumentCollection { Id = DocDB_Collection_ID });


            QueryAllDocuments(_collection.SelfLink);

        }

        private static void QueryAllDocuments(string collectionLink)
        {
            //// LINQ Query
            //var families =
            //    from item in client.CreateDocumentQuery<PresentationLinkModel>(collectionLink)
            //    where item.GetType() 
            //    select presentation_link;

            //// LINQ Lambda
            //families = client.CreateDocumentQuery<PresentationLinkModel>(collectionLink);

            // SQL
            var pres_links = client.CreateDocumentQuery<PresentationLinkModel>(collectionLink, $"SELECT c.link_id AS LinkId, c.link_out_element_id AS OutElementId, c.link_in_element_id AS InElementId, c.parent_collection_id AS ParentCollectionId FROM c WHERE c.type='{DocDB_DocumentType.Presentation_Link}'");

            var pres_links_result = pres_links.ToList();
        }

        //private static void QueryWithSqlQuerySpec(string collectionLink)
        //{
        //    // Simple query with a single property equality comparison
        //    // in SQL with SQL parameterization instead of inlining the 
        //    // parameter values in the query string
        //    // LINQ Query -- Id == "value"
        //    var query = client.CreateDocumentQuery<PresentationLinkModel>(collectionLink, new SqlQuerySpec()
        //    {
        //        QueryText = "SELECT * FROM Families f WHERE (f.id = @id)",
        //        Parameters = new SqlParameterCollection()
        //            {
        //                new SqlParameter("@id", "AndersenFamily")
        //            }
        //    });

        //    Debug.Assert("Expected only 1 family", query.ToList().Count == 1);

        //    // Query using two properties within each document. WHERE Id == "" AND Address.City == ""
        //    // notice here how we are doing an equality comparison on the string value of City

        //    query = client.CreateDocumentQuery<Family>(
        //        collectionLink,
        //        new SqlQuerySpec()
        //        {
        //            QueryText = "SELECT * FROM Families f WHERE f.id = @id AND f.Address.City = @city",
        //            Parameters = new SqlParameterCollection()
        //            {
        //                new SqlParameter("@id", "AndersenFamily"),
        //                new SqlParameter("@city", "Seattle")
        //            }
        //        });

        //    Debug.Assert("Expected only 1 family", query.ToList().Count == 1);

        //}
    }

    public static class DocumentDBConstants
    {
        /// <summary>
        /// The endpoint for the local document db emulator. If you do not have a document DB emulator you must install it from microsoft
        /// </summary>
        public static readonly string LocalEndpointUrl = "https://localhost:8081";

        /// <summary>
        /// The endpoint for the server document db database found on azure portal
        /// </summary>
        public static readonly string ServerEndpointUrl = "https://nusysdocdb.documents.azure.com:443/";

        /// <summary>
        /// The access key for the local document db emulator. This is default and is always the same
        /// </summary>
        public static readonly string LocalPrimaryKey =
            "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        /// <summary>
        /// The access key for the server document db database, this is secret and is found on the azure portal
        /// </summary>
        public static readonly string ServerPrimaryKey =
            "Nx25qMvkeb1djgkNUzyHwrWAmMk68zpEhPsMAnRmq6kDWTjeGl2XDfZiv7a0LNbyaGkxEN76xzVbLk27KR2wqA==";

        /// <summary>
        /// The name of our document db database
        /// </summary>
        public const string DocDB_Database_ID = "NuSysDocumentDB";

        /// <summary>
        /// The name of our document db collection
        /// </summary>
        public const string DocDB_Collection_ID = "NuSysMainDocumentCollection";

        /// <summary>
        /// The type field of each document in the database is used to distinguish types of documents from one another. All valid types
        /// are specified here
        /// </summary>
        public enum DocDB_DocumentType
        {
            Content,
            Alias,
            Ink,
            User,
            Last_used_collections,
            Metadata,
            Library_Element,
            Presentation_Link
        }


    }
}