
using System;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TextNode : Node
    {
        public TextNode(string data, string id): base(id)
        {
            Text = data;
            this.NodeType = "TextNode";
        }

        public string Text { get; set; }

        public override string GetContentSource()
        {
            return Text;
        }
    }
}
