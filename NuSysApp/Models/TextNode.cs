
using System;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TextNode : Node
    {
        public TextNode(string data, int id): base(id)
        {
            Text = data;
        }

        public string Text { get; set; }

        public override string GetContentSource()
        {
            return Text;
        }
    }
}
