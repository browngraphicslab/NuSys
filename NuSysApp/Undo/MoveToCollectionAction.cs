using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// This is an action that describes when a node is added to another collection.
    /// </summary>
    public class MoveToCollectionAction : IUndoable
    {
        //ID of element being moved into the collection
        public string ElementID { set; get; }
        //CollectionID of the collection it was dragged from
        public string OldCollectionID { set; get; }
        //CollectionID of the collection it was removed from
        public string NewCollectionID { set; get; }
        // Old location of the element on the old collection
        public Point2d OldLocation { set; get; }
        //New location of the element on the new collectoin
        public Point2d NewLocation { set; get; }

        public MoveToCollectionAction(string elementID, string oldCollectionID, string newCollectionID, Point2d oldLocation, Point2d newLocation)
        {
            ElementID = elementID;
            OldCollectionID = oldCollectionID;
            NewCollectionID = newCollectionID;
            OldLocation = oldLocation;
            NewLocation = newLocation;

        }

        /// <summary>
        /// Executes the action described by the parameters. In this case, it will move an element to the new collection and the new location
        /// </summary>
        public void ExecuteAction()
        {
            var controller = SessionController.Instance.IdToControllers[ElementID];
            controller?.RequestMoveToCollection(NewCollectionID, NewLocation.X, NewLocation.Y);
        }
        /// <summary>
        /// Gets the inverse action. In this case, it will move an element from the new collection back to the old collection.
        /// </summary>
        /// <returns></returns>
        public IUndoable GetInverse()
        {
            var moveToCollectionAction = new MoveToCollectionAction(ElementID, NewCollectionID, OldCollectionID, NewLocation, OldLocation);
            return moveToCollectionAction;
        }
    }
}
