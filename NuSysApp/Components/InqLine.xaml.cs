using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class InqLine : UserControl, ISelectable
    {

        private bool _isHighlighting = false;
        private bool _isSelected = false;

        public InqLine()
        {
            this.InitializeComponent();
        }

        public void AddPoint(Point p)
        {
            Line.Points.Add(p);
            SelectedBorder.Points.Add(p);
        }

        public void SetHighlighting(bool highlight)
        {
            if (highlight)
            {
                _isHighlighting = true;
                Line.Stroke = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                _isHighlighting = false;
                Line.Stroke = new SolidColorBrush(Colors.Black);
            }
        }


        public void ToggleSelection()
        {
            _isSelected = !_isSelected;
            if (_isSelected)
            {
                SelectedBorder.Opacity = .8;
            }
            else
            {
                SelectedBorder.Opacity = 0;
            }
        }

        public double StrokeThickness 
        {
            get { return Line.StrokeThickness; }
            set { Line.StrokeThickness = value; }
        }

        public bool IsHighlighting
        {
            get { return _isHighlighting; }
        }

        public List<Point> Points
        {
            get { return Line.Points.ToList(); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }

            set
            {
                if (value != _isSelected)
                {
                    ToggleSelection();
                }
            }
        }
    }
}
