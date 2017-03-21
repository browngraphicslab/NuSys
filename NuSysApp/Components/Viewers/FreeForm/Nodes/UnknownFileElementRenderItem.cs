using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class UnknownFileElementRenderItem : ElementRenderItem
    {
        private TextboxUIElement _text;
        public UnknownFileElementRenderItem(UnknownFileViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(vm, parent, resourceCreator)
        {
            _text = new TextboxUIElement(this,resourceCreator);
            vm.Controller.LibraryElementController.TitleChanged += LibraryElementControllerOnTitleChanged;
            AddChild(_text);
            UpdateText();
        }

        public override void Dispose()
        {
            if (ViewModel?.Controller?.LibraryElementController != null)
            {
                ViewModel.Controller.LibraryElementController.TitleChanged -= LibraryElementControllerOnTitleChanged;
            }
            base.Dispose();
        }

        private void LibraryElementControllerOnTitleChanged(object sender, string s)
        {
            UpdateText();
        }

        private async Task UpdateText()
        {
            if (_text != null)
            {
                _text.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, ViewModel.Controller.LibraryElementController.LargeIconUri);
            }
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsDisposed)
                return;
            _text.Width = (float)ViewModel.Controller.Model.Width;
            _text.Height = (float)ViewModel.Controller.Model.Height;
            base.Update(parentLocalToScreenTransform);
        }
    }
}
