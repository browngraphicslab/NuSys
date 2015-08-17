
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{
    public class TextNode : Node
    {
        public TextNode(string data, string id): base(id)
        {
            Text = data;
            this.ID = id;
        }

        public string Text { get; set; }

        public override string GetContentSource()
        {
            return Text;
        }

        public override void Update(Dictionary<string, string> props)
        {
            if (props.ContainsKey("text"))
            {
                Text = props["text"];
                this.DebounceDict.Add("text",Text);
            }
            base.Update(props);
        }
    }
}
