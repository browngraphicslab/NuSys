﻿using System;
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
            //Old position is where the element used to be before the action occured.
            _oldPosition = oldPosition;
            //New position is where the element is after the action occurs.
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

        /// <summary>
        /// Calls element controller's set position method, which adds location to debouncing dictionary
        /// </summary>
        public void ExecuteAction()
        {
            _elementController.SetPosition(_newPosition.X, _newPosition.Y);
           
        }
    }
}
