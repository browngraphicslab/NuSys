using NusysIntermediate;
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

            //the workspace id we are using is the passes in one, or the session's current workspace Id if it is null
            var collectionId = SessionController.Instance.ActiveFreeFormViewer.Model.LibraryId;
            var libraryElementModel = _elementController.LibraryElementModel;
            // get the element type
            var elementType = _elementController.Model.ElementType;

            // if the element is a collection, use the StaticServerCalls Method
            if (elementType == NusysConstants.ElementType.Collection)
            {
                var collectionLibraryElementModel = libraryElementModel as CollectionLibraryElementModel;
                Debug.Assert(collectionLibraryElementModel != null);

                var args = new NewElementRequestArgs();//create new args class for the putting on an element collection on the main instance
                args.X = _position.X;
                args.Y = _position.Y;
                args.LibraryElementId = libraryElementModel.LibraryElementId;
                args.Width = _elementController.Model.Width;
                args.Height = _elementController.Model.Height;

                // try to add the collection to the collection
                var success = await StaticServerCalls.PutCollectionInstanceOnMainCollection(args);

            }

            //create the request args 
            var elementArgs = new NewElementRequestArgs();
            elementArgs.LibraryElementId = libraryElementModel.LibraryElementId;
            elementArgs.Width = _elementController.Model.Width;
            elementArgs.Height = _elementController.Model.Height;
            elementArgs.ParentCollectionId = collectionId;
            elementArgs.X = _position.X;
            elementArgs.Y = _position.Y;

            //create the request
            var request = new NewElementRequest(elementArgs);

            //execute the request, await return
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

            if (request.WasSuccessful() == true) //if it returned sucesssfully
            {
                await request.AddReturnedElementToSessionAsync();
                var model = request.GetReturnedElementModel();
                var controller = SessionController.Instance.IdToControllers[model.Id];// model.Id;

                if(_elementController == null)
                {
                    return;
                }

                _elementController = controller;


            }


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
