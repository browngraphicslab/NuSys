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

        public void ExecuteRequest()
        {
            var model = _elementController.Model;
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(model.Id));
        }

        public IUndoable GetInverse()
        {
            var position = new Point(_elementController.Model.X, _elementController.Model.Y);
            var createElementAction = new CreateElementAction(_elementController, position);
            return createElementAction;

        }


    }
}
