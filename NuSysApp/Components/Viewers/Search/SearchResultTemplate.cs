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
        public string Keywords { get; set; }
        public string Metadata { get; set; }
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
            this.TimeStamp = parseTimeStampToDDMMYYFormat(model.Timestamp);
            this.Creator = model.Creator;
            this.Keywords = parseKeyWordsToCommaSeparatedList(model.Keywords);
            this.Metadata = parseMetaDataToHyphenBulletList(model.Metadata);

            // unused
            this.Id = model.LibraryElementId;
            this.Model = model;
            this.Data = model.Data;
        }

        private string parseTimeStampToDDMMYYFormat(string timestamp)
        {
            // trim whitespace then split on the first space and return the first element
            return timestamp.Trim().Split()[0];
        }

        private string parseKeyWordsToCommaSeparatedList(HashSet<Keyword> keywords)
        {
            StringBuilder output = new StringBuilder();
            var separator = ", ";
            foreach (var keyword in keywords)
            {
                output.Append(keyword.Text);
                output.Append(separator);
            }
            // remove the final comma if any output was added
            if (output.Length > 0)
                output.Remove(output.Length - separator.Length, separator.Length);
            return output.ToString();
        }

        private string parseMetaDataToHyphenBulletList(Dictionary<string, MetadataEntry> metadataDict)
        {
            StringBuilder output = new StringBuilder();
            var separator = ", ";
            // for each entry
            foreach (var entry in metadataDict)
            {
                // append the key
                output.Append(entry.Key);
                // append a separator
                output.Append(" - ");
                // TODO currently append one value, should append multiple
                output.Append(metadataDict[entry.Key].Values);
                // break to new line
                output.Append("\n");
            }
            // remove the final new line if any elements were added to output
            if (output.Length > 0)
                output.Remove(output.Length - 1, 1);
            return output.ToString();
        }
    }

}
