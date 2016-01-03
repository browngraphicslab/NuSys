using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

            var lines = props.GetNestedList<Point2d>("inqLines");
            if (lines != null) { 
                foreach (var line in lines)
                {
                    InqCanvas.AddLine(new InqLineModel(SessionController.Instance.GenerateId())
                    {
                        Points = new ObservableCollection<Point2d>(line)
                    });
                }
            }
            await base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("nodeType", NodeType.ToString());
            dict.Add("type", "node");
            dict.Add("contentId", ContentId);

            var lines = new List<List<Point2d>>();
            foreach (var inqLineModel in InqCanvas.Lines)
            {
                lines.Add(inqLineModel.Points.ToList());
            }

            dict.Add("inqLines", lines);
            return dict;
        }
    }
}