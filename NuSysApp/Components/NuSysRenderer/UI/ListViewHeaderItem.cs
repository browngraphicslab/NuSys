using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// a single element in the header corresponding to a certain attribute. this will be the handle used to sort, add/remove, and drag
    /// </summary>
    public class ListViewHeaderItem<T> : ButtonUIElement
    {
        /// <summary>
        /// list of the row elements that it will need to access
        /// </summary>
        private List<ListViewRowUIElement<T>> _rowElements;

        /// <summary>
        /// reference to listview container so popups can be brought up
        /// </summary>
        private ListViewUIElementContainer<T> _listview;

        public ListViewUIElementContainer<T> ListviewContainer
        {
            set { _listview = value; }
        }

        /// <summary>
        /// accessor for rowelement list
        /// </summary>
        public List<ListViewRowUIElement<T>> RowElements
        {
            get { return _rowElements; }
            set {
                if (value != null)
                {
                    _rowElements = value;
                }
            }
        }

        /// <summary>
        /// column that this headeritem corresponds to
        /// </summary>
        private ListColumn<T> _column;

        /// <summary>
        /// accessor for column that headeritem corresponds to
        /// </summary>
        public ListColumn<T> Column
        {
            get { return _column; }
            set
            {
                if (value != null)
                {
                    _column = value;
                }
            }
        }

        /// <summary>
        /// shape element that is passed in should always be a rectangle
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="shapeElement"></param>
        public ListViewHeaderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BaseInteractiveUIElement shapeElement) : base(parent, resourceCreator, shapeElement)
        {
            Tapped += ListViewHeaderItem_Tapped;
            Shape.RightPressed += ShapeOnRightPressed;
        }

        private void ShapeOnRightPressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _listview.ShowFlyout(pointer);
        }

        public override void Dispose()
        {
            Tapped -= ListViewHeaderItem_Tapped;
            Shape.RightPressed -= ShapeOnRightPressed;
            base.Dispose();
        }

        /// <summary>
        /// calls sort when button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ListViewHeaderItem_Tapped(ButtonUIElement item, CanvasPointer pointer)
        {
            SortElementsinColumn();
        }

        public void SortElementsinColumn()
        {

        }

    }
}
