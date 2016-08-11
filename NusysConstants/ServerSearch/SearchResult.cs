using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class SearchResult
    {
        public enum ResultType
        {
            Metadata,
            Title,
            Type,
            Creator,
            Region,
            Timestamp,
            Data
        }
        // Properties Returned By Server
        public string ContentID { get; set; }
        public ResultType Result { get; set; }
        public string ExtraInfo { get; set; }
        public SearchResult(string contentId, ResultType type)
        {
            ContentID = contentId;
            Result = type;
            ExtraInfo = string.Empty;
        }

        public SearchResult()
        {
        }

        public SearchResult(string contentId, ResultType type, string info) : this(contentId, type)
        {
            ExtraInfo = info;
        }


        //todo add methods for displaying the result based on given info
    }
}
