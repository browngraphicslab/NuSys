using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using System.Diagnostics;

namespace NuSysApp
{
    /// <summary>
    /// PieChartUIElement is a piechart for Tools.
    /// It keeps track of a list of PieChartElements, which it draws proportional to their 'value' property.
    /// Has dragging, clicking, etc.
    /// </summary>
    public class PieChartUIElement : RectangleUIElement
    {
        #region public properties
        /// <summary>
        /// Padding between the actual circle and the border of this RectangleUIElement
        /// </summary>
        public float Padding { set; get; }

        /// <summary>
        /// Font size of the labels
        /// </summary>
        public float FontSize { set; get; }

        /// <summary>
        /// Palette is the list of colors that make up the pieces of the pie. 
        /// </summary>
        public List<Color> Palette { set; get; }
        /// <summary>
        /// You can select multiple items from the pie if this is set to true.
        /// True by default
        /// </summary>
        public bool MultiSelect { get; set; }
        /// <summary>
        /// Pieces of the pie are selectable by clicking if this is set to true
        /// </summary>
        public bool DisableSelectionByClick { set; get; }
        #endregion public properties+

        #region events
        /// <summary>
        /// Called when element is selected (ie, you click on a piece of the pie). Passes in the PieChartElement, which contains the string item and its value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="element"></param>
        public delegate void PieChartElementSelectedEventHandler(object source, PieChartElement<string> element);
        public event PieChartElementSelectedEventHandler ElementSelected;

        /// <summary>
        /// Called when element is deselected (ie, you click on a piece of the pie that is selected or multiselect is on and you select another item).
        /// </summary>
        /// <param name="source"></param>
        /// <param name="element"></param>
        public delegate void PieChartElementDeselectedEventHandler(object source, PieChartElement<string> element);
        public event PieChartElementDeselectedEventHandler ElementDeselected;
        /// <summary>
        /// Element is the PieChartElement that represents the piece of the pie being dragged.
        /// Pointer is the location of the pointer
        /// </summary>
        /// <param name="source"></param>
        /// <param name="element"></param>
        /// <param name="pointer"></param>
        public delegate void PieChartElementDraggedEventHandler(object source, PieChartElement<string> element, CanvasPointer pointer);
        public event PieChartElementDraggedEventHandler ElementDragged;

        /// <summary>
        /// Called when element is released after being dragged. The CanvasPointer passed in is the pointer where the user released, not pressed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="element"></param>
        /// <param name="pointer"></param>
        public delegate void PieChartElementDragCompletedEventHandler(object source, PieChartElement<string> element, CanvasPointer pointer);
        public event PieChartElementDragCompletedEventHandler ElementDragCompleted;
        #endregion events

        #region private variables
        /// <summary>
        /// List of PieChartElements. It is private -- only way to populate it is with the AddElement method.
        /// </summary>
        private List<PieChartElement<string>> _elements;
        /// <summary>
        /// Set of selected elements. If multiselect is on, this can hold multiple elements. Otherwise, should only have one or 0.
        /// </summary>
        private HashSet<PieChartElement<string>> _selectedElements;
        /// <summary>
        /// Boolean used to check if you were dragging when the pointer was released
        /// </summary>
        private bool _isDragging;
        /// <summary>
        /// The element that was stored in the first call to the DraggedEventHandler, so that when we invoke PointerReleased, we have reference to the relevant element.
        /// </summary>
        private PieChartElement<String> _draggedElement;
        #endregion private variables

        public PieChartUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Background = Colors.Transparent;
            Padding = 50;
            FontSize = 18;

            MultiSelect = true;
            DisableSelectionByClick = false;
            Palette = new List<Color>(new[] { Colors.DarkSalmon, Colors.Azure, Colors.LemonChiffon, Colors.Honeydew, Colors.Pink });

            _elements = new List<PieChartElement<string>>();
            _selectedElements = new HashSet<PieChartElement<string>>();

            //These elements are added in as an example. Remove the following lines as needed.
            AddElement("A", 5);
            AddElement("B", 3);
            AddElement("C", 12);
            AddElement("D", 7);

            Released += PieChartUIElement_Released;
            Dragged += PieChartUIElement_Dragged;
        }
        /// <summary>
        /// Creates an element and adds it to the list of elements.
        /// Takes in the item and value 
        /// 
        /// Example of a pie chart with two elements:
        /// AddElement("Democrat", 51);
        /// AddElement("Republican", 49);
        /// 
        /// The value is not a percentage, but the actual number of occurances
        /// </summary>
        /// <param name="item"></param>
        /// <param name="value"></param>
        public void AddElement(string item, int value)
        {
            var element = new PieChartElement<string>();
            element.Item = item;
            element.Value = value;
            _elements?.Add(element);

        }
        /// <summary>
        /// Because we override HitTest to only hit the PieChart inside the circle, this only gets called when you drag from the pie,itself.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void PieChartUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _isDragging = true;

