using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NusysIntermediate
{
    public class Message : IEnumerable<KeyValuePair<string,object>>
    {
        private ConcurrentDictionary<string, object> _dictionary;
        public Message()
        {
            _dictionary = new ConcurrentDictionary<string, object>();
        }

        /// <summary>
        /// creates a new message based on the on passed in.
        /// Uses the IEnumerable's builtin constructor for copying ienumerables
        /// </summary>
        /// <param name="message"></param>
        public Message(Message message)
        {
            _dictionary = new ConcurrentDictionary<string, object>(message);
        }

        /// <summary>
        /// to desialize a string to a message.
        /// This constructor calls the json convert method and de-json-stringifies the string you pass in
        /// </summary>
        /// <param name="m"></param>
        public Message(string m)
        {
            try
            {
                var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                _dictionary = new ConcurrentDictionary<string, object>(JsonConvert.DeserializeObject<Dictionary<string, object>>(m, settings));
            }
            catch (Exception e)
            {
                var matches = Regex.Match(m, "(?:({[^}]+}) *)*");
                string[] miniStrings = matches.Groups[1].Captures.Cast<Capture>().Select(c => c.Value).ToArray();
                try
                {
                    var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
                    _dictionary = JsonConvert.DeserializeObject<ConcurrentDictionary<string, object>>(miniStrings[0], settings);
                }
                catch (Exception f)
                {

                }
            }
            if (_dictionary == null)
            {
                _dictionary = new ConcurrentDictionary<string, object>();
            }
        }

        public Message(Dictionary<string, string> dict)
        {
            _dictionary = new ConcurrentDictionary<string, object>();
            foreach (var kvp in dict)
            {
                _dictionary.TryAdd(kvp.Key, kvp.Value);
            }
        }
        public Message(Dictionary<string, object> dict)
        {
            _dictionary = new ConcurrentDictionary<string, object>(dict);
        }
        public void Add(string key, object value)//TODO get rid of this
        {
            if (_dictionary.ContainsKey(key))
            {
                throw new InvalidOperationException("Key " + key + " already exists");
            }
            _dictionary.TryAdd(key, value);
        }
        public bool Remove(string key)//TODO get rid of this too, only use is hacky fix
        {
            if (_dictionary.ContainsKey(key))
            {
                object outVar;
                _dictionary.TryRemove(key, out outVar);
                return true;
            }
            return false;
        }
        public object this[string key]
        {
            get { return Get(key); }
            set { _dictionary[key] = value; }
        }

        public object GetObject(string key)
        {
            if (_dictionary.ContainsKey(key))
            {
                return _dictionary[key];
            }
            return null;
        }
        public string Get(string key)
        {
            if (_dictionary.ContainsKey(key) && _dictionary[key] != null)
            {
                return _dictionary[key].ToString();
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
            try
            {
                return ContainsKey(key) ? bool.Parse(Get(key)) : defaultValue;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// returns an enum of the specified type.  The type passed in must be an enum type or this will probably crash.  
        /// The key must also be present in the dictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetEnum<T>(string key)
        {
            Debug.Assert(ContainsKey(key));
            try
            {
                var e = (T) (Enum.Parse(typeof(T), Get(key), true));
                return e;
            }
            catch (Exception e)
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
        }
        public byte[] GetByteArray(string key)
        {
            return ContainsKey(key) ? Convert.FromBase64String(Get(key)) : null;
        }

        public T Get<T>(string key)
        {
            var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };
            Debug.Assert(_dictionary.ContainsKey(key));
            return JsonConvert.DeserializeObject<T>(Get(key), settings);
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
            return _dictionary.ContainsKey(key);
        }

        public string GetSerialized()
        {
            var r = JsonConvert.SerializeObject(new Dictionary<string,object>(_dictionary), new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });
            return r;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
    }
}
