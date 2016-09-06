using System;
using System.Diagnostics;
using Newtonsoft.Json;
using NuSysApp;

namespace NusysIntermediate
{
    public class ElementModelFactory
    {
        /// <summary>
        /// Returns the element model for the passed in message.  
        /// Must have an elementType stored in the key NusysConstants.LIBRARY_ELEMENT_TYPE_KEY
        /// </summary>
        public static ElementModel CreateFromMessage(Message message)
        {
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_TYPE_KEY));
            var type = message.GetEnum<NusysConstants.ElementType>(NusysConstants.LIBRARY_ELEMENT_TYPE_KEY);
            ElementModel elementModel;

            var id = message.GetString(NusysConstants.ALIAS_ID_KEY);
            Debug.Assert(id != null);

            switch (type)
            {
                case NusysConstants.ElementType.Text:
                    elementModel = new TextElementModel(id);
                    break;
                case NusysConstants.ElementType.Image:
                    elementModel = new ImageElementModel(id);
                    break;
                case NusysConstants.ElementType.Word:
                    elementModel = new WordNodeModel(id);
                    break;
                case NusysConstants.ElementType.PDF:
                    elementModel = new PdfNodeModel(id);
                    break;
                case NusysConstants.ElementType.Audio:
                    elementModel = new AudioNodeModel(id);
                    break;
                case NusysConstants.ElementType.Video:
                    elementModel = new VideoNodeModel(id);
                    break;
                case NusysConstants.ElementType.Collection:
                    elementModel = new CollectionElementModel(id);
                    break;
                case NusysConstants.ElementType.Area:
                    elementModel = new AreaModel(id);
                    break;
                default:
                    elementModel = new ElementModel(id);
                    break;
            }
            elementModel.UnPackFromDatabaseMessage(message);
            return elementModel;
        }

        /// <summary>
        /// will take in a string that is a serialzed ElementModel.  
        /// Will return the element model or throw exceptions if it is invalid
        /// </summary>
        public static ElementModel DeserializeFromString(string elementJSON)
        {
            try
            {
                var elementModel = JsonConvert.DeserializeObject<ElementModel>(elementJSON);
                switch (elementModel.ElementType)
                {
                case NusysConstants.ElementType.Text:
                     elementModel = JsonConvert.DeserializeObject<TextElementModel>(elementJSON);
                    break;
                case NusysConstants.ElementType.Image:
                     elementModel = JsonConvert.DeserializeObject<ImageElementModel>(elementJSON);
                    break;
                case NusysConstants.ElementType.Word:
                     elementModel = JsonConvert.DeserializeObject<WordNodeModel>(elementJSON);
                    break;
                case NusysConstants.ElementType.PDF:
                     elementModel = JsonConvert.DeserializeObject<PdfNodeModel>(elementJSON);
                    break;
                case NusysConstants.ElementType.Audio:
                        elementModel = JsonConvert.DeserializeObject<AudioNodeModel>(elementJSON);
                    break;
                case NusysConstants.ElementType.Video:
                        elementModel = JsonConvert.DeserializeObject<VideoNodeModel>(elementJSON);
                    break;
                case NusysConstants.ElementType.Collection:
                        elementModel = JsonConvert.DeserializeObject<CollectionElementModel>(elementJSON);
                    break;
                case NusysConstants.ElementType.Area:
                        elementModel = JsonConvert.DeserializeObject<AreaModel>(elementJSON);
                    break;
                }
                //VERY IMPORTANT
                //TODO put debug.Asserts below all these to check states
                return elementModel;
            }
            catch (JsonException jsonException)
            {
                throw new Exception("Could not create ElementModel from Json string.  originalException: " + jsonException.Message);
            }
        }
    }
}