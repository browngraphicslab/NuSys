using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{

    public abstract class ListColumn<T>
    {
        /// <summary>
        /// This function takes in a generic item, and returns the rectangle UI element
        /// that will be placed in the list. The height will always fill the row completely. The width will be set based on the
        /// width parameter of the list column. The opacity will also be overwritten.
        /// </summary>
        public virtual Func<T, object> ColumnFunction { private get; set; }

        /// <summary>
        /// Title of the column
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// This width will override whatever width the rectangle UI element had in the column function passed above. This is relative to all other column widths in the list.
        /// E.G. If col1 relativewidths is 1, and col2 is 2, and the total list width is 300, then col1 width will be 100 wide, and col will be 300 wide
        /// </summary>
        public float RelativeWidth { get; set; }

        /// <summary>
        /// This is the width of the border of the cells in this column
        /// </summary>
        public float BorderWidth { get; set; }

        /// <summary>
        /// This is the color of the border of the cells in this column
        /// </summary>
        public Color BorderColor { get; set; }

        public abstract void UpdateColumnCellFromItem(T item, RectangleUIElement rectangleUIElement);

        /// <summary>
        /// This function will return the cell based on the column function you you give. It will first run the column function,
        /// then set the width, heigh, and transparancy appropriately. Use this function when creating the cell to place in the row.You must pass in the total sum of all 
        /// column relativewidths.
        /// </summary>
        /// <param name="itemSource"></param>
        /// <param name="baseRenderItem"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="RowHeight"></param>
        /// <returns></returns>
        public abstract RectangleUIElement GetColumnCellFromItem(T itemSource,
            ListViewRowUIElement<T> listViewRowUIElement,
            ICanvasResourceCreatorWithDpi resourceCreator, float rowHeight, float sumOfAllColumnRelativeWidths);
    }
}