using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class MoveElementAction : IUndoable
    {
 

        private ElementController _elementController;
        private Point _oldPosition;
        private Point _newPosition;

        public MoveElementAction(ElementController controller, Point oldPosition, Point newPosition)
        {
            _elementController = controller;
            _oldPosition = oldPosition;
            _newPosition = newPosition;
        }
        
        /// <summary>
        /// Creates a new MoveElementAction and gives it the reversed state messages.
        /// </summary>
        /// <returns></returns>
        public IUndoable GetInverse()
        {
            var inverseMoveElementAction = new MoveElementAction(_elementController, _newPosition,_oldPosition);
            return inverseMoveElementAction;
        }

        public void ExecuteAction()
        {
            _elementController.SetPosition(_newPosition.X, _newPosition.Y);
           
        }
    }
}
