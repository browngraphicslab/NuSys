using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Util;

namespace NuSysApp
{
    public class ToolViewModel : ElementViewModel
    {
        public delegate void PropertiesToDisplayChangedEventHandler(string selection);
        public event PropertiesToDisplayChangedEventHandler PropertiesToDisplayChanged;

        private ToolController _controller;
        public ToolViewModel(ToolController toolController) : base(toolController)
        {
            _controller = toolController;
            _controller.LibraryIdsChanged += ControllerOnLibraryIdsChanged;
            PropertiesToDisplay = new ObservableCollection<string>();
        }

        private void ControllerOnLibraryIdsChanged(object sender, HashSet<string> libraryIds)
        {
            reloadPropertiesToDisplay();
            
            if (_controller.Model.Selection != null && !PropertiesToDisplay.Contains(_controller.Model.Selection))
            {
                _controller.UnSelect();
                reloadPropertiesToDisplay();
            }
            PropertiesToDisplayChanged?.Invoke(_controller.Model.Selection);
        }



        public string Selection { get { return _controller.Model.Selection; } set { _controller.SetSelection(value);} }

        public ToolModel.FilterTitle Filter { get { return _controller.Model.Filter;}  set { _controller.SetFilter(value);} }

        public void AddChildFilter(ToolController controller)
        {
            controller.AddParent(_controller);
            
        }

        public void reloadPropertiesToDisplay()
        {
            var temp = new ObservableCollection<string>(_controller.GetAllProperties());
            PropertiesToDisplay.Clear();
            foreach (var item in temp)
            {
                PropertiesToDisplay.Add(item);
            }
        }

        public ObservableCollection<string> PropertiesToDisplay { get; set; }
    }
}