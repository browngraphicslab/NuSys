using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RemoveElementAction : IUndoable
    {
        private ElementController _elementController;
        public RemoveElementAction(ElementController controller)
        {
            _elementController = controller;
        }

        /// <summary>
        /// Element controller creates DeleteSendableRequest and sends it to the server. 
        /// </summary>
        public void ExecuteAction()
        {
            _elementController.RequestDelete();
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
