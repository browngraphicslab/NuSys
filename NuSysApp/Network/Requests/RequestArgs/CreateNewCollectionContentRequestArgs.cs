using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// request args class for creating collections 
    /// </summary>
    public class CreateNewCollectionContentRequestArgs : CreateNewContentRequestArgs
    {
        /// <summary>
        /// The shape of the collection you are making.  
        /// Should be null if you don't want to add a shape.
        /// </summary>
        public CollectionShapeModel Shape { get; set; }

        /// <summary>
        /// this override just serializes and adds the shape if it isn't null;
        /// </summary>
        /// <returns></returns>
        public override Message PackToRequestKeys()
        {
            var m = base.PackToRequestKeys();
            if (Shape != null)
            {
                m[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = JsonConvert.SerializeObject(Shape);
            }
            return m;
        }
    }
}
