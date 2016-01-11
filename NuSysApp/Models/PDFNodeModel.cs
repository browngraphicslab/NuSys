using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class PdfNodeModel : NodeModel
    {
        private int _currentPageNumber;

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
            NodeType = NodeType.PDF;
        }
        
        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            props.Add("page", CurrentPageNumber.ToString());
            return props;
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("page"))
            {
                CurrentPageNumber = props.GetInt("page", 0);
            }
            await base.UnPack(props);
        }
    }
}