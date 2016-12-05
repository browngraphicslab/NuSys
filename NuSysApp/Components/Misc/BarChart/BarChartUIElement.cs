using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using System.Diagnostics;
using System.Numerics;

namespace NuSysApp
{
    public class BarChartUIElement : RectangleUIElement
    {
        public List<Color> Palette { set; get; }

        public float InnerPadding { set; get; }
        public string Title { set; get; }


        public delegate void BarChartElementSelectedEventHandler(object source, BarChartElement element);
        public event BarChartElementSelectedEventHandler BarSelected;

        public delegate void BarChartElementDeselectedEventHandler(object source, BarChartElement element);
        public event BarChartElementDeselectedEventHandler BarDeselected;

        public delegate void BarChartElementDraggedEventHandler(BarChartElement bar, CanvasPointer pointer);
        public event BarChartElementDraggedEventHandler BarDragged;


        public delegate void BarChartElementDragCompletedEventHandler(object source, BarChartElement element, CanvasPointer pointer);
        public event BarChartElementDragCompletedEventHandler BarDragCompleted;

        public delegate void BarChartElementTappedEventHandler(BarChartElement bar, CanvasPointer pointer);
        public event BarChartElementTappedEventHandler BarTapped;

        public bool DisableSelectionByClick { set; get; }

        private BarChartElement _draggedElement;

        private HashSet<BarChartElement> _selectedElements;

        public bool MultiSelect;
        private bool _isDragging;

        /// <summary>
        /// Padding is by default 50
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public BarChartUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            MultiSelect = false;
            Title = "My Bar Chart";
            Palette = new List<Color>(new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow });
            Background = Colors.LightBlue;
            AddElement("Kiana", 8);
            AddElement("John", 3);
            AddElement("Henri", 5);
            AddElement("Howard", 12);
            AddElement("Joanna", 1);
            InnerPadding = 50;
            _isDragging = false;
            _selectedElements = new HashSet<BarChartElement>();
        }

        public void AddElement(string item, int value)
        {
            var element = new BarChartElement(Parent, ResourceCreator);

            element.Background = Palette[_children.Count % Palette.Count];
            element.Item = item;
            element.Value = value;
            _children?.Add(element);

            element.Dragged += Element_Dragged;
            element.Released += Element_Released;
        }

        private void Element_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var bar = item as BarChartElement;
            Debug.Assert(bar != null);
            //If we are dragging, then the release of the pointer signifies the end of a drag
            if (_isDragging)
            {
                if(_draggedElement != null)
                {
                    BarDragCompleted?.Invoke(this, _draggedElement, pointer);
                }
                _isDragging = false;
                _draggedElement = null;
            }
            //If we are not dragging (Element_Dragged was not called before this), this is a click
            else
            {
                //Here we select/deselect the bar.
                if (_selectedElements.Contains(bar))
                {
                    if (!DisableSelectionByClick)
                    {
                        DeselectElement(bar);
                    }
                }else
                {
                    if (!DisableSelectionByClick)
                    {
                        SelectElement(bar);
                    }
                }


            }
        }

        private void SelectElement(BarChartElement bar)
        {
            if (bar == null)
            {
                Debug.Write("Trying to select a null element, idiot.");
                return;
            }

            if (!MultiSelect)
            {
                //var elementsToDeselect = new List<BarChartElement>();
                foreach(var e in _selectedElements.ToList())
                {
                    DeselectElement(e);
                }
            }
            _selectedElements.Add(bar);
            BarSelected?.Invoke(this, bar);

            
        }

        private void DeselectElement(BarChartElement bar)
        {
            if (bar == null)
            {
                Debug.Write("Trying to select a null element, idiot.");
                return;
            }

            if (_selectedElements.Contains(bar))
            {
                _selectedElements.Remove(bar);
                BarDeselected?.Invoke(this, bar);
            }
        }

        private void Element_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var bar = item as BarChartElement;
            Debug.Assert(bar != null);
            BarDragged?.Invoke(bar, pointer);
            _draggedElement = bar;
            _isDragging = true;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            ds.DrawRectangle(new Windows.Foundation.Rect(InnerPadding, InnerPadding, Width - InnerPadding * 2, Height - InnerPadding * 2), Colors.Purple);
            DrawBars(ds);
            DrawTitle(ds);
            DrawScale(ds);
            ds.Transform = orgTransform;


        }

        private void DrawScale(CanvasDrawingSession ds)
        {
            var total = _children.Sum(element => (element as BarChartElement).Value);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            var numLines = 10f;

            var realHeight = Height - InnerPadding * 2;
            var start = Height - InnerPadding;
            for (int i = 0; i < numLines; i++)
            {
                var point0 = new Vector2(InnerPadding - 5,  start - realHeight * i/numLines);
                var point1 = new Vector2(InnerPadding, start - realHeight * i/numLines);

                ds.DrawLine(point0, point1, Colors.Black);

                var p = point0;
                var text = (total* i/numLines).ToString();
                ds.DrawText(
                    text,
                    p,
                    Colors.Black,
                    new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
                    {
                        FontSize = 12,
                        HorizontalAlignment = Microsoft.Graphics.Canvas.Text.CanvasHorizontalAlignment.Right,
                        VerticalAlignment = Microsoft.Graphics.Canvas.Text.CanvasVerticalAlignment.Center
                    });

            }

            ds.Transform = orgTransform;

        }

        private void RemoveHandlers(BarChartElement bar)
        {
            bar.Dragged -= Element_Dragged;
            bar.Released -= Element_Released;
        }

        private void DrawTitle(CanvasDrawingSession ds)
        {
            var p = new Vector2(Width/2, InnerPadding + 16);
                ds.DrawText(
        Title,
        p,
        Colors.Black,
        new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
        {
            FontSize = 16,
            HorizontalAlignment = Microsoft.Graphics.Canvas.Text.CanvasHorizontalAlignment.Center
        });

        }
        private void DrawBars(CanvasDrawingSession ds)
        {
            var total = _children.Sum(element => (element as BarChartElement).Value);
            var offset = InnerPadding;
            var h = Height - InnerPadding * 2;
            var w = Width - InnerPadding * 2;
            var barWidth = 0.8f * w / _children.Count;
            var spacing = 0.2f * w / (_children.Count - 1);
            foreach (var child in _children)
            {

                var element = child as BarChartElement;

                //Marks if selected
                if (_selectedElements.Contains(element))
                {
                    element.Bordercolor = Colors.Black;
                    element.BorderWidth = 4;
                }else
                {
                    //element.Bordercolor = Colors.Black;
                    element.BorderWidth = 0;
                }



                element.Width = barWidth;
                element.Height = element.Value / total * h;

                element.Transform.LocalPosition = new System.Numerics.Vector2(offset, Height - InnerPadding - element.Height);
                offset += barWidth + spacing;
            }

        }
    }
}
