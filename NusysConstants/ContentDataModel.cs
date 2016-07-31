using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class ContentDataModel
    {
        public string ContentId { get; private set; }
        public string Data { get; private set; }
        public NusysConstants.ContentType ContentType { get; set; }
        
        public ContentDataModel(string contentId, string data)
        {
            Data = data;
            ContentId = contentId;
        }

        public void SetData(string data)
        {
            Data = data;
        }
    }
}
