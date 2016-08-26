using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class TextNodeController : ElementController
    {
        public delegate void TextChangedHandler(object source, string text);
        public event TextChangedHandler TextChanged;
        
        public TextNodeController(TextElementModel model) : base(model)
        {
            if (LibraryElementController.ContentDataController != null)
            {
                LibraryElementController.ContentDataController.ContentDataUpdated += ContentChanged;
            }
        }

        private void ContentChanged(object originalSenderViewModel, string newData)
        {
            var libraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(Model.LibraryId);
            TextChanged?.Invoke(this, libraryElementController.Data);
        }

        public override void Dispose()
        {
           
            if (LibraryElementController?.ContentDataController != null)
            {
                LibraryElementController.ContentDataController.ContentDataUpdated -= ContentChanged;
            }
            base.Dispose();

        }
    }
}
