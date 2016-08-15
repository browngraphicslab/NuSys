using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NusysIntermediate
{
    /// <summary>
    /// this factory class will be where all contentData models are made from.  
    /// This is the only place in NusysApp that contentDataModels should be instantiated.  
    /// </summary>
    public class ContentDataModelFactory
    {
        /// <summary>
        /// creates a content model from a message using the Content SQL table keys.  
        /// The message must also contain the Data key
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ContentDataModel CreateFromMessage(Message message)
        {
            //make sure all the keys are present
            Debug.Assert(message.ContainsKey(NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CONTENT_DATA_MODEL_DATA_STRING_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CONTENT_TABLE_TYPE_KEY));

            //get the content type, contentDataModel Id, and data strings
            var contentType = message.GetEnum<NusysConstants.ContentType>(NusysConstants.CONTENT_TABLE_TYPE_KEY);
            var contentId = message.GetString(NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY);
            var data = message.GetString(NusysConstants.CONTENT_DATA_MODEL_DATA_STRING_KEY);

            //based on the needed content type, instatiate different contentDataModels
            ContentDataModel model;
            switch (contentType)
            {
                case NusysConstants.ContentType.PDF:
                    model = new PdfContentDataModel(contentId,data);
                    break;
                default:
                    model = new ContentDataModel(contentId,data);
                    break;
            }
            model.ContentType = contentType;

            return model;
        }

        /// <summary>
        /// will take in a string that is a serialzed contentDataModel.  
        /// Will return the model or throw exceptions if it is invalid
        /// </summary>
        /// <param name="contentDataModelJSON"></param>
        /// <returns></returns>
        public static ContentDataModel DeserializeFromString(string contentDataModelJSON)
        {
            try
            {
                //deserialize once to start
                var model = JsonConvert.DeserializeObject<ContentDataModel>(contentDataModelJSON);

                //if its type requires a second, more specialized deserialization, do so
                switch (model.ContentType)
                {
                    case NusysConstants.ContentType.PDF:
                        model = JsonConvert.DeserializeObject<PdfContentDataModel>(contentDataModelJSON);
                        break;
                }
                
                return model;
            }
            catch (JsonException jsonException)
            {
                throw new Exception("Could not create ContentDataModel from Json string.  originalException: " + jsonException.Message);
            }
        }
    }
}

