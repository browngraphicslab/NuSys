using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;


namespace NuSysApp
{
    public class PdfLibraryElementController : ImageLibraryElementController
    {
        public delegate void PageStartChangedEventHandler(object sender, int pageStart);
        public event PageStartChangedEventHandler PageStartChanged;

        public delegate void PageEndChangedEventHandler(object sender, int pageEnd);
        public event PageEndChangedEventHandler PageEndChanged;

        public PdfLibraryElementModel PdfLibraryElementModel
        {
            get
            {
                return base.LibraryElementModel as PdfLibraryElementModel;
            }
        }

        public PdfLibraryElementController(PdfLibraryElementModel model) : base(model)
        {
        }

        public void SetPageEnd(int page)
        {
            PdfLibraryElementModel.PageEnd = page;
            _debouncingDictionary.Add(NusysConstants.PDF_PAGE_END_KEY, page);
            PageEndChanged?.Invoke(this, page);
        }

        public void SetPageStart(int page)
        {
            PdfLibraryElementModel.PageStart = page;
            _debouncingDictionary.Add(NusysConstants.PDF_PAGE_START_KEY, page);
            PageStartChanged?.Invoke(this, page);
        }

        public override void UnPack(Message message)
        {
            SetBlockServerBoolean(true);
            if (message.ContainsKey(NusysConstants.PDF_PAGE_START_KEY))
            {
                SetPageStart(message.GetInt(NusysConstants.PDF_PAGE_START_KEY, 1));
            }
            if (message.ContainsKey(NusysConstants.PDF_PAGE_END_KEY))
            {
                SetPageEnd(message.GetInt(NusysConstants.PDF_PAGE_END_KEY, 1));
            }
            base.UnPack(message);
            SetBlockServerBoolean(false);
        }
    }
}
