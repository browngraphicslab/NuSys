using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    /// <summary>
    /// Describes a CreateElementAction, which is to be instantiated when the client creates an element/node
    /// </summary>
    public class CreateElementAction : IUndoable
    {

        private ElementController _elementController;
        private Point _position;

        /// <summary>
        /// The action must have reference to the ElementController and position of the newly created Element
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="point"></param>
        public CreateElementAction(ElementController controller, Point point)
        {
            //Position of removed element must be passed in so created element takes its place
            _elementController = controller;
            _position = point;    

        }
        /// <summary>
        /// Using position and element controller, calls libraryelementcontroller's addelementatposition method.
        /// Adds a copy of the removed element to the workspace in its old position.
        /// </summary>
        public async void ExecuteAction()
        {
            _elementController.LibraryElementController.AddElementAtPosition(
                _position.X, _position.Y , null, _elementController.Model.Width,
                _elementController.Model.Height);

        }

        /// <summary>
        /// Creates an inverse DeleteElementAction.
        /// </summary>
        /// <returns></returns>
        public IUndoable GetInverse()
        {
            var removeElementAction = new DeleteElementAction(_elementController);
            return removeElementAction;
        }
    }
}
