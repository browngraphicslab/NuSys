using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    /// <summary>
    /// Describes a DeleteElementAction, which is instantiated when the client deletes an element
    /// </summary>
    public class DeleteElementAction : IUndoable
    {

        private ElementController _elementController;

        /// <summary>
        /// A DeleteElementAction needs a reference to the ElementController of the deleted element
        /// </summary>
        /// <param name="controller"></param>
        public DeleteElementAction(ElementController controller)
        {
            _elementController = controller;
        }

        /// <summary>
        /// Element controller creates DeleteSendableRequest and sends it to the server. 
        /// </summary>
        public void ExecuteAction()
        {
            Debug.Assert(_elementController != null);
            if(_elementController != null)
            {
                _elementController.RequestDelete();

            }
        }

        /// <summary>
        /// Returns a CreateElementAction that holds the deleted element's position so that position is
        /// preserved.
        /// </summary>
        /// <returns></returns>
        public IUndoable GetInverse()
        {
            var position = new Point(_elementController.Model.X, _elementController.Model.Y);
            var createElementAction = new CreateElementAction(_elementController, position);
            return createElementAction;

        }


    }
}
