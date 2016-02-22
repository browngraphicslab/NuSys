using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class ElementCollectionInstanceModel : ElementInstanceModel
    {
        public ElementCollectionModel ElementCollectionModel { get; set; }

        public ElementCollectionInstanceModel(string id):base(id)
        {

        }
    }
}