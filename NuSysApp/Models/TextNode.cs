
using System;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{
    public class TextNode : Node
    {
        public TextNode(string data, int id): base(id)
        {
            Text = data;
            this.ID = id;
        }

        public string Text { get; set; }

        public override string GetContentSource()
        {
            return Text;
        }
    }
}
