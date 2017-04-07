using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    public class VariableElementRenderItem : TextElementRenderItem
    {
        private ICanvasImage _icon;
        public VariableElementRenderItem(VariableNodeViewModel vm, BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(vm, parent, resourceCreator)
        {
            vm.VariableElementController.StoredLibraryIdChanged += VariableElementControllerOnStoredLibraryIdChanged;
            vm.VariableElementController.SizeChanged += VariableElementControllerOnSizeChanged;
            vm.VariableElementController.UpdateText();
            vm.VariableElementController.SetSize(vm.Model.Width, vm.Model.Height);
        }

        private void VariableElementControllerOnSizeChanged(object source, double width, double height)
        {
            IsDirty = true;
        }

        private void VariableElementControllerOnStoredLibraryIdChanged(object sender, string s)
        {
            SetController(s);
            IsDirty = true;
        }

        private async Task SetController(string libraryElementId)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementId);
            if (controller != null)
            {
                _icon = await MediaUtil.LoadCanvasBitmapAsync(ResourceCreator, controller.MediumIconUri);
            }
        }

        public override async Task Load()
        {
            SetController((ViewModel as VariableNodeViewModel).VariableElementController.VariableModel.StoredLibraryId);
             await base.Load();
        }

        public override void Dispose()
        {
            Debug.Assert(ViewModel is VariableNodeViewModel);
            (ViewModel as VariableNodeViewModel).VariableElementController.StoredLibraryIdChanged -= VariableElementControllerOnStoredLibraryIdChanged;
            (ViewModel as VariableNodeViewModel).VariableElementController.SizeChanged -= VariableElementControllerOnSizeChanged;
            base.Dispose();
        }

        protected override void HackyUpdate()
        {
            var elController = ViewModel.Controller as VariableElementController;
            if ((ViewModel as VariableNodeViewModel).VariableElementController.ValueString.Length > VariableElementController.MaxChars)
            {
                _textBox.FuckedWidth = null;
                _textBox.FuckedHeight = null;
            }
            else
            {
                _textBox.FuckedWidth = (float) (32*elController.ValueAspectRatio);
                _textBox.FuckedHeight = 32;
            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
            if (_icon != null)
            {
                var orgTransform = ds.Transform;
                ds.Transform = Transform.LocalToScreenMatrix;

                var s = Constants.DefaultNodeSize*.1;

                using (ds.CreateLayer(1, CanvasGeometry.CreateRectangle(Canvas, new Rect(0, 0, ViewModel.Width, ViewModel.Height))))
                {
                    var b = _icon.GetBounds(ResourceCreator);
                    ds.DrawImage(_icon, new Rect(ViewModel.Width-s, ViewModel.Height-s,s,s*(b.Height/b.Width)), _icon.GetBounds(Canvas));
                }

                ds.Transform = orgTransform;
            }
        }
    }
}
