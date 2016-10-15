﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;
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
            set { _item = value; }
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
        //private List<RectangleUIElement> _cells;

        public ListViewRowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, T item) : base(parent, resourceCreator)
        {
            _isSelected = false;
            //_cells = new List<RectangleUIElement>();
            _item = item;
        }
        /// <summary>
        /// Switches the cells at index1 and index2 in _cells. This will not graphically reload everything.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="cell"></param>
        public void SwapCell(int index1, int index2)
        {
            if (index1 < 0 || index1 > _children.Count || index2 < 0 || index2 > _children.Count)
            {
                Debug.Write("You are trying to swap the cells of a listview row ui element but one of your indices is out of bounds you idiot");
                return;
            }
            var tmpCell = _children[index1];
            _children[index1] = _children[index2];
            _children[index2] = tmpCell;
        }

        /// <summary>
        /// Adds the cell to the end of the row
        /// </summary>
        /// <param name="cell"></param>
        public void AddCell(RectangleUIElement cell)
        {
            if (cell == null)
            {
                Debug.Write("Your trying to add a null cell to a listviewrowuielement you idiot");
                return;
            }
            //_cells.Add(cell);
            _children.Add(cell);
            cell.Pressed += Cell_Pressed;
            cell.Released += Cell_Released;
        }

        /// <summary>
        /// This simply changes the background color of the row to the selected color, and 
        /// sets the isSelected bool to true. 
        /// </summary>
        public void Select(InteractiveBaseRenderItem cell = null)
        {
            Background = Colors.CadetBlue;
            _isSelected = true;
        }

        /// <summary>
        /// This simply changes the bacakground color of the row to the deselected color, and sets the 
        /// is Selected bool to true
        /// </summary>
        public void Deselect(InteractiveBaseRenderItem cell = null)
        {
            Background = Colors.White;
            _isSelected = false;
        }

        /// <summary>
        /// This function is called when the cell ui element has been released. 
        /// This method will fire either the selected or deslected event handler that the listview
        /// will be listening to
        /// </summary>
        private void Cell_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_isSelected == true)
            {
                Deselected?.Invoke(this, item as RectangleUIElement);
            }
            else
            {
                Selected?.Invoke(this, item as RectangleUIElement);
            }
        }


        /// <summary>
        /// This function is called when the cell ui element has been pressed.
        /// </summary>
        private void Cell_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            
        }

        /// <summary>
        /// Deletes cell at the index
        /// </summary>
        public void DeleteCell(int index)
        {
            if (index < 0 || index > _children.Count)
            {
                Debug.Write("Your trying to delete a cell at an out of bounds index you idiot");
            }
            var cell = _children[index] as RectangleUIElement;
            Debug.Assert(cell != null);
            cell.Pressed -= Cell_Pressed;
            cell.Released -= Cell_Released;
            _children.RemoveAt(index);
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

            var cellHorizontalOffset = BorderWidth;

            foreach (var child in _children)
            {
                var cell = child as RectangleUIElement;
                Debug.Assert(cell != null);
                cell.Transform.LocalPosition = new Vector2(cellHorizontalOffset, BorderWidth);
                cellHorizontalOffset += cell.Width;
            }

            base.Draw(ds);
        }

       

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}