            // If this is the first call to Dragged ever or since Released, _draggedElement is null
            if (_draggedElement == null)
            {
                var point = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
                //Store the element that represents the piece of the pie being dragged.
                _draggedElement = GetElementFromAngle(GetAngleFromPoint(point));
            }

            ElementDragged?.Invoke(this, _draggedElement, pointer);

        }
        /// <summary>
        /// Because we override HitTest to only hit the PieChart inside the circle, this only gets called when you release your pointer within the circle.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void PieChartUIElement_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // If we were dragging when we released, we should invoke the ElementDragCompleted event.
            if (_isDragging)
            {
                if(_draggedElement != null)
                {
                    ElementDragCompleted?.Invoke(this, _draggedElement, pointer);
                }
                _isDragging = false;
                _draggedElement = null;
            }

            //If we were not dragging when we released, but simply pressing, we should Select or Deselect based on settings. 
            else
            {
                var point = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
                var element = GetElementFromAngle(GetAngleFromPoint(point));
                if (element == null)
                {
                    return;
                }
                if (_selectedElements.Contains(element))
                {
                    if (!DisableSelectionByClick)
                    {
                        DeselectElement(element);
                    }
                }
                else
                {
                    if (!DisableSelectionByClick)
                    {
                        SelectElement(element);
                    }
                }


            }

        }
        /// <summary>
        /// Selects the element and invokes ElementSelected
        /// If MultiSelect is off, we deselect everything else first.
        /// </summary>
        /// <param name="element"></param>
        private void SelectElement(PieChartElement<string> element)
        {
            if (element == null)
            {
                Debug.Write("Trying to select a null element idiot");
                return;
            }

     
            if (MultiSelect == false)
            {
                //If multiselect is off, deselect every element first
                foreach (var e in _selectedElements)
                {
                    DeselectElement(e);
                }
            }
            _selectedElements.Add(element);
            ElementSelected?.Invoke(this, element);

        }
        /// <summary>
        /// Deselects the element and invokes ElementSelected
        /// </summary>
        /// <param name="element"></param>
        private void DeselectElement(PieChartElement<string> element)
        {

            if (element == null)
            {
                Debug.Write("Trying to deselect a null element idiot");
                return;
            }

            if (_selectedElements.Contains(element))
            {
                _selectedElements.Remove(element);

            }

            ElementDeselected?.Invoke(this, element);

            
        }
        /// <summary>
        /// Finds the element where the angle lies by iterating through the elements.
        /// </summary>
        /// <param name="theta"></param>
        /// <returns></returns>
        private PieChartElement<string> GetElementFromAngle(double theta)
        {
            for(int i = 0; i < _elements.Count; i++)
            {
                if (_elements[i].IsHit(theta))
                {
                    return _elements[i];
                }
            }
            return null;
        }
        /// <summary>
        /// Gets the point on the screen, finds the equivalent point relative to the center of the pie graph.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private double GetAngleFromPoint(Vector2 point)
        {
            var w = Width;
            var h = Height;

            //Center of the circle
            var midx = w / 2;
            var midy = h / 2;

            //r = radius
            var r = Math.Min(w, h) / 2 - Padding;

            var y = midy - point.Y;
            var x = point.X - midx;

            /* We get theta from Math.Atan2(x,y) as opposed to Math.Atan2(y,x) 
             * This is because we are going from the polar coordinate representation
             * of the angle to the weird representation used by the DrawPie method.
             * (For example, what would be 0 in polar coordiantes is PI/2 here,
             * what would be PI/2 in polar coordinates is 0 here.)
             * 
             */ 

            var theta = Math.Atan2(x,y);
            
            if (theta < 0)
            {
                //Theta should be from 0 to 2PI.
                theta = 2 * Math.PI + theta;
            }
            return theta;
        } 

        /// <summary>
        /// DrawPie draws the pieces of the pie and the selection outline if necessary for each element.
        /// </summary>
        /// <param name="ds"></param>
        private void DrawPie(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            var total = _elements.Sum(element => element.Value);

            var w = Width;
            var h = Height;
            var midx = w / 2;
            var midy = h / 2;
            var padding = Padding;
            var lineOffset = 20;
            var r = Math.Min(w, h) / 2 - padding;


            var center = new Vector2(midx, midy);
            //This for loop draws the pieces of the pie
            for (int i = 0; i < _elements.Count; i++)
            {
                float sweepAngle = _elements[i].SweepAngle;
                var arcStartPoint = new Vector2((float)(midx + r * Math.Sin(_elements[i].StartAngle)), (float)(midy - r * Math.Cos(_elements[i].StartAngle)));

                using (var cpb = new CanvasPathBuilder(ds))
                {
                    cpb.BeginFigure(center);
                    cpb.AddLine(arcStartPoint);
                    cpb.AddArc(new Vector2(midx, midy), r, r, _elements[i].StartAngle - (float)(Math.PI / 2), sweepAngle);
                    cpb.EndFigure(CanvasFigureLoop.Closed);
                    ds.FillGeometry(CanvasGeometry.CreatePath(cpb), Palette[i % Palette.Count]);

                    
                }




            }


            var lineBrush = new CanvasSolidColorBrush(ds, Colors.Black);
            var selectionBrush = new CanvasSolidColorBrush(ds, Colors.Black);
            //This for loop draws the line and the text for each piece of the pie
            for (int i = 0; i < _elements.Count; i++)
            {
                float sweepAngle = _elements[i].SweepAngle;
                var midAngle = _elements[i].StartAngle + sweepAngle / 2;
                var isRightHalf = midAngle < Math.PI;
                var isTopHalf = midAngle <= Math.PI / 2 || midAngle >= Math.PI * 3 / 2;

                var p0 = new Vector2((float)(midx + (r - lineOffset) * Math.Sin(midAngle)), (float)(midy - (r - lineOffset) * Math.Cos(midAngle)));
                var p1 = new Vector2((float)(midx + (r + lineOffset) * Math.Sin(midAngle)), (float)(midy - (r + lineOffset) * Math.Cos(midAngle)));
                var p2 = isRightHalf ? new Vector2(p1.X + 50, p1.Y) : new Vector2(p1.X - 50, p1.Y);

                using (var cpb = new CanvasPathBuilder(ds))
                {
                    cpb.BeginFigure(p0);
                    cpb.AddLine(p1);
                    cpb.AddLine(p2);
                    cpb.EndFigure(CanvasFigureLoop.Open);

                    ds.DrawGeometry(
                        CanvasGeometry.CreatePath(cpb),
                        lineBrush,
                        1);
                }
                
                var point = new Vector2((float)(midx + (r/4) * Math.Sin(midAngle)), (float)(midy - (r/4) * Math.Cos(midAngle)));

                var text = _elements[i].Item + " " + _elements[i].Value.ToString();
                ds.DrawText(
                    text,
                    p1,
                    Colors.Black,
                    new CanvasTextFormat
                    {
                        HorizontalAlignment = isRightHalf ? CanvasHorizontalAlignment.Left : CanvasHorizontalAlignment.Right,
                        VerticalAlignment = isTopHalf ? CanvasVerticalAlignment.Bottom : CanvasVerticalAlignment.Top,
                        FontSize = FontSize
                    });

                if (_selectedElements.Contains(_elements[i]))
                {
                    var arcStartPoint = new Vector2((float)(midx + r * Math.Sin(_elements[i].StartAngle)), (float)(midy - r * Math.Cos(_elements[i].StartAngle)));

                    using (var cpb = new CanvasPathBuilder(ds))
                    {
                        cpb.BeginFigure(center);
                        cpb.AddLine(arcStartPoint);
                        cpb.AddArc(new Vector2(midx, midy), r, r, _elements[i].StartAngle - (float)(Math.PI / 2), _elements[i].SweepAngle);
                        cpb.EndFigure(CanvasFigureLoop.Closed);
                        ds.DrawGeometry(CanvasGeometry.CreatePath(cpb), selectionBrush, 3);

                    }

                }
            }

            ds.Transform = orgTransform;

        }
        /// <summary>
        /// HitTest is overriden so that we hit PieChartUIElement iff the point is within the pie chart.
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            var w = Width;
            var h = Height;
            //Center of circle
            var midx = w / 2;
            var midy = h / 2;


            var r = Math.Min(w, h) / 2 - Padding;

            var point = Vector2.Transform(screenPoint, Transform.ScreenToLocalMatrix);
            var differenceInX = point.X - midx;
            var differenceInY = point.Y - midy;
            //Distance formula
            var distanceFromOrigin = Math.Sqrt(differenceInX * differenceInX + differenceInY * differenceInY);
            //If distance from origin <= radius, you have hit the inside of the circle
            if(distanceFromOrigin > r)
            {
                return null;
            }
            return base.HitTest(screenPoint);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            DrawPie(ds);
            base.Draw(ds);
        }
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            var total = _elements.Sum(element => element.Value);
            float angle = 0f;
            for (int i = 0; i < _elements.Count; i++)
            {
                //Sweep angle is the angle of the piece of the pie. It is the element's 'value' property relative to all of the values of the elements.
                float sweepAngle = (float)(2 * Math.PI * _elements[i].Value / total);
                
                _elements[i].SweepAngle = sweepAngle;
                _elements[i].StartAngle = angle;
                angle += sweepAngle;
                _elements[i].EndAngle = angle;

            }
            base.Update(parentLocalToScreenTransform);
        }

        public override void Dispose()
        {
            base.Dispose();
        }


    }
}
