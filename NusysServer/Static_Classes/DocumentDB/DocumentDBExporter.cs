using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using NusysIntermediate;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;

namespace NusysServer
{
    public static class DocumentDBExporter
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

        public static async Task Initialize()
        {
            client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);

            // Creates the database with the passed in id if it does not exist, otherwise returns the database with the passed in id
            _db = await client.CreateDatabaseIfNotExistsAsync(new Database { Id = NusysConstants.DocDB_Database_ID });

            // Creates the collection 
            _collection = await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(NusysConstants.DocDB_Database_ID), new DocumentCollection { Id = NusysConstants.DocDB_Collection_ID });
        }


        /// <summary>
        /// Creates all the exported data files which will be used for the import to DocumentDB using the docDB migration tool
        /// </summary>
        public static void GetExportDataForDocumentDB(SqlConnection db)
        {
            var export_filepath = Constants.WWW_ROOT + "DocumentDBIntermediaryFiles/";

            //import content
            var import_contents_cmd_text =
                "SELECT c.content_id as content_id, content_type as content_type, content_ur as content_data, JSON_QUERY(a.analysis_model) as analysis_model, 'Content' as type, STRING_AGG(l.library_id, ',') as [Libary_Elements.libary_id] FROM contents c JOIN library_elements l ON c.content_id = l.content_id JOIN analysis_model a ON a.content_id = c.content_id GROUP BY c.content_id, content_type, content_ur, analysis_model FOR JSON PATH";
            var import_contents_file = export_filepath + "contents.json";
            GetJsonFileForDocumentDB(db, import_contents_cmd_text, import_contents_file);

            //import ink
            var import_ink_cmd_text =
                $"SELECT stroke_id AS InkStrokeId, content_id AS ContentId, color AS Color, thickness AS Thickness, JSON_QUERY(points) as InkPoints, '{NusysConstants.DocDB_DocumentType.Ink}' AS DocType FROM ink FOR JSON PATH";
            var import_ink_file = export_filepath + "ink.json";
            GetJsonFileForDocumentDB(db, import_ink_cmd_text, import_ink_file);

            //import users
            var import_users_cmd_text =
                "SELECT [user_id], user_password, user_salt_key, display_name, 'User' AS type FROM users FOR JSON PATH";
            var import_users_file = export_filepath + "users.json";
            GetJsonFileForDocumentDB(db, import_users_cmd_text, import_users_file);

            //import last used collections
            var import_last_used_collection_cmd_text =
                "SELECT last_used_date, [user_id], collection_library_id, 'Last_used_collections' AS type FROM last_used_collections FOR JSON PATH";
            var import_last_used_collection_file = export_filepath + "last_used_collection.json";
            GetJsonFileForDocumentDB(db, import_last_used_collection_cmd_text, import_last_used_collection_file);

            //import metadata
            var import_metadata_cmd_text =
                "SELECT * FROM ( SELECT metadata_library_id, (SELECT metadata_key_string, JSON_QUERY(metadata_value_string) as metadata_value_string, metadata_mutability_string FROM metadata AS m1 WHERE m1.metadata_library_id = m.metadata_library_id AND metadata_key_string != 'Search_Url' FOR JSON PATH) as metadata, 'Metadata' AS type FROM metadata AS m ) as m2 WHERE metadata IS NOT NULL FOR JSON PATH";
            var import_metadata_file = export_filepath + "metadata.json";
            GetJsonFileForDocumentDB(db, import_metadata_cmd_text, import_metadata_file);

            //import presentation links
            var import_presentation_links_cmd_text =
                $"SELECT link_id AS LinkId, link_in_element_id AS InElementId, link_out_element_id AS OutElementId, parent_collection_id AS ParentCollectionId, annotation AS AnnotationText, '{NusysConstants.DocDB_DocumentType.Presentation_Link}' AS DocType FROM presentation_link FOR JSON PATH ";
            var import_presentation_links_file = export_filepath + "presentation_links.json";
            GetJsonFileForDocumentDB(db, import_presentation_links_cmd_text, import_presentation_links_file);
        }

        /// <summary>
        /// Creates a json file using the output of the sql cmd stored in cmd_text. Outputs the json to the file output_file_path. Deletes the current file if one exists in output_file_path.
        /// Creates a new file if none exists
        /// </summary>
        /// <param name="cmd_text"></param>
        /// <param name="output_file_path"></param>
        private static void GetJsonFileForDocumentDB(SqlConnection db, string cmd_text, string output_file_path)
        {
            // delete current file
            File.Delete(output_file_path);

            using (var cmd = db.CreateCommand())
            {
                // set the sql for the db command
                cmd.CommandText = cmd_text;
                using (var reader = cmd.ExecuteReader())
                {
                    // while there are more rows in the result
                    while (reader.Read())
                    {
                        // append the result text to the file
                        using (StreamWriter sw = File.AppendText(output_file_path))
                        {
                            sw.Write(reader.GetString(0));
                        }
                    }
                }
            }
        }


        public static async Task ExportInkToDocumentDB(SqlConnection db)
        {
            // get a list of all the ink stroke id's
            var ink_stroke_ids = new List<string>();

            using (var cmd = db.CreateCommand())
            {
                // set the sql for the db command
                cmd.CommandText = "SELECT DISTINCT stroke_id FROM ink";
                using (var reader = cmd.ExecuteReader())
                {
                    // while there are more rows in the result
                    while (reader.Read())
                    {
                        ink_stroke_ids.Add(reader.GetString(0));
                    }
                }
            }

            // Create a list of ink models
            var ink_model_list = new List<InkModel>();

            // for each stroke id create the corresponding ink model
            foreach (var stroke in ink_stroke_ids)
            {
                using (var cmd = db.CreateCommand())
                {
                    // set the sql for the db command
                    cmd.CommandText = $"SELECT * FROM ink WHERE stroke_id = '{stroke}'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        // while there are more rows in the result
                        while (reader.Read())
                        {
                            InkModel tmpInkModel = new InkModel
                            {
                                InkStrokeId = reader.GetString(0),
                                ContentId = reader.GetString(1),
                                Color = JsonConvert.DeserializeObject<ColorModel>(reader.GetString(2)),
                                Thickness = double.Parse(reader.GetString(3)),
                                InkPoints = JsonConvert.DeserializeObject<List<PointModel>>(reader.GetString(4))
                            };
                            ink_model_list.Add(tmpInkModel);
                        }
                    }
                }
            }

            // make sure that we have a connection to the Document Database
            await Initialize();

            // delete all the current ink models in the database
            var currInkModelDocIds = client.CreateDocumentQuery<string>(_collection.SelfLink, $"SELECT VALUE c.id FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.Ink}'").ToList();
            foreach (var docId in currInkModelDocIds)
            {
                await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(NusysConstants.DocDB_Database_ID, NusysConstants.DocDB_Collection_ID, docId));
            }

            // transfer over all the new models
            foreach (var model in ink_model_list)
            {
                await client.CreateDocumentAsync(_collection.SelfLink, model);
            }
        }


        public static async Task ExportPresentationLinksToDocumentDB(SqlConnection db)
        {
            // get a list of all the ink stroke id's
            var presentation_link_ids = new List<string>();

            using (var cmd = db.CreateCommand())
            {
                // set the sql for the db command
                cmd.CommandText = "SELECT DISTINCT link_id FROM presentation_link";
                using (var reader = cmd.ExecuteReader())
                {
                    // while there are more rows in the result
                    while (reader.Read())
                    {
                        presentation_link_ids.Add(reader.GetString(0));
                    }
                }
            }

            // Create a list of ink models
            var presentation_model_list = new List<PresentationLinkModel>();

            // for each stroke id create the corresponding ink model
            foreach (var link_id in presentation_link_ids)
            {
                using (var cmd = db.CreateCommand())
                {
                    // set the sql for the db command
                    cmd.CommandText = $"SELECT * FROM presentation_link WHERE link_id = '{link_id}'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        // while there are more rows in the result
                        while (reader.Read())
                        {
                            PresentationLinkModel tmpPresLinkModel = new PresentationLinkModel
                            {
                                LinkId = reader.GetString(0),
                                InElementId = reader.GetString(1),
                                OutElementId = reader.GetString(2),
                                ParentCollectionId = reader.GetString(3),
                                AnnotationText = reader.GetString(4)
                            };
                            presentation_model_list.Add(tmpPresLinkModel);
                        }
                    }
                }
            }

            // make sure that we have a connection to the Document Database
            await Initialize();

            // delete all the current ink models in the database
            var currPresModelDocIds = client.CreateDocumentQuery<string>(_collection.SelfLink, $"SELECT VALUE c.id FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.Presentation_Link}'").ToList();
            foreach (var docId in currPresModelDocIds)
            {
                await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(NusysConstants.DocDB_Database_ID, NusysConstants.DocDB_Collection_ID, docId));
            }

            // transfer over all the new models
            foreach (var model in presentation_model_list)
            {
                await client.CreateDocumentAsync(_collection.SelfLink, model);
            }
        }


        public static async Task ExportUsersToDocumentDB(SqlConnection db)
        {
            // get a list of all the user ids
            var user_ids = new List<string>();

            using (var cmd = db.CreateCommand())
            {
                // set the sql for the db command
                cmd.CommandText = "SELECT DISTINCT user_id FROM users";
                using (var reader = cmd.ExecuteReader())
                {
                    // while there are more rows in the result
                    while (reader.Read())
                    {
                        user_ids.Add(reader.GetString(0));
                    }
                }
            }

            // Create a list of user models
            var user_model_list = new List<DocDBUserModel>();

            // for each user id create the corresponding ink model
            foreach (var user_id in user_ids)
            {
                using (var cmd = db.CreateCommand())
                {
                    // set the sql for the db command
                    cmd.CommandText = $"SELECT * FROM users WHERE user_id = '{user_id}'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        // while there are more rows in the result
                        while (reader.Read())
                        {
                            DocDBUserModel tmpUserModel = new DocDBUserModel
                            {
                                UserId = reader.GetString(0),
                                UserPassword = reader.GetString(1),
                                UserSaltKey = reader.GetString(2),
                                DisplayName = reader.GetString(3),
                                LastVisitedCollections =
                                    JsonConvert.DeserializeObject<List<string>>(reader.GetString(4))
                            };
                            user_model_list.Add(tmpUserModel);
                        }
                    }
                }
            }

            // make sure that we have a connection to the Document Database
            await Initialize();

            // delete all the current models in the database
            var currUserModelIds = client.CreateDocumentQuery<string>(_collection.SelfLink, $"SELECT VALUE c.id FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.User}'").ToList();
            foreach (var docId in currUserModelIds)
            {
                await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(NusysConstants.DocDB_Database_ID, NusysConstants.DocDB_Collection_ID, docId));
            }

            // transfer over all the new models
            foreach (var model in user_model_list)
            {
                await client.CreateDocumentAsync(_collection.SelfLink, model);
            }
        }
    }

}