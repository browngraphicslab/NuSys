using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class Message
    {
        private Dictionary<string, object> _dict;
        private Dictionary<string, Message> _children;

        public Message()
        {
            _dict = new Dictionary<string, object>();
            _children = new Dictionary<string, Message>();
            //Init(message);
        }

        public async Task Init(string m)
        {
            var settings = new JsonSerializerSettings {StringEscapeHandling = StringEscapeHandling.EscapeNonAscii};
            var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(m, settings);
            foreach (KeyValuePair<string, object> kvp in message)
            {
                if (kvp.Key != "children")
                {
                    _dict.Add(kvp.Key,kvp.Value);
                }
                else
                {
                    var children = await JsonConvert.DeserializeObjectAsync<Dictionary<string, object>>(kvp.Value.ToString(), settings);
                    foreach (var child in children)
                    {
                        var msg = new Message();
                        await msg.Init(child.Value.ToString());
                        _children.Add(child.Key, msg);
                    }
                }
            }
        }

        public void Add(string key, string value)//TODO get rid of this
        {
            if (_dict.ContainsKey(key))
            {
                throw new InvalidOperationException("Key "+key+" already exists");
            }
            _dict.Add(key, value);
        }
        public void Remove(string key)//TODO get rid of this too, only use is hacky fix
        {
            if (_dict.ContainsKey(key))
            {
                _dict.Remove(key);
                return;
            }
            throw new KeyNotFoundException("Key "+key+" not found when attemped to remove it.");
        }
        public string this[string key]
        {
            get { return Get(key); }
        }
        public string Get(string key)
        {
            if (key == "children")
            {
                throw new Exception("Cannot get children as string.  Call Children()");
            }
            if (_dict.ContainsKey(key) && _dict[key] != null)
            {
                return _dict[key].ToString();
            }
            return null;
        }

        public double GetDouble(string key, double defaultValue)
        {
            return ContainsKey(key) ? double.Parse(Get(key)) : defaultValue;
        }

        public int GetInt(string key, int defaultValue)
        {
            return ContainsKey(key) ? int.Parse(Get(key)) : defaultValue;
        }

        public string GetString(string key, string defaultValue)
        {
            return ContainsKey(key) ? Get(key) : defaultValue;
        }

        public byte[] GetByteArray(string key)
        {
            return ContainsKey(key) ? Convert.FromBase64String(Get(key)) : null;
        }

        public List<T> GetList<T>(string key)
        {
            return ContainsKey(key) ? JsonConvert.DeserializeObject<List<T>>(Get(key)) : null;
        }

        public Dictionary<T, K> GetDict<T, K>(string key)
        {
            return ContainsKey(key) ? JsonConvert.DeserializeObject<Dictionary<T, K>>(Get(key)) : new Dictionary<T, K>();
        }

        public List<List<T>> GetNestedList<T>(string key)
        {
            return ContainsKey(key) ? JsonConvert.DeserializeObject<List<List<T>>>(Get(key)) : null;
        } 

        public Dictionary<string, Message> Children()
        {
            return _children;
        }

        public bool ContainsKey(string key)
        {
            if (key == "children")
            {
                return HasChildren();
            }
            return _dict.ContainsKey(key);
        }

        public bool HasChildren()
        {
            return _children.Count > 0;
        }
    }
}
