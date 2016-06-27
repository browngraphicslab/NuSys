using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Util;

namespace NuSysApp
{
    public class ToolViewModel : BaseINPC
    {
        public ToolController Controller;
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
                _y= value;
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
            Controller = toolController;
            Controller.LibraryIdsChanged += ControllerOnLibraryIdsChanged;
            Controller.SizeChanged += OnSizeChanged;
            Controller.LocationChanged += OnLocationChanged;
        }

        private void ControllerOnLibraryIdsChanged(object sender, HashSet<string> libraryIds)
        {
            PropertiesToDisplay = new ObservableCollection<string>(Controller.GetAllProperties());
        }

        public void SetSelection(string selection)
        {
            Controller.SetSelection(selection);
        }

        public ToolModel.FilterTitle Filter { get { return Controller.Model.Filter;}  set { Controller.SetFilter(value);} }

        public void CreateNewToolWindow(Canvas canvas, double x, double y)
        {
            ToolModel model = new ToolModel();
            ToolController controller = new ToolController(model);
            controller.AddParent(controller);
            ToolViewModel viewmodel = new ToolViewModel(controller);
            TemporaryToolView view = new TemporaryToolView(viewmodel, x, y);
            canvas.Children.Add(view);
            
        }

        public ObservableCollection<string> PropertiesToDisplay
        {
            get { return new ObservableCollection<string>(Controller.GetAllProperties()); }
            set { PropertiesToDisplay = value; }
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
    }
}