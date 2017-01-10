using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    ///  Data class that will extend contentDataModel and always extract a Shape class from the data string
    /// </summary>
    public class CollectionContentDataModel : ContentDataModel
    {
        /// <summary>
        /// This is the shape class of the collection.  All the information about the collection should be in this class .
        /// If this is null, then the collection is not shaped.
        /// </summary>
        public CollectionShapeModel Shape { get; private set; }

        /// <summary>
        /// This constructor will call the same-declaration constructor in the base ContentDataModel class and will set the Shape for this contentDataModel.
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <param name="contentData"></param>
        public CollectionContentDataModel(string contentDataModelId, string contentData) : base(contentDataModelId, contentData)
        {
            SetShape(contentData);
        }

        /// <summary>
        /// this override updates the shape and then sets the base's data.
        /// </summary>
        /// <param name="data"></param>
        public override void SetData(string data)
        {
            SetShape(data);
            base.SetData(data);
        }

        /// <summary>
        /// method used to set the shape.
        /// Simply should deserialize by making a call to a factory
        /// </summary>
        private void SetShape(string contentData)
        {
            if (contentData == null)
            {
                Shape = null;
                return;
            }
            //Shape = JsonConvert.DeserializeObject<CollectionShapeModel>(contentData);
        }

        /// <summary>
        /// this override simply just sets the Shape to null;
        /// </summary>
        public override void Dispose()
        {
            Shape = null;
            base.Dispose();
        }
    }
}
