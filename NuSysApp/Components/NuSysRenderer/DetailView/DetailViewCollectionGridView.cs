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
        private ScrollableTextboxUIElement _gridSearchBox;

        /// <summary>
        /// A dropdown menu offering different sorting options for the grid view
        /// </summary>
        private DropdownUIElement _gridSortDropDown;     

        public enum GridSortOption { Title, Date, Creator}

        private ScrollingGrid<LibraryElementController> _scrollingGrid;

        public DetailViewCollectionGridView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionLibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            _scrollingGrid = new ScrollingGrid<LibraryElementController>(this, resourceCreator)
            {
                ItemFunction = lec => new CollectionGridViewUIElement(this, resourceCreator, lec),
                Width = Width,
                Height = Height
            };

            _gridSearchBox = new ScrollableTextboxUIElement(this, resourceCreator, false, false)
            {
                //ColumnFunction = elementModel => elementModel.Title,
                //FilterFunction = s => new List<ElementModel>(_collectionElementModels.Where(em => em.Title.Contains(s))),
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

        }


        public override void Dispose()
        {

            _gridSortDropDown.Selected -= _gridSortDropDown_Selected;

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

            Debug.Assert(_collectionElementModels != null);
            foreach (var elementModel in _collectionElementModels)
            {
                _scrollingGrid.AddItem(SessionController.Instance.ContentController.GetLibraryElementController(elementModel.LibraryId));
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
