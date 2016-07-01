using System.Collections.Generic;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class ToolViewModel : BaseINPC
    {
        public delegate void PropertiesToDisplayChangedEventHandler();
        public event PropertiesToDisplayChangedEventHandler PropertiesToDisplayChanged;

        public ToolController Controller { get { return _controller; } }
        protected ToolController _controller;
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

        public void InvokePropertiesToDisplayChanged()
        {
            PropertiesToDisplayChanged?.Invoke();
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
            Height = 400;
            Width = 260;
        }

        public void AddChildFilter(ToolController controller)
        {
            controller.AddParent(_controller);

        }

        private void ControllerOnLibraryIdsChanged(object sender, HashSet<string> libraryIds)
        {
            ReloadPropertiesToDisplay();
        }

        protected abstract void ReloadPropertiesToDisplay();

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