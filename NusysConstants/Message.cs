using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NusysIntermediate
{
    public class Message
    {
        private ConcurrentDictionary<string, object> _dict;

        public Message()
        {
            _dict = new ConcurrentDictionary<string, object>();
        }
        public Message(string m)
        {
            try
            {
                var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                _dict = new ConcurrentDictionary<string, object>(JsonConvert.DeserializeObject<Dictionary<string, object>>(m, settings));
            }
            catch (Exception e)
            {
                var matches = Regex.Match(m, "(?:({[^}]+}) *)*");
                string[] miniStrings = matches.Groups[1].Captures.Cast<Capture>().Select(c => c.Value).ToArray();
                try
                {
                    var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                    _dict = JsonConvert.DeserializeObject<ConcurrentDictionary<string, object>>(miniStrings[0], settings);
                }
                catch (Exception f)
                {

                }
            }
            if (_dict == null)
            {
                _dict = new ConcurrentDictionary<string, object>();
            }
        }

        public Message(Dictionary<string, string> dict)
        {
            _dict = new ConcurrentDictionary<string, object>();
            foreach (var kvp in dict)
            {
                _dict.TryAdd(kvp.Key, kvp.Value);
            }
        }
        public Message(Dictionary<string, object> dict)
        {
            _dict = new ConcurrentDictionary<string, object>(dict);
        }
        public void Add(string key, object value)//TODO get rid of this
        {
            if (_dict.ContainsKey(key))
            {
                throw new InvalidOperationException("Key " + key + " already exists");
            }
            _dict.TryAdd(key, value);
        }
        public void Remove(string key)//TODO get rid of this too, only use is hacky fix
        {
            if (_dict.ContainsKey(key))
            {
                object outVar;
                _dict.TryRemove(key, out outVar);
                return;
            }
            throw new KeyNotFoundException("Key " + key + " not found when attemped to remove it.");
        }
        public object this[string key]
        {
            get { return Get(key); }
            set { _dict[key] = value; }
        }

        public object GetObject(string key)
        {
            if (_dict.ContainsKey(key))
            {
                return _dict[key];
            }
            return null;
        }
        public string Get(string key)
        {
            if (_dict.ContainsKey(key) && _dict[key] != null)
            {
                return _dict[key].ToString();
            }
            return null;
        }

        public double GetDouble(string key, double defaultValue = 0)
        {
            return ContainsKey(key) ? double.Parse(Get(key)) : defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return ContainsKey(key) ? int.Parse(Get(key)) : defaultValue;
        }

        public long GetLong(string key, int defaultValue = 0)
        {
            return ContainsKey(key) ? long.Parse(Get(key)) : defaultValue;
        }

        public string GetString(string key, string defaultValue = null)
        {
            return ContainsKey(key) ? Get(key) : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return ContainsKey(key) ? bool.Parse(Get(key)) : defaultValue;
        }

        public byte[] GetByteArray(string key)
        {
            return ContainsKey(key) ? Convert.FromBase64String(Get(key)) : null;
        }

        public List<T> GetList<T>(string key, List<T> def = null)
        {
            var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            return ContainsKey(key) ? JsonConvert.DeserializeObject<List<T>>(Get(key), settings) : def;
        }
        public HashSet<T> GetHashSet<T>(string key, HashSet<T> def = null)
        {
            var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            return ContainsKey(key) ? JsonConvert.DeserializeObject<HashSet<T>>(Get(key), settings) : def;
        }


        public Dictionary<T, K> GetDict<T, K>(string key)
        {
            if (ContainsKey(key))
            {
                try
                {
                    return JsonConvert.DeserializeObject<Dictionary<T, K>>(Get(key));
                }
                catch (Exception e)
                {
                    return new Dictionary<T, K>();
                }
            }
            return new Dictionary<T, K>();
        }

        public List<List<T>> GetNestedList<T>(string key)
        {
            return ContainsKey(key) ? JsonConvert.DeserializeObject<List<List<T>>>(Get(key)) : null;
        }
        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public string GetSerialized()
        {
            var r = JsonConvert.SerializeObject(_dict, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });
            return r;
        }
    }
}
