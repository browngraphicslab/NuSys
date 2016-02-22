using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class ElementInstanceCollectionModel : ElementInstanceModel
    {
        public List<ElementInstanceModel> Children { get; set; } 

        public ElementInstanceCollectionModel(string id):base(id)
        {
            Children = new List<ElementInstanceModel>();
        }
    }
}