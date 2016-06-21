using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Nodes.AudioNode;

namespace NuSysApp
{
    public class AudioRegionViewModel : RegionViewModel 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void DoubleChanged(object sender, double e);
        public event DoubleChanged WidthChanged;
        public event DoubleChanged Bound1Changed;
        public event DoubleChanged Bound2Changed;

        public double LeftHandleX
        {
            get
            {
                var model = this.Model as TimeRegionModel;
                return RegionWidth * model.Start;
            } 
        }
        public double LefthandleY1 { get; set; }
        public double LefthandleY2 { get; set; }

        public double RightHandleX
        {
            get
            {
                var model = this.Model as TimeRegionModel;
                return RegionWidth * model.End;
            } 
        }
        public double RightHandleY1 { get;  }
        public double RightHandleY2 { get; set; }
        public double RegionHeight { get; set; }
        public double RegionWidth { get; set; }
        public Boolean Editable { get; set; }

        public AudioRegionViewModel(TimeRegionModel model, LibraryElementController controller, RegionController regionController, Sizeable sizeable) : base(model,controller, regionController, sizeable)
        {
            ContainerSizeChanged += BaseSizeChanged;
            RegionWidth = sizeable.GetWidth();
            LefthandleY1 = 10;
            LefthandleY2 = 110; //+ contentView.ActualHeight;
            RightHandleY1 = 10;
            RightHandleY2 = 110; //+ contentView.ActualHeight;
            WidthChanged?.Invoke(this,RegionWidth);
            Bound1Changed?.Invoke(this,LeftHandleX);
            Bound2Changed?.Invoke(this,RightHandleX);
        }
        private void BaseSizeChanged(object sender, double width, double height)
        {
            var model = Model as TimeRegionModel;
            if (model == null)
            {
                return;
            }
//            Height = (model.BottomRightPoint.Y - model.TopLeftPoint.Y)*height;
            RegionWidth = (model.End - model.Start)*width;
            RaisePropertyChanged("RegionWidth");
            WidthChanged?.Invoke(this,RegionWidth);
        }

        public void SetNewPoints(double Start, double End)
        {
            var model = Model as TimeRegionModel;
            if (model == null)
            {
                return;
            }
            model.Start += Start / ContainerViewModel.GetWidth();
            model.End += End / ContainerViewModel.GetWidth();
            RegionWidth = (model.End - model.Start)*ContainerViewModel.GetWidth();
            Controller.UpdateRegion(model);

            RaisePropertyChanged("LeftHandleX");
            RaisePropertyChanged("RightHandleX");
            RaisePropertyChanged("RegionWidth");
        }

        //private int _startTime;
        //private int _endTime;
        //public ProgressBar _scrubBar;
        //private ProgressBar _scrubBar2;
        //private Boolean _onBlock = false;
        //private ObservableCollection<Tuple<IThumbnailable, Image>> _nodeImageTuples;
        //private Dictionary<string, Object> _line1;
        //private double _detailx1;
        //private double _detailx2;

        //public TimeRegionModel Model { get; set; }

        //public double Detailx2 { get { return _detailx2; } set { _detailx2 = value; RaisePropertyChanged("Detailx2"); } }

        //public double Detailx1 { get { return _detailx1; } set { _detailx1 = value; RaisePropertyChanged("Detailx1"); } }

        //public AudioRegionViewModel(TimeRegionModel model, ProgressBar scrubBar)
        //{
            
        //    _scrubBar = scrubBar;
        //    _startTime = (int)(model.Start * scrubBar.Maximum);
        //    _endTime = (int)(model.End * scrubBar.Maximum);
        //    Model = model;

        //    _nodeImageTuples = new ObservableCollection<Tuple<IThumbnailable, Image>>();
            
        //    //this.setUpLine1();

            

        //}

        //public void SetStart(TimeSpan time)
        //{
        //    double time1 = time.TotalMilliseconds;
        //    Model.Start = time1;
        //    _startTime = (int)(time1 * _scrubBar.Maximum);
        //    this.setUpLine1();


        //}

        //public void SetEnd(TimeSpan time)
        //{
        //    double time2 = time.TotalMilliseconds;
        //    Model.End = time2;
        //    _endTime = (int)(time2 * _scrubBar.Maximum);
        //    this.setUpLine1();         
        //}



        //public async Task RefreshThumbnail()
        //{
        //    ObservableCollection<Tuple<IThumbnailable, Image>> temp = new ObservableCollection<Tuple<IThumbnailable, Image>>();
           
        //    foreach (var element in _nodeImageTuples)
        //    {
        //        Image image = new Image();
        //        image.Source = await PreviewLink(element.Item1);
        //        image.Height = 100;
        //        image.Width = 100;
               
