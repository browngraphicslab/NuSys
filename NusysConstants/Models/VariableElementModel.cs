using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class VariableElementModel : TextElementModel
    {
        public VariableElementModel(string id) : base(id)
        {
            ElementType = NusysConstants.ElementType.Variable;
        }
        public string StoredLibraryId { get; set; }
        public override void UnPackFromDatabaseMessage(Message props)
        {
            if (props.ContainsKey("StoredLibraryId"))
            {
                StoredLibraryId = props.GetString("StoredLibraryId");
            }
            base.UnPackFromDatabaseMessage(props);
        }
    }
}
