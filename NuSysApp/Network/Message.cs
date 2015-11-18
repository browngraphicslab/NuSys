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
        private Dictionary<string, string> _dict;
        private Dictionary<string, Message> _children;

        public Message()
        {
            _dict = new Dictionary<string, string>();
            _children = new Dictionary<string, Message>();
            //Init(message);
        }

        public async Task Init(string m)
        {           
            Dictionary<string, string> message = JsonConvert.DeserializeObject<Dictionary<string, string>>(m);
            foreach (KeyValuePair<string, string> kvp in message)
            {
                if (kvp.Key != "children")
                {
                    _dict.Add(kvp.Key,kvp.Value);
                }
                else
                {
                    Dictionary<string, string> children = await JsonConvert.DeserializeObjectAsync<Dictionary<string, string>>(kvp.Value);
                    foreach (KeyValuePair<string,string> child in children)
                    {
                        var msg = new Message();
                        await msg.Init(child.Value);
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
            if (_dict.ContainsKey(key))
            {
                return _dict[key];
            }
            return null;
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
