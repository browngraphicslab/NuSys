using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class BaseToolListInnerView : BaseToolInnerView
    {
        private ListViewUIElementContainer<LibraryElementModel> listView;
        public BaseToolListInnerView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Background = Colors.Green;
            listView = new ListViewUIElementContainer<LibraryElementModel>(this, ResourceCreator);

            listView.RowBorderThickness = 1;

            var listColumn = new ListTextColumn<LibraryElementModel>();
            listColumn.Title = "Title";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = model => model.Title;

            var listColumn2 = new ListTextColumn<LibraryElementModel>();
            listColumn2.Title = "Creator";
            listColumn2.RelativeWidth = 2;
            listColumn2.ColumnFunction = model => SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(model.Creator);

            var listColumn3 = new ListTextColumn<LibraryElementModel>();
            listColumn3.Title = "Last Edited Timestamp";
            listColumn3.RelativeWidth = 3;
            listColumn3.ColumnFunction = model => model.LastEditedTimestamp;

            listView.AddColumns(new List<ListColumn<LibraryElementModel>>() { listColumn, listColumn2, listColumn3 });


            listView.AddItems(SessionController.Instance.ContentController.ContentValues.ToList());
            AddChild(listView);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            listView.Width = this.Width;
            listView.Height = this.Height;
            base.Update(parentLocalToScreenTransform);
            
        }

        public override void SetProperties(List<string> propertiesList)
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public override void SetVisualSelection(HashSet<string> itemsToSelect)
        {
            throw new System.NotImplementedException();
        }
    }
}