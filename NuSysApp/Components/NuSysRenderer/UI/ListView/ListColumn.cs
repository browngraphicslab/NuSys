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

    public class ListColumn<T>
    {
        /// <summary>
        /// This function takes in a generic item, and returns the rectangle UI element
        /// that will be placed in the list. The height will always fill the row completely. The width will be set based on the
        /// width parameter of the list column. The opacity will also be overwritten.
        /// </summary>
        public Func<T, BaseRenderItem, ICanvasResourceCreatorWithDpi, RectangleUIElement> ColumnFunction { private get; set; }

        /// <summary>
        /// Title of the column
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// This width will override whatever width the rectangle UI element had in the column function passed above
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// This function will return the cell based on the column function you you give. It will first run the column function,
        /// then set the width, heigh, and transparancy appropriately. Use this function when creating the cell to place in the row.
        /// You can also override the cell width (for example if you have to make the last cell larger to fill the entire row), but by default
        /// the Width instance variable will be used.
        /// </summary>
        /// <param name="itemSource"></param>
        /// <param name="baseRenderItem"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="RowHeight"></param>
        /// <returns></returns>
        public RectangleUIElement GetColumnCellFromItem(T itemSource, BaseRenderItem baseRenderItem,
            ICanvasResourceCreatorWithDpi resourceCreator, float RowHeight, float cellWidthOverride = -1)
        {
            var cell = ColumnFunction(itemSource, baseRenderItem, resourceCreator);
            if (cellWidthOverride < 0)
            {
                cell.Width = Width;
            }
            else
            {
                cell.Width = cellWidthOverride;
            }
            cell.Height = RowHeight;
            cell.Background = Colors.Transparent;
            return cell;
        }
    }
}