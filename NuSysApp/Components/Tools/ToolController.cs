using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyToolkit.Utilities;

namespace NuSysApp
{
    public class ToolController
    {
        public static Dictionary<string, ToolController> ToolControllers = new Dictionary<string, ToolController>();
        public delegate void FilterChangedEventHandler(object sender, ToolModel.FilterTitle filter);
        public delegate void SelectionChangedEventHandler(object sender, string selection);
        public delegate void LibraryIdsChangedEventHandler(object sender, HashSet<string> libraryIds);
        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public delegate void LocationChangedEventHandler(object sender, double x, double y);

        public event FilterChangedEventHandler FilterChanged;
        public event SelectionChangedEventHandler SelectionChanged;
        public event LibraryIdsChangedEventHandler LibraryIdsChanged;
        public event SizeChangedEventHandler SizeChanged;
        public event LocationChangedEventHandler LocationChanged;

        public ToolModel Model { get;}
        public ToolController(ToolModel model) 
        {
            Debug.Assert(model != null);
            Model = model;
            ToolControllers.Add(model.Id, this);
        }

        public void SetFilter(ToolModel.FilterTitle filter)
        {
            Model.SetFilter(filter);
            FilterChanged?.Invoke(this,filter);
        }

        public void UnSelect()
        {
            Model.SetSelection(null);
            SelectionChanged?.Invoke(this, null);
        }
        public void SetSelection(string selection)
        {
            Model.SetSelection(selection);
            SelectionChanged?.Invoke(this,selection);
        }
        public void MakeStartOfChain()
        {
            Debug.Assert(Model.ParentIds.Count == 0);
            Model.SetLibraryIds(SessionController.Instance.ContentController.IdList);
            LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
        }
        public void AddParent(ToolController parentController)
        {
            if (parentController != null)
            {
                if (Model.ParentIds.Add(parentController.Model?.Id))
                {
                    parentController.FilterChanged += ParentFilterChanged;
                    parentController.LibraryIdsChanged += ParentLibraryIdsChanged;
                    parentController.SelectionChanged += ParentSelectionChanged;
                    Model.SetLibraryIds(GetUpdatedDataList());
                    LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
                }
            }
        }

        public void RemoveParent(ToolController parentController)
        {
            if (Model.RemoveParentId(parentController?.Model?.Id))
            {
                if (parentController != null)
                {
                    parentController.FilterChanged -= ParentFilterChanged;
                    parentController.LibraryIdsChanged -= ParentLibraryIdsChanged;
                    parentController.SelectionChanged -= ParentSelectionChanged;
                }
            }
        }

        public List<string> GetAllProperties()
        {
            var libraryElementControllers = Model.LibraryIds.Select( id => SessionController.Instance.ContentController.GetLibraryElementController(id));
            var allMetadata = new Dictionary<string, List<string>>();
            foreach (var controller in libraryElementControllers)
            {
                foreach (var kvp in GetMetadata(controller.LibraryElementModel.LibraryElementId))
                {
                    if (!allMetadata.ContainsKey(kvp.Key))
                    {
                        allMetadata.Add(kvp.Key, new List<string>());
                    }
                    allMetadata[kvp.Key].Add(kvp.Value);
                }
            }
            switch (Model.Filter)
            {
                case ToolModel.FilterTitle.Creator:
                    return allMetadata.ContainsKey("Creator") ? allMetadata["Creator"] : new List<string>();
                    break;
                case ToolModel.FilterTitle.Title:
                    return allMetadata.ContainsKey("Title") ? allMetadata["Title"] : new List<string>();
                    break;
                case ToolModel.FilterTitle.Type:
                    return allMetadata.ContainsKey("Type") ? allMetadata["Type"] : new List<string>();
                    break;
                case ToolModel.FilterTitle.Date:
                    return allMetadata.ContainsKey("Date") ? allMetadata["Date"] : new List<string>();
                    break;
                case ToolModel.FilterTitle.MetadataKeys:
                    return allMetadata.Keys.ToList();
                    break;
                case ToolModel.FilterTitle.MetadataValues:
                    var ret = new List<string>();
                    foreach (var values in allMetadata.Values)
                    {
                        ret.AddRange(values);
                    }
                    return ret;
                    break;
            }
            return new List<string>();
        }

