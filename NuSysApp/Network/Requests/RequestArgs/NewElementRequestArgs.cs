using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// args class for sending a createNewElementRequest.  
    /// Check each property's comments to see what is required.
    /// oddly Nullable properties are only nullable so we can check that have an actual value before sending.
    /// </summary>
    public class NewElementRequestArgs
    {
        /// <summary>
        /// Empty constructor just sets nullable enums and booleans and numbers
        /// </summary>
        public NewElementRequestArgs()
        {
            X = null;
            Y = null;
        }
        
        /// <summary>
        /// the id of the new element.  Will create a new one if this isn't set. 
        /// </summary>
        public string Id { get; set; }


        #region Required

        /// <summary>
        /// REQUIRED.  
        /// the X-coordinate for the new node.
        /// </summary>
        public double? X { get; set; }

        /// <summary>
        /// REQUIRED.  
        /// the Y-coordinate for the new node.  
        /// </summary>
        public double? Y { get; set; }

        /// <summary>
        /// REQUIRED.  
        /// the width of the new node.
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// REQUIRED.  
        /// the Height of the new node.
        /// </summary>
        public double? Height { get; set; }

        /// <summary>
        /// REQUIRED.  
        /// the id of the library element that this Element will point to.
        /// Cannot be null;
        /// </summary>
        public string LibraryElementId { get; set; }

        /// <summary>
        /// REQUIRED.  
        /// The LibraryElementId of the collection that this node will be in. 
        /// </summary>
        public string ParentCollectionId { get; set; }

        #endregion Required

    }
}
