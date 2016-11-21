using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class LibraryListUIElement : ResizeableWindowUIElement
    {
        public ListViewUIElementContainer<LibraryElementModel> libraryListView;

        public LibraryListUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
            : base(parent, resourceCreator)
        {

            InitializeLibraryList();
            AddChild(libraryListView);



            // events so that the library list view adds and removes elements dynamically
            SessionController.Instance.ContentController.OnNewLibraryElement += UpdateLibraryListWithNewElement;
            SessionController.Instance.ContentController.OnLibraryElementDelete += UpdateLibraryListToRemoveElement;
        }

        public override void Dispose()
        {

            SessionController.Instance.ContentController.OnNewLibraryElement -= UpdateLibraryListWithNewElement;
            SessionController.Instance.ContentController.OnLibraryElementDelete -= UpdateLibraryListToRemoveElement;
            base.Dispose();
        }

        public void InitializeLibraryList()
        {
            libraryListView = new ListViewUIElementContainer<LibraryElementModel>(this, Canvas);

            var listColumn = new ListTextColumn<LibraryElementModel>();
            listColumn.Title = "Title";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = model => model.Title;

            var listColumn2 = new ListTextColumn<LibraryElementModel>();
            listColumn2.Title = "Creator";
            listColumn2.RelativeWidth = 2;
            listColumn2.ColumnFunction =
                model => SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(model.Creator);

            var listColumn3 = new ListTextColumn<LibraryElementModel>();
            listColumn3.Title = "Last Edited Timestamp";
            listColumn3.RelativeWidth = 3;
            listColumn3.ColumnFunction = model => model.LastEditedTimestamp;

            libraryListView.AddColumns(new List<ListColumn<LibraryElementModel>> { listColumn, listColumn2, listColumn3 });


            libraryListView.AddItems(
                           SessionController.Instance.ContentController.ContentValues.ToList());
        }

        private void UpdateLibraryListToRemoveElement(LibraryElementModel element)
        {
            libraryListView.RemoveItems(new List<LibraryElementModel> {element});
        }

        private void UpdateLibraryListWithNewElement(LibraryElementModel libraryElement)
        {
            libraryListView.AddItems(new List<LibraryElementModel> {libraryElement});
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            libraryListView.Width = Width - 2 * BorderWidth;
            libraryListView.Height = Height - TopBarHeight - BorderWidth;
            libraryListView.Transform.LocalPosition = new Vector2(BorderWidth, TopBarHeight);
            base.Update(parentLocalToScreenTransform);
        }
    }
}