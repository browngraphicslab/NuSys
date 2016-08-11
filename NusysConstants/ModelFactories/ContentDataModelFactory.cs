using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace NusysIntermediate
{
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
            Debug.Assert(message.ContainsKey(NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.CONTENT_DATA_MODEL_DATA_STRING_KEY));

            var contentId = message.GetString(NusysConstants.CONTENT_TABLE_CONTENT_ID_KEY);
            var data = message.GetString(NusysConstants.CONTENT_DATA_MODEL_DATA_STRING_KEY);

            var model = new ContentDataModel(contentId,data);
            model.ContentType = message.GetEnum<NusysConstants.ContentType>(NusysConstants.CONTENT_TABLE_TYPE_KEY);
            return model;
        }

        /// <summary>
        /// will take in a string that is a serialzed contentDataModel.  
        /// Will return the model or throw exceptions if it is invalid
        /// </summary>
        /// <param name="libraryElementJSON"></param>
        /// <returns></returns>
        public static ContentDataModel DeserializeFromString(string contentDataModelJSON)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<ContentDataModel>(contentDataModelJSON);
                return model;
            }
            catch (JsonException jsonException)
            {
                throw new Exception("Could not create LibraryElementModel from Json string.  originalException: " + jsonException.Message);
            }
        }
    }
}

