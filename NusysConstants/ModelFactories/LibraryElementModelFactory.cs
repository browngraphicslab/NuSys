using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;


namespace NusysIntermediate
{
    public class LibraryElementModelFactory
    {
        public static LibraryElementModel CreateFromMessage(Message message)
        {
            Debug.Assert(message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_TYPE_KEY));
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
                    Debug.Assert(message.ContainsKey(NusysConstants.LINK_LIBRARY_ELEMENT_IN_ID_KEY) && message.ContainsKey(NusysConstants.LINK_LIBRARY_ELEMENT_OUT_ID_KEY));
                    var id1 = message.Get(NusysConstants.LINK_LIBRARY_ELEMENT_IN_ID_KEY);
                    var id2 = message.Get(NusysConstants.LINK_LIBRARY_ELEMENT_OUT_ID_KEY);

                    model = new LinkLibraryElementModel(id1, id2, id);
                    break;
                default:
                    model = new LibraryElementModel(id, type);
                    break;
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY))
            {
                model.Title = message.GetString(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY))
            {
                model.ContentDataModelId = message.GetString(NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY))
            {
                model.Favorited = message.GetBool(NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY);
            }
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
