using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using System;

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

        public bool IsSelected
        {
            get { return _isSelected; }
        }

        public delegate void PointerReleasedEventHandler(ListViewRowUIElement<T> rowUIElement, int colIndex, CanvasPointer pointer, T item);
        public event PointerReleasedEventHandler RowPointerReleased;

        public delegate void DraggedEventHandler(
            ListViewRowUIElement<T> rowUIElement, int colIndex, CanvasPointer pointer);

        public event DraggedEventHandler RowDragged;

        public delegate void PointerWheelChangedEventHandler(ListViewRowUIElement<T> rowUIElement, RectangleUIElement cell, CanvasPointer pointer, float delta);
        public event PointerWheelChangedEventHandler PointerWheelChanged;




        public ListViewRowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, T item) : base(parent, resourceCreator)
        {
            _isSelected = false;
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
            _children.Add(cell);
            cell.Pressed += Cell_Pressed;
            cell.Released += Cell_Released;
            cell.Dragged += Cell_Dragged;
            cell.PointerWheelChanged += Cell_PointerWheelChanged;
        }
        /// <summary>
        /// Invokes PointerWheelChanged, which is listened to the ListViewUIElement, which then updates the position of the scrollbar.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        /// <param name="delta"></param>
        private void Cell_PointerWheelChanged(InteractiveBaseRenderItem item, CanvasPointer pointer, float delta)
        {
            var cell = item as RectangleUIElement;
            Debug.Assert(cell != null);
            PointerWheelChanged?.Invoke(this, cell, pointer, delta);
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
        /// This method is called when the cell ui element has been released. 
        /// This method will fire either the selected or deslected event handler that the listview
        /// will be listening to
        /// </summary>
        private void Cell_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {

            RowPointerReleased?.Invoke(this, _children.IndexOf(item), pointer, Item);

        }

        /// <summary>
        /// This method is called when the cell ui element has been dragged. 
        /// This method will fire either the dragged event handler that the listview
        /// will be listening to
        /// </summary>
        private void Cell_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var cell = item as RectangleUIElement;
            Debug.Assert(cell != null);
            RowDragged?.Invoke(this, _children.IndexOf(cell), pointer);
        }

        /// <summary>
        /// This returns the column index of the cell passed in.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public int GetColumnIndex(RectangleUIElement cell)
        {
            return _children.IndexOf(cell);
        }


        /// <summary>
        /// This method is called when the cell ui element has been pressed.
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
                return;
            }
            var cell = _children[index] as RectangleUIElement;
            Debug.Assert(cell != null);
            RemoveHandlers(cell);
            _children.RemoveAt(index);
        }

        /// <summary>
        /// This removes all the handlers for the cell
        /// </summary>
        /// <param name="cell"></param>
        private void RemoveHandlers(RectangleUIElement cell)
        {
            if (cell == null)
            {
                return;
            }
            cell.Pressed -= Cell_Pressed;
            cell.Released -= Cell_Released;
            cell.Dragged -= Cell_Dragged;
            cell.PointerWheelChanged -= Cell_PointerWheelChanged;
        }


        /// <summary>
        /// This method adds the sizeChange to the width of cell at leftColIndex, and subtracts sizeChange from cell at (leftColIndex + 1) width and adds sizeChanged to the position of the (leftColIndex + 1) cell
        /// </summary>
        /// <param name="leftColIndex"></param>
        /// <param name="rightColIndex"></param>
        /// <param name="distanceToMove"></param>
        public void MoveBorderAfterCell(int leftColIndex, float sizeChange)
        {
            Debug.Assert(leftColIndex < _children.Count - 1);
            var left = _children[leftColIndex] as RectangleUIElement;
            var right = _children[leftColIndex + 1] as RectangleUIElement;
            Debug.Assert(left != null && right != null);
            left.Width += sizeChange;
            right.Width -= sizeChange;
            right.Transform.LocalX += sizeChange;
        }

        /// <summary>
        /// This sets the width of the cell at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="width"></param>
        public void SetCellWidth(int index, float width)
        {
            if (index < 0 || index >= _children.Count)
            {
                Debug.WriteLine("Pass in a non negative or in bound index you idiot.");
                return;
            }
            var cell = _children[index] as RectangleUIElement;
            Debug.Assert(cell != null);
            cell.Width = width;
        }

        /// <summary>
        /// Removes all cells in this row. It just clears the children list
        /// </summary>
        public void RemoveAllCells()
        {
            foreach (var cell in GetChildren())
            {
                RemoveHandlers(cell as RectangleUIElement);
            }
            ClearChildren();
        }

        /// <summary>
        /// If the column is a text column, then this method will return the string held in that cell. If the cell is not a textbox ui element it just returns null.
        ///  This is used for sorting
        /// </summary>
        /// <returns></returns>
        public string GetStringValueOfCell(int colIndex)
        {
            Debug.Assert(colIndex < _children.Count);
            var textCell = _children[colIndex] as TextboxUIElement;
            if (textCell == null)
            {
                return null;
            }
            else
            {
                return textCell.Text;
            }
        }
        /// <summary>
        /// Draw sets position of each cell in the row, then calls base.draw()
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {

            var cellHorizontalOffset = BorderWidth;
            foreach (var child in GetChildren())
            {
                if(child == null)
                {
                    continue;
                }
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


        /// <summary>
        /// Updates the content of the child in the given index of this row
        /// (e.g., the text of the textbox).
        /// </summary>
        /// <param name="column"></param>
        /// <param name="index"></param>
        public void UpdateContent(ListColumn<T> column, int index)
        {
            column.UpdateColumnCellFromItem(Item, _children[index] as RectangleUIElement);
        }
    }
}