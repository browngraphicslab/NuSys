using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NusysIntermediate;

namespace NuSysApp
{
    public class AudioElementRenderItem : ElementRenderItem
    {
        /// <summary>
        /// The view model for the audio node, we use it here to determine when the region bounds have changed, and get the normalized start and duration times
        /// </summary>
        private AudioNodeViewModel _vm;

        /// <summary>
        /// The bitmap image corresponding to the audio wave form of the original audio image, if we are displaying a region then we crop this in ReRender
        /// </summary>
        private CanvasBitmap _bmp;

        public AudioElementRenderItem(AudioNodeViewModel vm, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _vm.ViewModelIsDirty += _vm_ViewModelIsDirty;
        }

        /// <summary>
        /// Called whenever the vm's properties change in such a way that the node has to be rerendered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _vm_ViewModelIsDirty(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        /// <summary>
        /// Call this to load resources, in our case we are loading the content data model image
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            IsDirty = false;
            var url = _vm.Controller.LibraryElementController.LargeIconUri;

            _bmp = await MediaUtil.LoadCanvasBitmapAsync(ResourceCreator, url, ResourceCreator.Dpi);

            IsDirty = true;
        }

        /// <summary>
        /// Call this to remove resources, we are destroying the canvas bitmap, as well as removing view model events
        /// </summary>
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            if (_vm != null)
            {
                _vm.ViewModelIsDirty -= _vm_ViewModelIsDirty;
            }

            _bmp = null;
            _vm = null;
            base.Dispose();
        }

        /// <summary>
        /// Called 60 times a second, we check if the region has to be rerendered, if so we rerender it
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsDirty)
            {
                ReRender();
                IsDirty = false;
            }
            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Rerenders the image if the size of the region has changed
        /// </summary>
        protected void ReRender()
        {
            // we cannot render if the image does not exist
            if (_bmp == null) return;

            // create a crop effect, used for displaying cropped portions of images
            var croppy = new CropEffect()
            {
                Source = _bmp
            };

            // get the denormalized portion of the image we are going to display
            var x = _vm.NormalizedStartTime * _bmp.Size.Width;
            var w = _vm.NormalizedDuration * _bmp.Size.Width;

            // create a rectangle using the denormalized portion
            croppy.SourceRectangle = new Rect(x, 0, w, _bmp.Size.Height);

            // set the background image to draw the correct image
            Image = croppy;
        }

    }
}
