using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TextNodeController : ElementController
    {
        public delegate void TextChangedHandler(object source, string text);
        public event TextChangedHandler TextChanged;
        
        public TextNodeController(TextElementModel model) : base(model)
        {
            if (SessionController.Instance.ContentController.GetLibraryElementModel(Model.LibraryId) != null)
            {
                LibraryElementController.ContentChanged += ContentChanged;
            }
        }

        private void ContentChanged(object originalSenderViewModel, string newData)
        {
            var content = SessionController.Instance.ContentController.GetLibraryElementModel(Model.LibraryId);
            TextChanged?.Invoke(this, content.Data);
        }

        public override void Dispose()
        {
           
            if (SessionController.Instance.ContentController.GetLibraryElementModel(Model.LibraryId) != null)
            {
                LibraryElementController.ContentChanged -= ContentChanged;
            }
            base.Dispose();

        }
    }
}
