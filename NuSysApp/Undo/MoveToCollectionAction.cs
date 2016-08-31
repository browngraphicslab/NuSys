using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// This is a
    /// </summary>
    public class MoveToCollectionAction : IUndoable
    {
        public ElementController ElementController { set; get; }
        public string ElementID { set; get; }
        public string OldCollectionID { set; get; }
        public string NewCollectionID { set; get; }
        public Point2d OldLocation { set; get; }
        public Point2d NewLocation { set; get; }

        public MoveToCollectionAction(string elementID, string oldCollectionID, string newCollectionID, Point2d oldLocation, Point2d newLocation)
        {
            ElementID = elementID;
            OldCollectionID = oldCollectionID;
            NewCollectionID = newCollectionID;
            OldLocation = oldLocation;
            NewLocation = newLocation;

        }
        public void ExecuteAction()
        {
            var controller = SessionController.Instance.IdToControllers[ElementID];
            controller.RequestMoveToCollection(NewCollectionID, NewLocation.X, NewLocation.Y);
        }

        public IUndoable GetInverse()
        {
            var moveToCollectionAction = new MoveToCollectionAction(ElementID, NewCollectionID, OldCollectionID, NewLocation, OldLocation);
            return moveToCollectionAction;
        }
    }
}
