using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class CollectionLibraryElementModel : LibraryElementModel
    {
        public HashSet<string> Children { get; private set; }
        public CollectionLibraryElementModel(string id, Dictionary<String, MetadataEntry> metadata = null) : base(id, NusysConstants.ElementType.Collection)
        {
            Children = new HashSet<string>();
        }

        protected override void OnSessionControllerEnterNewCollection()
        {
            Children.Clear();
            base.OnSessionControllerEnterNewCollection();
        }
    }
}
