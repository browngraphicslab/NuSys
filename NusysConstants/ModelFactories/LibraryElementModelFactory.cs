using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;


namespace NusysIntermediate
{
    public class LibraryElementModelFactory
    {
        /// <summary>
        /// Returns the library element model for the passed in message
        /// </summary>
        public static LibraryElementModel CreateFromMessage(Message message)
        {
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_TYPE_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY));
            var type = message.GetEnum<NusysConstants.ElementType>(NusysConstants.LIBRARY_ELEMENT_TYPE_KEY);
            LibraryElementModel model;

            var id = message.GetString(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY);

            Debug.Assert(id != null);
            switch (type)
            {
                case NusysConstants.ElementType.Collection:
                    model = new CollectionLibraryElementModel(id);
                    break;
                case NusysConstants.ElementType.Link:
                    model = new LinkLibraryElementModel(id);
                    break;
                case NusysConstants.ElementType.Video:
                    model = new VideoLibraryElementModel(id);
                    break;
                case NusysConstants.ElementType.Image:
                    model = new ImageLibraryElementModel(id);
                    break;
                case NusysConstants.ElementType.Audio:
                    model = new AudioLibraryElementModel(id);
                    break;
                case NusysConstants.ElementType.PDF:
                    model = new PdfLibraryElementModel(id);
                    break;
                default:
                    model = new LibraryElementModel(id, type);
                    break;
            }
            model.UnPackFromDatabaseKeys(message);
            return model;
        }

        /// <summary>
        /// will take in a string that is a serialzed librayElementModel.  
        /// Will return the model or throw exceptions if it is invalid
        /// </summary>
        /// <param name="libraryElementJSON"></param>
        /// <returns></returns>
        public static LibraryElementModel DeserializeFromString(string libraryElementJSON)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<LibraryElementModel>(libraryElementJSON);
                switch (model.Type)
                {
                    case NusysConstants.ElementType.Collection:
                        model = JsonConvert.DeserializeObject<CollectionLibraryElementModel>(libraryElementJSON);
                        break;
                    case NusysConstants.ElementType.Link:
                        model = JsonConvert.DeserializeObject<LinkLibraryElementModel>(libraryElementJSON);
                        break;
                    case NusysConstants.ElementType.Video:
                        model = JsonConvert.DeserializeObject<VideoLibraryElementModel>(libraryElementJSON);
                        break;
                    case NusysConstants.ElementType.Audio:
                        model = JsonConvert.DeserializeObject<AudioLibraryElementModel>(libraryElementJSON);
                        break;
                    case NusysConstants.ElementType.PDF:
                        model = JsonConvert.DeserializeObject<PdfLibraryElementModel>(libraryElementJSON);
                        break;
                    case NusysConstants.ElementType.Image:
                        model = JsonConvert.DeserializeObject<ImageLibraryElementModel>(libraryElementJSON);
                        break;
                }
                //VERY IMPORTANT
                //TODO put debug.Asserts below all these to check states
                return model;
            }
            catch (JsonException jsonException)
            {
                throw new Exception("Could not create LibraryElementModel from Json string.  originalException: "+jsonException.Message);
            }
        }
    }
}
