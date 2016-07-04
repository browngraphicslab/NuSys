using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class LibraryElementModelFactory
    {
        public static LibraryElementModel CreateFromMessage(Message message)
        {
            Debug.Assert(message.ContainsKey("type"));
            var type = (ElementType)Enum.Parse(typeof(ElementType),message.GetString("type"), true);
            LibraryElementModel model;

            var id = message.GetString("id");
            var title = message.GetString("title");
            var metadata = message.GetDict<string, MetadataEntry>("metadata");
            var favorited = message.GetBool("favorited");

            Debug.Assert(id != null);
            switch (type)
            {
                case ElementType.Collection:
                    model = new CollectionLibraryElementModel(id,metadata,title,favorited);
                    break;
                case ElementType.Link:
                    Debug.Assert(message.ContainsKey("id1") && message.ContainsKey("id2"));
                    //TODO dont have this length check and just use linkIds
                    var id1 = message.Get("id1").Length == 32 ? new LinkId(message.GetString("id1")) : JsonConvert.DeserializeObject<LinkId>(message.GetString("id1"));
                    var id2 = message.Get("id2").Length == 32 ? new LinkId(message.GetString("id2")) : JsonConvert.DeserializeObject<LinkId>(message.GetString("id2"));

                    model = new LinkLibraryElementModel(id1,id2,id);
                    break;
                default:
                    model = new LibraryElementModel(id, type,metadata,title,favorited);
                    break;
            }
            model.UnPack(message);
            SessionController.Instance.ContentController.Add(model);
            if (type == ElementType.Link)
            {
                SessionController.Instance.LinkController.AddLink(id);
            }
            return model;
        }
    }
}
