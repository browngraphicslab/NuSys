using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Util;

namespace NuSysApp
{
    public class ChartSlice
    {
        public string Name { get; set; }
        public int Amount { get; set; }
    }
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
            PropertiesToDisplayPieChart = new ObservableCollection<ChartSlice>();
            Height = 400;
            Width = 260;
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
            if (_controller.Model.Selection != null && !PropertiesToDisplay.Contains(_controller.Model.Selection))
            {
                _controller.UnSelect();
                temp = new ObservableCollection<string>(_controller.GetAllProperties());
            }

            PieChartDictionary = new Dictionary<string, int>();
            PropertiesToDisplay.Clear();
            PropertiesToDisplayUnique.Clear();
            PropertiesToDisplayPieChart = new ObservableCollection<ChartSlice>();
            foreach (var item in temp)
            {
                if (item != null)
                {
                    PropertiesToDisplay.Add(item);
                    if (!PieChartDictionary.ContainsKey(item))
                    {
                        PieChartDictionary.Add(item, 1);
                        PropertiesToDisplayUnique.Add(item);
                    }
                    else
                    {
                        PieChartDictionary[item] = PieChartDictionary[item] + 1;
                    }
                }
            }
            //foreach (var item in dic)
            //{
            //    ChartSlice slice = new ChartSlice();
            //    slice.Name = item.Key;
            //    slice.Amount = item.Value;
            //    PropertiesToDisplayPieChart.Add(slice);
            //}
            PropertiesToDisplayChanged?.Invoke(_controller.Model.Selection);

        }

        public Dictionary<string, int> PieChartDictionary { get; set; }

        public ObservableCollection<string> PropertiesToDisplay { get; set; }

        public ObservableCollection<string> PropertiesToDisplayUnique { get; set; }

        public ObservableCollection<ChartSlice> PropertiesToDisplayPieChart { get; set; } 
    }
}