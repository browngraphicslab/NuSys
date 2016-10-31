using System;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ListTextColumn<T> : ListColumn<T>
    {
        /// <summary>
        /// This function takes in a generic item, and returns the string to be displayed in the list.
        /// </summary>
        public Func<T, string> ColumnFunction { private get; set; }

        /// <summary>
        /// This function will return the cell based on the string outputed by the column function you give
        /// </summary>
        public override RectangleUIElement GetColumnCellFromItem(T itemSource, ListViewRowUIElement<T> listViewRowUIElement,
            ICanvasResourceCreatorWithDpi resourceCreator, float rowHeight, float sumOfAllColumnRelativeWidths)

        {
            var cell = new TextBoxUIElement(listViewRowUIElement, resourceCreator);
            cell.Width = (RelativeWidth / sumOfAllColumnRelativeWidths) * listViewRowUIElement.Width;
            cell.BorderWidth = BorderWidth;
            cell.Bordercolor = BorderColor;
            cell.Height = rowHeight;
            cell.Background = Colors.Transparent;
            cell.TextBoxText = ColumnFunction(itemSource);
            return cell;
        }
    }
}