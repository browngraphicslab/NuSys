﻿using System;
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
        /// the double aspect ratio of the collection if it is shaped.  
        /// This is not requiresd, but should be used if the ShapePoints list is used.
        /// will only be included in the request if this nullable double is not null.
        /// Should be calculated as width/height.
        /// </summary>
        public double? AspectRatio { get; set; }

        /// <summary>
        /// The ColorModel that represents the color of the shape of the collection.  
        /// If there is no color, don't set this.
        /// A null value will mean that the value won't be included in the request.
        /// </summary>
        public ColorModel Color { get; set; }

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
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_SHAPED_COLLECTION_POINTS_KEY] = ShapePoints;
            }
            if (AspectRatio != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_SHAPED_COLLECTION_ASPECT_RATIO_KEY] = AspectRatio.Value;
            }
            if(Color != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_SHAPED_COLLECTION_COLOR_KEY] = Color;
            }
            return message;
        }
    }
}
