using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NuSysApp
{
    public class MetadataToolController : ToolController
    {
        public MetadataToolController(ToolModel model) : base(model)
        {
        }

        public MetadataToolModel MetadataToolModel
        {
            get
            {
                Debug.Assert(base.Model is MetadataToolModel);
                return base.Model as MetadataToolModel;
            }
        }

        public delegate void FilterChangedEventHandler(object sender, ToolModel.ToolFilterTypeTitle filter);
        public delegate void SelectionChangedEventHandler(object sender);

        public event FilterChangedEventHandler FilterChanged;
        public event SelectionChangedEventHandler SelectionChanged;
        

        public override Func<string, bool> GetFunc()
        {
            return IncludeInFilter;

        }

        public void SetFilter(ToolModel.ToolFilterTypeTitle filter)
        {
            MetadataToolModel.SetFilter(filter);
            FilterChanged?.Invoke(this, filter);
            FireLibraryIdsChanged();
        }

        private bool IncludeInFilter(string libraryId)
        {
            var libraryElementModel = SessionController.Instance.ContentController.GetContent(libraryId);
            if (libraryElementModel == null)
            {
                return false;
            }
            switch (MetadataToolModel.Filter)
            {
                case ToolModel.ToolFilterTypeTitle.AllMetadata:
                    var metadata = GetMetadata(libraryId);
                    if (metadata.ContainsKey(MetadataToolModel.Selection.Item1))
                    {
                        if (MetadataToolModel.Selection.Item2 == null)
                        {
                            return true;
                        }
                        else if (metadata[MetadataToolModel.Selection.Item1] ==
                           MetadataToolModel.Selection.Item2)
                        {
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }
        public void UnSelect()
        {
            MetadataToolModel.SetSelected(false);
            MetadataToolModel.SetLibraryIds(Filter(GetUpdatedDataList()));
            SelectionChanged?.Invoke(this);
            FireLibraryIdsChanged();
        }
        public virtual void SetSelection(Tuple<string, string> selection)
        {
            MetadataToolModel.SetSelection(selection);
            MetadataToolModel.SetSelected(true);
            MetadataToolModel.SetLibraryIds(Filter(GetUpdatedDataList()));
            SelectionChanged?.Invoke(this);
            FireLibraryIdsChanged();
        }

        public Dictionary<string, HashSet<string>> GetAllMetadata()
        {
            var libraryElementControllers = GetUpdatedDataList().Select(id => SessionController.Instance.ContentController.GetLibraryElementController(id));
            var allMetadata = new Dictionary<string, HashSet<string>>();
            foreach (var controller in libraryElementControllers)
            {
                foreach (var kvp in GetMetadata(controller.LibraryElementModel.LibraryElementId))
                {
                    if (!allMetadata.ContainsKey(kvp.Key))
                    {
                        allMetadata.Add(kvp.Key, new HashSet<string>());
                    }
                    allMetadata[kvp.Key].Add(kvp.Value);
                }
            }
            return allMetadata;
        }
    }
}