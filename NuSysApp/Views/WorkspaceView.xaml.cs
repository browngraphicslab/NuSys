using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Diagnostics;
using Windows.UI.Popups;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

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
        private InkDrawingAttributes _drawingAttributes; //initialized in SetUpInk()
        private bool _isZooming;
        #endregion Private Members

        public WorkspaceView()
        {
            this.InitializeComponent();
            this.DataContext = new WorkspaceViewModel();
            this.SetUpInk();
            _isZooming = false;
        }

        #region Helper Methods

        
        //InkCanvas inkCanvas = null;
        /// <summary>
        /// Performs initial ink setup. 
        /// </summary>
        private void SetUpInk()
        {
            _drawingAttributes = new InkDrawingAttributes
            {
                Color = Windows.UI.Colors.Black,
                Size = new Windows.Foundation.Size(2, 2),
                IgnorePressure = false
            };
            
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(_drawingAttributes);      
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |   
            Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch; //This line is setting the Devices that can be used to display ink
            var vm = (WorkspaceViewModel)this.DataContext;
            inkCanvas.InkPresenter.IsInputEnabled = false;
            Canvas.SetZIndex(inkCanvas, -3);

        }
        
        private void ToggleInk()
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            if (vm.CurrentMode == WorkspaceViewModel.Mode.Globalink)
            {
                inkCanvas.InkPresenter.IsInputEnabled = true;
                inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = Windows.UI.Input.Inking.InkInputProcessingMode.Inking; //input can be changed using this line erasing works the same way, but instead the input is changed to erasing instead of inking
            }
            else
            {
                
                inkCanvas.InkPresenter.IsInputEnabled = false; //when text button is clicked in the app bar, it disables the ink presenter using this line and the line above allows the TEXTNODE to be displayed on double tap
            }
        }

        #endregion Helper Methods

        #region Event Handlers

        #region Page Handlers


        private async void Page_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(e.GetPosition(this));
            await vm.CreateNewNode(p.X, p.Y,"");
            vm.ClearSelection();
        }

        private void Page_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            var FMT = new CompositeTransform();

            var p = e.GetPosition(this);
            FMT.TranslateX = p.X;
            FMT.TranslateY = p.Y;

            vm.FMTransform = FMT;

            FM.Visibility = FM.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            
        }


        private void Page_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            vm.ClearSelection();  
        }

        private void Page_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;

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
            /*
            var x = e.Position.X - vm.TransformX;
            var y = e.Position.Y - vm.TransformY;
            Debug.WriteLine(x + ", " + y);
            vm.Origin = new Point(x / 10000.0, y / 10000.0);
            vm.ScaleX *= e.Delta.Scale;
            vm.ScaleY *= e.Delta.Scale;    */
            //  vm.TransformX += e.Delta.Translation.X / vm.ScaleX;
            //  vm.TransformY += e.Delta.Translation.Y / vm.ScaleY;


            e.Handled = true;
        }


        private void Page_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {

            e.Container = this;
            e.Handled = true;
        }

        private void Page_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isZooming = false;
            e.Handled = true;

        }
        private void Page_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void inkCanvas_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var menu = new PopupMenu();
            menu.Commands.Add(new UICommand("Erase", (command) =>
            {
                inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = Windows.UI.Input.Inking.InkInputProcessingMode.Erasing; //when erase button is clicked in the appbar, erasing mode is enabled
            }));
          //  inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = Windows.UI.Input.Inking.InkInputProcessingMode.Erasing;
        }
        #endregion Page Handlers
        #region App Bar Handlers
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            inkButton.Opacity = .5;
            linkButton.Opacity = 1;
            textButton.Opacity = 1;
            scribbleButton.Opacity = 1;
            docButton.Opacity = 1;
            Canvas.SetZIndex(inkCanvas, -2);
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.Globalink;
            inkCanvas.InkPresenter.IsInputEnabled = true;
            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = Windows.UI.Input.Inking.InkInputProcessingMode.Inking; //input can be changed using this line erasing works the same way, but instead the input is changed to erasing instead of inking
        }

        private void AppBarButton_Click_Text(object sender, RoutedEventArgs e)
        {
            inkButton.Opacity = 1;
            linkButton.Opacity = 1;
            textButton.Opacity = .5;
            scribbleButton.Opacity = 1;
            docButton.Opacity = 1;
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.Textnode;
            this.ToggleInk();
        }

        private void AppBarButton_Click_Erase(object sender, RoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.Erase;
            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = Windows.UI.Input.Inking.InkInputProcessingMode.Erasing;
        }

        private void AppBarButton_Click_Scribble(object sender, RoutedEventArgs e)
        {
            inkButton.Opacity = 1;
            linkButton.Opacity = 1;
            textButton.Opacity = 1;
            scribbleButton.Opacity = .5;
            docButton.Opacity = 1;
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.Ink;  //initializes ink canvas to be created to the viewmodel
            inkCanvas.InkPresenter.IsInputEnabled = false;
        }

        /// <summary>
        /// PDF nodes have a special asynchronous initialization function that must be called from PdfNodeViewModel,
        /// in order to allow users to select PDF files from file explorer without disrupting other processes in the workspace.
        /// Currently, only PDFs contained in the Pictures library are accessible.
        /// </summary>
        private async void AppBarButton_Click_Document(object sender, RoutedEventArgs e)
        {
            //OfficeInteropWord.GenerateTestDocument();
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
            var p = vm.CompositeTransform.Inverse.TransformPoint(new Point(0, 0));
            await vm.CreateNewNode(p.X, p.Y, storageFile);
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

        private void MenuFlyoutItem_Click_Bezier(object sender, RoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentLinkMode = WorkspaceViewModel.LinkMode.Bezierlink;
        }

        private void MenuFlyoutItem_Click_Line(object sender, RoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentLinkMode = WorkspaceViewModel.LinkMode.Linelink;
        }

        void AddButtonClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void Page_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {

            var vm = (WorkspaceViewModel)this.DataContext;
            var compositeTransform = vm.CompositeTransform;
            var center = compositeTransform.Inverse.TransformPoint(e.GetCurrentPoint(this).Position);

            Debug.WriteLine(((double)e.GetCurrentPoint(this).Properties.MouseWheelDelta +240)/240);
            compositeTransform.ScaleX *= (3+((double)e.GetCurrentPoint(this).Properties.MouseWheelDelta +240)/240)/4;
            compositeTransform.ScaleY *= (3+((double)e.GetCurrentPoint(this).Properties.MouseWheelDelta +240)/ 240)/4;

            compositeTransform.CenterX = center.X;
            compositeTransform.CenterY = center.Y;

            vm.CompositeTransform = compositeTransform;
        }
        private void FM_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            var vm = (WorkspaceViewModel)this.DataContext;

            var compositeTransform = vm.FMTransform;

            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;

            vm.FMTransform = compositeTransform;
            if (compositeTransform.TranslateX < -85 || compositeTransform.TranslateX > this.ActualWidth || compositeTransform.TranslateY < -85 + FM.Children.Count*-100 || compositeTransform.TranslateY > this.ActualHeight)
            {
                FM.Visibility = Visibility.Collapsed;
                e.Complete();
            }
            e.Handled = true;
        }

        private void FM_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            e.Handled = true;
        }
        #endregion App Bar Handlers

        #endregion Event Handlers

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }



}