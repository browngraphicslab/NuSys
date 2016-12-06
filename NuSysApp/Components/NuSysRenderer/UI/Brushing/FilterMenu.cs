using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class FilterMenu : ResizeableWindowUIElement
    {
        private ListViewUIElementContainer<LibraryElementModel> _filterList;


        public FilterMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set default ui values
            //TopBarHeight = 0;
            //Height = 500;
            //Width = 300;
            //MinWidth = 300;
            //MinHeight = 300;
            //BorderWidth = 3;
            //Bordercolor = Colors.Black;
            IsDraggable = false;

            InitializeFilterList();
            AddChild(_filterList);
        }

        /// <summary>
        /// Initialize the UI for the library list 
        /// </summary>
        public void InitializeFilterList()
        {
            _filterList = new ListViewUIElementContainer<LibraryElementModel>(this, Canvas)
            {
                DisableSelectionByClick = true
            };

            var listColumn = new ListCheckBoxColumn<LibraryElementModel>();
            listColumn.Title = "Title";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = model => model.Title;

            var listColumn2 = new ListCheckBoxColumn<LibraryElementModel>();
            listColumn2.Title = "Creator";
            listColumn2.RelativeWidth = 2;
            listColumn2.ColumnFunction =
                model => SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(model.Creator);

            var listColumn3 = new ListCheckBoxColumn<LibraryElementModel>();
            listColumn3.Title = "Last Edited Timestamp";
            listColumn3.RelativeWidth = 3;
            listColumn3.ColumnFunction = model => model.LastEditedTimestamp;

            _filterList.AddColumns(new List<ListColumn<LibraryElementModel>> { listColumn, listColumn2, listColumn3 });


            _filterList.AddItems(
                           SessionController.Instance.ContentController.ContentValues.ToList());

            BorderWidth = 5;
            Bordercolor = Colors.Black;
            TopBarColor = Colors.Azure;
            Height = 400;
            Width = 400;
            MinWidth = 400;
            MinHeight = 400;


        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // make the library fill the resizeable window leaving room for the search bar and filter button
            _filterList.Width = Width - 2 * BorderWidth;
            _filterList.Height = Height - TopBarHeight - BorderWidth;
            _filterList.Transform.LocalPosition = new Vector2(BorderWidth, TopBarHeight);

            base.Update(parentLocalToScreenTransform);
        }
    }
}
