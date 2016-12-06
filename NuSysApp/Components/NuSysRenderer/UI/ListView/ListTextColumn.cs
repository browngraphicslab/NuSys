﻿using System;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class ListTextColumn<T> : ListColumn<T>
    {
        /// <summary>
        /// This function takes in a generic item, and returns the string to be displayed in the list.
        /// </summary>
        public Func<T, string> ColumnFunction { private get; set; }

        /// <summary>
        /// The horizontal alignment of the text within the column, default is left
        /// </summary>
        public CanvasHorizontalAlignment TextHorizontalAlignment { get; set; }

        /// <summary>
        /// This function will return the cell based on the string outputed by the column function you give
        /// </summary>
        public override RectangleUIElement GetColumnCellFromItem(T itemSource, ListViewRowUIElement<T> listViewRowUIElement,
            ICanvasResourceCreatorWithDpi resourceCreator, float rowHeight, float sumOfAllColumnRelativeWidths)

        {
            var cell = new TextboxUIElement(listViewRowUIElement, resourceCreator);
            cell.Width = (RelativeWidth / sumOfAllColumnRelativeWidths) * listViewRowUIElement.Width;
            cell.BorderWidth = BorderWidth;
            cell.Bordercolor = BorderColor;
            cell.Height = rowHeight;
            cell.Background = Colors.Transparent;
            cell.Text = ColumnFunction(itemSource);
            cell.TextHorizontalAlignment = TextHorizontalAlignment;
            return cell;
        }



        public override void UpdateColumnCellFromItem(T itemSource, RectangleUIElement rectangleUIElement)
        {
            var cell = rectangleUIElement as TextboxUIElement;
            cell.Text = ColumnFunction(itemSource);
        }
    }
}