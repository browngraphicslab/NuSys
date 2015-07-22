using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System;
using Windows.Storage;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NuStarterProject
{
    /// <summary>
    /// This is the view for the entire workspace. It instantiates the WorkspaceViewModel. 
    /// </summary>
    public sealed partial class WorkspaceView : Page
    {
        #region Private Members
       
        private int penSize = Constants.INITIAL_PEN_SIZE;
        private InkDrawingAttributes _drawingAttributes; //initialized in SetUpInk()
        #endregion Private Members

        public WorkspaceView()
        {
            this.InitializeComponent();
            this.DataContext = new WorkspaceViewModel();
            this.SetUpInk();
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
            Canvas.SetZIndex(inkCanvas, -2);

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
            vm.CreateNewNode(e.GetPosition(this).X, e.GetPosition(this).Y);
            vm.ClearSelection();
        }



        private void Page_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            vm.ClearSelection();  
        }

        private void Page_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            WorkspaceViewModel vm = (WorkspaceViewModel)this.DataContext;
            vm.TransformX += e.Delta.Translation.X;
            vm.TransformY += e.Delta.Translation.Y;

            e.Handled = true;
        }


        private void Page_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {

            e.Container = this;
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

        private void AppBarButton_Click_Document(object sender, RoutedEventArgs e)
        {
            //TO DO 
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
            var openFile = new Windows.Storage.Pickers.FileOpenPicker(); //open's file explorer changes to this code will be made by Adil allowing images to be added to canvas: Currently working on this
            openFile.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            openFile.FileTypeFilter.Add(".gif");
            openFile.FileTypeFilter.Add(".png");
            openFile.FileTypeFilter.Add(".docx");
            openFile.FileTypeFilter.Add(".jpeg");
            openFile.FileTypeFilter.Add(".ppt");
            openFile.FileTypeFilter.Add(".jpg");
            Windows.Storage.StorageFile file = await openFile.PickSingleFileAsync();
            /*   if (null != file)
               {
                   using (var stream = await file.OpenSequentialReadAsync())
                   {
                       catch(Exception ex)
                       {
                           System.Diagnostics.Debug.WriteLine("Exception");
                       }

                   }

               } */
        }

        #endregion App Bar Handlers
       
        #endregion Event Handlers
    }



}