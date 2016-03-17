using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class CollectionElementModel : ElementModel
    {
        public CollectionLibraryElementModel LibraryElementCollectionModel { get; set; }

        public CollectionElementModel(string id):base(id)
        {

        }
    }
}