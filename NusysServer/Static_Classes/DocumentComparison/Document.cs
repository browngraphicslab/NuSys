using System.Collections.Generic;

namespace NusysServer.DocumentComparison
{
    public class Document
    {
        public string Id { get; set; }
        private Dictionary<string, int> _termFreqDictionary;
        private Dictionary<int, double> _tfidfVector; 

        public Document(Dictionary<string, int> dict, string id = "")
        {
            _termFreqDictionary = dict;
            Id = id;
        }

        public Dictionary<int, double> GetTFIDFVector()
        {
            return _tfidfVector;
        } 

        public int ReturnFrequency(string key)
        {
            int value;

            if (_termFreqDictionary.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }

        public int UniqueWordsFreq()
        {
            return _termFreqDictionary.Count;
        }

        public List<string> ReturnKeysList()
        {
            return new List<string>(_termFreqDictionary.Keys);
        }

        public Dictionary<string, int> GetDictionary()
        {
            return _termFreqDictionary;
        }

        public void setTfidfVector(Dictionary<int, double> tfidf)
        {
            _tfidfVector = tfidf;
        }
    }
}