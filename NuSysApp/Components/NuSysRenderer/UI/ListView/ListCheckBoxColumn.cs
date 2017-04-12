using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class ListCheckBoxColumn<T> : ListColumn<T>
    {

        /// <summary>
        /// This function takes in a generic item, and returns the string to be displayed in the list.
        /// </summary>
        public Func<T, string> ColumnFunction { private get; set; }

        /// <summary>
        /// the position of the label relative to the checkbox, if right the label is to the right of the 
        /// checkbox if left the label is to the left of the checkbox
        /// </summary>
        public CheckBoxUIElement.CheckBoxLabelPosition LabelPosition { get; set; } = UIDefaults.CheckBoxLabelPosition;

        /// <summary>
        /// the horizontal alignment of the label text
        /// </summary>
        public CanvasHorizontalAlignment LabelTextHorizontalAlignment { get; set; } =
            UIDefaults.CheckBoxLabelTextHorizontalAlignmentAlignment;

        /// <summary>
        /// The height of the checkbox itself, not of the entire row or column
        /// </summary>
        public float CheckBoxHeight { get; set; } = UIDefaults.CheckBoxHeight;

        /// <summary>
        /// the width of the checkbox itself, not of the entire row or column
        /// </summary>
        public float CheckBoxWidth { get; set; } = UIDefaults.CheckBoxWidth;

        /// <summary>
        /// The fontsize of the label
        /// </summary>
        public float LabelFontSize { get; set; } = UIDefaults.FontSize;

        /// <summary>
        /// This function will return the cell based on the string outputed by the column function you give
        /// </summary>
        public override RectangleUIElement GetColumnCellFromItem(T itemSource, ListViewRowUIElement<T> listViewRowUIElement,
            ICanvasResourceCreatorWithDpi resourceCreator, float rowHeight, float sumOfAllColumnRelativeWidths)

        {
            var cell = new CheckBoxUIElement(listViewRowUIElement, resourceCreator)
            {
                Width = (RelativeWidth/sumOfAllColumnRelativeWidths)*listViewRowUIElement.Width,
                BorderWidth = BorderWidth,
                BorderColor = BorderColor,
                Height = rowHeight,
                Background = Colors.Transparent,
                LabelText = ColumnFunction(itemSource),
                LabelBackground = Colors.Transparent,
                DisableSelectionOnTap = true
            };
            return cell;
        }

        public override void UpdateColumnCellFromItem(T itemSource, RectangleUIElement rectangleUiElement, bool isSelected)
        {
            var cell = rectangleUiElement as CheckBoxUIElement;
            Debug.Assert(cell != null);
            cell.LabelText = ColumnFunction(itemSource);
            cell.LabelPosition = LabelPosition;
            cell.LabelTextHorizontalAlignment = LabelTextHorizontalAlignment;
            cell.CheckBoxHeight = CheckBoxHeight;
            cell.CheckBoxWidth = CheckBoxWidth;
            cell.LabelFontSize = LabelFontSize;
            cell.SetCheckBoxSelection(isSelected);
        }


    }
}
