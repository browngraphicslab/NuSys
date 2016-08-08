using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class LibraryElementModelFactory
    {
        public static LibraryElementModel CreateFromMessage(Message message)
        {
            Debug.Assert(message.ContainsKey("type"));
            ElementType type = ElementType.Area;
            try
            {
                type = (ElementType) Enum.Parse(typeof (ElementType), message.GetString("type"), true);
            }
            catch (Exception e)
            {
                var req = new DeleteLibraryElementRequest(message.GetString("id"));
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(req);
            }
            LibraryElementModel model;

            var id = message.GetString("id");
            if (SessionController.Instance.ContentController.GetContent(id) == null)
            {
                var title = message.GetString("title");
                Dictionary<string, MetadataEntry> metadata = new Dictionary<string, MetadataEntry>();
                foreach (
                    var kvp in
                        message.GetDict<string, MetadataEntry>("metadata") ?? new Dictionary<string, MetadataEntry>())
                {
                    metadata.Add(kvp.Key, new MetadataEntry(kvp.Value.Key, new List<string>(new HashSet<string>(kvp.Value.Values)), kvp.Value.Mutability));
                }
                var favorited = message.GetBool("favorited");

                var finite = message.GetBool("finite");
                var shapepoints = message.GetList<Windows.Foundation.Point>("shape_points");
                if (shapepoints == null)
                {
                    shapepoints = new List<Windows.Foundation.Point>();
                }

                Debug.Assert(id != null);
                switch (type)
                {
                    case ElementType.ImageRegion:
                        model = new RectangleRegion(id, ElementType.ImageRegion);
                        break;
                    case ElementType.VideoRegion:
                        model = new VideoRegionModel(id);
                        break;
                    case ElementType.PdfRegion:
                        model = new PdfRegionModel(id);
                        break;
                    case ElementType.AudioRegion:
                        model = new AudioRegionModel(id);
                        break;
                    case ElementType.Collection:
                        model = new CollectionLibraryElementModel(id, metadata, title, favorited, finite, shapepoints);
                        break;
                    case ElementType.Link:
                        Debug.Assert(message.ContainsKey("id1") && message.ContainsKey("id2"));
                        var id1 = message.Get("id1");
                        var id2 = message.Get("id2");
                        model = new LinkLibraryElementModel(id1, id2, id);
                        break;
                    default:
                        model = new LibraryElementModel(id, type, metadata, title, favorited);
                        break;
                }

                
                SessionController.Instance.ContentController.Add(model);
                var controller = SessionController.Instance.ContentController.GetLibraryElementController(id);
                Debug.Assert(controller != null);
                controller.UnPack(message);
                if (Constants.IsRegionType(type))
                {
                    Debug.Assert(model is Region);
                    SessionController.Instance.RegionsController.AddRegion(model as Region);
                }
                if (type == ElementType.Link)
                {
                    var linkController =
                        SessionController.Instance.ContentController.GetLibraryElementController(id) as
                            LinkLibraryElementController;
                    Debug.Assert(linkController != null);
                    SessionController.Instance.LinksController.AddLinkLibraryElementController(linkController);
                }
                return model;
            }
            return null;
        }
    }
}
