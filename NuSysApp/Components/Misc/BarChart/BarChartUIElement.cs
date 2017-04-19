using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using System.Diagnostics;
using System.Numerics;
using Windows.Devices.Input;

namespace NuSysApp
{
    public class BarChartUIElement : RectangleUIElement
    {
        /// <summary>
        /// Possible colors of the bars. If there are more bars than colors, we reuse colors
        /// </summary>
        public List<Color> Palette { set; get; }
        /// <summary>
        /// InnerPadding is the distance from the actual bar chart to the outline of the RectangleUIElement
        /// </summary>
        public float InnerPadding { set; get; }
        /// <summary>
        /// Title of the bar chart displayed at the top
        /// </summary>
        public string Title { set; get; }

        public bool MultiSelect { set; get; }


        public int MaxLabels { set; get; }

        public delegate void BarChartElementSelectedEventHandler(object source, BarChartElement element);
        public event BarChartElementSelectedEventHandler BarSelected;

        public delegate void BarChartElementDeselectedEventHandler(object source, BarChartElement element);
        public event BarChartElementDeselectedEventHandler BarDeselected;

        public delegate void BarChartElementDraggedEventHandler(BarChartElement bar, CanvasPointer pointer);
        public event BarChartElementDraggedEventHandler BarDragged;


        public delegate void BarChartElementDragCompletedEventHandler(object source, BarChartElement element, CanvasPointer pointer);
        public event BarChartElementDragCompletedEventHandler BarDragCompleted;

        public delegate void BarChartElementTappedEventHandler(object source, BarChartElement bar, PointerDeviceType type);



        public event BarChartElementTappedEventHandler BarTapped;

        public delegate void BarChartElementDoubleTappedEventHandler(object source, BarChartElement bar);

        public event BarChartElementDoubleTappedEventHandler BarDoubleTapped;



        public bool DisableSelectionByClick { set; get; }

        public float VerticalScale { set; get; }
        /// <summary>
        /// Sum of the normalized widths of the bars.
        /// For example, if the chart is 100 in width and it has four elements, then each bar is 20 in width.
        /// </summary>
        public float HorizontalScale { set; get; }
        /// <summary>
        /// Number of lines in the scale of the bar chart
        /// </summary>
        public int NumberOfLines { set; get; }


        //private BarChartElement _draggedElement;
        /// <summary>
        /// Normalized height of the maximum value in the bar chart
        /// For example, if the chart is 100 in height, then the largest value is 80 in height
        /// </summary>

        private HashSet<BarChartElement> _selectedElements;

        //private bool _isDragging;

        /// <summary>
        /// Padding is by default 50
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public BarChartUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            MultiSelect = false;
            Title = "";
            Palette = new List<Color>(new[] { Colors.DarkSalmon, Colors.Azure, Colors.LemonChiffon, Colors.Honeydew, Colors.Pink });
            InnerPadding = 50;
            VerticalScale = 0.8f;
            HorizontalScale = 0.8f;
            NumberOfLines = 10;
            MaxLabels = 12;
            _selectedElements = new HashSet<BarChartElement>();
        }

        public void AddElement(string item, int value)
        {
            var element = new BarChartElement(Parent, ResourceCreator);
            element.BorderColor = Colors.Black;
            element.Background = Palette[_children.Count % Palette.Count];
            element.Item = item;
            element.Value = value;
            _children?.Add(element);

            AddBarHandlers(element);

        }

        public void AddBarHandlers(BarChartElement element)
        {
            element.Dragged += Element_Dragged;
            element.DragCompleted += Element_DragCompleted;

            var tapRecognizer = new TapGestureRecognizer();
            element.GestureRecognizers.Add(tapRecognizer);
            tapRecognizer.OnTapped += delegate(TapGestureRecognizer sender, TapEventArgs args)
            {
                if (args.TapType == TapEventArgs.Tap.SingleTap)
                {
                    Element_Tapped(element, args.DeviceType);
                }
                else if (args.TapType == TapEventArgs.Tap.DoubleTap)
                {
                    Element_DoubleTapped(element);
                }
            };
        }

        public void RemoveBarHandlers(BarChartElement element)
        {
            element.GestureRecognizers.Clear();
        }

        public void DeselectAllItems()
        {
            _selectedElements.Clear();
        }

