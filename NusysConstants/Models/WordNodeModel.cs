using System.Collections.Generic;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class WordNodeModel : ElementModel
    {
        public WordNodeModel(string id) : base(id)
        {
            ElementType = NusysConstants.ElementType.Word;
        }
        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            return props;
        }

        public override void UnPackFromDatabaseMessage(Message props)
        {
            base.UnPackFromDatabaseMessage(props);
        }
    }
}
