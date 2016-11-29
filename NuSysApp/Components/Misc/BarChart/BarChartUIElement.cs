using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using System.Diagnostics;

namespace NuSysApp
{
    public class BarChartUIElement : RectangleUIElement
    {
        public List<Color> Palette { set; get; }

        public float Padding { set; get; }
        public string Title { set; get; }


        public delegate void BarChartElementDraggedEventHandler(BarChartElement bar, CanvasPointer pointer);
        public event BarChartElementDraggedEventHandler BarDragged;



        public delegate void BarChartElementReleasedEventHandler(BarChartElement bar, CanvasPointer pointer);
        public event BarChartElementReleasedEventHandler BarReleased;

        public BarChartUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Palette = new List<Color>(new[] { Colors.Red, Colors.Blue, Colors.Green, Colors.Yellow });
            Background = Colors.Transparent;
            //Palette = new List<Color>(new[] { Colors.DarkSalmon, Colors.Azure, Colors.LemonChiffon, Colors.Honeydew, Colors.Pink });
            AddElement("Kiana", 8);
            AddElement("John", 3);
            AddElement("Henri", 5);


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
            BarReleased?.Invoke(bar, pointer);
        }

        private void Element_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var bar = item as BarChartElement;
            Debug.Assert(bar != null);
            BarDragged?.Invoke(bar, pointer);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            DrawBars(ds);
            DrawTitle(ds);

            base.Draw(ds);


        }

        private void RemoveHandlers(BarChartElement bar)
        {
            bar.Dragged -= Element_Dragged;
            bar.Released -= Element_Released;
        }

        private void DrawTitle(CanvasDrawingSession ds)
        {

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
                element.Transform.LocalPosition = new System.Numerics.Vector2(offset, h * 2 /3 - element.Height);
                offset += 100;
            }


            ds.Transform = orgTransform;

        }
    }
}
