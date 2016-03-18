using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class TextNodeController : ElementController
    {
        public delegate void TextChangedHandler(object source, string text, ElementViewModel originalSenderViewModel = null);
        public event TextChangedHandler TextChanged;
        
        public TextNodeController(TextElementModel model) : base(model)
        {
            if (SessionController.Instance.ContentController.Get(Model.LibraryId) != null)
            {
                var content = SessionController.Instance.ContentController.Get(Model.LibraryId);
                content.OnContentChanged += ContentChanged;
            }
        }

        private void ContentChanged(ElementViewModel originalSenderViewModel = null)
        {
            var content = SessionController.Instance.ContentController.Get(Model.LibraryId);
            TextChanged?.Invoke(this, content.Data, originalSenderViewModel);
        }
    }
}
