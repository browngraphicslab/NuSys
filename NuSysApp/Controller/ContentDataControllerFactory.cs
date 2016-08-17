using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class ContentDataControllerFactory
    {
        /// <summary>
        /// Takes in a ContentDataModel and returns a newly created ContentDataController ofr the model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static ContentDataController CreateFromContentDataModel(ContentDataModel model)
        {
            ContentDataController controller;
            switch (model.ContentType)
            {
                default:
                    controller = new ContentDataController(model);
                    break;
            }
            return controller;

        }
    }
}
