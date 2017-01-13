using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysServer
{
    public class PdfDataHolder : DataHolder
    {
        private Uri _uri; 
        public Uri Uri { get { return _uri; }
            set
            {
                _uri = value;
                Content.Data = _uri.AbsoluteUri;
            }
        }
        public PdfDataHolder(Uri uri,string title) : base(title)
        {
            Content = new PdfContentDataModel(Content.ContentId,uri.AbsoluteUri);
            LibraryElement = new PdfLibraryElementModel(NusysConstants.GenerateId());
            LibraryElement.Title = title;
            LibraryElement.ContentDataModelId = Content.ContentId;
            Content.Data = uri.AbsoluteUri;
            Content.ContentType = NusysConstants.ContentType.PDF;
            this.Uri = uri;
        }
    }
}
