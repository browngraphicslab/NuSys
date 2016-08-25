using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the request args class used whenever you want to create a new collection.
    /// Create this class and then pass them into a new CreateNewLibraryElementRequest.
    /// Each of the properties will indicate to you if it's required or not.  
    /// </summary>
    public class CreateNewCollectionLibraryElementRequestArgs : CreateNewLibraryElementRequestArgs
    {
        /// <summary>
        /// the constructor is empty and simply sets the element type for the base CreateNewLibraryElementRequestArgs class.
        /// </summary>
        public CreateNewCollectionLibraryElementRequestArgs()
        {
            LibraryElementType = NusysConstants.ElementType.Collection;
        }

        /// <summary>
        /// the boolean representing whether the newly made collection will be finite or inifnite.  
        /// True for finite, false for infinite.
        /// Null means it wasn't set and won't be included in the new library element request;
        /// </summary>
        public bool? IsFiniteCollection { get; set; }

        /// <summary>
        /// the list of points used to represent the outer bounds of a bounded collection.  
        /// If this is null, it wont be included in the request
        /// </summary>
        public List<PointModel> ShapePoints { get; set; }

        /// <summary>
        /// this override pack to request keys simply calls the base class's method to get a message.
        /// THen it will add to the message the key value pairs for the finite boolean and the shape points using request keys;
        /// </summary>
        /// <returns></returns>
        public override Message PackToRequestKeys()
        {
            var message = base.PackToRequestKeys();
            if (IsFiniteCollection != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_COLLECTION_FINITE_BOOLEAN_KEY] = IsFiniteCollection.Value;
            }
            if (ShapePoints != null)
            {
                message[NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPED_POINTS_LIST_KEY] = ShapePoints;
            }
            return message;
        }
    }
}
