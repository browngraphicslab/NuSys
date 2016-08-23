using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NuSysApp2
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
    }
}