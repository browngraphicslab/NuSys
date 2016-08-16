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
        public string LibraryElementId { get; set; }
        public ResultType Result { get; set; }
        public string ExtraInfo { get; set; }
        public SearchResult(string libraryElementId, ResultType type)
        {
            LibraryElementId = libraryElementId;
            Result = type;
            ExtraInfo = string.Empty;
        }

        public SearchResult()
        {
        }

        public SearchResult(string libraryElementId, ResultType type, string info) : this(libraryElementId, type)
        {
            ExtraInfo = info;
        }
        
    }
}
