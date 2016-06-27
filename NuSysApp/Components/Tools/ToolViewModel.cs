using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Util;

namespace NuSysApp
{
    public class ToolViewModel : BaseINPC
    {
        public delegate void PropertiesToDisplayChangedEventHandler(string selection);
        public event PropertiesToDisplayChangedEventHandler PropertiesToDisplayChanged;
        public ToolController Controller {get { return _controller; } }
        private ToolController _controller;
        private double _width;
        private double _height;
        private double _x;
        private double _y;
        private CompositeTransform _transform = new CompositeTransform();
        public double Width
        {
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
            get
            {
                return _width;
            }
        }

        public double Height
        {
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
            get
            {
                return _height;
            }
        }

        public double X
        {
            set
            {
                _x = value;
                RaisePropertyChanged("X");
            }
            get
            {
                return _x;
            }
        }
        public double Y
        {
            set
            {
                _y = value;
                RaisePropertyChanged("Y");
            }
            get
            {
                return _y;
            }
        }
        public CompositeTransform Transform
        {
            get { return _transform; }
            set
            {
                if (_transform == value)
                {
                    return;
                }
                _transform = value;
                RaisePropertyChanged("Transform");
            }
        }
        public ToolViewModel(ToolController toolController) 
        {
            _controller = toolController;
            _controller.LibraryIdsChanged += ControllerOnLibraryIdsChanged;
            Controller.SizeChanged += OnSizeChanged;
            Controller.LocationChanged += OnLocationChanged;
            PropertiesToDisplay = new ObservableCollection<string>();
            PropertiesToDisplayUnique = new ObservableCollection<string>();
        }

        public void OnSizeChanged(object sender, double width, double height)
        {
            Width = width;
            Height = height;
        }

        public void OnLocationChanged(object sender, double x, double y)
        {
            X = x;
            Y = y;
            Transform.TranslateX = x;
            Transform.TranslateY = y;
            RaisePropertyChanged("Transform");
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
            HashSet<string> hs = new HashSet<string>();
            PropertiesToDisplay.Clear();
            PropertiesToDisplayUnique.Clear();
            foreach (var item in temp)
            {
                PropertiesToDisplay.Add(item);
                if (!hs.Contains(item))
                {
                    hs.Add(item);
                    PropertiesToDisplayUnique.Add(item);
                }
            }
        }

        public ObservableCollection<string> PropertiesToDisplay { get; set; }

        public ObservableCollection<string> PropertiesToDisplayUnique { get; set; }
    }
}