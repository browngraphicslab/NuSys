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
        public Message OriginalState;
        public Message FinalState;

        private ElementController _elementController;
        private Point _oldPosition;
        private Point _newPosition;

        public MoveElementAction(ElementController controller, Point oldPosition, Point newPosition)
        {
            _elementController = controller;
            _oldPosition = oldPosition;
            _newPosition = newPosition;

            OriginalState = GenerateMessage(controller.Model.Id, oldPosition.X, oldPosition.Y);
            FinalState = GenerateMessage(controller.Model.Id, newPosition.X, newPosition.Y);

        }
        /// <summary>
        /// Helper method that returns a message that can be passed into the SendableUpdateRequest's constructor.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Message GenerateMessage(string id, double x, double y)
        {
            var message = new Message();
            message["id"] = id;
            message["x"] = x;
            message["y"] = y;
            return message;

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

        public void ExecuteRequest()
        {
            _elementController.SetPosition(_newPosition.X, _newPosition.Y);
            //var request = new SendableUpdateRequest(FinalState, true);
            //SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);

        }
    }
}
