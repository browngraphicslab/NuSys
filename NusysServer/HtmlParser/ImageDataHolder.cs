using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysServer
{
    public class ImageDataHolder : DataHolder
    {
        private Uri _uri; 
        public Uri Uri { get { return _uri; }
            set
            {
                _uri = value;
                Content.Data = _uri.AbsoluteUri;
            }
        }
        public ImageDataHolder(Uri uri, String title) : base(title)
        {
            LibraryElement = new ImageLibraryElementModel(NusysConstants.GenerateId());
            LibraryElement.Title = title;
            LibraryElement.ContentDataModelId = Content.ContentId;
            Content.ContentType=NusysConstants.ContentType.Image;
            Content.Data = uri.AbsoluteUri;
            Uri = uri;
        }
    }
}
