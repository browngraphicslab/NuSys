using System;
using System.Collections.Generic;
using System.Diagnostics;


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

            //TODO add back in this debug assertion somehow.  although we can't access the sessioncontroller from here...
            //Debug.Assert(SessionController.Instance.ContentController.GetLibraryElementModel(id) == null);

            var title = message.GetString(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY);
            Dictionary<string, MetadataEntry> metadata = new Dictionary<string, MetadataEntry>();
            foreach (
                var kvp in
                    message.GetDict<string, MetadataEntry>(NusysConstants.LIBRARY_ELEMENT_METADATA_KEY) ?? new Dictionary<string, MetadataEntry>())
            {
                metadata.Add(kvp.Key, new MetadataEntry(kvp.Value.Key, new List<string>(new HashSet<string>(kvp.Value.Values)), kvp.Value.Mutability));
            }
            var favorited = message.GetBool(NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY);

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
                    model = new CollectionLibraryElementModel(id, metadata, title, favorited);
                    break;
                case NusysConstants.ElementType.Link:
                    Debug.Assert(message.ContainsKey("id1") && message.ContainsKey("id2"));
                    var id1 = message.Get("id1");
                    var id2 = message.Get("id2");

                    model = new LinkLibraryElementModel(id1, id2, id);
                    break;
                default:
                    model = new LibraryElementModel(id, type, metadata, title, favorited);
                    break;
            }
            return model;
        }
    }
}
