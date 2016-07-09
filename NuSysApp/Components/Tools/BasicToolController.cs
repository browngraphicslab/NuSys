using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyToolkit.Utilities;
using NetTopologySuite.Utilities;

namespace NuSysApp
{
    public class BasicToolController : ToolController
    {
        public BasicToolController(BasicToolModel model) : base(model){}

        public BasicToolModel BasicToolModel
        {
            get
            {
                Debug.Assert(base.Model is BasicToolModel);
                return base.Model as BasicToolModel;
            }
        }

        public delegate void FilterChangedEventHandler(object sender, ToolModel.ToolFilterTypeTitle filter);
        public delegate void SelectionChangedEventHandler(object sender);

        public event FilterChangedEventHandler FilterChanged;
        public event SelectionChangedEventHandler SelectionChanged;

        public void SetFilter(ToolModel.ToolFilterTypeTitle filter)
        {
            BasicToolModel.SetFilter(filter);
            FilterChanged?.Invoke(this, filter);
            FireLibraryIdsChanged();
        }

        public override Func<string, bool> GetFunc()
        {
            return IncludeInFilter;
        }
        private bool IncludeInFilter(string libraryId)
        {
            var libraryElementModel = SessionController.Instance.ContentController.GetContent(libraryId);
            if (libraryElementModel == null)
            {
                return false;
            }
            if (BasicToolModel.Selection == null)
            {
                return false;
            }
            switch (BasicToolModel.Filter)
            {
                case ToolModel.ToolFilterTypeTitle.Title:
                    return libraryElementModel.Title == BasicToolModel.Selection;
                    break;
                case ToolModel.ToolFilterTypeTitle.Type:
                    return libraryElementModel.Type.ToString().ToLower().Equals(BasicToolModel.Selection.ToLower());
                    break;
                case ToolModel.ToolFilterTypeTitle.Creator:
                    return libraryElementModel.Creator == BasicToolModel.Selection;
                    break;
                case ToolModel.ToolFilterTypeTitle.Date:
                    return GetDate(libraryElementModel) == BasicToolModel.Selection;
                    break;
                case ToolModel.ToolFilterTypeTitle.MetadataKeys:
                    return libraryElementModel.Metadata?.ContainsKey(BasicToolModel.Selection) ?? false;
                    break;
                case ToolModel.ToolFilterTypeTitle.LastEditedDate:
                    return GetLastEditedDate(libraryElementModel) == BasicToolModel.Selection;
                    break;
            }
            return false;
        }
        public override void UnSelect()
        {
            BasicToolModel.SetSelected(false);
            BasicToolModel.SetLibraryIds(Filter(GetUpdatedDataList()));
            SelectionChanged?.Invoke(this);
            FireLibraryIdsChanged();
        }
        public virtual void SetSelection(string selection)
        {
            BasicToolModel.SetSelection(selection);
            BasicToolModel.SetSelected(true);
            BasicToolModel.SetLibraryIds(Filter(GetUpdatedDataList()));
            SelectionChanged?.Invoke(this);
            FireLibraryIdsChanged();
        }
        public List<string> GetAllProperties()
        {
            var libraryElementControllers = GetUpdatedDataList().Select(id => SessionController.Instance.ContentController.GetLibraryElementController(id));
            var allMetadata = new Dictionary<string, List<string>>();
            foreach (var controller in libraryElementControllers)
            {
                foreach (var kvp in GetMetadata(controller.LibraryElementModel.LibraryElementId))
                {
                    if (!allMetadata.ContainsKey(kvp.Key))
                    {
                        allMetadata.Add(kvp.Key, new List<string>());
                    }
                    allMetadata[kvp.Key].AddRange(kvp.Value);
                }
            }
            switch (BasicToolModel.Filter)
            {
                case ToolModel.ToolFilterTypeTitle.Creator:
                    return allMetadata.ContainsKey("Creator") ? allMetadata["Creator"] : new List<string>();
                case ToolModel.ToolFilterTypeTitle.Title:
                    return allMetadata.ContainsKey("Title") ? allMetadata["Title"] : new List<string>();
                case ToolModel.ToolFilterTypeTitle.Type:
                    return allMetadata.ContainsKey("Type") ? allMetadata["Type"] : new List<string>();
                case ToolModel.ToolFilterTypeTitle.Date:
                    return allMetadata.ContainsKey("Date") ? allMetadata["Date"] : new List<string>();
                case ToolModel.ToolFilterTypeTitle.LastEditedDate:
                    return allMetadata.ContainsKey("LastEditedDate") ? allMetadata["LastEditedDate"] : new List<string>();
                case ToolModel.ToolFilterTypeTitle.MetadataKeys:
                    return allMetadata.Keys.ToList();
                case ToolModel.ToolFilterTypeTitle.MetadataValues:
                    var ret = new List<string>();
                    foreach (var values in allMetadata.Values)
                    {
                        ret.AddRange(values);
                    }
                    return ret;
                    
            }
            return new List<string>();
        }
    }
}
