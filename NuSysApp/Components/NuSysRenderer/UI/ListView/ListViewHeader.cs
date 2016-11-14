using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the header of a list view
    /// </summary>
    public class ListViewHeader<T> : RectangleUIElement
    {
        /// <summary>
        /// list view from which the header will base its titles
        /// </summary>
        private ListViewUIElement<T> _listview;

        /// <summary>
        /// store resource creator in instance variable so we can pass it to new textboxUIElement later
        /// </summary>
        private ICanvasResourceCreatorWithDpi _resourceCreator;

        public ListViewHeader(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

        }

        /// <summary>
        /// make the titles for all the columns of the header based off of a listview parameter
        /// </summary>
        /// <param name="listview"></param>
        public void MakeTitles(ListViewUIElement<T> listview, ICanvasResourceCreatorWithDpi resourceCreator)
        {
            var indexPointer = 0f;
            _listview = listview;
            foreach (ListColumn<T> c in _listview.ListColumns)
            {
                var headerItem = new ListViewHeaderItem<T>(this, resourceCreator, new RectangleUIElement(this, resourceCreator));
                headerItem.BorderWidth = 2;
                headerItem.Background = Colors.LightGray;
                headerItem.ButtonTextColor = Colors.Black;
                headerItem.ButtonText = c.Title;
                headerItem.ButtonTextSize = 15;
                headerItem.Width = c.RelativeWidth / listview.SumOfColRelWidths * listview.Width;
                headerItem.Height = Height;
                headerItem.Transform.LocalPosition = new Vector2(indexPointer, 0);
                this.AddChild(headerItem);
                indexPointer += headerItem.Width;

                //var title = new TextboxUIElement(this, resourceCreator);
                //title.BorderWidth = 2;
                //title.Background = Colors.LightGray;
                //title.TextColor = Colors.Black;
                //title.FontSize = 15;
                //title.Text = c.Title;
                //title.Width = c.RelativeWidth / listview.SumOfColRelWidths * listview.Width;
                //title.Height = Height;
                //title.Transform.LocalPosition = new Vector2(indexPointer, 0);
                //this.AddChild(title);
                //indexPointer += title.Width;
            }
        }
    }
}
