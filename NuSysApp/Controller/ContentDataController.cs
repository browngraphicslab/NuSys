using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class ContentDataController
    {
        public string ContentId { get; private set; }
        public string Data { get; private set; }
        public NusysConstants.ContentType ContentType { get; set; }

        public ContentDataController(string contentId, string data)
        {
            Data = data;
            ContentId = contentId;
        }
    }
}
