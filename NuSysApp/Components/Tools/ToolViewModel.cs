using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Util;

namespace NuSysApp
{
    public class ToolViewModel : ElementViewModel
    {
        private ToolController _controller;
        public ToolViewModel(ToolController toolController) : base(toolController)
        {
            _controller = toolController;
            _controller.LibraryIdsChanged += ControllerOnLibraryIdsChanged;
        }

        private void ControllerOnLibraryIdsChanged(object sender, HashSet<string> libraryIds)
        {
            PropertiesToDisplay = new ObservableCollection<string>(_controller.GetAllProperties());
        }

        public void SetSelection(string selection)
        {
            _controller.SetSelection(selection);
        }

        public ToolModel.FilterTitle Filter { get { return _controller.Model.Filter;}  set { _controller.SetFilter(value);} }

        public void AddChildFilter(ToolController controller)
        {
            controller.AddParent(_controller);
            
        }

        public void reloadPropertiesToDisplay()
        {
            PropertiesToDisplay = new ObservableCollection<string>(_controller.GetAllProperties());
        }

        public ObservableCollection<string> PropertiesToDisplay { get; set; }
    }
}