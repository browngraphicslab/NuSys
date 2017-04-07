using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class VariableLibraryElementModel : LibraryElementModel
    {
        public VariableLibraryElementModel(string libraryElementId, NusysConstants.ElementType elementType = NusysConstants.ElementType.Variable) : base(libraryElementId, elementType)
        {
        }
        //public double AspectRatio { get; set; }
        //public string MetadataKey { get; set; }
        public override void UnPackFromDatabaseKeys(Message message)
        {
            if (message.ContainsKey("metadataKey"))
            {
                //MetadataKey = message.GetString("metadataKey");
            }
            if (message.ContainsKey("aspectRatio"))
            {
                //AspectRatio = message.GetDouble("aspectRatio");
            }
            base.UnPackFromDatabaseKeys(message);
        }
    }
}