        //        temp.Add(new Tuple<IThumbnailable, Image>(element.Item1, image));
        //        image.PointerPressed += Thumbnail_OnPointerPressed;
        //        image.PointerReleased += Thumbnail_OnPointerReleased;

        //    }
        //    _nodeImageTuples = temp;
        //}


        //private async Task<RenderTargetBitmap> PreviewLink(IThumbnailable node)
        //{
        //    return await node.ToThumbnail(100, 100);

        //}

        //public void setUpLine1()
        //{
        //    _line1 = new Dictionary<string, Object>();
        //    double x = EndRatio * _scrubBar.ActualWidth;
        //    double y = StartRatio * _scrubBar.ActualWidth;
        //    _line1.Add("StrokeThickness", _scrubBar.ActualHeight);
        //    _line1.Add("Stroke", new SolidColorBrush(Colors.Aqua));
        //    _line1.Add("Opacity", 0.3);
        //    this.Detailx1 = y + Canvas.GetLeft(_scrubBar) + _scrubBar.Margin.Left;
        //    this.Detailx2 = x + Canvas.GetLeft(_scrubBar) + _scrubBar.Margin.Left;
        //    _line1.Add("Y", Canvas.GetTop(_scrubBar) + (double)_line1["StrokeThickness"] / 2);
        //    _line1.Add("TopMargin", _scrubBar.Margin.Top);
        //}

        //public void ResizeLine1()
        //{
        //    double x = Model.End * _scrubBar.ActualWidth;
        //    Debug.WriteLine(x);
        //    double y = Model.Start * _scrubBar.ActualWidth;
        //    Detailx1 = y + Canvas.GetLeft(_scrubBar) + _scrubBar.Margin.Left;
        //    Detailx2 = x + Canvas.GetLeft(_scrubBar) + _scrubBar.Margin.Left;
        //    _line1["Y"] = Canvas.GetTop(_scrubBar) + (double)_line1["StrokeThickness"] / 2;
        //    _line1["TopMargin"] = _scrubBar.Margin.Top;
        //}

        //public double StartTime
        //{
        //    get { return _startTime; }
        //}

        //public double EndTime
        //{
        //    get { return _endTime; }
        //}

        //public bool OnBlock
        //{
        //    get { return _onBlock; }
        //    set { _onBlock = value; }
        //}

        //public double StartRatio
        //{
        //    get { return Model.Start; }
        //}

        //public double EndRatio
        //{
        //    get { return Model.End; }
        //}


        //public Dictionary<string, object> Line1
        //{
        //    get { return _line1; }
        //}

        //public ObservableCollection<Tuple<IThumbnailable, Image>> NodeImageTuples
        //{
        //    get { return _nodeImageTuples; }
        //}


        //public void setUpHandlers(Line line)
        //{
        //    line.PointerPressed += Line_OnPointerPressed;
        //    line.PointerReleased += Line_OnPointerReleased;
        //}

     
        //public bool HasLinkedNode()
        //{
        //    return _nodeImageTuples.Count != 0;
        //}


        //private void Thumbnail_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        //{
        //    IEnumerable<UIElement> elementStack = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(null).Position, null);
        //    bool remove = true;
        //    foreach (var element in elementStack)
        //    {
        //        if (((FrameworkElement)element).DataContext != null)
        //        {
        //            if ((FrameworkElement)element == ((FrameworkElement)sender).Parent)
        //            {
        //                remove = false;
        //            }
        //        }

        //    }
        //    if (remove == true)
        //    {
        //        for (int i = 0; i < _nodeImageTuples.Count; i++)
        //        {
        //            if (_nodeImageTuples[i].Item2 == sender)
        //            {
        //                GridView gridView = (GridView)((FrameworkElement)sender).Parent;
        //                if (gridView != null)
        //                {
        //                    gridView.Items.Remove((Image)sender);

        //                }
              
        //                _nodeImageTuples.RemoveAt(i);
        //                OnBlock = false;
        //                ((Image)sender).ReleasePointerCapture(e.Pointer);


        //                return;
        //            }
        //        }
        //    }
        //}

        //private void Thumbnail_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        //{

        //    ((Image)sender).CapturePointer(e.Pointer);
        //}

        //private void Line_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        //{
        //    IEnumerable<UIElement> elementStack = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(null).Position, null);
        //    foreach (var element in elementStack)
        //    {
        //        if (((FrameworkElement)element).DataContext != null)
        //        {
        //            if (((FrameworkElement)element).DataContext is ElementViewModel && element is IThumbnailable)
        //            {
        //                _nodeImageTuples.Add(new Tuple<IThumbnailable, Image>((IThumbnailable)element, null));
        //                OnBlock = false;
        //                return;
        //            }
        //        }

        //    }
        //    ((Line)sender).ReleasePointerCapture(e.Pointer);

        //}

        //private void Line_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        //{

        //    ((Line)sender).CapturePointer(e.Pointer);
        //}
    }

}
