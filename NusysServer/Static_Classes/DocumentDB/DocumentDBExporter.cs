using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using NusysIntermediate;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Threading.Tasks;
using JetBrains.dotMemoryUnit;
using Microsoft.Azure.Documents.Linq;

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
            var currInkModelDocIds = await QueryAsyncWithRetries(client.CreateDocumentQuery<string>(_collection.SelfLink, $"SELECT VALUE c.id FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.Ink}'"));
            foreach (var docId in currInkModelDocIds)
            {
                await ExecuteWithRetries(() => client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(NusysConstants.DocDB_Database_ID, NusysConstants.DocDB_Collection_ID, docId)));
            }

            // transfer over all the new models
            foreach (var model in ink_model_list)
            {
                await ExecuteWithRetries(()=> client.CreateDocumentAsync(_collection.SelfLink, model));
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
            var currPresModelDocIds = await QueryAsyncWithRetries(client.CreateDocumentQuery<string>(_collection.SelfLink, $"SELECT VALUE c.id FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.Presentation_Link}'"));
            foreach (var docId in currPresModelDocIds)
            {
                await ExecuteWithRetries(() => client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(NusysConstants.DocDB_Database_ID, NusysConstants.DocDB_Collection_ID, docId)));
            }

            // transfer over all the new models
            foreach (var model in presentation_model_list)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
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
            var currUserModelIds = await QueryAsyncWithRetries(client.CreateDocumentQuery<string>(_collection.SelfLink, $"SELECT VALUE c.id FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.User}'"));
            foreach (var docId in currUserModelIds)
            {
                await ExecuteWithRetries(() => client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(NusysConstants.DocDB_Database_ID, NusysConstants.DocDB_Collection_ID, docId)));
            }

            // transfer over all the new models
            foreach (var model in user_model_list)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }
        }

        public static async Task ExportLastUsedCollectionsToDocumentDB(SqlConnection db)
        {
            // Create a list of last used collection models
            var last_used_collection_model_list = new List<LastUsedCollectionModel>();

            // for each row in the database create a model and add it to the list
            using (var cmd = db.CreateCommand())
            {
                // set the sql for the db command
                cmd.CommandText = $"SELECT * FROM last_used_collections";
                using (var reader = cmd.ExecuteReader())
                {
                    // while there are more rows in the result
                    while (reader.Read())
                    {
                        var tmpLastUsedCollectionModel = new LastUsedCollectionModel
                        {
                            DateTime = reader.GetDateTime(0).ToString(),
                            UserId = reader.GetString(1),
                            CollectionId = reader.GetString(2)
                        };
                        last_used_collection_model_list.Add(tmpLastUsedCollectionModel);
                    }
                }
            }

            // make sure that we have a connection to the Document Database
            await Initialize();

            // delete all the current models in the database
            var currLastUsedCollectionModels = await QueryAsyncWithRetries(client.CreateDocumentQuery<string>(_collection.SelfLink, $"SELECT VALUE c.id FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.Last_used_collections}'"));
            foreach (var docId in currLastUsedCollectionModels)
            {
                await ExecuteWithRetries(() => client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(NusysConstants.DocDB_Database_ID, NusysConstants.DocDB_Collection_ID, docId)));
            }

            // transfer over all the new models
            foreach (var model in last_used_collection_model_list)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }
        }

        public static async Task ExportContentDataModelsToDocumentDB(SqlConnection db)
        {
            // Create a list of last used collection models
            var regularContentDataModels = new List<ContentDataModel>();
            var collectionContentDataModels = new List<CollectionContentDataModel>();
            var pdfContentDataModels = new List<PdfContentDataModel>();

            // make sure that we have a connection to the Document Database
            await Initialize();

            // for each row in the database create a model and add it to the list
            using (var cmd = db.CreateCommand())
            {
                // set the sql for the db command
                cmd.CommandText = $"SELECT * FROM contents";
                using (var reader = cmd.ExecuteReader())
                {
                    // while there are more rows in the result
                    while (reader.Read())
                    {
                        var contentId = reader.GetString(0);
                        var contentType = (NusysConstants.ContentType) Enum.Parse(typeof(NusysConstants.ContentType), reader.GetString(1), true);
                        var data = reader.GetString(2);
                        var strokes = new List<InkModel>();
                        try
                        {
                            var query = (from doc in client.CreateDocumentQuery<InkModel>(_collection.SelfLink)
                                        where
                                        doc.DocType == NusysConstants.DocDB_DocumentType.Ink.ToString() &&
                                        doc.ContentId == contentId
                                        select doc);
                            strokes = (await QueryAsyncWithRetries(query)).ToList();
                        }
                        catch (DocumentClientException e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                        
                        // build the model based on its type
                        switch (contentType)
                        {
                            case NusysConstants.ContentType.PDF:
                            case NusysConstants.ContentType.Word:
                                pdfContentDataModels.Add(new PdfContentDataModel(contentId, data) {ContentType = contentType, Strokes = strokes});
                                break;
                            case NusysConstants.ContentType.Collection:
                                collectionContentDataModels.Add(new CollectionContentDataModel(contentId, data) { ContentType = contentType, Strokes = strokes });
                                break;
                            default:
                                regularContentDataModels.Add(new ContentDataModel(contentId, data) { ContentType = contentType, Strokes = strokes });
                                break;
                        }
                    }
                }
            }

            // delete all the current models in the database
            var currLastUsedCollectionModels = await QueryAsyncWithRetries(client.CreateDocumentQuery<string>(_collection.SelfLink, $"SELECT VALUE c.id FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.Content}'"));
            foreach (var docId in currLastUsedCollectionModels)
            {
                await ExecuteWithRetries(() => client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(NusysConstants.DocDB_Database_ID, NusysConstants.DocDB_Collection_ID, docId)));
            }

            // transfer over all the new models
            foreach (var model in pdfContentDataModels)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }

            foreach (var model in collectionContentDataModels)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }

            foreach (var model in regularContentDataModels)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }
        }


        public static async Task ExportAnalysisModelToDocumentDB(SqlConnection db)
        {
            // Create a list of last used collection models
            var ImageAnalysisModelList = new List<NusysImageAnalysisModel>();
            var PdfAnalysisModelList = new List<NusysPdfAnalysisModel>();

            // for each row in the database create a model and add it to the list
            using (var cmd = db.CreateCommand())
            {
                // set the sql for the db command
                cmd.CommandText = $"SELECT * FROM analysis_model";
                using (var reader = cmd.ExecuteReader())
                {
                    // while there are more rows in the result
                    while (reader.Read())
                    {

                        AnalysisModel model = JsonConvert.DeserializeObject<AnalysisModel>(reader.GetString(1));

                        //switch on the content type
                        switch (model.Type)
                        {
                            case NusysConstants.ContentType.Image:
                                ImageAnalysisModelList.Add(JsonConvert.DeserializeObject<NusysImageAnalysisModel>(reader.GetString(1)));
                                break;
                            case NusysConstants.ContentType.PDF:
                                PdfAnalysisModelList.Add(JsonConvert.DeserializeObject<NusysPdfAnalysisModel>(reader.GetString(1)));
                                break;
                            default:
                                throw new Exception(" this content type does not support analysis models yet.");
                        }
                    }
                }
            }

            // make sure that we have a connection to the Document Database
            await Initialize();

            // delete all the current models in the database
            var currAnalysisModels = await QueryAsyncWithRetries(client.CreateDocumentQuery<string>(_collection.SelfLink, $"SELECT VALUE c.id FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.Analysis_Model}'"));
            foreach (var docId in currAnalysisModels)
            {
                await ExecuteWithRetries(() => client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(NusysConstants.DocDB_Database_ID, NusysConstants.DocDB_Collection_ID, docId)));
            }

            // transfer over all the new models
            foreach (var model in ImageAnalysisModelList)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }

            // transfer over all the new models
            foreach (var model in PdfAnalysisModelList)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }
        }

        public static async Task ExportLibraryElementModelsToDocumentDB(SqlConnection db)
        {
            // Create a list of last used collection models
            var lem_list = new List<LibraryElementModel>();

            // for each row in the database create a model and add it to the list
            using (var cmd = db.CreateCommand())
            {
                // set the sql for the db command
                cmd.CommandText = $"SELECT TOP 10 PERCENT * FROM library_elements ORDER BY NEWID()";
                //cmd.CommandText = $"SELECT * FROM library_elements";

                using (var reader = cmd.ExecuteReader())
                {
                    // while there are more rows in the result
                    while (reader.Read())
                    {
                        var library_id = reader.GetString(0);
                        var elementType = (NusysConstants.ElementType)Enum.Parse(typeof(NusysConstants.ElementType), reader.GetString(2), true);
                        var tmpLem = new LibraryElementModel(library_id, elementType)
                        {
                            ContentDataModelId = reader.GetString(1),
                            Timestamp = reader.GetString(3),
                            LastEditedTimestamp = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            Creator = reader.GetString(5),
                            Favorited = reader.IsDBNull(6) ? false : string.IsNullOrEmpty(reader.GetString(6)) ? false : bool.Parse(reader.GetString(6)),
                            Keywords = reader.IsDBNull(7) ? new HashSet<Keyword>() : JsonConvert.DeserializeObject<HashSet<Keyword>>(reader.GetString(7)),
                            Title = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                            SmallIconUrl = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                            MediumIconUrl = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),                          
                            AccessType = (NusysConstants.AccessType)Enum.Parse(typeof(NusysConstants.AccessType), reader.GetString(11), true),
                            LargeIconUrl = reader.IsDBNull(12) ? string.Empty : reader.GetString(12)
                        };
                        lem_list.Add(tmpLem);
                    }
                }
            }

            // add the metadata to each lem
            foreach (var lem_id in lem_list.Select(lem => lem.LibraryElementId).ToList())
            {
                // get the library element we are querying metadata for
                var lem = lem_list.FirstOrDefault(e => e.LibraryElementId == lem_id);
                lem.Metadata = new ConcurrentDictionary<string, MetadataEntry>();
                Debug.Assert(lem != null);

                // get all the metadata from the database
                using (var cmd = db.CreateCommand())
                {
                    // set the sql for the db command
                    cmd.CommandText = $"SELECT * FROM metadata WHERE metadata_library_id = '{lem_id}'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        // while there are more rows in the result
                        while (reader.Read())
                        {
                            var metadata_key = reader.GetString(1);
                            var metadata_value = JsonConvert.DeserializeObject<List<string>>(reader.GetString(2));
                            var metadata_mutability = (MetadataMutability)Enum.Parse(typeof(MetadataMutability), reader.GetString(3), true);
                            lem.Metadata[metadata_key] = new MetadataEntry(metadata_key, metadata_value, metadata_mutability);
                        }
                    }
                }
            }

            var audioLibaryElementModelList = new List<AudioLibraryElementModel>();
            var collectionLibaryElementModelList = new List<CollectionLibraryElementModel>();
            var imageLibaryElementModelList = new List<ImageLibraryElementModel>();
            var regularLibaryElementModelList = new List<LibraryElementModel>();
            var linkLibaryElementModelList = new List<LinkLibraryElementModel>();
            var pdfLibaryElementModelList = new List<PdfLibraryElementModel>();
            var videoLibaryElementModelList = new List<VideoLibraryElementModel>();
            var wordLibaryElementModelList = new List<WordLibraryElementModel>();

            foreach (var lem in lem_list)
            {
                double height_key = 0;
                List<PointModel> shape_points;
                double width_key = 0;
                double video_ratio = 0;
                double end_time_key = 0;
                LibraryElementOrigin origin_key = new LibraryElementOrigin();
                string library_element_id_to_update;
                double start_time_key = 0;
                double top_left_point_x = 0;
                double image_ratio = 0;
                string parent_id_key = null;
                string link_in_id = null;
                string link_out_id = null;
                bool finite_bool = false;
                int page_end_number = 0;
                int page_start_number = 0;
                bool title_visibility;
                ColorModel shape_color;
                double aspect_ration;
                NusysConstants.LinkDirection link_direction_enum = NusysConstants.LinkDirection.Forward;
                double top_left_point_y = 0;

                using (var cmd = db.CreateCommand())
                {
                    // set the sql for the db command
                    cmd.CommandText = $"SELECT * FROM properties WHERE library_or_alias_id = '{lem.LibraryElementId}'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        // while there are more rows in the result
                        while (reader.Read())
                        {
                            var property_key = reader.GetString(1);
                            var property_value = reader.GetString(4);
                            if (property_key == "height_key")
                            {
                                height_key = double.Parse(property_value);
                            } else if (property_key == "shape_points")
                            {
                                shape_points = JsonConvert.DeserializeObject<List<PointModel>>(property_value);
                            } else if (property_key == "width_key")
                            {
                                width_key = double.Parse(property_value);
                            }
                            else if (property_key == "video_ratio")
                            {
                                video_ratio = double.Parse(property_value);
                            }
                            else if (property_key == "end_time_key")
                            {
                                end_time_key = double.Parse(property_value);
                            }
                            else if (property_key == "origin_key")
                            {
                                origin_key = JsonConvert.DeserializeObject<LibraryElementOrigin>(property_value);
                            }
                            else if (property_key == "library_element_id_to_update")
                            {
                                library_element_id_to_update = property_value;
                            }
                            else if (property_key == "start_time_key")
                            {
                                start_time_key = double.Parse(property_value);
                            }
                            else if (property_key == "top_left_point_x")
                            {
                                top_left_point_x = double.Parse(property_value);
                            }
                            else if (property_key == "image_ratio")
                            {
                                image_ratio = double.Parse(property_value);
                            }
                            else if (property_key == "parent_id_key")
                            {
                                parent_id_key = property_value;
                            }
                            else if (property_key == "link_in_id")
                            {
                                link_in_id = property_value;
                            }
                            else if (property_key == "link_out_id")
                            {
                                link_out_id = property_value;
                            }
                            else if (property_key == "finite_bool")
                            {
                                finite_bool = bool.Parse(property_value);
                            }
                            else if (property_key == "page_end_number")
                            {
                                page_end_number = int.Parse(property_value);
                            }
                            else if (property_key == "page_start_number")
                            {
                                page_start_number = int.Parse(property_value);
                            }
                            else if (property_key == "title_visibility")
                            {
                                title_visibility = bool.Parse(property_value);
                            }
                            else if (property_key == "shape_color")
                            {
                                shape_color = JsonConvert.DeserializeObject<ColorModel>(property_value);
                            }
                            else if (property_key == "aspect_ration")
                            {
                                aspect_ration = double.Parse(property_value);
                            }
                            else if (property_key == "link_direction_enum")
                            {
                                link_direction_enum = (NusysConstants.LinkDirection) Enum.Parse(typeof(NusysConstants.LinkDirection), property_value, true);
                            }
                            else if (property_key == "top_left_point_y")
                            {
                                top_left_point_y = double.Parse(property_value);
                            }
                        }
                    }
                }
                switch (lem.Type)
                {
                    case NusysConstants.ElementType.Image:
                        imageLibaryElementModelList.Add(new ImageLibraryElementModel(lem.LibraryElementId, lem.Type)
                        {
                            Ratio = image_ratio,
                            NormalizedHeight = height_key,
                            NormalizedWidth = width_key,
                            NormalizedX = top_left_point_x,
                            NormalizedY = top_left_point_y,
                            ContentDataModelId = lem.ContentDataModelId,
                            Timestamp = lem.Timestamp,
                            LastEditedTimestamp = lem.LastEditedTimestamp,
                            Creator = lem.Creator,
                            Favorited = lem.Favorited,
                            Keywords = lem.Keywords,
                            Title = lem.Title,
                            SmallIconUrl = lem.SmallIconUrl,
                            MediumIconUrl = lem.MediumIconUrl,
                            AccessType = lem.AccessType,
                            LargeIconUrl = lem.LargeIconUrl,
                            ParentId = parent_id_key,
                            Origin = origin_key,
                            Metadata = lem.Metadata

                        });
                    break;
                    case NusysConstants.ElementType.PDF:
                        pdfLibaryElementModelList.Add(new PdfLibraryElementModel(lem.LibraryElementId, lem.Type)
                        {
                            Ratio = image_ratio,
                            NormalizedHeight = height_key,
                            NormalizedWidth = width_key,
                            NormalizedX = top_left_point_x,
                            NormalizedY = top_left_point_y,
                            PageEnd = page_end_number,
                            PageStart = page_start_number,
                            ContentDataModelId = lem.ContentDataModelId,
                            Timestamp = lem.Timestamp,
                            LastEditedTimestamp = lem.LastEditedTimestamp,
                            Creator = lem.Creator,
                            Favorited = lem.Favorited,
                            Keywords = lem.Keywords,
                            Title = lem.Title,
                            SmallIconUrl = lem.SmallIconUrl,
                            MediumIconUrl = lem.MediumIconUrl,
                            AccessType = lem.AccessType,
                            LargeIconUrl = lem.LargeIconUrl,
                            ParentId = parent_id_key,
                            Origin = origin_key,
                            Metadata = lem.Metadata
                        });
                        break;
                    case NusysConstants.ElementType.Word:
                        wordLibaryElementModelList.Add(new WordLibraryElementModel(lem.LibraryElementId)
                        {
                            Ratio = image_ratio,
                            NormalizedHeight = height_key,
                            NormalizedWidth = width_key,
                            NormalizedX = top_left_point_x,
                            NormalizedY = top_left_point_y,
                            PageEnd = page_end_number,
                            PageStart = page_start_number,
                            ContentDataModelId = lem.ContentDataModelId,
                            Timestamp = lem.Timestamp,
                            LastEditedTimestamp = lem.LastEditedTimestamp,
                            Creator = lem.Creator,
                            Favorited = lem.Favorited,
                            Keywords = lem.Keywords,
                            Title = lem.Title,
                            SmallIconUrl = lem.SmallIconUrl,
                            MediumIconUrl = lem.MediumIconUrl,
                            AccessType = lem.AccessType,
                            LargeIconUrl = lem.LargeIconUrl,
                            ParentId = parent_id_key,
                            Origin = origin_key,
                            Metadata = lem.Metadata
                        });
                        break;
                    
                    
                    case NusysConstants.ElementType.Audio:
                        audioLibaryElementModelList.Add(new AudioLibraryElementModel(lem.LibraryElementId)
                        {
                            NormalizedStartTime = start_time_key,
                            NormalizedDuration = end_time_key,
                            ContentDataModelId = lem.ContentDataModelId,
                            Timestamp = lem.Timestamp,
                            LastEditedTimestamp = lem.LastEditedTimestamp,
                            Creator = lem.Creator,
                            Favorited = lem.Favorited,
                            Keywords = lem.Keywords,
                            Title = lem.Title,
                            SmallIconUrl = lem.SmallIconUrl,
                            MediumIconUrl = lem.MediumIconUrl,
                            AccessType = lem.AccessType,
                            LargeIconUrl = lem.LargeIconUrl,
                            ParentId = parent_id_key,
                            Origin = origin_key,
                            Metadata = lem.Metadata
                        });
                        break;
                    case NusysConstants.ElementType.Video:
                        videoLibaryElementModelList.Add(new VideoLibraryElementModel(lem.LibraryElementId)
                        {
                            NormalizedStartTime = start_time_key,
                            NormalizedDuration = end_time_key,
                            Ratio = video_ratio,
                            ContentDataModelId = lem.ContentDataModelId,
                            Timestamp = lem.Timestamp,
                            LastEditedTimestamp = lem.LastEditedTimestamp,
                            Creator = lem.Creator,
                            Favorited = lem.Favorited,
                            Keywords = lem.Keywords,
                            Title = lem.Title,
                            SmallIconUrl = lem.SmallIconUrl,
                            MediumIconUrl = lem.MediumIconUrl,
                            AccessType = lem.AccessType,
                            LargeIconUrl = lem.LargeIconUrl,
                            ParentId = parent_id_key,
                            Origin = origin_key,
                            Metadata = lem.Metadata
                        });
                        break;

                    case NusysConstants.ElementType.Collection:
                        collectionLibaryElementModelList.Add(new CollectionLibraryElementModel(lem.LibraryElementId)
                        {
                            IsFinite = finite_bool,
                            ContentDataModelId = lem.ContentDataModelId,
                            Timestamp = lem.Timestamp,
                            LastEditedTimestamp = lem.LastEditedTimestamp,
                            Creator = lem.Creator,
                            Favorited = lem.Favorited,
                            Keywords = lem.Keywords,
                            Title = lem.Title,
                            SmallIconUrl = lem.SmallIconUrl,
                            MediumIconUrl = lem.MediumIconUrl,
                            AccessType = lem.AccessType,
                            LargeIconUrl = lem.LargeIconUrl,
                            ParentId = parent_id_key,
                            Origin = origin_key,
                            Metadata = lem.Metadata
                        });
                        break;

                    case NusysConstants.ElementType.Link:
                        linkLibaryElementModelList.Add(new LinkLibraryElementModel(lem.LibraryElementId)
                        {
                            InAtomId = link_in_id,
                            OutAtomId = link_out_id,
                            Direction = link_direction_enum,
                            ContentDataModelId = lem.ContentDataModelId,
                            Timestamp = lem.Timestamp,
                            LastEditedTimestamp = lem.LastEditedTimestamp,
                            Creator = lem.Creator,
                            Favorited = lem.Favorited,
                            Keywords = lem.Keywords,
                            Title = lem.Title,
                            SmallIconUrl = lem.SmallIconUrl,
                            MediumIconUrl = lem.MediumIconUrl,
                            AccessType = lem.AccessType,
                            LargeIconUrl = lem.LargeIconUrl,
                            ParentId = parent_id_key,
                            Origin = origin_key,
                            Metadata = lem.Metadata
                        });
                        break;

                    default:
                        lem.ParentId = parent_id_key;
                        lem.Origin = origin_key;
                        regularLibaryElementModelList.Add(lem);
                        break;
                }
            }



            //// make sure that we have a connection to the Document Database
            await Initialize();

            //// delete all the current models in the database
            //var currLastUsedCollectionModels = await QueryAsyncWithRetries(client.CreateDocumentQuery<string>(_collection.SelfLink, $"SELECT VALUE c.id FROM c WHERE c.DocType='{NusysConstants.DocDB_DocumentType.Last_used_collections}'"));
            //foreach (var docId in currLastUsedCollectionModels)
            //{
            //    await ExecuteWithRetries(() => client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(NusysConstants.DocDB_Database_ID, NusysConstants.DocDB_Collection_ID, docId)));
            //}


            //// transfer over all the new models
            foreach (var model in audioLibaryElementModelList)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }

            foreach (var model in collectionLibaryElementModelList)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }

            foreach (var model in imageLibaryElementModelList)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }

            foreach (var model in regularLibaryElementModelList)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }

            foreach (var model in linkLibaryElementModelList)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }

            foreach (var model in pdfLibaryElementModelList)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }

            foreach (var model in videoLibaryElementModelList)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }

            foreach (var model in wordLibaryElementModelList)
            {
                await ExecuteWithRetries(() => client.CreateDocumentAsync(_collection.SelfLink, model));
            }
        }

        /// <summary>
        /// Executes the documentdb query asynchronously with retries on throttle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        private static async Task<IEnumerable<T>> QueryAsyncWithRetries<T>(IQueryable<T> query)
        {
            var docQuery = query.AsDocumentQuery();
            var batches = new List<IEnumerable<T>>();

            do
            {
                var batch = await ExecuteWithRetries(() => docQuery.ExecuteNextAsync<T>());

                batches.Add(batch);
            }
            while (docQuery.HasMoreResults);

            var docs = batches.SelectMany(b => b);

            return docs;
        }


        /// <summary>
        /// Execute the function with retries on throttle
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="client"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        private static async Task<V> ExecuteWithRetries<V>(Func<Task<V>> function)
        {
            TimeSpan sleepTime = TimeSpan.Zero;

            while (true)
            {
                try
                {
                    return await function();
                }
                catch (DocumentClientException de)
                {
                    if ((int)de.StatusCode != 429)
                    {
                        throw;
                    }
                    sleepTime = de.RetryAfter;
                }
                catch (AggregateException ae)
                {
                    if (!(ae.InnerException is DocumentClientException))
                    {
                        throw;
                    }

                    DocumentClientException de = (DocumentClientException)ae.InnerException;
                    if ((int)de.StatusCode != 429)
                    {
                        throw;
                    }
                    sleepTime = de.RetryAfter;
                }

                await Task.Delay(sleepTime);
            }
        }
    }



}