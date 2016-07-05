using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NuSysApp
{
    // TODO: remove BASEINPC
    public abstract class Sendable
    {

        public delegate void UnPackedEventHandler(object source);
        

        public event UnPackedEventHandler UnPacked;



        protected Sendable(string id)
        {
            Id = id;
        }

        public bool IsUnpacked { get; private set; }

        public string Id { get; set; }


        
        public virtual async Task<Dictionary<string, object>> Pack()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("id", Id);
            return dict;
        }

        public virtual async Task UnPack(Message props)
        {
            IsUnpacked = true;
            
            UnPacked?.Invoke(this);
        }

        public virtual async Task<string> Stringify()
        {
            var props = await Pack();

            // TODO: Remove
            /*
            var childs = new Dictionary<string, string>();

            if (Children.Count == 0)
                return JsonConvert.SerializeObject(props);
            
            foreach (var child in Children)
            {
                childs.Add(child.Value.ID, await child.Value.Stringify());
            }
            props["children"] = JsonConvert.SerializeObject(childs);
            */
            var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };

            return JsonConvert.SerializeObject(props, settings);
        }
    }
}