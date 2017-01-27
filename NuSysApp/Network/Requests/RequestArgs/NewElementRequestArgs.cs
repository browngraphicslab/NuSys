﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// args class for sending a createNewElementRequest.  
    /// Check each property's comments to see what is required.
    /// oddly Nullable properties are only nullable so we can check that have an actual value before sending.
    /// </summary>
    public class NewElementRequestArgs : IRequestArgumentable
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

        /// <summary>
        /// the bool for the node to have visible or invisible titles
        /// </summary>
        public bool ShowTitle { get; set; } = true;

        /// <summary>
        /// the access that this new element will have upon creation.  
        /// Even if this access type is public, it will be hidden if the library element's access type is private
        /// </summary>
        public NusysConstants.AccessType? AccessType{ get;set;}

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

        /// <summary>
        /// Simply packs the request args.  
        /// Will also debug.assert for required keys.  
        /// see the individual keys to check which are required.
        /// </summary>
        /// <returns>
        /// Returns the message which will tell the server what to add to the new element
        /// </returns>
        public Message PackToRequestKeys()
        {
            //create message to return when fully packed
            var message = new Message();

            //asserts for required properties
            //TODO not make width and height required, just have defaults in nusysApp constants in they're not set;
            Debug.Assert(ParentCollectionId != null);
            Debug.Assert(LibraryElementId != null);
            Debug.Assert(Height != null);
            Debug.Assert(Width != null);
            Debug.Assert(Y != null);
            Debug.Assert(X != null);

            //set properties after assertions
            message[NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY] = Id ?? SessionController.Instance.GenerateId();
            message[NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_Y_KEY] = Y;
            message[NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_X_KEY] = X;
            message[NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_PARENT_COLLECTION_ID_KEY] = ParentCollectionId;
            message[NusysConstants.NEW_ELEMENT_REQUEST_SIZE_HEIGHT_KEY] = Height;
            message[NusysConstants.NEW_ELEMENT_REQUEST_SIZE_WIDTH_KEY] = Width;
            message[NusysConstants.NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY] = LibraryElementId;
            message[NusysConstants.NEW_ELEMENT_REQUEST_TITLE_VISIBILITY_KEY] = ShowTitle;

            if (AccessType == null) //access type can be null because it is nullable.  If it's null (i.e. not set), default to public for now
            {
                message[NusysConstants.NEW_ELEMENT_REQUEST_ACCESS_KEY] = NusysConstants.AccessType.Public.ToString(); //TODO remove this later
            }
            else {
                message[NusysConstants.NEW_ELEMENT_REQUEST_ACCESS_KEY] = AccessType.ToString();
            }
            return message;
        }
    }
}
