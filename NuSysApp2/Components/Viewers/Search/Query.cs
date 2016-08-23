using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NuSysApp2
{
    //Grant was here
    public class Query
    {
        private List<string> _keywords;
        // todo change this to private
        public string SearchString { get; }
        private Dictionary<string, List<string>> _keywordArgs;


        public Query(string searchString)
        {
            SearchString = searchString;
            _keywords = new List<string>() { "contentType", "Luke", "fileType" };
            _keywordArgs = new Dictionary<string, List<string>>();
            foreach (string s in _keywords)
            {
                Debug.WriteLine(s);
                _keywordArgs.Add(s, new List<string>());
            }
            this.parseString();
        }

        public void parseString()
        {
            Regex rgx = new Regex(@"\b(" + string.Join("|", _keywords.Select(Regex.Escape).ToArray()) + @"\b):(( [a-zA-Z0-9-]+,)*( [a-zA-Z0-9-]+))");
            MatchCollection mCol = rgx.Matches(SearchString);
            foreach (Match match in mCol)
            {
                Debug.WriteLine("match: " + match);
                string[] fieldAndArgs = match.Value.Split(':');
                if (_keywords.Contains(fieldAndArgs[0]))
                {
                    fieldAndArgs[1] = Regex.Replace(fieldAndArgs[1], ",", "");
                    foreach (string s in fieldAndArgs[1].Split(null))
                    {
                        if (!s.Equals(""))
                        {
                            _keywordArgs[fieldAndArgs[0]].Add(s);
                            Debug.WriteLine(s);
                        }
                    }
                }
            }
        }

        public Dictionary<string, List<string>> KeywordArgs
        {
            get { return _keywordArgs; }
        }

    }
}
