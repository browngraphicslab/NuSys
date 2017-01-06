using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class ContentDataControllerFactory
    {
        /// <summary>
        /// Takes in a ContentDataModel and returns a newly created ContentDataController off the model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ContentDataController CreateFromContentDataModel(ContentDataModel model)
        {
            ContentDataController controller;
            if (model.Data.Length < 10)
            {
                
            }
            switch (model.ContentType)
            {
                case NusysConstants.ContentType.Collection:
                    Debug.Assert(model is CollectionContentDataModel);
                    controller = new CollectionContentDataController(model as CollectionContentDataModel);
                    break;
                default:
                    controller = new ContentDataController(model);
                    break;
            }
            return controller;

        }
    }
}