        public void ClearItems()
        {
            foreach (var child in _children)
            {
                RemoveBarHandlers(child as BarChartElement);
            }
            ClearChildren();
        }
        private void Element_DragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            BarDragCompleted?.Invoke(this, item as BarChartElement, pointer);

        }

        private void Element_DoubleTapped(BarChartElement bar)
        {
            BarDoubleTapped?.Invoke(this, bar);
        }

        private void Element_Tapped(BarChartElement bar, PointerDeviceType type)
        {
            BarTapped?.Invoke(this, bar, type);

            //Here we select/deselect the bar.
            if (_selectedElements.Contains(bar))
            {
                if (!DisableSelectionByClick)
                {
                    DeselectElement(bar);
                }
            }
            else
            {
                if (!DisableSelectionByClick)
                {
                    SelectElement(bar);
                }
            }
        }


        public void AddItems(List<string> items)
        {
            var dict = new Dictionary<string, int>();
            foreach (var item in items)
            {
                if (!dict.ContainsKey(item))
                {
                    dict[item] = 1;
                }
                else
                {
                    dict[item] += 1;
                }

            }

            foreach (var kvp in dict)
            {
                AddElement(kvp.Key, kvp.Value);
            }
        }



        public void SelectElement(string name)
        {
            var barChartElement = from element in _children where (name == (element as BarChartElement).Item) select element;
            if (barChartElement.ToList().Count != 1)
            {
                return;
            }
            SelectElement(barChartElement.Single() as BarChartElement);

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
                foreach (var e in _selectedElements.ToList())
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
        /// <summary>
        /// Draws the scale on the side
        /// </summary>
        /// <param name="ds"></param>
        private void DrawScale(CanvasDrawingSession ds)
        {
            var total = _children.Sum(element => (element as BarChartElement).Value);
            var max = _children.Max(element => (element as BarChartElement).Value);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            var maxYVal = max + NumberOfLines - max % NumberOfLines;

            var realHeight = Height - InnerPadding * 2;
            var start = Height - InnerPadding;
            for (float i = 0; i <= NumberOfLines; i += 1f)
            {
                var point0 = new Vector2(InnerPadding - 5, start - realHeight * i / NumberOfLines);
                var point1 = new Vector2(InnerPadding, start - realHeight * i / NumberOfLines);

                ds.DrawLine(point0, point1, Colors.Black);

                var p = point0;
                var text = (maxYVal * i / NumberOfLines).ToString();
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



        private void DrawTitle(CanvasDrawingSession ds)
        {
            var p = new Vector2(Width / 2, InnerPadding / 2);
            ds.DrawText(
    Title,
    p,
    Colors.Black,
    new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
    {
        FontSize = 20,
        HorizontalAlignment = Microsoft.Graphics.Canvas.Text.CanvasHorizontalAlignment.Center
    });

        }
        private void DrawBars(CanvasDrawingSession ds)
        {
            var total = _children.Sum(element => (element as BarChartElement).Value);
            var max = _children.Max(element => (element as BarChartElement).Value);
            var maxYval = max + NumberOfLines - max % NumberOfLines;
            var offset = InnerPadding;
            var h = Height - InnerPadding * 2;
            var w = Width - InnerPadding * 2;
            var barWidth = HorizontalScale * w / _children.Count;
            var spacing = 0.2f * w / (_children.Count - 1);
            //canDrawAllLabels is true iff we have enough space to draw all labels. Currently this is naive and uses an arbitrary
            //max number of labels
            bool canDrawAllLabels = _children.Count <= MaxLabels;

            foreach (var child in _children)
            {
                var element = child as BarChartElement;
                Debug.Assert(element!= null);

                element.CanDrawLabel = canDrawAllLabels;
                element.IsSelected = false;
                element.Width = barWidth;
                element.Height = element.Value / maxYval * h;

                element.Transform.LocalPosition = new System.Numerics.Vector2(offset, Height - InnerPadding - element.Height);
                offset += barWidth + spacing;
            }

            foreach (var selectedElement in _selectedElements)
            {
                selectedElement.IsSelected = true;
                if (!canDrawAllLabels)
                {
                    selectedElement.CanDrawLabel = true;
                }

            }


        }
    }
}