using System.Collections.Generic;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public abstract class Sendable
    {

        protected Sendable(string id)
        {
            Id = id;
        }
        

        public string Id { get; set; }
        
        public virtual async Task<Dictionary<string, object>> Pack()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("id", Id);
            return dict;
        }

        public virtual void UnPackFromDatabaseMessage(Message props)
        {
        }
    }
}