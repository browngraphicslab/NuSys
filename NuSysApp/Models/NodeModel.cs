using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{
    [DataContract]
    public class NodeModel : AtomModel
    {
        public NodeType NodeType { get; set; }

        public InqCanvasModel InqCanvas { get; set; }
        //public NodeContentModel Content { set; get; }
        public string ContentId { set; get; }

        public NodeModel(string id) : base(id)
        {
            InqCanvas = new InqCanvasModel(id);
            if (SessionController.Instance.ActiveWorkspace != null)
            {
                Metadata["workspace"] = SessionController.Instance.ActiveWorkspace.Id;
            }
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("nodeType")) { 
                string t = props["nodeType"];
                NodeType = (NodeType)Enum.Parse(typeof(NodeType), t);
            }
            ContentId = props.GetString("contentId", null);
            await base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("nodeType", NodeType.ToString());
            dict.Add("type", "node");
            dict.Add("contentId", ContentId);
            return dict;
        }
    }
}