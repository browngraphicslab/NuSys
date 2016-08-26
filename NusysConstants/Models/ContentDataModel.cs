using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class ContentDataModel
    {
        public delegate Task RegionAddedEventHandler(string regionLibraryElementModelId);

        public event RegionAddedEventHandler OnRegionAdded;
        public delegate void RegionRemovedEventHandler(string regionLibraryElementModelId);

        public event RegionRemovedEventHandler OnRegionRemoved;

        public string ContentId { get; set; }

        public string Data { get; set; }
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

        /// <summary>
        /// Invokes OnRegionAdded Event which updates the RectangleWrapper's region views
        /// </summary>
        /// <param name="regionLibraryElementModelId"></param>
        public void AddRegion(string regionLibraryElementModelId)
        {
            OnRegionAdded?.Invoke(regionLibraryElementModelId);
        }

        /// <summary>
        /// Invokes OnRegionRemoved Event which updates the RectangleWrapper's region views
        /// </summary>
        /// <param name="regionLibraryElementModelId"></param>
        public void RemoveRegion(string regionLibraryElementModelId)
        {
            OnRegionRemoved?.Invoke(regionLibraryElementModelId);
        }
    }
}