        public void SetSize(double width, double height)
        {
            SizeChanged?.Invoke(this,width,height);
        }

        public void SetLocation(double x, double y)
        {
            LocationChanged?.Invoke(this,x,y);
        }
        public void Dispose()
        {
            foreach(var parentController in Model.ParentIds.Select(id => ToolControllers.ContainsKey(id) ? ToolControllers[id] : null))
            {
                RemoveParent(parentController);
            }
        }

        private HashSet<string> Filter(HashSet<string> ids)
        {
            if (Model.Selection == null)
            {
                return ids;
            }
            var ret = new HashSet<string>();
            foreach (string id in ids)
            {
                if (IncludeInFilter(id, Model.Selection))
                {
                    ret.Add(id);
                }
            }
            return ret;
        }

        private bool IncludeInFilter(string libraryId, string selection)
        {
            var libraryElementModel = SessionController.Instance.ContentController.GetContent(libraryId);
            if (libraryElementModel == null)
            {
                return false;
            }
            switch (Model.Filter)
            {
                case ToolModel.FilterTitle.Title:
                    return libraryElementModel.Title == selection;
                    break;
                case ToolModel.FilterTitle.Type:
                    return libraryElementModel.Type.ToString().ToLower().Equals(selection.ToLower());
                    break;
                case ToolModel.FilterTitle.Creator:
                    return libraryElementModel.Creator == selection;
                    break;
                case ToolModel.FilterTitle.Date:
                    return GetDate(libraryElementModel) == selection;
                    break;
                case ToolModel.FilterTitle.MetadataKeys:
                    return libraryElementModel.Metadata.ContainsKey(selection);
                    break;
                case ToolModel.FilterTitle.MetadataValues:
                    return libraryElementModel.Metadata.Any(item => item.Value.Item1 == selection);
                    break;
            }
            return false;
        }

        private string GetDate(LibraryElementModel libraryElementModel)
        {
            return DateTime.Parse(libraryElementModel.Timestamp).ToStartOfDay().ToString();
        }
        private Dictionary<string, string> GetMetadata(string libraryId)
        {
            var element = SessionController.Instance.ContentController.GetContent(libraryId);
            if (element != null)
            {
                var metadata = element.Metadata.ToDictionary(k=>k.Key,v=>v.Value.Item1);
                metadata["Title"] = element.Title;
                metadata["Type"] = element.Type.ToString();
                metadata["Date"] = GetDate(element);
                metadata["Creator"] = element.Creator;
                return metadata;
            }
            return new Dictionary<string, string>();
        }
        private void ParentFilterChanged(object sender, ToolModel.FilterTitle filter)
        {
            Model.SetLibraryIds(Filter(GetUpdatedDataList()));
            LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
        }
        private void ParentSelectionChanged(object sender, string selection)
        {
            Model.SetLibraryIds(Filter(GetUpdatedDataList()));
            LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
        }
        private void ParentLibraryIdsChanged(object sender, HashSet<string> libraryIds)
        {
            Model.SetLibraryIds(Filter(GetUpdatedDataList()));
            LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
        }
        private HashSet<string> GetUpdatedDataList()
        {
            var controllers = new List<ToolController>(Model.ParentIds.Select(item => ToolControllers.ContainsKey(item) ? ToolControllers[item] : null));
            var list = new List<string>();
            foreach (var enumerable in controllers.Select(controller => controller?.Model.LibraryIds))
            {
                list.AddRange(enumerable ?? new HashSet<string>());
            }
            return new HashSet<string>(list);
        }

    }
}
