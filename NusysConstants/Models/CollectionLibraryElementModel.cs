using System;
using System.Collections.Generic;

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

        public override void UnPackFromDatabaseKeys(Message message)
        {
            base.UnPackFromDatabaseKeys(message);
            if (message.ContainsKey(NusysConstants.COLLECTION_CHILDREN_KEY))
            {
                Children = message.GetHashSet<string>(NusysConstants.COLLECTION_CHILDREN_KEY);
            }
        }
    }
}
