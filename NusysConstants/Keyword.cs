using System.Diagnostics;

namespace NusysIntermediate
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
        //defining hash in terms of fields
       public override int GetHashCode()
       {
           return Text.GetHashCode()*17 + Source.GetHashCode();
       }
        public override bool Equals(object obj)
        {
            if(obj is Keyword)
            {
                var kw = obj as Keyword;
                Debug.WriteLine("here in keyword equals");
                return Text == kw.Text && kw.Source.Equals(Source);
            }
            return base.Equals(obj);
        }
    }
}
