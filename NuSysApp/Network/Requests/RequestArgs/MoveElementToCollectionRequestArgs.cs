using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class MoveElementToCollectionRequestArgs : IRequestArgumentable
    {
        /// <summary>
        /// REQUIRED. This is the id of the element you wish to move to a different collection
        /// </summary>
        public string ElementId { get; set; }

        /// <summary>
        /// REQUIRED. This is the id of the collection that you wish you move the element into
        /// </summary>
        public string NewParentCollectionId { get; set; }

        /// <summary>
        /// OPTIONAL. The x coordinate of where you want to place the element in the collection 
        /// that was moved into. If it is not supplied, the Default is 5000 (MAX canvas size/2).
        /// </summary>
        public double? XCoordinate { get; set; }

        /// <summary>
        /// OPTIONAL. The y coordinate of where you want to place the element in the collection 
        /// that was moved into. If it is not supplied, the default is 5000 (MAX canvas size/2).
        /// </summary>
        public double? YCoordinate { get; set; }

        /// <summary>
        /// This returns a message that contains all the data in this class ready to send of to the server. It also checks that the message has 
        /// everything it needs, in this case, the id of the element to move, and the new parent collection id
        /// </summary>
        /// <returns></returns>
        public Message PackToRequestKeys()
        {
            Debug.Assert(ElementId != null);
            Debug.Assert(NewParentCollectionId != null);
            Message message = new Message();
            message[NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_ELEMENT_ID_KEY] = ElementId;
            message[NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_NEW_PARENT_COLLECTION_ID_KEY] = NewParentCollectionId;
            message[NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_X_KEY] = XCoordinate ?? (double)(Constants.MaxCanvasSize / 2);
            message[NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_Y_KEY] = YCoordinate ?? (double)(Constants.MaxCanvasSize / 2);
            return message;
        }
    }
}
