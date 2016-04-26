using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Nodes.AudioNode;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp.Components.Nodes
{
    public sealed partial class LinkedTimeBlock : UserControl
    {
        private bool _handleOnePressed;
        public delegate void TimeChangeHandler();
        public event TimeChangeHandler OnTimeChange;
        public static TextBox _box1;
        //private TextBox _box2;
        private bool hasMoved = false;

        public LinkedTimeBlock(LinkedTimeBlockViewModel vm)
        {
            _box1 = new TextBox();
            //_box2 = new TextBox();
            _box1.LostFocus += Box1_LostFocus;
            //_box2.LostFocus +=Box2_LostFocus;

            DataContext = vm;
            InitializeComponent();
            setUpLine(vm);
            vm.Model.OnSelect += OnLinkSelect;
            vm.Model.OnDeselect += OnLinkDeselect;
            _handleOnePressed = false;
            line.SetValue(Canvas.ZIndexProperty, 0);
            HandleOne.SetValue(Canvas.ZIndexProperty, 2);
            HandleTwo.SetValue(Canvas.ZIndexProperty, 2);
            

        }

        private void Box2_LostFocus(object sender, RoutedEventArgs e)
        {
            var times = (sender as TextBox).Text.Split(':');
            TimeSpan time = new TimeSpan(0, 0, Convert.ToInt32(times[0]), Convert.ToInt32(times[1]), Convert.ToInt32(times[2]));
            if (time.TotalMilliseconds <= (DataContext as LinkedTimeBlockViewModel)._totalAudioDuration.TotalMilliseconds)
            {
                (DataContext as LinkedTimeBlockViewModel).SetEnd(time);
                this.setUpLine((DataContext as LinkedTimeBlockViewModel));
                OnTimeChange?.Invoke();
            }
        }

        private void OnLinkSelect(LinkedTimeBlockModel model)
        {
            line.Stroke = new SolidColorBrush(Colors.Red);
            Debug.WriteLine("1");
        }

        private void OnLinkDeselect(LinkedTimeBlockModel model)
        {
            line.Stroke = new SolidColorBrush(Colors.Yellow);
            Debug.WriteLine("2");
        }

        public Line getLine()
        {
            return line;
        }

        public void setUpLine(LinkedTimeBlockViewModel vm)
        {

            this.DataContext = vm;
            line.StrokeThickness = (double)vm.Line1["StrokeThickness"];
            line.Stroke = new SolidColorBrush(Colors.Yellow);
            line.Opacity = (double)vm.Line1["Opacity"];

            //line.Detailx1 = (double)vm.Line1["Detailx1"];
            //Binding b = new Binding();
            //b.Source = "Detailx2";
            //Debug.WriteLine(d["Detailx2"]);
            //b.Mode = BindingMode.TwoWay;
            //line.Detailx2 = vm.Detailx2;
            //.SetBinding(Line.X2Property, b);
            try
            {
                Binding b1 = new Binding();
                b1.Path = new PropertyPath("Detailx1");
                line.SetBinding(Line.X1Property, b1);
                Binding b2 = new Binding();
                b2.Path = new PropertyPath("Detailx2");
                line.SetBinding(Line.X2Property, b2);

                Binding b3 = new Binding();
                b3.Path = new PropertyPath("Ellipsex1");
                Binding b4 = new Binding();
                b4.Path = new PropertyPath("Ellipsex2");

                EllipseOne.SetBinding(Canvas.LeftProperty, b3);
                EllipseTwo.SetBinding(Canvas.LeftProperty, b4);

                

                HandleOne.SetBinding(Line.X2Property, b1);
                HandleOne.SetBinding(Line.X1Property, b1);
                HandleTwo.SetBinding(Line.X2Property, b2);
                HandleTwo.SetBinding(Line.X1Property, b2);
            }
            catch (Exception e)
            {
                
            }
                

            line.Y1 = (double)vm.Line1["Y"];
            line.Y2 = (double)vm.Line1["Y"];



            

            //_box1.SetBinding(Canvas.LeftProperty, b1);
            Canvas.SetTop(_box1, 0);

            //_box2.SetBinding(Canvas.LeftProperty, b2);
            //Canvas.SetTop(_box1, 0);

            line.Margin = new Thickness(0, (double)vm.Line1["TopMargin"], 0, 0);

            var y = Canvas.GetTop(vm._scrubBar) + vm._scrubBar.Margin.Top + vm._scrubBar.ActualHeight/4;
            HandleOne.Y1 = y;
            //HandleOne.Y2 = y+vm._scrubBar.ActualHeight;
            HandleTwo.Y1 = y;
            //HandleTwo.Y2 = y+ vm._scrubBar.ActualHeight;

            HandleOne.Y2 = Canvas.GetTop(vm._scrubBar) + vm._scrubBar.ActualHeight + vm._scrubBar.Margin.Top + 5;
            HandleTwo.Y2 = Canvas.GetTop(vm._scrubBar) + vm._scrubBar.ActualHeight + vm._scrubBar.Margin.Top + 5;
            EllipseOne.SetValue(Canvas.TopProperty, Canvas.GetTop(vm._scrubBar) + vm._scrubBar.ActualHeight + vm._scrubBar.Margin.Top);
            EllipseTwo.SetValue(Canvas.TopProperty, Canvas.GetTop(vm._scrubBar) + vm._scrubBar.ActualHeight + vm._scrubBar.Margin.Top);
            //EllipseOne.SetValue(Canvas.LeftProperty, (DataContext as LinkedTimeBlockViewModel).Detailx1 - EllipseOne.ActualWidth / 2);
            //EllipseTwo.SetValue(Canvas.LeftProperty, (DataContext as LinkedTimeBlockViewModel).Detailx2 - EllipseTwo.ActualWidth / 2);
            vm._scrubBar.SizeChanged += ScrubBarOnSizeChanged;

        }

        private void ScrubBarOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            //EllipseOne.SetValue(Canvas.LeftProperty, (DataContext as LinkedTimeBlockViewModel).Detailx1 - EllipseOne.ActualWidth/2);
            //EllipseTwo.SetValue(Canvas.LeftProperty, (DataContext as LinkedTimeBlockViewModel).Detailx2 - EllipseTwo.ActualWidth/2);
            
        }

        public void changeColor()
        {
            //line.Fill = new SolidColorBrush(Colors.Red);
        }

        private void HandleOne_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //show text box ontop then hide if pressed again
            //if (!Canvas.Children.Contains(_box1))
            //{
            //    _box1.Text = (DataContext as LinkedTimeBlockViewModel).Model.Start.Minutes + ":" +
            //                 (DataContext as LinkedTimeBlockViewModel).Model.Start.Seconds + ":" +
            //                 (DataContext as LinkedTimeBlockViewModel).Model.Start.Milliseconds;
            //    LinkedTimeBlock.removeBox();

            //    Canvas.Children.Add(_box1);
            //}
            //else
            //{
            //    Canvas.Children.Remove(_box1);
            //}
            hasMoved = false;


        }

        public static void removeBox()
        {
            if (_box1.Parent != null)
            {
                ((Canvas)_box1.Parent).Children.Remove(_box1);
            }
        }

        private void Box1_LostFocus(object sender, RoutedEventArgs e)
        {
            var times = (sender as TextBox).Text.Split(':');
            try
            {
                TimeSpan time = new TimeSpan(0, 0, Convert.ToInt32(times[0]), Convert.ToInt32(times[1]), Convert.ToInt32(times[2]));
                if (time.TotalMilliseconds <= (DataContext as LinkedTimeBlockViewModel)._totalAudioDuration.TotalMilliseconds)
                {
                    if ((sender as FrameworkElement).Parent != null)
                    {
                        var block = (((sender as FrameworkElement).Parent as FrameworkElement));
                        while (!(block is LinkedTimeBlock))
                        {
                            block = (block as FrameworkElement).Parent as FrameworkElement;
                        }
                        if (Canvas.GetLeft(_box1) == (block as LinkedTimeBlock).line.X1)
                        {
                            (block.DataContext as LinkedTimeBlockViewModel).SetStart(time);
                        }
                        else if (Canvas.GetLeft(_box1) == (block as LinkedTimeBlock).line.X2)
                        {
                            (block.DataContext as LinkedTimeBlockViewModel).SetEnd(time);
                        }

                    (block as LinkedTimeBlock).setUpLine((block.DataContext as LinkedTimeBlockViewModel));
                        LinkedTimeBlock.removeBox();
                        OnTimeChange?.Invoke();
                    }


                }
            }
            catch (Exception ee)
            {
                //this is the wrong format for time
                LinkedTimeBlock.removeBox();
                OnTimeChange?.Invoke();

            }

        }

        private void HandleOne_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            int milli = (int)(((HandleOne.X1 - Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) - (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left)/ (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth) * (DataContext as LinkedTimeBlockViewModel)._totalAudioDuration.TotalMilliseconds);
            TimeSpan time = new TimeSpan(0,0,0,0,milli);

            (DataContext as LinkedTimeBlockViewModel).SetStart(time);
            this.setUpLine((DataContext as LinkedTimeBlockViewModel));
            //if (hasMoved == false)
            //{
            //    var test = Canvas.GetLeft(_box1);
            //    var test2 = line.X1;
            //    if (Canvas.GetLeft(_box1) == line.X1 && Canvas.Children.Contains(_box1))
            //    {
            //        Canvas.Children.Remove(_box1);
            //    }
            //    else
            //    {
            //        _box1.SetValue(Canvas.LeftProperty, line.X1);
            //        _box1.Text = (DataContext as LinkedTimeBlockViewModel).Model.Start.Minutes + ":" +
            //                     (DataContext as LinkedTimeBlockViewModel).Model.Start.Seconds + ":" +
            //                     (DataContext as LinkedTimeBlockViewModel).Model.Start.Milliseconds;
            //        LinkedTimeBlock.removeBox();
            //        Canvas.Children.Add(_box1);

            //    }
                
                
            //}
            //else
            //{
                Canvas.Children.Remove(_box1);
            //}
            OnTimeChange?.Invoke();
            hasMoved = false;



        }

        private void HandleOne_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint((UIElement) sender).Properties.IsLeftButtonPressed)
            {
                if (e.GetCurrentPoint(grid).Position.X >=
                    Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) + (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left &&
                    e.GetCurrentPoint(grid).Position.X <=
                    Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) +
                    (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth + (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left)
                {

                    ((UIElement)sender).CapturePointer(e.Pointer);
                    line.X1 = e.GetCurrentPoint(grid).Position.X;
                    HandleOne.X1 = e.GetCurrentPoint(grid).Position.X;
                    HandleOne.X2 = e.GetCurrentPoint(grid).Position.X;
                    _box1.SetValue(Canvas.LeftProperty, line.X1);
                    int milli = (int)(((HandleOne.X1 - Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) - (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left) / (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth) * (DataContext as LinkedTimeBlockViewModel)._totalAudioDuration.TotalMilliseconds);
                    TimeSpan time = new TimeSpan(0, 0, 0, 0, milli);
                    _box1.Text = time.Minutes + ":" +
                             time.Seconds + ":" +
                             time.Milliseconds;
                    EllipseOne.SetValue(Canvas.LeftProperty, HandleOne.X1 - 15);

                }
                if (!Canvas.Children.Contains(_box1))
                {
                    LinkedTimeBlock.removeBox();
                    Canvas.Children.Add(_box1);
                }
                hasMoved = true;

            }
        }

        private void HandleTwo_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //if (!Canvas.Children.Contains(_box2))
            //{
            //    _box2.Text = (DataContext as LinkedTimeBlockViewModel).Model.End.Minutes + ":" +
            //                 (DataContext as LinkedTimeBlockViewModel).Model.End.Seconds + ":" +
            //                 (DataContext as LinkedTimeBlockViewModel).Model.End.Milliseconds;
            //    Canvas.Children.Add(_box2);
            //}
            //else
            //{
            //    Canvas.Children.Remove(_box2);
            //}
            hasMoved = false;
        }

        private void HandleTwo_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            int milli = (int)(((HandleTwo.X1 - Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) - (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left) / (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth) * (DataContext as LinkedTimeBlockViewModel)._totalAudioDuration.TotalMilliseconds);
            TimeSpan time = new TimeSpan(0, 0, 0, 0, milli);

            (DataContext as LinkedTimeBlockViewModel).SetEnd(time);
            this.setUpLine((DataContext as LinkedTimeBlockViewModel));
            OnTimeChange?.Invoke();

            if (hasMoved == false)
            {
                if (Canvas.GetLeft(_box1) == line.X2 && Canvas.Children.Contains(_box1))
                {
                    Canvas.Children.Remove(_box1);
                }
                else
                {
                    _box1.SetValue(Canvas.LeftProperty, line.X2);
                    _box1.Text = (DataContext as LinkedTimeBlockViewModel).Model.End.Minutes + ":" +
                                 (DataContext as LinkedTimeBlockViewModel).Model.End.Seconds + ":" +
                                 (DataContext as LinkedTimeBlockViewModel).Model.End.Milliseconds;
                    LinkedTimeBlock.removeBox();
                    Canvas.Children.Add(_box1);
                }
                
                
            }

            else
            {
                Canvas.Children.Remove(_box1);
            }

            hasMoved = false;

        }

        private void HandleTwo_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            {
                hasMoved = true;
                if (e.GetCurrentPoint(grid).Position.X >=
                    Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) + (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left &&
                    e.GetCurrentPoint(grid).Position.X <=
                    Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) +
                    (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth + (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left) 
                {
                    ((UIElement)sender).CapturePointer(e.Pointer);
                    line.X2 = e.GetCurrentPoint(grid).Position.X;
                    HandleTwo.X1 = e.GetCurrentPoint(grid).Position.X;
                    HandleTwo.X2 = e.GetCurrentPoint(grid).Position.X;
                    _box1.SetValue(Canvas.LeftProperty, line.X2);
                    int milli = (int)(((HandleTwo.X1 - Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) - (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left) / (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth) * (DataContext as LinkedTimeBlockViewModel)._totalAudioDuration.TotalMilliseconds);
                    TimeSpan time = new TimeSpan(0, 0, 0, 0, milli);
                    _box1.Text = time.Minutes + ":" +
                             time.Seconds + ":" +
                             time.Milliseconds;
                    EllipseTwo.SetValue(Canvas.LeftProperty, HandleTwo.X1 - 15);

                }
                if (!Canvas.Children.Contains(_box1))
                {
                    LinkedTimeBlock.removeBox();
                    Canvas.Children.Add(_box1);
                }

            }
        }

        private void Line_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            
            //((UIElement) sender).CapturePointer(e.Poi);
            //((UIElement)sender).cap
            var line = sender as Line;
            if (line.X1 + e.Delta.Translation.X > Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) + (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left
                && line.X1 + e.Delta.Translation.X < (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth + Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) + (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left
                && line.X2 + e.Delta.Translation.X > Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) + (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left
                && line.X2 + e.Delta.Translation.X < (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth + Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) + (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left)
            {
                line.X1 += e.Delta.Translation.X;
                line.X2 += e.Delta.Translation.X;
                HandleOne.X1 += e.Delta.Translation.X;
                HandleOne.X2 += e.Delta.Translation.X;
                HandleTwo.X1 += e.Delta.Translation.X;
                HandleTwo.X2 += e.Delta.Translation.X;
                EllipseTwo.SetValue(Canvas.LeftProperty, Canvas.GetLeft(EllipseTwo)+ e.Delta.Translation.X);
                EllipseOne.SetValue(Canvas.LeftProperty, Canvas.GetLeft(EllipseOne) + e.Delta.Translation.X);
                if (Canvas.Children.Contains(_box1))
                {
                    _box1.SetValue(Canvas.LeftProperty, Canvas.GetLeft(_box1) + e.Delta.Translation.X);

                }

            }
            e.Handled = true;
        }

        private void Line_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            ((UIElement) sender).CapturePointer(e.Pointer);
            int milli = (int)((((e.GetCurrentPoint(this).Position.X + Canvas.GetLeft(this) - Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) - (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left)) / (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth) * (DataContext as LinkedTimeBlockViewModel)._totalAudioDuration.TotalMilliseconds);
            TimeSpan time = new TimeSpan(0, 0, 0, 0, milli);
            this.jumpTo(time);
           
        }

        private void HandleOne_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.jumpTo((DataContext as LinkedTimeBlockViewModel).Model.Start);
        }

        private void HandleTwo_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.jumpTo((DataContext as LinkedTimeBlockViewModel).Model.End);
        }

        private void jumpTo(TimeSpan time)
        {
            var test = (this.Parent as FrameworkElement);
            while (test.Parent != null && !(((FrameworkElement)test.Parent).DataContext is VideoNodeViewModel) && !(((FrameworkElement)test.Parent).DataContext is AudioNodeViewModel))
            {
                test = (FrameworkElement)test.Parent as FrameworkElement;
            }
            if (test.DataContext is VideoNodeViewModel)
            {
                ((test.DataContext as VideoNodeViewModel).Model as VideoNodeModel).Jump(time);
            }
            else if (test.DataContext is AudioNodeViewModel)
            {
                ((test.DataContext as AudioNodeViewModel).Model as AudioNodeModel).Jump(time);
            }
        }

        private void Line_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ((UIElement) sender).CapturePointer(e.Pointer);
            int milli = (int)(((HandleTwo.X1 - Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) - (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left) / (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth) * (DataContext as LinkedTimeBlockViewModel)._totalAudioDuration.TotalMilliseconds);
            TimeSpan time = new TimeSpan(0, 0, 0, 0, milli);
            (DataContext as LinkedTimeBlockViewModel).SetEnd(time);
            milli = (int)(((HandleOne.X1 - Canvas.GetLeft((DataContext as LinkedTimeBlockViewModel)._scrubBar) - (DataContext as LinkedTimeBlockViewModel)._scrubBar.Margin.Left) / (DataContext as LinkedTimeBlockViewModel)._scrubBar.ActualWidth) * (DataContext as LinkedTimeBlockViewModel)._totalAudioDuration.TotalMilliseconds);
            time = new TimeSpan(0, 0, 0, 0, milli);
            (DataContext as LinkedTimeBlockViewModel).SetStart(time);
            this.setUpLine((DataContext as LinkedTimeBlockViewModel));
            OnTimeChange?.Invoke();
        }

        private void EllipseOne_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine(Canvas.GetLeft(_box1));
            if (Canvas.GetLeft(_box1) == line.X1 && Canvas.Children.Contains(_box1))
            {
                Canvas.Children.Remove(_box1);
            }
            else
            {
                _box1.SetValue(Canvas.LeftProperty, line.X1);
                _box1.Text = (DataContext as LinkedTimeBlockViewModel).Model.Start.Minutes + ":" +
                             (DataContext as LinkedTimeBlockViewModel).Model.Start.Seconds + ":" +
                             (DataContext as LinkedTimeBlockViewModel).Model.Start.Milliseconds;
                LinkedTimeBlock.removeBox();
                Canvas.Children.Add(_box1);
            }
            e.Handled = true;
        }
        private void EllipseTwo_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (Canvas.GetLeft(_box1) == line.X2 && Canvas.Children.Contains(_box1))
            {
                Canvas.Children.Remove(_box1);
            }
            else
            {
                _box1.SetValue(Canvas.LeftProperty, line.X2);
                _box1.Text = (DataContext as LinkedTimeBlockViewModel).Model.End.Minutes + ":" +
                             (DataContext as LinkedTimeBlockViewModel).Model.End.Seconds + ":" +
                             (DataContext as LinkedTimeBlockViewModel).Model.End.Milliseconds;
                LinkedTimeBlock.removeBox();
                Canvas.Children.Add(_box1);
            }
            e.Handled = true;
        }
    }
}
