using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ToolController
    {
        public static Dictionary<string, ToolController> ToolControllers = new Dictionary<string, ToolController>();
        public delegate void FilterChangedEventHandler(object sender, ToolModel.FilterTitle filter);
        public delegate void SelectionChangedEventHandler(object sender, string selection);
        public delegate void LibraryIdsChangedEventHandler(object sender, HashSet<string> libraryIds);

        public event FilterChangedEventHandler FilterChanged;
        public event SelectionChangedEventHandler SelectionChanged;
        public event LibraryIdsChangedEventHandler LibraryIdsChanged;

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

        public void Dispose()
        {
            foreach(var parentController in Model.ParentIds.Select(id => ToolControllers.ContainsKey(id) ? ToolControllers[id] : null))
            {
                RemoveParent(parentController);
            }
        }
        private void ParentFilterChanged(object sender, ToolModel.FilterTitle filter)
        {
            Model.SetLibraryIds(GetUpdatedDataList());
            LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
        }
        private void ParentSelectionChanged(object sender, string selection)
        {
            Model.SetLibraryIds(GetUpdatedDataList());
            LibraryIdsChanged?.Invoke(this, Model.LibraryIds);
        }
        private void ParentLibraryIdsChanged(object sender, HashSet<string> libraryIds)
        {
            Model.SetLibraryIds(GetUpdatedDataList());
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
