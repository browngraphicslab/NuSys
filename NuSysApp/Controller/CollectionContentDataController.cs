using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Class used as the content data controller for all collections
    /// </summary>
    public class CollectionContentDataController : ContentDataController
    {
        /// <summary>
        /// constructor just enforces you pass in a collection content data model
        /// </summary>
        /// <param name="model"></param>
        public CollectionContentDataController(CollectionContentDataModel model) : base(model){}

        /// <summary>
        /// casts the base class's ContentDataModel as a CollectionContentDataModel
        /// </summary>
        public CollectionContentDataModel CollectionModel
        {
            get
            {
                Debug.Assert(ContentDataModel is CollectionContentDataModel);
                return ContentDataModel as CollectionContentDataModel;
            }
        }

        /// <summary>
        /// Override method simply serializes the shape before sending it to the server.
        /// </summary>
        /// <param name="data"></param>
        public void SetData(CollectionShapeModel shape)
        {
            base.SetData(JsonConvert.SerializeObject(shape));
        }

        /// <summary>
        /// returns the collection model's shape if it exists, otherwise a new shape model
        /// </summary>
        /// <returns></returns>
        private CollectionShapeModel GetShape()
        {
            Debug.Assert(CollectionModel != null);
            return CollectionModel.Shape ?? new CollectionShapeModel();
        }

        /// <summary>
        /// method to be called to set the points of the shape.  
        /// Will create a new shape if it doesn't exist.
        /// This will automatically set the aspect ratio of the shape.
        /// This will send a server call to update
        /// </summary>
        /// <param name="points"></param>
        public void SetShapePoints(IEnumerable<PointModel> points)
        {
            Debug.Assert(points != null && points.Count() >= 4);
            var minX = points.Min(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxX = points.Max(p => p.X);
            var maxY = points.Max(p => p.Y);
            var x = maxX - minX;
            var y = maxY - minY;
            var shape = GetShape();
            shape.ShapePoints = new List<PointModel>(points);
            shape.AspectRatio = x / y;
            SetData(shape);
        }

        /// <summary>
        /// method to be called to set the color of the shape.  
        /// Will create a new shape if it doesn't exist.
        /// This will send a server call to update
        /// </summary>
        /// <param name="points"></param>
        public void SetShapeColor(ColorModel color)
        {
            var shape = GetShape();
            shape.ShapeColor = color;
            SetData(shape);
        }

        /// <summary>
        /// Method to update the server with a completely null shape object.
        /// This will fire the 'contentUpdated' event with a null string for content.
        /// </summary>
        public void ClearShape()
        {
            base.SetData(null);
        }

        /// <summary>
        /// method to be called to set the string URL of the shape.  
        /// Will create a new shape if it doesn't exist.
        /// This will send a server call to update
        /// </summary>
        /// <param name="points"></param>
        public void SetShapeUrl(string url, double aspectRatio)
        {
            var shape = GetShape();
            shape.ImageUrl = url;
            shape.AspectRatio = aspectRatio;
            SetData(shape);
        }
    }
}
