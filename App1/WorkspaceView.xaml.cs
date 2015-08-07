using System;
using System.Collections.Generic;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Diagnostics;
using Windows.UI.Popups;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NuSysApp
{
    /// <summary>
    /// This is the view for the entire workspace. It instantiates the WorkspaceViewModel. 
    /// </summary>
    public sealed partial class WorkspaceView : Page
    {
        #region Private Members
       
        private int penSize = Constants.InitialPenSize;
        private bool _isZooming, _isErasing, _isHighlighting;
        private PointerPoint _prevPoint;
        private InkManager _inkManager = new InkManager();
        private bool _isDrawing = false;
        private bool _isInkingEnabled = true;
        private Polyline _currentStroke;
        #endregion Private Members

        public WorkspaceView()
        {
            this.InitializeComponent();
            this.DataContext = new WorkspaceViewModel();
            this.SetUpInk();

            inkBorder.AddHandler(PointerPressedEvent, new PointerEventHandler(inkBorder_OnPointerPressed), true);
            inkBorder.AddHandler(PointerReleasedEvent, new PointerEventHandler(inkBorder_OnPointerReleased), true);
            inkBorder.AddHandler(PointerMovedEvent, new PointerEventHandler(inkBorder_OnPointerMoved), true);

            _isZooming = false;
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.Globalink;
            this.SetGlobalInk(true);
        }

        #region Helper Methods

        /// <summary>
        /// Performs initial ink setup. 
        /// </summary>
        private void SetUpInk()
        {
            var drawingAttributes = new InkDrawingAttributes
            {
                Color = Windows.UI.Colors.Black,
                Size = new Windows.Foundation.Size(2, 2),
                IgnorePressure = false
            };
            _inkManager.SetDefaultDrawingAttributes(drawingAttributes);
            var vm = (WorkspaceViewModel)this.DataContext;

        }

        /// <summary>
        /// Sets global ink on or off
        /// </summary>
        /// <param name="ink"></param>
        private void SetGlobalInk(bool ink)
        {
            //inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = Windows.UI.Input.Inking.InkInputProcessingMode.Inking; //input can be changed using this line erasing works the same way, but instead the input is changed to erasing instead of inking
            //inkCanvas.InkPresenter.IsInputEnabled = ink;
            _inkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Inking;
            _isInkingEnabled = ink;
            
        }

        /// <summary>
        /// Turns erasing on or off
        /// </summary>
        /// <param name="erase"></param>
        private void SetErasing(bool erase)
        {
            if (erase)
            {
                _inkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Erasing;
            }
            else
            {
                _inkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Inking;
            }
        }

        /// <summary>
        /// Turns highlighting on or off
        /// </summary>
        /// <param name="highlight"></param>
        private void SetHighlighting(bool highlight)
        {
            InkDrawingAttributes drawingAttributes;
            _isErasing = false;
            if (highlight)
            {
                drawingAttributes = new InkDrawingAttributes
                {
                    Color = Windows.UI.Colors.Yellow,
                    Size = new Windows.Foundation.Size(6, 6),
                    IgnorePressure = false
                };
            }
            else
            {
                drawingAttributes = new InkDrawingAttributes
                {
                    Color = Windows.UI.Colors.Black,
                    Size = new Windows.Foundation.Size(2, 2),
                    IgnorePressure = false
                };
            }
            _inkManager.SetDefaultDrawingAttributes((drawingAttributes));
        }

        #endregion Helper Methods
        #region Event Handlers
        #region Page Handlers
        private void Page_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {

            var vm = (WorkspaceViewModel)this.DataContext;
            var compositeTransform = vm.CompositeTransform;

            var direction = Math.Sign((double) e.GetCurrentPoint(this).Properties.MouseWheelDelta);

            Debug.WriteLine(direction);

            var zoomspeed = 0.08 * direction;
            var translateSpeed = 10;

            if (compositeTransform.ScaleX + zoomspeed > 0.01)
            {
                var center = compositeTransform.Inverse.TransformPoint(e.GetCurrentPoint(this).Position);
                compositeTransform.ScaleX += zoomspeed;
                compositeTransform.ScaleY += zoomspeed;

                if (direction > 0)
                {
                    compositeTransform.CenterX = center.X;
                    compositeTransform.CenterY = center.Y;
                }
                vm.CompositeTransform = compositeTransform;
            }
        }

        private void inkBorder_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isInkingEnabled && !_isErasing)
            {
                _prevPoint = e.GetCurrentPoint(inkBorder);

                _currentStroke = new Polyline();
                _currentStroke.StrokeThickness = Math.Max(4.0*e.GetCurrentPoint(inkBorder).Properties.Pressure, 2);
                _currentStroke.Stroke = new SolidColorBrush(Colors.Black);
                _currentStroke.PointerPressed += delegate (object o, PointerRoutedEventArgs e2)
                {
                    if (_isErasing)
                    {
                        canvas.Children.Remove(o as Polyline);
                        _inkManager.SelectWithLine(e2.GetCurrentPoint(inkBorder).Position, e2.GetCurrentPoint(inkBorder).Position);
                    }
                };

                canvas.Children.Add(_currentStroke);
                _inkManager.ProcessPointerDown(e.GetCurrentPoint(inkBorder));
                _isDrawing = true;
            }
        }

        private void inkBorder_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isErasing || !_isDrawing || !_isInkingEnabled)
                return;

            _inkManager.ProcessPointerUpdate(e.GetCurrentPoint(inkBorder));


            var currentPoint = e.GetCurrentPoint(inkBorder);
            Debug.WriteLine(currentPoint.Properties.Pressure);

            _currentStroke.Points.Add(new Point(currentPoint.Position.X, currentPoint.Position.Y));

            _prevPoint = currentPoint;

            _isDrawing = true;
        }

        private void inkBorder_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isInkingEnabled && !_isErasing)
            {
                _inkManager.ProcessPointerUp(e.GetCurrentPoint(inkBorder));
                _isDrawing = false;
            }
        }

        private void inkCanvas_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand("Erase", (command) =>
            {
                _inkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Erasing;
            }));
            //  inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = Windows.UI.Input.Inking.InkInputProcessingMode.Erasing; 
        }

        private async void Page_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(e.GetPosition(this));
            if (vm.CurrentMode == WorkspaceViewModel.Mode.InkSelect)
            {
                int d = 20;
               
                var point1 = new Point(p.X-d, p.Y-d);
                var point2 = new Point(p.X + d, p.Y - d);
                var point3 = new Point(p.X  + d, p.Y + d);
                var point4 = new Point(p.X - d, p.Y + d);
                
                
                var result = _inkManager.SelectWithLine(point1, point3);
                if (result.IsEmpty)
                {
                    result = _inkManager.SelectWithLine(point2, point4);
                }
                if (result.IsEmpty) { return;}
                _inkManager.CopySelectedToClipboard();
                _inkManager.DeleteSelected();
                p.X = result.X;
                p.Y = result.Y;
                if (result.Width == 0 && result.Height == 0)
                {
                    return;
                }
                
            }

            if (vm.CurrentMode == WorkspaceViewModel.Mode.Pdf) return;

            await vm.CreateNewNode(p.X, p.Y,"");
            vm.ClearSelection();
            e.Handled = true;
        }

        private void Page_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            var floatingMenuTransform = new CompositeTransform();

            var p = e.GetPosition(this);
            floatingMenuTransform.TranslateX = p.X;
            floatingMenuTransform.TranslateY = p.Y;

            vm.FMTransform = floatingMenuTransform;

            FM.Visibility = FM.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            
        }

        /// <summary>
        /// Handler gets called when the mouse (or touch) is pressed on the whiteboard. Unselects current
        /// selection in workspace.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.ClearSelection();  
            
        }

        /// <summary>
        /// This handler gets called when the mouse (or touch) is dragged across the workspace.
        /// This handler contains pan and zoom functionality. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;

            if (vm.CurrentMode == WorkspaceViewModel.Mode.InkSelect)
            {
                return;
            }

            var compositeTransform = vm.CompositeTransform;
            var tmpTranslate = new TranslateTransform
            {
                X = compositeTransform.CenterX,
                Y = compositeTransform.CenterY
            };

            var center = compositeTransform.Inverse.TransformPoint(e.Position);

            var localPoint = tmpTranslate.Inverse.TransformPoint(center);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            //Transform local space into world space again
            var worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            var distance = new Point(
                worldPoint.X - center.X,
                worldPoint.Y - center.Y);

            //...amd balance the jump of the changed scaling origin by changing the translation            

            compositeTransform.TranslateX += distance.X;
            compositeTransform.TranslateY += distance.Y;

            //Also set the scaling values themselves, especially set the new scale center...

            compositeTransform.ScaleX *= e.Delta.Scale;
            compositeTransform.ScaleY *= e.Delta.Scale;

            compositeTransform.CenterX = center.X;
            compositeTransform.CenterY = center.Y;

            //And consider a translational shift

            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;
            
            vm.CompositeTransform = compositeTransform;

            e.Handled = true;
        }

        /// <summary>
        /// Hanlder gets called when mouse (or touch) first starts manipulating workspace.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            e.Container = this;
            e.Handled = true;
        }

        /// <summary>
        /// Handler gest called when mouse (or touch) finishes manipulating workspace. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isZooming = false;
            e.Handled = true;

        }


        private void Page_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            e.Handled = true;
        }

        #endregion Page Handlers
        #region Floating Menu Handlers
        private void FloatingMenu_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            var vm = (WorkspaceViewModel)this.DataContext;

            var compositeTransform = vm.FMTransform;

            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;

            vm.FMTransform = compositeTransform;
            if (compositeTransform.TranslateX < -85 || compositeTransform.TranslateX > this.ActualWidth || compositeTransform.TranslateY < -85 + FM.Children.Count * -100 || compositeTransform.TranslateY > this.ActualHeight)
            {
                FM.Visibility = Visibility.Collapsed;
                e.Complete();
            }
            e.Handled = true;
        }

        private void FloatingMenu_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            e.Handled = true;
        }
        #endregion Floating Menu Handlers
        #region Floating Menu Button Handlers
        private void GlobalInkButton_Click(object sender, RoutedEventArgs e)
        {
            this.SetErasing(false);
            _isErasing = false;
            this.SetHighlighting(false);
            _isHighlighting = false;
            inkButton.Opacity = .5;
            linkButton.Opacity = 1;
            textButton.Opacity = 1;
            scribbleButton.Opacity = 1;
            docButton.Opacity = 1;
            Canvas.SetZIndex(inkBorder, -2);
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.Globalink;
            this.SetGlobalInk(true);
        }

        private void LinkButton_Click(object sender, TappedRoutedEventArgs e)
        {
            inkButton.Opacity = 1;
            linkButton.Opacity = 0.5;
            textButton.Opacity = 1;
            scribbleButton.Opacity = 1;
            docButton.Opacity = 1;
            var vm = (WorkspaceViewModel)DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.InkSelect;  //initializes ink canvas to be created to the viewmodel
            _isInkingEnabled = false;
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            inkButton.Opacity = 1;
            linkButton.Opacity = 1;
            textButton.Opacity = .5;
            scribbleButton.Opacity = 1;
            docButton.Opacity = 1;
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.Textnode;
            this.SetGlobalInk(false);
        }

        /// <summary>
        /// Curently unused.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.Erase;
            _inkManager.Mode = Windows.UI.Input.Inking.InkManipulationMode.Erasing;
        }

        private void InkNodeButton_Click(object sender, RoutedEventArgs e)
        {
            inkButton.Opacity = 1;
            linkButton.Opacity = 1;
            textButton.Opacity = 1;
            scribbleButton.Opacity = .5;
            docButton.Opacity = 1;
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.Ink;  
            _isInkingEnabled = false;
        }

        /// <summary>
        /// PDF nodes have a special asynchronous initialization function that must be called from PdfNodeViewModel,
        /// in order to allow users to select PDF files from file explorer without disrupting other processes in the workspace.
        /// Currently, only PDFs contained in the Pictures library are accessible.
        /// </summary>
        private async void DocumentButton_Click(object sender, RoutedEventArgs e)
        {
            var storageFile = await FileManager.PromptUserForFile(Constants.AllFileTypes);
            if (storageFile == null) return;
            var vm = (WorkspaceViewModel)DataContext;
            if (Constants.ImageFileTypes.Contains(storageFile.FileType.ToLower()))
            {
                vm.CurrentMode = WorkspaceViewModel.Mode.Image;
            }
            else if (Constants.PdfFileTypes.Contains(storageFile.FileType))
            {
                vm.CurrentMode = WorkspaceViewModel.Mode.Pdf;
            }
            else return;
            var p = vm.CompositeTransform.Inverse.TransformPoint(new Point((ActualWidth - Constants.DefaultNodeSize)/2, (ActualHeight - Constants.DefaultNodeSize) / 2));
            await vm.CreateNewNode(p.X, p.Y, storageFile);
        }

        #endregion Floating Menu Button Handlers
        #region Unused Handlers
        private void Erase_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _isErasing = true;
            this.SetErasing(_isErasing);
            inkButton.Flyout.Hide();
        }

        private void Highlight_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _isHighlighting = true;
            this.SetHighlighting(_isHighlighting);
            inkButton.Flyout.Hide();
        }
        private void AppBarButton_Click_OFile(object sender, RoutedEventArgs e)
        {
            //TO DO
        }

        private void AppBarButton_Click_AFriend(object sender, RoutedEventArgs e)
        {
            //TO DO
        }

        private void AppBarButton_Click_Undo(object sender, RoutedEventArgs e)
        {
            //TO DO 
        }

        private void AppBarButton_Click_Redo(object sender, RoutedEventArgs e)
        {
            //TO DO 
        }

        private void AppBarButton_Click_Pictures(object sender, RoutedEventArgs e)
        {

        }
        private void AppBarButton_Click_Save(object sender, RoutedEventArgs e)
        {

        }

        #endregion Unused Handlers
        #endregion Event Handlers
    }
}