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
        /// Using position and element controller, creates a message and makes a NewElementRequest with that message.
        /// Adds a copy of the removed element to the workspace in its old position.
        /// </summary>
        public async void ExecuteAction()
        {
            _elementController.LibraryElementController.AddElementAtPosition(_position.X, _position.Y);
            /*
            var element = _elementController.LibraryElementModel;
            //var dict = new Message();
            Dictionary<string, object> metadata;
            

            dict = new Message();
            dict["title"] = _elementController.Model.Title;
            dict["width"] = _elementController.Model.Width;
            dict["height"] = _elementController.Model.Height;
            dict["type"] = _elementController.Model.ElementType.ToString();
            dict["x"] = _position.X;
            dict["y"] = _position.Y;
            dict["contentId"] = element.ContentDataModelId;
            dict["metadata"] = _elementController.LibraryElementModel.Metadata;
            dict["autoCreate"] = true;
            //dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
            var request = new NewElementRequest(dict);
            //await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            //TODO fix this 817
            */
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
