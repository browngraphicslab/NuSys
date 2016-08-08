using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class PdfNodeModel : ElementModel
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
        
        public event PageChangeHandler PageChange;
        public delegate void PageChangeHandler(int page);

        public PdfNodeModel(string id) : base(id)
        {
            ElementType = NusysConstants.ElementType.PDF;
        }
        
        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            props.Add("page", CurrentPageNumber.ToString());
            return props;
        }

        public override void UnPackFromDatabaseMessage(Message props)
        {
            if (props.ContainsKey(NusysConstants.PDF_ELEMENT_PAGE_LOCATION_KEY))
            {
                CurrentPageNumber = props.GetInt(NusysConstants.PDF_ELEMENT_PAGE_LOCATION_KEY, 0);
            }

            base.UnPackFromDatabaseMessage(props);
        }
    }
}