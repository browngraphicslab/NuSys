using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysServer
{
    public class VideoDataHolder : DataHolder
    {

        private Uri _uri; 
        public Uri Uri { get { return _uri; }
            set
            {
                _uri = value;
                Content.Data = _uri.AbsoluteUri;
            }
        }
        public VideoDataHolder(Uri uri,string title)  : base(title)
        {
            LibraryElement = new VideoLibraryElementModel(NusysConstants.GenerateId());
            LibraryElement.Title = title;
            LibraryElement.ContentDataModelId = Content.ContentId;
            Content.ContentType=NusysConstants.ContentType.Video;
            Content.Data = uri.AbsoluteUri;
            this.Uri = uri;
        }
    }
}
