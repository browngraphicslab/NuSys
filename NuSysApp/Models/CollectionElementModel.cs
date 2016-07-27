using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class CollectionElementModel : ElementModel
    {
        public CollectionLibraryElementModel CollectionLibraryElementModel
        {
            get
            {
                return base.LibraryElementModel as CollectionLibraryElementModel;
            }
        }

        public enum CollectionViewType { List, FreeForm, Timeline }

        public CollectionViewType ActiveCollectionViewType;

        public double LocationX { get; set; }
        public double LocationY { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Zoom { get; set; }

        public CollectionElementModel(string id):base(id)
        {

        }



      

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("collectionview"))
            {
                string t = props.GetString("collectionview");
                ActiveCollectionViewType = (CollectionViewType)Enum.Parse(typeof(CollectionViewType), t);
            }

         
            await base.UnPack(props);
        }
    }
}