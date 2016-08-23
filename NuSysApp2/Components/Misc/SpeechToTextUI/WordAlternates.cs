using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp2
{
    public struct WordAlternates
    {
        public string Word { get; set; }
        public HashSet<string> Alternates { get; set; }

        public WordAlternates(string word, HashSet<string> alternates)
        {
            if (string.IsNullOrEmpty(word)) throw new InvalidProgramException("Cannot instantiate a WordAlternate with an empty or null word");
            Word = word;

            if (alternates == null || alternates.Count == 0)
            {
                Alternates = new HashSet<string>();
            }
            else
            {
                Alternates = alternates;
            }

        }

        // override the equals method
        public override bool Equals(object obj)
        {
            if (!(obj is WordAlternates))
                return false;

            var other = (WordAlternates)obj;
            return string.Compare(this.Word, other.Word) == 0 && this.Alternates.All(alternate => other.Alternates.Contains(alternate));
        }

        public override string ToString()
        {
            StringBuilder  s = new StringBuilder();
            s.Append($"{{{Word}}}: ");
            foreach (var alternate in Alternates)
            {
                s.Append($"{alternate}, ");
            }
            return s.ToString();
        }
    }
}
