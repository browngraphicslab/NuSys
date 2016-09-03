using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class WrapRenderItem : BaseRenderItem
    {
        public double MaxWidth;
        public List<BaseRenderItem> Items { get; private set; } = new List<BaseRenderItem>();
        private Rect _measurement = new Rect();

        public WrapRenderItem(double maxWidth, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator) : base(parent, resourceCreator)
        {
            MaxWidth = maxWidth;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            base.Update();
            var lineWidth = 0.0;
            var lineY = 0.0;
            var lineX = 0.0;
            var linNum = 0;
            Rect measure;
            var items = Items.ToArray();
            for (int index = 0; index < items.Length; index++)
            {
                var baseRenderItem = items[index];
                baseRenderItem.Update();
                measure = baseRenderItem.GetMeasure();
                // lineHeight = lineHeight + measure.Height > lineHeight ? lineHeight + measure.Height : lineHeight;


                ds.Transform = Matrix3x2.CreateTranslation((float) lineX, (float) lineY)*GetTransform()*orgTransform;
                baseRenderItem.Draw(ds);

                if (index >= Items.Count - 2)
                    continue;

                var nextMeasure = items[index + 1].GetMeasure();

                if (lineX + measure.Width + 10 + nextMeasure.Width < MaxWidth)
                {
                    lineX += measure.Width + 10;
                } else if (lineX + measure.Width + 10 + nextMeasure.Width > MaxWidth)
                {
                    linNum += 1;
                    lineY = linNum*(measure.Height + 10);
                    lineX = 0.0;
                }
            }
            _measurement.X = 0;
            _measurement.Y = 0;
            _measurement.Width = MaxWidth;
            _measurement.Height = lineY + measure.Height;
            ds.Transform = orgTransform;
        }

        public override void Update()
        {
            base.Update();
            foreach (var baseRenderItem in Items.ToArray())
            {
                baseRenderItem.Update();
            }
        }

        public override Rect GetMeasure()
        {
            return _measurement;
        }

        public void AddItem(BaseRenderItem item)
        {
            Items.Add(item);
        }

        public void RemoveItem(BaseRenderItem item)
        {
            Items.Remove(item);
        }
    }
}
