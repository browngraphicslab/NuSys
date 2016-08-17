using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class ContentDataController
    {
        public ContentDataModel ContentDataModel { get; }

        public ContentDataController(ContentDataModel contentDataModel)
        {
            ContentDataModel = contentDataModel;
        }

        public void SetData(string data)
        {
            ContentDataModel.Data = data;
        }
    }
}
