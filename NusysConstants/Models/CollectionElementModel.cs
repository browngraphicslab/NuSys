using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class CollectionElementModel : ElementModel
    {

        public enum CollectionViewType { List, FreeForm, Timeline }

        public CollectionViewType ActiveCollectionViewType;

        public double LocationX { get; set; }
        public double LocationY { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Zoom { get; set; }

        public CollectionElementModel(string id):base(id)
        {
            ElementType = NusysConstants.ElementType.Collection;
        }

        public override void UnPackFromDatabaseMessage(Message props)
        {
            if (props.ContainsKey(NusysConstants.COLLECTION_ELEMENT_COLLECTION_VIEW_KEY))
            {
                string t = props.GetString(NusysConstants.COLLECTION_ELEMENT_COLLECTION_VIEW_KEY);
                ActiveCollectionViewType = (CollectionViewType)Enum.Parse(typeof(CollectionViewType), t);
            }
            base.UnPackFromDatabaseMessage(props);
        }
    }
}