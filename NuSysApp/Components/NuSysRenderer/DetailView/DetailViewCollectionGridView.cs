using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    public class DetailViewCollectionGridView : RectangleUIElement
    {
        /// <summary>
        /// The controller for the collection associated with this collection grid view
        /// </summary>
        private CollectionLibraryElementController _controller;

        /// <summary>
        /// the collection element models on the collection associated with the controller
        /// </summary>
        private List<ElementModel> _collectionElementModels;

        /// <summary>
        /// options for how to sort the grid
        /// </summary>
        public enum GridSortOption
        {
            Title,
            Date,
            Creator
        }

        /// <summary>
        /// the list ui elements which are displayed on the grid
        /// </summary>
        private List<CollectionGridViewUIElement> _collectionGridElements;

        public DetailViewCollectionGridView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            CollectionLibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            _collectionGridElements = new List<CollectionGridViewUIElement>();

            _controller.OnChildAdded += _controller_OnChildAdded;
            _controller.OnChildRemoved += _controller_OnChildRemoved;


        }

        public override void Dispose()
        {
            _controller.OnChildAdded -= _controller_OnChildAdded;
            _controller.OnChildRemoved -= _controller_OnChildRemoved;
            base.Dispose();
        }

        /// <summary>
        /// Called whenever a child is removed from the controller assocaited with this collection
        /// removes the child from the grid view
        /// </summary>
        /// <param name="elementModelId"></param>
        private void _controller_OnChildRemoved(string elementModelId)
        {
            var elementController = SessionController.Instance.ElementModelIdToElementController[elementModelId];
            var gridElementToBeRemoved = _collectionGridElements.FirstOrDefault(item => item.Controller == elementController.LibraryElementController);
            Debug.Assert(gridElementToBeRemoved != null);
            _collectionGridElements.Remove(gridElementToBeRemoved);
            var elementModelToBeRemoved =
                _collectionElementModels.FirstOrDefault(item => item.LibraryId == elementController.LibraryElementId);
            Debug.Assert(elementModelToBeRemoved != null);
            _collectionElementModels.Remove(elementModelToBeRemoved);
        }

        /// <summary>
        /// Called whenever a child is added to the controller associated with this collection
        /// adds the child to the grid view
        /// </summary>
        /// <param name="elementModelId"></param>
        private void _controller_OnChildAdded(string elementModelId)
        {
            var elementController = SessionController.Instance.ElementModelIdToElementController[elementModelId];
            _collectionGridElements.Add(new CollectionGridViewUIElement(this, Canvas, elementController.LibraryElementController));
            _collectionElementModels.Add(elementController.Model);
        }


        /// <summary>
        /// helper method to convert a string to a GridSortOption
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static GridSortOption StringToGridSortOption(string str)
        {
            if (str == GridSortOptionToString(GridSortOption.Creator))
            {
                return GridSortOption.Creator;
            }
            if (str == GridSortOptionToString(GridSortOption.Date))
            {
                return GridSortOption.Date;
            }

            // make sure the passed in string is proper
            if (str != GridSortOptionToString(GridSortOption.Title))
            {
                Debug.Fail($"The passed in string {str} is invalid");
            }
            return GridSortOption.Title;

        }

        /// <summary>
        /// helper method to convert a grid sort option to a string
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private static string GridSortOptionToString(GridSortOption option)
        {
            switch (option)
            {
                case GridSortOption.Title:
                    return "Title";
                case GridSortOption.Date:
                    return "Date";
                case GridSortOption.Creator:
                    return "Creator";
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
        }

        public override async Task Load()
        {
            var request = new GetEntireWorkspaceRequest(_controller.CollectionModel.LibraryElementId, 0);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            if (request.WasSuccessful() == true)
            {
                _collectionElementModels = request.GetReturnedElementModels();
            }

            base.Load();
        }
    }
}
