using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuStarterProject
{
    public class TextNode : Node
    {
        private string _text;
        public TextNode(string data, int id): base(id)
        {
            _text = data;
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
    }
}
