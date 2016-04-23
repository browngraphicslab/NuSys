using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using NuSysApp.Viewers;

namespace NuSysApp
{
    public class PdfNodeModel : ElementModel
    {
        private int _currentPageNumber;
        public MuPDFWinRT.Document Document { get; set; }
        public Dictionary<int, List<RectangleViewModel>> PageRegionDict { get; set; }

        public int CurrentPageNumber {
            get { return _currentPageNumber; }
            set
            {
                _currentPageNumber = value;
                PageChange?.Invoke(value);
            }
        }

        public event PdfImagesCreatedHandler OnPdfImagesCreated;
        public event PageChangeHandler PageChange;
        public delegate void PdfImagesCreatedHandler();
        public delegate void PageChangeHandler(int page);

        public PdfNodeModel(string id) : base(id)
        {
            ElementType = ElementType.PDF;
            PageRegionDict = new Dictionary<int, List<RectangleViewModel>>();
        }
        
        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            props.Add("page", CurrentPageNumber.ToString());
            props.Add("pageRegionDict", PageRegionDict);
            return props;
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("page"))
            {
                CurrentPageNumber = props.GetInt("page", 0);
            }

            if (props.ContainsKey("pageRegionDict"))
            {
                PageRegionDict = props.GetDict<int, List<RectangleViewModel>>("pageRegionDict");
            }

            await base.UnPack(props);
        }
    }
}