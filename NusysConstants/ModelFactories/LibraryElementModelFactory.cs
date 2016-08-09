using System;
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
                case NusysConstants.ElementType.ImageRegion:
                    model = new RectangleRegion(id, NusysConstants.ElementType.ImageRegion);
                    break;
                case NusysConstants.ElementType.VideoRegion:
                    model = new VideoRegionModel(id);
                    break;
                case NusysConstants.ElementType.PdfRegion:
                    model = new PdfRegionModel(id);
                    break;
                case NusysConstants.ElementType.AudioRegion:
                    model = new AudioRegionModel(id);
                    break;
                case NusysConstants.ElementType.Collection:
                    model = new CollectionLibraryElementModel(id);
                    break;
                case NusysConstants.ElementType.Link:
                    model = new LinkLibraryElementModel(id);
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
                    case NusysConstants.ElementType.ImageRegion:
                        model = JsonConvert.DeserializeObject<RectangleRegion>(libraryElementJSON);
                        break;
                    case NusysConstants.ElementType.VideoRegion:
                        model = JsonConvert.DeserializeObject<VideoRegionModel>(libraryElementJSON);
                        break;
                    case NusysConstants.ElementType.PdfRegion:
                        model = JsonConvert.DeserializeObject<PdfRegionModel>(libraryElementJSON);
                        break;
                    case NusysConstants.ElementType.AudioRegion:
                        model = JsonConvert.DeserializeObject<AudioRegionModel>(libraryElementJSON);
                        break;
                    case NusysConstants.ElementType.Collection:
                        model = JsonConvert.DeserializeObject<CollectionLibraryElementModel>(libraryElementJSON);
                        break;
                    case NusysConstants.ElementType.Link:
                        model = JsonConvert.DeserializeObject<LinkLibraryElementModel>(libraryElementJSON);
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
