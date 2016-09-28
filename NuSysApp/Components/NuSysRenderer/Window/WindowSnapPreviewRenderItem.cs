using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class WindowSnapPreviewRenderItem : BaseRenderItem
    {
        /// <summary>
        /// The size in pixels of the WindowSnapPreviewRenderItem. Contains
        /// height and width variables
        /// </summary>
        private Size _size;

        /// <summary>
        /// The default color of the preview
        /// </summary>
        public Color PreviewColor = Color.FromArgb(211, 211, 211, 50); //todo add code so this can be change dynamically

        public WindowSnapPreviewRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            IsVisible = false;
        }

        /// <summary>
        /// Draws the window onto the screen with the offset of the Local
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed || !IsVisible)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            ds.FillRectangle(new Rect(0, 0, _size.Width, _size.Height), PreviewColor);

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        /// <summary>
        /// Used to show the preview
        /// </summary>
        /// <param name="size"></param>
        /// <param name="offset"></param>
        public void ShowPreview(Size size, Vector2 offset)
        {
            _size.Width = size.Width;
            _size.Height = size.Height;
            Transform.LocalPosition = offset;
            IsVisible = true;
        }

        /// <summary>
        /// Used to hide the preview
        /// </summary>
        public void HidePreview()
        {
            IsVisible = false;
        }

    }
}
