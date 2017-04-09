using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// Additional Services Needed to Support DocumentDB
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
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

        public static async void Initialize()
        {
            client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            //CreateDatabaseIfNotExistsAsync().Wait();
            //CreateCollectionIfNotExistsAsync().Wait();

            // Creates the database with the passed in id if it does not exist, otherwise returns the database with the passed in id
            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = DocumentDB_Database_ID });

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DocumentDB_Database_ID), new DocumentCollection { Id = DocumentDB_Collection_ID });

        }

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
        public const string DocumentDB_Database_ID = "NuSysDocumentDB";

        /// <summary>
        /// The name of our document db collection
        /// </summary>
        public const string DocumentDB_Collection_ID = "NuSysMainDocumentCollection";


    }
}