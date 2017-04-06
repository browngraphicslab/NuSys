using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using System.Diagnostics;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Brushes;

namespace NuSysApp
{
    public class BarChartUIElement : RectangleUIElement
    {

        /// <summary>
        /// Possible colors of the bars. If there are more bars than colors, we reuse colors
        /// </summary>
        public List<Color> Palette { set; get; }



        //private BarChartElement _draggedElement;
        /// <summary>
        /// Normalized height of the maximum value in the bar chart
        /// For example, if the chart is 100 in height, then the largest value is 80 in height
        /// </summary>

        private HashSet<BarChartElement> _selectedElements;



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

        public delegate void BarChartElementTappedEventHandler(object source, BarChartElement bar, CanvasPointer pointer);
        public event BarChartElementTappedEventHandler BarTapped;

        public delegate void BarChartElementDoubleTappedEventHandler(object source, BarChartElement bar, CanvasPointer pointer);
        public event BarChartElementDoubleTappedEventHandler BarDoubleTapped;



        public Thickness Margin { set; get; }

        private Thickness _denormalizedMargin {
            get { return new Thickness(Margin.Left * Width, Margin.Top * Height, Margin.Right * Width, Margin.Bottom * Height);}
        }
        public bool DisableSelectionByClick { get; internal set; }



        public enum ScaleType { LOG, LINEAR}


        public struct ChartPropertiesStruct
        {
            public CanvasSolidColorBrush Background;
            //Title of the chart
            public string Title;

        }
        public struct AxisPropertiesStruct
        {
            public ScaleType ScaleType;
            //Domain is the domain of the possible inputs, normalized
            public Tuple<double, double> Domain;
            //Range is normalized.
            public Tuple<double, double> Range;
            //number of ticks
            public int Ticks;
            //label that will be drawn under
            public string Label;
            //max labels
            public int MaxLabels;

        }


        public AxisPropertiesStruct YAxisProperties;
        public ChartPropertiesStruct ChartProperties;
        public BarChartUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            SetUpDefaultProperties();

            _selectedElements = new HashSet<BarChartElement>();

            Palette = new List<Color>(new[] { Colors.DarkSalmon, Colors.Azure, Colors.LemonChiffon, Colors.Honeydew, Colors.Pink });

        }

        private void SetUpDefaultProperties()
        {
            ChartProperties = new ChartPropertiesStruct()
            {
                Background = new CanvasSolidColorBrush(ResourceCreator, Colors.Blue),
                Title = String.Empty

            };

            YAxisProperties = new AxisPropertiesStruct()
            {
                ScaleType =  ScaleType.LINEAR,
                //arbitrarily set domain to be 0, 1. This is only because we will update it later. Ideally domain should be something close to (min of Data, max of Data)
                Domain = new Tuple<double, double>(0,1),

                //go to 0.8 so that we have room for title
                Range = new Tuple<double, double>(0,0.8),
                Ticks = 12,
                Label = String.Empty,
                MaxLabels = 12
                
            };

            Margin = new Thickness(0.1f,0.1f,0.1f,0.1f);


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


        public void ClearItems()
        {
            foreach (var child in _children)
            {
                RemoveBarHandlers(child as BarChartElement);
            }
            ClearChildren();
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
        //START OF EVENTS
        private void AddBarHandlers(BarChartElement element)
        {
            element.Dragged += Element_Dragged;
            element.Tapped += Element_Tapped;
            element.DoubleTapped += Element_DoubleTapped;
            element.DragCompleted += Element_DragCompleted;
        }
        public void RemoveBarHandlers(BarChartElement element)
        {
            element.Dragged -= Element_Dragged;
            element.Tapped -= Element_Tapped;
            element.DoubleTapped -= Element_DoubleTapped;
            element.DragCompleted -= Element_DragCompleted;
        }


        private void Element_DragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            BarDragCompleted?.Invoke(this, item as BarChartElement, pointer);

        }

        private void Element_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            BarDoubleTapped?.Invoke(this, item as BarChartElement, pointer);
        }

        private void Element_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var bar = item as BarChartElement;
            BarTapped?.Invoke(this, bar, pointer);

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

        private void Element_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var bar = item as BarChartElement;
            Debug.Assert(bar != null);
            BarDragged?.Invoke(bar, pointer);

        }


        //START OF SELECTING
        #region selection

        

        public void DeselectAllItems()
        {
            _selectedElements.Clear();
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

        #endregion

        //START OF DRAWING/UPDATING

        #region drawing/updating

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            DrawChartBackground(ds);
            DrawTitle(ds);
            DrawAxes(ds);


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
        private void DrawAxes(CanvasDrawingSession ds)
        {
            //Draw y-axis


            var total = _children.Sum(element => (element as BarChartElement).Value);
            var max = _children.Max(element => (element as BarChartElement).Value);

            var marginTop = Margin.Top*Height;
            var marginBottom = Margin.Bottom*Height;
            var marginLeft = Margin.Left*Width;
            var marginRight = Margin.Right*Width;

            var realHeight = (float) (Height - marginTop - marginBottom);
            var start = (float) (Height - marginBottom);


            for (float i = 0; i <= YAxisProperties.Ticks; i += 1f)
            {
                var point0 = new Vector2((float)marginLeft - 5, start - realHeight * i / YAxisProperties.Ticks);
                var point1 = new Vector2((float)marginLeft, start - realHeight * i / YAxisProperties.Ticks);

                ds.DrawLine(point0, point1, Colors.Black);

                var p = point0;
                var text = (max * i / YAxisProperties.Ticks).ToString();
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

        }

        private void DrawChartBackground(CanvasDrawingSession ds)
        {
            //width taking to account margin
            var width = (1 - Margin.Left - Margin.Right) * Width;
            //TODO: edge case of negative width and height

            var height = (1 - Margin.Top - Margin.Bottom) * Height;

            if (width > 0 && height > 0)
            {
                ds.FillRectangle(new Rect(Margin.Left * Width, Margin.Top * Height, width, height), ChartProperties.Background);
            }
        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            UpdateBars();
            base.Update(parentLocalToScreenTransform);
        }

        private void UpdateBars()
        {
            /*
            var total = _children.Sum(element => (element as BarChartElement).Value);
            var max = _children.Max(element => (element as BarChartElement).Value);

            var marginLeft = Margin.Left * Width;
            var marginRight = Margin.Right*Width;
            var marginBottom = Margin.Bottom*Height;
            var marginTop = Margin.Top*Height;

            var offset = marginLeft;
            var h = Height - marginTop - marginBottom;
            var w = Width - marginLeft - marginRight;
            var barWidth = HorizontalScale * w / _children.Count;
            var spacing = 0.2f * w / (_children.Count - 1);
            //canDrawAllLabels is true iff we have enough space to draw all labels. Currently this is naive and uses an arbitrary
            //max number of labels
            bool canDrawAllLabels = _children.Count <= MaxLabels;

            foreach (var child in _children)
            {
                var element = child as BarChartElement;
                Debug.Assert(element != null);

                element.CanDrawLabel = canDrawAllLabels;
                element.IsSelected = false;
                element.Width = barWidth;
                element.Height = element.Value / max * h;

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
            */
        }


            #endregion

            /*









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



            }
                */
        }

}