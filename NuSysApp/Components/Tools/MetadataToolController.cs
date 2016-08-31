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
            FireOutputLibraryIdsChanged();
        }

        /// <summary>
        /// Given a library ID, this function returns a bool on whether or not this property should be included in its output library ids based on the filter and the selection
        /// </summary>
        private bool IncludeInFilter(string libraryId)
        {
            var libraryElementModel = SessionController.Instance.ContentController.GetLibraryElementModel(libraryId);
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
                        if (MetadataToolModel.Selection.Item2 == null || MetadataToolModel.Selection.Item2.Count == 0)
                        {
                            return true;
                        }
                        else if (metadata[MetadataToolModel.Selection.Item1].Keys.Intersect(
                           MetadataToolModel.Selection.Item2).Any())
                        {
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Sets the Selected boolean to false, clears the current selection tuple, and invokes the selection changed event. Then refreshes its output library ids now that there is no selection and invokes the output library ids changed event
        /// </summary>
        public override void UnSelect()
        {
            MetadataToolModel.SetSelection(new Tuple<string, HashSet<string>>(null, new HashSet<string>()));
            MetadataToolModel.SetSelected(false);
            MetadataToolModel.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
            SelectionChanged?.Invoke(this);
            FireOutputLibraryIdsChanged();
        }

        /// <summary>
        /// Sets the Selection, and invokes the selection changed event. Then refreshes its output library ids now that there is a new selection and invokes the output library ids changed event
        /// </summary>
        public virtual void SetSelection(Tuple<string, HashSet<string>> selection)
        {
            MetadataToolModel.SetSelection(selection);
            MetadataToolModel.SetSelected(true);
            MetadataToolModel.SetOutputLibraryIds(Filter(GetUpdatedDataList()));
            SelectionChanged?.Invoke(this);
            FireOutputLibraryIdsChanged();
        }

        /// <summary>
        /// Returns the dictionary (from key to set of values) to display. If you want to RELOAD ALL LIBRARY ELEMENTS from the start of the filter chain
        /// set recursivelyRefresh = true. By Default it is false.
        /// </summary>
        public Dictionary<string, Dictionary<string, int>> GetAllMetadata()
        {
            var libraryElementControllers = GetUpdatedDataList().Select(id => SessionController.Instance.ContentController.GetLibraryElementController(id));
            var allMetadata = new Dictionary<string, Dictionary<string, int>>();
            foreach (var controller in libraryElementControllers)
            {
                foreach (var kvp in GetMetadata(controller.LibraryElementModel.LibraryElementId))
                {
                    if (!allMetadata.ContainsKey(kvp.Key))
                    {
                        allMetadata.Add(kvp.Key, new Dictionary<string, int>());
                    }
                    foreach(var metadataValue in kvp.Value)
                    {
                        if (allMetadata[kvp.Key].ContainsKey(metadataValue.Key))
                        {
                            allMetadata[kvp.Key][metadataValue.Key] += metadataValue.Value;
                        }
                        else
                        {
                            allMetadata[kvp.Key].Add(metadataValue.Key, metadataValue.Value);
                        }
                    }
                    //allMetadata[kvp.Key] = new HashSet<string>(allMetadata[kvp.Key].Concat(kvp.Value));
                }
            }
            return allMetadata;
        }
    }
}