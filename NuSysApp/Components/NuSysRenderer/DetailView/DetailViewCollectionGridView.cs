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
        /// Search box used to filter the elements currently in the grid
        /// </summary>
        private AutoSuggestTextBox<ElementModel> _gridSearchBox;

        /// <summary>
        /// A dropdown menu offering different sorting options for the grid view
        /// </summary>
        private DropdownUIElement _gridSortDropDown;     

        public enum GridSortOption { Title, Date, Creator}

        private ScrollingGrid<ElementModel> _scrollingGrid;

        public DetailViewCollectionGridView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionLibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            _scrollingGrid = new ScrollingGrid<ElementModel>(this, resourceCreator)
            {
                ItemFunction = em => SessionController.Instance.ContentController.GetLibraryElementController(em.LibraryId) == null ? null : new CollectionGridViewUIElement(this, resourceCreator, SessionController.Instance.ContentController.GetLibraryElementController(em.LibraryId)),
                ColumnWidth = 100,
                Resize = ScrollingGrid<ElementModel>.Resizing.Relative,
                RowHeight = 1,
                Width = Width,
                Height = Height,
                ItemRelativeWidth = .9f,
                ItemRelativeHeight = .9f,
                ItemHorizontalAlignment = HorizontalAlignment.Center,
                ItemVerticalAlignment = VerticalAlignment.Center,
                Background = Colors.White
            };
            AddChild(_scrollingGrid);

            _gridSearchBox = new AutoSuggestTextBox<ElementModel>(this, Canvas)
            {
                Height = 30,
                ColumnFunction = elementModel => elementModel.Title,
                FilterFunction = str => new List<ElementModel>(_collectionElementModels.Where(elementModel => elementModel.Title.Contains(str)))
            };
            //AddChild(_gridSearchBox);

            _gridSortDropDown = new DropdownUIElement(this, resourceCreator)
            {
                Width = 100,
                Height = 30,
                ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center,
                ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                Prompt = "Sort By..."
            };
            //AddChild(_gridSortDropDown);

            _gridSortDropDown.AddOptionRange(new List<string>
            {
                string.Empty,
                GridSortOptionToString(GridSortOption.Creator),
                GridSortOptionToString(GridSortOption.Date),
                GridSortOptionToString(GridSortOption.Title)
            });

            _gridSortDropDown.Selected += _gridSortDropDown_Selected;
            _controller.OnChildAdded += Controller_OnChildAdded;
            _controller.OnChildRemoved += Controller_OnChildRemoved;
            SessionController.Instance.EnterNewCollectionStarting += Instance_EnterNewCollectionStarting;
        }

        /// <summary>
        /// Called whenever we switch collections clears all the current children
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Instance_EnterNewCollectionStarting(object sender, string e)
        {
            foreach (var element in _collectionElementModels.ToArray())
            {
                _scrollingGrid.RemoveItem(element);
                _collectionElementModels.Remove(element);
            }
        }

        private void Controller_OnChildRemoved(string id)
        {
            Debug.Assert(SessionController.Instance.ElementModelIdToElementController.ContainsKey(id));
            var elementModel = SessionController.Instance.ElementModelIdToElementController[id].Model;
            _scrollingGrid.RemoveItem(elementModel);
            _collectionElementModels.Remove(elementModel);

        }

        private void Controller_OnChildAdded(string id)
        {
            Debug.Assert(SessionController.Instance.ElementModelIdToElementController.ContainsKey(id));
            var elementModel = SessionController.Instance.ElementModelIdToElementController[id].Model;
            if (elementModel.ParentCollectionId == _controller.LibraryElementModel.LibraryElementId)
            {
                _collectionElementModels.Add(elementModel);
                _scrollingGrid.AddItem(elementModel);
            }
        }

        public override void Dispose()
        {

            _gridSortDropDown.Selected -= _gridSortDropDown_Selected;
            _controller.OnChildAdded -= Controller_OnChildAdded;
            _controller.OnChildRemoved -= Controller_OnChildRemoved;
            SessionController.Instance.EnterNewCollectionStarting -= Instance_EnterNewCollectionStarting;

            base.Dispose();
        }

        private void _gridSortDropDown_Selected(DropdownUIElement sender, string item)
        {
            SortGridBy(StringToGridSortOption(item));
        }

        private void SortGridBy(GridSortOption sortOption)
        {
            throw new NotImplementedException();
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
                Debug.Assert(false,$"The passed in string {str} is invalid");
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
            if (_collectionElementModels != null)
            {
                // remove all current elements, we're essentially going to rrecreate the grid each time
                foreach (var element in _collectionElementModels.ToArray())
                {
                    _scrollingGrid.RemoveItem(element);
                    _collectionElementModels.Remove(element);
                }
            }


            // get all the elements on the workspace using a request, this returns elements to a certain level of 
            // collection recursion, but we only want the top level
            var request = new GetEntireWorkspaceRequest(_controller.CollectionModel.LibraryElementId, 0);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

            // if the request was succesful
            if (request.WasSuccessful() == true)
            {
                // take only those element's who are a direct child of the current collection
                _collectionElementModels = request.GetReturnedElementModels().Where(em => em.ParentCollectionId == _controller.LibraryElementModel.LibraryElementId).ToList();
            }

            // add all the items which made it past the filter to the grid
            Debug.Assert(_collectionElementModels != null);
            foreach (var elementModel in _collectionElementModels)
            {
                _scrollingGrid.AddItem(elementModel);
            }

            base.Load();
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
            {
                return;
            }
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            base.Draw(ds);
            ds.Transform = orgTransform;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _scrollingGrid.Width = Width;
            _scrollingGrid.Height = Height;
            base.Update(parentLocalToScreenTransform);
        }






    }
}
