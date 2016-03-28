using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class CollectionElementModel : ElementModel
    {
        public CollectionLibraryElementModel CollectionLibraryElementModel { get; set; }

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

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("collectionview", ActiveCollectionViewType);
            dict.Add("zoom", ActiveCollectionViewType);
            dict.Add("locationX", ActiveCollectionViewType);
            dict.Add("locationY", ActiveCollectionViewType);
            dict.Add("centerX", ActiveCollectionViewType);
            dict.Add("centerY", ActiveCollectionViewType);
            return dict;
        }

      

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("collectionview"))
            {
                string t = props.GetString("collectionview");
                ActiveCollectionViewType = (CollectionViewType)Enum.Parse(typeof(CollectionViewType), t);
            }

          
                LocationX = props.GetDouble("locationX", -Constants.MaxCanvasSize / 2.0);
                LocationY = props.GetDouble("locationY", -Constants.MaxCanvasSize / 2.0);
                CenterX = props.GetDouble("centerX", -Constants.MaxCanvasSize / 2.0);
                CenterY = props.GetDouble("centerY", -Constants.MaxCanvasSize / 2.0);
                Zoom = props.GetDouble("zoom", 1);
          

            await base.UnPack(props);
        }
    }
}