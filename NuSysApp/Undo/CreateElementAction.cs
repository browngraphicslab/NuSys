using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class CreateElementAction : IUndoable
    {

        private ElementController _elementController;
        private Point _position;

        public CreateElementAction(ElementController controller, Point point)
        {
            _elementController = controller;
            _position = point;    

        }

        public async void ExecuteRequest()
        {
            var element = _elementController.LibraryElementModel;
            var dict = new Message();
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
            dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
            var request = new NewElementRequest(dict);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);

        }


        public IUndoable GetInverse()
        {
            var removeElementAction = new RemoveElementAction(_elementController);
            return removeElementAction;
        }
    }
}
