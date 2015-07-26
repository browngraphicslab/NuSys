﻿using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using System.Diagnostics;
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
       
        private int penSize = Constants.INITIAL_PEN_SIZE;
        private InkDrawingAttributes _drawingAttributes; //initialized in SetUpInk()
        private Boolean _isZooming;
        #endregion Private Members

        public WorkspaceView()
        {
            this.InitializeComponent();
            this.DataContext = new WorkspaceViewModel();
            this.SetUpInk();
            _isZooming = false;
        }

        #region Helper Methods
        /// <summary>
        /// Performs initial ink setup. 
        /// </summary>
        private void SetUpInk()
        {
            _drawingAttributes = new InkDrawingAttributes();
            _drawingAttributes.Color = Windows.UI.Colors.Black; //ink set to black
            _drawingAttributes.Size = new Windows.Foundation.Size(2, 2); //ink can be thicker or thinner 
            _drawingAttributes.IgnorePressure = false;
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(_drawingAttributes);      
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |   
            Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch; //This line is setting the Devices that can be used to display ink
            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            inkCanvas.InkPresenter.IsInputEnabled = false;
            Canvas.SetZIndex(inkCanvas, -3);

        }
        
        private void ToggleInk()
        {
            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            if (vm.CurrentMode == WorkspaceViewModel.Mode.GLOBALINK)
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


        private void Page_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(e.GetPosition(this));
            vm.CreateNewNode(p.X, p.Y,"");
            vm.ClearSelection();
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
            var tmpTranslate = new TranslateTransform();
            

            tmpTranslate.X = compositeTransform.CenterX;
            tmpTranslate.Y = compositeTransform.CenterY;

            Point center = compositeTransform.Inverse.TransformPoint(e.Position);

            Point localPoint = tmpTranslate.Inverse.TransformPoint(center);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            //Transform local space into world space again
            Point worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            Point distance = new Point(
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
            Canvas.SetZIndex(inkCanvas, -2);
            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.GLOBALINK;
            inkCanvas.InkPresenter.IsInputEnabled = true;
            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = Windows.UI.Input.Inking.InkInputProcessingMode.Inking; //input can be changed using this line erasing works the same way, but instead the input is changed to erasing instead of inking
        }

        private void AppBarButton_Click_Text(object sender, RoutedEventArgs e)
        {

            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.TEXTNODE;
            this.ToggleInk();
        }

        private void AppBarButton_Click_Erase(object sender, RoutedEventArgs e)
        {

            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.ERASE;
            inkCanvas.InkPresenter.InputProcessingConfiguration.Mode = Windows.UI.Input.Inking.InkInputProcessingMode.Erasing;
        }

        private void AppBarButton_Click_Scribble(object sender, RoutedEventArgs e)
        {

            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.INK;  //initializes ink canvas to be created to the viewmodel
            inkCanvas.InkPresenter.IsInputEnabled = false;
        }

        /// <summary>
        /// PDF nodes have a special asynchronous initialization function that must be called from PdfNodeViewModel,
        /// in order to allow users to select PDF files from file explorer without disrupting other processes in the workspace.
        /// Currently, only PDFs contained in the Pictures library are accessible.
        /// </summary>
        private async void AppBarButton_Click_Document(object sender, RoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentMode = WorkspaceViewModel.Mode.PDF;
            var p = vm.CompositeTransform.Inverse.TransformPoint(new Point(0, 0));
            var pdfNodeViewModel = (PdfNodeViewModel)vm.CreateNewNode(p.X, p.Y, null);
            await pdfNodeViewModel.InitializePdfNodeAsync();
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
            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentLinkMode = WorkspaceViewModel.LinkMode.BEZIERLINK;
        }

        private void MenuFlyoutItem_Click_Line(object sender, RoutedEventArgs e)
        {
            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            vm.CurrentLinkMode = WorkspaceViewModel.LinkMode.LINELINK;
        }

        async void AddButtonClick(object sender, RoutedEventArgs e)
        {
            StorageFile file = await FileManager.PromptUserForFile(new List<string> { ".bmp", ".png", ".jpeg", ".jpg" });
            // 'file' is null if user cancels the file picker.
            if (file != null)
            {
                // Open a stream for the selected file.
                // The 'using' block ensures the stream is disposed
                // after the image is loaded.
                using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await file.OpenAsync(FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap.
                    BitmapImage bitmapImage = new BitmapImage();

                    bitmapImage.SetSource(fileStream);
                    WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
                    vm.CurrentMode = WorkspaceViewModel.Mode.IMAGE;
                    var p = vm.CompositeTransform.Inverse.TransformPoint(new Point(0, 0));
                    vm.CreateNewNode(p.X, p.Y, bitmapImage);
                }
            }
        }

        private void Page_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {

            var vm = (WorkspaceViewModel)this.DataContext;
            var compositeTransform = vm.CompositeTransform;
            Point center = compositeTransform.Inverse.TransformPoint(e.GetCurrentPoint(this).Position);

            Debug.WriteLine(((double)e.GetCurrentPoint(this).Properties.MouseWheelDelta +240)/240);
            compositeTransform.ScaleX *= (3+((double)e.GetCurrentPoint(this).Properties.MouseWheelDelta +240)/240)/4;
            compositeTransform.ScaleY *= (3+((double)e.GetCurrentPoint(this).Properties.MouseWheelDelta +240)/ 240)/4;

            compositeTransform.CenterX = center.X;
            compositeTransform.CenterY = center.Y;

            vm.CompositeTransform = compositeTransform;
        }
        #endregion App Bar Handlers

        #endregion Event Handlers

    }



}