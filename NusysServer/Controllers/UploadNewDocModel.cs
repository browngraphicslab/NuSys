using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NusysServer
{
    public class UploadNewDocModel
    {
        public enum SelectionType
        {
            Website,
            Text,
            Pdf,
            Img,
            Video
        }
        public string data { get; set; }
        public string msg { get; set; }
        public string selectionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SelectionType type { get; set; }
        public string url { get; set; }
    }
}