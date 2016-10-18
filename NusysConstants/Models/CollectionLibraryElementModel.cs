using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysIntermediate
{
    /// <summary>nk
    /// this is the library element sub class used for all collections. 
    ///  The only additions to the regular library element class are the local hashset of children element model Ids,  and properties defining the settings on this collection.
    /// </summary>
    public class CollectionLibraryElementModel : LibraryElementModel
    {
        /// <summary>
        /// This hashset is of ElementModel Ids.  
        /// It represents the list of nodes (elements) in this Collection library element.
        /// This hashset should not be saved on the server.  
        /// This is only used locally for each client.   
        /// This list should absolutely not be saved on the server
        /// </summary>
        public HashSet<string> Children { get; private set; }

        /// <summary>
        /// this boolean represents whether this collection is bounded or unbounded.
        /// Being bounded means there is some shape, custom or default, that acts as the outer bounds that no inner element can be outside of.
        /// </summary>
        public bool IsFinite
        {
            get;
            set;
        }

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
        /// The image url of the background of the collection if it exists.  
        /// </summary>
        public String ImageBackground { get; set; }

        /// <summary>
        /// The double aspect ratio of a shaped collection. 
        /// This should only be used shapepoints is not null and three or more shapepoints exist.
        /// Should be calculated as width/height.
        /// </summary>
        public double AspectRatio { get; set; }

        /// <summary>
        /// this constructor just takes in the id of the library element.
        /// This also instantiates the list of children to be used locally on each client.
        /// </summary>
        /// <param name="id"></param>
        public CollectionLibraryElementModel(string id) : base(id, NusysConstants.ElementType.Collection)
        {
            Children = new HashSet<string>();
        }

        /// <summary>
        /// this method is used when the server wants to create a library elmeent model from the message recieved from the sql database.
        /// This will just unpack the properties of a collection that are saved in the sql database; i.e. Finite boolean and ShapePoints list
        /// </summary>
        /// <param name="message"></param>
        public override void UnPackFromDatabaseKeys(Message message)
        {
            base.UnPackFromDatabaseKeys(message);
            if (message.ContainsKey(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_FINITE_BOOLEAN_KEY))
            {
                IsFinite = message.GetBool(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_FINITE_BOOLEAN_KEY);
            }
            if (message.ContainsKey(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPED_POINTS_LIST_KEY))
            {
                ShapePoints = message.GetList<PointModel>(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPED_POINTS_LIST_KEY);
            }
            if (message.ContainsKey(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPED_ASPECT_RATIO_KEY))
            {
                AspectRatio = message.GetDouble(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPED_ASPECT_RATIO_KEY);
            }
            if (message.ContainsKey(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPE_COLOR_KEY))
            {
                ShapeColor = message.Get<ColorModel>(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPE_COLOR_KEY);
            }
            if (message.ContainsKey(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPE_IMAGE_BACKGROUND_KEY))
            {
                ImageBackground = message.Get(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPE_IMAGE_BACKGROUND_KEY);
            }
        }
    }
}
