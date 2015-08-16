﻿
namespace NuSysApp
{
    public class RichTextNode : Node
    {
        public RichTextNode(string data, string id): base(id)
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
