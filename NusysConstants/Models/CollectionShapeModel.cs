using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// This shape class will be the class used to represent a shape of a collection.
    /// It will specify images, sizes, opactiys, shape points, etc.
    /// Whatever is needed to specify a special collection should be saved in this hacky, serializeable class.
    /// </summary>
    public class CollectionShapeModel
    {
        /// <summary>
        /// This list of points is used to define a custom shape that acts as the outer edge of a bounded collection.
        /// </summary>
        public List<PointModel> ShapePoints
        {
            get;
            set;
        }

        /// <summary>
        /// The color of the shape of the collection if it exists.  
        /// </summary>
        public ColorModel ShapeColor { get; set; }

        /// <summary>
        /// The double aspect ratio of a shaped collection. 
        /// This should only be used shapepoints is not null and three or more shapepoints exist.
        /// Should be calculated as width/height.
        /// </summary>
        public double AspectRatio { get; set; }
    }
}
