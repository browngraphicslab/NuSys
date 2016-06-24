using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class SearchResultTemplate
    {
        public string Title { get; set; }
        public ElementType Type { get; set; }
        public string TimeStamp { get; set; }
        public string Creator { get; set; }
        public HashSet<Keyword> Keywords { get; set; }
        public Dictionary<string, Tuple<string, bool>> MetaData { get; set; }
        public string Data { get; set; }

        // unused
        public string Id { get; set; }
        public LibraryElementModel Model { get; set; }


        public SearchResultTemplate(SearchResult result)
        {
            // return if library element model doesn't exist or if result parameter is null
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(result?.ContentID);
            var model = controller.LibraryElementModel;
            if (model == null) return;

            this.Title = model.Title;
            this.Type = model.Type;
            this.TimeStamp = model.Timestamp;
            this.Creator = model.Creator;
            this.Keywords = model.Keywords;
            this.MetaData = model.Metadata;

            // unused
            this.Id = model.LibraryElementId;
            this.Model = model;
            this.Data = model.Data;


        }
    }

}
