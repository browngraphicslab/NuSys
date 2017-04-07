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
        private static readonly string EndpointUrl = DEVELOP_LOCALLY ? "https://localhost:8081" : "https://nusysdocdb.documents.azure.com:443/";

        /// <summary>
        /// The key authorizing the DocumentDB to trust us
        /// primarykey found on azure portal under documentDB -> keys
        /// primarykey is the primary key field 
        /// </summary>
        private static readonly string PrimaryKey = DEVELOP_LOCALLY ? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" :  // this local key is always the same
                                                                      "Nx25qMvkeb1djgkNUzyHwrWAmMk68zpEhPsMAnRmq6kDWTjeGl2XDfZiv7a0LNbyaGkxEN76xzVbLk27KR2wqA=="; // this secret key can be refreshed on the azure portal and might change
        /// <summary>
        /// Clientside representation of DocumentDB service used to communicate with the database
        /// </summary>
        private static DocumentClient client;

        public static void Initialize()
        {
            client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            //CreateDatabaseIfNotExistsAsync().Wait();
            //CreateCollectionIfNotExistsAsync().Wait();
        }

    }
}