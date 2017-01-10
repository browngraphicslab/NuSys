using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserHelper
{
    public class TextDataHolder : DataHolder
    {
        public String Text { get; set; }
        public List<string> links { get; set; }
        public TextDataHolder(string text,string title="") : base(DataType.Text,title)
        {

            this.Text = text;
        }
    }
}
