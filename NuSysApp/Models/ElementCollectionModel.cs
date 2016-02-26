using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class ElementCollectionModel : ElementModel
    {
        public LibraryElementCollectionModel LibraryElementCollectionModel { get; set; }

        public ElementCollectionModel(string id):base(id)
        {

        }
    }
}