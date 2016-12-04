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

        public float Padding { set; get; }
        public string Title { set; get; }


        public delegate void BarChartElementDraggedEventHandler(BarChartElement bar, CanvasPointer pointer);
        public event BarChartElementDraggedEventHandler BarDragged;



        //public delegate void BarChartElementTappedEventHandler(BarChartElement bar, CanvasPointer pointer);
        //public event BarChartElementReleasedEventHandler BarTapped;


        private bool _isDragging;
        public BarChartUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Title = "My Bar Chart";
            Palette = new List<Color>(new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow });
            Background = Colors.LightBlue;
            AddElement("Kiana", 8);
            AddElement("John", 3);
            AddElement("Henri", 5);
            Padding = 50;
            _isDragging = false;

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

            if (_isDragging)
            {
                //BarReleased?.Invoke(bar, pointer);

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

            DrawBars(ds);
            DrawTitle(ds);
            DrawScale(ds);


        }

        private void DrawScale(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            var numLines = 10f;

            for (int i = 0; i < numLines; i++)
            {
                var point0 = new Vector2(0,  Height - Padding - Height*i/numLines);
                var point1 = new Vector2(10, Height - Padding - Height* i/numLines);

                ds.DrawLine(point0, point1, Colors.Black);

                var p = point1;
                var text = "4";
                ds.DrawText(
                    text,
                    p,
                    Colors.Black,
                    new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
                    {
                        FontSize = 12,
                        HorizontalAlignment = Microsoft.Graphics.Canvas.Text.CanvasHorizontalAlignment.Right
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
            var p = new Vector2(0, Padding - 5);
                ds.DrawText(
        Title,
        p,
        Colors.Black,
        new Microsoft.Graphics.Canvas.Text.CanvasTextFormat
        {
            FontSize = 12,
            HorizontalAlignment = Microsoft.Graphics.Canvas.Text.CanvasHorizontalAlignment.Left
        });

        }
        private void DrawBars(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            var total = _children.Sum(element => (element as BarChartElement).Value);
            var offset = Padding;
            foreach (var child in _children)
            {

                var element = child as BarChartElement;
                var h = Height - Padding * 2;
                element.Height = element.Value / total * h;
                var w = Width - Padding * 2;
                element.Width = w / (_children.Count * 2);
                element.Transform.LocalPosition = new System.Numerics.Vector2(offset, Height - Padding - element.Height);
                offset += 100;
            }
            ds.Transform = orgTransform;

        }
    }
}
