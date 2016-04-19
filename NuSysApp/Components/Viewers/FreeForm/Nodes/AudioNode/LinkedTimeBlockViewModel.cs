using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using MyToolkit.Utilities;

namespace NuSysApp.Nodes.AudioNode
{
    public class LinkedTimeBlockViewModel : BaseINPC
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _startRatio;
        private double _endRatio;
        private int _startTime;
        private int _endTime;
        private ProgressBar _scrubBar;
        private ProgressBar _scrubBar2;
        private Boolean _onBlock = false;

        private ObservableCollection<Tuple<IThumbnailable, Image>> _nodeImageTuples;
        
        private Dictionary<string, Object> _line1;
        private double _detailx1;
        private double _detailx2;
        
        public LinkedTimeBlockModel Model { get; set; }

        public double Detailx2 {get { return _detailx2; }set{ _detailx2 = value; RaisePropertyChanged("Detailx2");}}

        public double Detailx1{get { return _detailx1; }set {_detailx1 = value;RaisePropertyChanged("Detailx1");}}
        
        public LinkedTimeBlockViewModel(LinkedTimeBlockModel model,  TimeSpan totalAudioDuration, ProgressBar scrubBar)
        {
            _scrubBar = scrubBar;
            Debug.WriteLine(model.Start.TotalMilliseconds);
            _startRatio = model.Start.TotalMilliseconds / totalAudioDuration.TotalMilliseconds;
            _endRatio = model.End.TotalMilliseconds / totalAudioDuration.TotalMilliseconds;
            _startTime = (int)(_startRatio * scrubBar.Maximum);
            _endTime = (int)(_endRatio * scrubBar.Maximum);
            _line1 = new Dictionary<string, Object>();
            Model = model;

            _nodeImageTuples = new ObservableCollection<Tuple<IThumbnailable, Image>>();
            //foreach (var element in model.LinkedNodes)
            //{
            //    _nodeImageTuples.Add(new Tuple<IThumbnailable, Image>(element, null));
            //}
            
            this.setUpLine1();
            RefreshThumbnail();

        }

        

        public async Task RefreshThumbnail()
        {
            ObservableCollection<Tuple<IThumbnailable, Image>> temp = new ObservableCollection<Tuple<IThumbnailable, Image>>();
            //_thumbnails.Clear();
            foreach (var element in _nodeImageTuples)
            {
                Image image = new Image();
                image.Source = await PreviewLink(element.Item1);
                image.Height = 100;
                image.Width = 100;
                //_thumbnails.Add(image);
                temp.Add(new Tuple<IThumbnailable, Image>(element.Item1, image));
                image.PointerPressed += Thumbnail_OnPointerPressed;
                image.PointerReleased += Thumbnail_OnPointerReleased;

            }
            _nodeImageTuples = temp;
        }


        private async Task<RenderTargetBitmap> PreviewLink(IThumbnailable node)
        {
            return await node.ToThumbnail(100, 100);

        }

        public void setUpLine1()
        {
            double x = EndRatio * _scrubBar.ActualWidth;
            double y = StartRatio * _scrubBar.ActualWidth;
            _line1.Add("StrokeThickness", _scrubBar.ActualHeight);
            _line1.Add("Stroke", new SolidColorBrush(Colors.Aqua));
            _line1.Add("Opacity", 0.3);
            this.Detailx1 = y + Canvas.GetLeft(_scrubBar) + _scrubBar.Margin.Left;
            this.Detailx2 = x + Canvas.GetLeft(_scrubBar) + _scrubBar.Margin.Left;
            _line1.Add("Y", Canvas.GetTop(_scrubBar) + (double)_line1["StrokeThickness"] / 2);
            _line1.Add("TopMargin", _scrubBar.Margin.Top);
        }

        public void ResizeLine1()
        {
            double x = _endRatio * _scrubBar.ActualWidth;
            Debug.WriteLine(x);
            double y = _startRatio * _scrubBar.ActualWidth;
            Detailx1 = y + Canvas.GetLeft(_scrubBar) + _scrubBar.Margin.Left;
            Detailx2 = x + Canvas.GetLeft(_scrubBar) + _scrubBar.Margin.Left;
            _line1["Y"] = Canvas.GetTop(_scrubBar) + (double)_line1["StrokeThickness"] / 2;
            _line1["TopMargin"] = _scrubBar.Margin.Top;
        }
        
        public double StartTime
        {
            get { return _startTime; }
        }

        public double EndTime
        {
            get { return _endTime; }
        }

        public bool OnBlock
        {
            get { return _onBlock; }
            set { _onBlock = value; }
        }

        public double StartRatio
        {
            get { return _startRatio; }
        }

        public double EndRatio
        {
            get { return _endRatio; }
            set { _endRatio = value; }
        }


        public Dictionary<string, object> Line1
        {
            get { return _line1; }
        }

        public ObservableCollection<Tuple<IThumbnailable, Image>> NodeImageTuples
        {
            get { return _nodeImageTuples; }
        }


        public void setUpHandlers(Line line)
        {
            line.PointerPressed += Line_OnPointerPressed;
            line.PointerReleased += Line_OnPointerReleased;
        }

        //public Collection<Image> Thumbnails
        //{
        //    get { return _thumbnails; }
        //}

        

        public bool HasLinkedNode()
        {
            return _nodeImageTuples.Count != 0;
        }


        private void Thumbnail_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<UIElement> elementStack = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(null).Position, null);
            bool remove = true;
            foreach (var element in elementStack)
            {
                if (((FrameworkElement)element).DataContext != null)
                {
                    if ((FrameworkElement)element == ((FrameworkElement)sender).Parent)
                    {
                        remove = false;
                    }
                }

            }
            if (remove == true)
            {
                for (int i = 0; i < _nodeImageTuples.Count; i++)
                {
                    if (_nodeImageTuples[i].Item2 == sender)
                    {
                        GridView gridView = (GridView)((FrameworkElement)sender).Parent;
                        if (gridView != null)
                        {
                            gridView.Items.Remove((Image)sender);

                        }
                        //Model.LinkedNodes.Remove(_nodeImageTuples[i].Item1);
                        _nodeImageTuples.RemoveAt(i);
                        OnBlock = false;
                        ((Image)sender).ReleasePointerCapture(e.Pointer);


                        return;
                    }
                }
            }
        }

        private void Thumbnail_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {

            ((Image)sender).CapturePointer(e.Pointer);
        }

        private void Line_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IEnumerable<UIElement> elementStack = VisualTreeHelper.FindElementsInHostCoordinates(e.GetCurrentPoint(null).Position, null);
            foreach (var element in elementStack)
            {
                if (((FrameworkElement)element).DataContext != null)
                {
                    if (((FrameworkElement)element).DataContext is ElementViewModel && element is IThumbnailable)
                    {
                        //Model.LinkedNodes.Add((IThumbnailable)element);
                        _nodeImageTuples.Add(new Tuple<IThumbnailable, Image>((IThumbnailable)element, null));
                        OnBlock = false;
                        return;
                    }
                }

            }
            ((Line)sender).ReleasePointerCapture(e.Pointer);

        }

        private void Line_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {

            ((Line)sender).CapturePointer(e.Pointer);
        }
    }
}