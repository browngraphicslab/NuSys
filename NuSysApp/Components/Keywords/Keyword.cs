using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
   public class Keyword
    {
        public enum KeywordSource
        {
            UserInput,
            TagExtraction,
            TopicModeling,
        }
        public string Text { private set; get; }
        public KeywordSource Source { private set; get; }
        public Keyword(string text, KeywordSource source = KeywordSource.UserInput)
        {
            Text = text;
        }
        public override bool Equals(object obj)
        {
            if(obj is Keyword)
            {
                var kw = obj as Keyword;
                return Text == kw.Text && kw.Source == Source;
            }
            return base.Equals(obj);
        }
    }
}
