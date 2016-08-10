using System.Collections.Generic;

namespace NusysServer.DocumentComparison
{
    public class IdToTFIDF
    {
        private static Dictionary<string, Dictionary<string, int>> _idToTfidfDictionary;

        public static void Add(string key, Dictionary<string, int> value)
        {
            _idToTfidfDictionary.Add(key, value);
        }

        public static Dictionary<string, Dictionary<string, int>> getDictionary()
        {
            return _idToTfidfDictionary;
        } 
    }
}