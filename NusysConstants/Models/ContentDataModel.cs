using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// The base model class that actually hold the information for all content. 
    /// This is where text strings, image urls, pdf page urls, mp3 urls, etc should be contained.
    /// This should also be just a data holding class and not contain any events.
    /// </summary>
    public class ContentDataModel
    {
        public string ContentId { get; set; }

        public string Data { get; set; }

        public List<InkModel> Strokes { get; set; } = new List<InkModel>();
        public NusysConstants.ContentType ContentType { get; set; }
        
        public ContentDataModel(string contentId, string data)
        {
            Data = data;
            ContentId = contentId;
        }

        /// <summary>
        /// method used to set the data.
        /// This should only be called through the controller for this content data model and should be the only way to set the data.
        /// </summary>
        /// <param name="data"></param>
        public virtual void SetData(string data)
        {
            Data = data;
        }

        /// <summary>
        /// Generic dispose method.
        /// This base one only sets the data and ink strokes to null.
        /// </summary>
        public virtual void Dispose()
        {
            Data = null;
            Strokes = null;
        }
    }
}
