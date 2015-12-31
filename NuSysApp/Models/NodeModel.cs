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
        public NodeContentModel Content { set; get; }

        public NodeModel(string id) : base(id)
        {
            InqCanvas = new InqCanvasModel(id);
            if (SessionController.Instance.ActiveWorkspace != null)
            {
                Metadata["workspace"] = SessionController.Instance.ActiveWorkspace.Id;
            }
        }
        
        


        public void MoveToGroup(NodeContainerModel nodeContainer, bool keepInOld = false)
        {
            //this.ParentGroup = nodeContainer;
            var oldGroupId = (string)Metadata["nodeContainer"];
            Metadata["nodeContainer"] = nodeContainer.Id;
            nodeContainer?.AddChild(this); //only add if nodeContainer isn't null

            if (!keepInOld)
            {
                var currentGroup = SessionController.Instance.IdToSendables[oldGroupId] as NodeContainerModel;
                currentGroup.RemoveChild(this);
            }
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("nodeType")) { 
                string t = props["nodeType"];
                NodeType = (NodeType)Enum.Parse(typeof(NodeType), t);
            }
            await base.UnPack(props);
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("nodeType", NodeType.ToString());

            dict.Add("type", "node");
            return dict;
        }
    }
}