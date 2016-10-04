using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ListViewRowUIElement<T> : RectangleUIElement
    {
        /// <summary>
        /// The item that this row corresponds to
        /// </summary>
        private T _item;
        public T Item {
            get { return _item; }
        }

        private bool _isSelected;

        /// <summary>
        /// event fired when the row is selected
        /// </summary>
        /// <param name="rowUIElement"></param>
        /// <param name="cell"></param>
        public delegate void SelectedEventHandler(ListViewRowUIElement<T> rowUIElement, RectangleUIElement cell);
        public event SelectedEventHandler Selected;

        /// <summary>
        /// event fired when row is deselected
        /// </summary>
        /// <param name="rowUIElement"></param>
        /// <param name="cell"></param>
        public delegate void DeSelectedEventHandler(ListViewRowUIElement<T> rowUIElement, RectangleUIElement cell);
        public event DeSelectedEventHandler Deselected;



        /// <summary>
        /// These are the cells that will be placed on this row. The order is from left to right.
        /// Index 0 is left most.
        /// </summary>
        private List<RectangleUIElement> _cells;

        public ListViewRowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, T item) : base(parent, resourceCreator)
        {
            _isSelected = false;
            _cells = new List<RectangleUIElement>();
            _item = item;
        }
        /// <summary>
        /// Switches the cells at index1 and index2 in _cells. This will not graphically reload everything.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cell"></param>
        public void SwitchCell(int index1, int index2)
        {
            
        }

        /// <summary>
        /// Adds the cell to the end of the row
        /// </summary>
        /// <param name="cell"></param>
        public void AddCell(RectangleUIElement cell)
        {
            cell.Pressed += Cell_Pressed;
            cell.Released += Cell_Released;
        }

        /// <summary>
        /// This function is called when the cell ui element has been released. 
        /// This method will fire either the selected or deslected event handler that the listview
        /// will be listening to
        /// </summary>
        private void Cell_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            throw new System.NotImplementedException();
        }


        /// <summary>
        /// This function is called when the cell ui element has been pressed.
        /// </summary>
        private void Cell_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Deletes cell at the index
        /// </summary>
        public void DeleteCell(int index)
        {
            var cell = _cells[index];
            cell.Pressed -= Cell_Pressed;
            cell.Released -= Cell_Released;
        }

        /// <summary>
        /// Highlights the row
        /// </summary>
        public override Task Load()
        {
            return base.Load();
        }

        

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
        }

       

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}