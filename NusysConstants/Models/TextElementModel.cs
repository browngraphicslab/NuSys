using System.Collections.Generic;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class TextElementModel : ElementModel
    {
        public delegate void TextChangedEventHandler(object source, string text);
        public event TextChangedEventHandler TextChanged;

        public TextElementModel(string id): base(id)
        {
            ElementType = NusysConstants.ElementType.Text;
        }

        public override void UnPackFromDatabaseMessage(Message props)
        {
            base.UnPackFromDatabaseMessage(props);
        }

        public override async Task<Dictionary<string,object>> Pack()
        {
            return await base.Pack();
        }
    }
}
