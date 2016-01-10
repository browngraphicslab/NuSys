using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.UI.Xaml.Shapes;
using NuSysApp.EventArgs;
using System.Collections.Concurrent;
using System.ComponentModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class InqLineView : UserControl
    {

        private bool _isHighlighting = false;
        private bool _isSelected = false;

        public InqLineView(InqLineViewModel vm)
        {
            InitializeComponent();
            CanEdit = AtomModel.EditStatus.Maybe;
            DataContext = vm;
            VisibleLine.Stroke = vm.Model.Stroke;
        }

        public InqLineView(InqLineViewModel vm, double thickness, Brush stroke)
        {
            this.InitializeComponent();
            DataContext = vm;

            VisibleLine.Stroke = stroke;
            VisibleLine.StrokeThickness = thickness;
        }


        public void Delete()
        {
            (this.Parent as InqCanvasView).ViewModel.Lines.Remove(this);
        }


        public AtomModel.EditStatus CanEdit { set; get; }

        public void SetHighlighting(bool highlight)
        {
            if (highlight)
            {
                _isHighlighting = true;
                VisibleLine.Stroke = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                _isHighlighting = false;
                VisibleLine.Stroke = new SolidColorBrush(Colors.Black);
            }
        }


        public void ToggleSelection()
        {
            _isSelected = !_isSelected;
            if (_isSelected)
            {
                                                
                //this.BorderThickness = new Thickness(Double.MaxValue);
            }
            else
            {
                this.BorderThickness = new Thickness(0);
            }
        }

        public double StrokeThickness
        {
            get { return VisibleLine.StrokeThickness; }
            set { VisibleLine.StrokeThickness = value; }
        }

        public Brush Stroke
        {
            get { return VisibleLine.Stroke; }
            set { VisibleLine.Stroke = value; }

        }

        public bool IsHighlighting
        {
            get { return _isHighlighting; }
        }

        public List<Point> Points
        {
            get { return VisibleLine.Points.ToList(); }
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
        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {

            var inqCanvas = Parent as InqCanvasView;
            if (inqCanvas?.Mode is EraseInqMode && inqCanvas.IsPressed)
            {
                //NetworkConnector.Instance.RequestDeleteSendable((DataContext as InqLineViewModel).Model.Id);
            }
        }
    }
}

