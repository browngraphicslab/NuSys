
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyToolkit.UI;
using System.Threading.Tasks;
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class RegionEditorTabView : UserControl
    {        
        public  RegionEditorTabView()
        {
            this.InitializeComponent();
            Canvas.SetZIndex(xButtonStack, 20);
        }

        private async void AddRegion_Clicked(object sender, RoutedEventArgs e)
        {
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            Debug.Assert(detailViewerView != null);
            var vm = detailViewerView.DataContext as DetailViewerViewModel;
            if (vm == null)
            {
                return;
            }

            // get appropriate new region message based on the type of library element currently
            // loaded in the detail view. i.e. for images top left point, width, and height.
            var detailHomeTabViewModel = vm.RegionView.DataContext as DetailHomeTabViewModel;
            NusysConstants.ElementType type;

            CreateNewLibraryElementRequestArgs regionRequestArgs = new CreateNewLibraryElementRequestArgs();
            //in each case, create a new CreateNewRegionLibraryElementRequestArgs or subclass
            switch (vm.CurrentElementController.LibraryElementModel.Type)
            {
                case NusysConstants.ElementType.Image:
                    regionRequestArgs = detailHomeTabViewModel?.GetNewCreateLibraryElementRequestArgs();
                    type = NusysConstants.ElementType.Image;
                    break;
                case NusysConstants.ElementType.Audio:
                    regionRequestArgs = detailHomeTabViewModel?.GetNewCreateLibraryElementRequestArgs();
                    type = NusysConstants.ElementType.Audio;
                    break;
                case NusysConstants.ElementType.Video:
                    regionRequestArgs = detailHomeTabViewModel?.GetNewCreateLibraryElementRequestArgs();
                    type  = NusysConstants.ElementType.Video;
                    break;
                case NusysConstants.ElementType.PDF:
                    regionRequestArgs = detailHomeTabViewModel?.GetNewCreateLibraryElementRequestArgs();
                    type = NusysConstants.ElementType.PDF;
                    break;
                default:
                    Debug.Fail("This should never occur, if it does we just return to be safe but this is a massive bug");
                    return;
            }
            Debug.Assert(regionRequestArgs != null);

            //create the args and set the parameters that all regions will need
            regionRequestArgs.ContentId = vm.CurrentElementController.LibraryElementModel.ContentDataModelId;
            regionRequestArgs.LibraryElementType = type;
            regionRequestArgs.Title = "Region " + vm.CurrentElementController.Title; // TODO factor out this hard-coded string to a constant
            regionRequestArgs.ParentLibraryElementId = vm.CurrentElementController.LibraryElementModel.LibraryElementId;
            regionRequestArgs.Large_Thumbnail_Url = vm.CurrentElementController.LibraryElementModel.LargeIconUrl;
            regionRequestArgs.Medium_Thumbnail_Url = vm.CurrentElementController.LibraryElementModel.MediumIconUrl;
            regionRequestArgs.Small_Thumbnail_Url = vm.CurrentElementController.LibraryElementModel.SmallIconUrl;
            if (PublicRegionButton.IsChecked == true)
            {
                regionRequestArgs.AccessType = NusysConstants.AccessType.Public;
            }
            else
            {
                regionRequestArgs.AccessType = NusysConstants.AccessType.Private;
            }

            var request = new CreateNewLibraryElementRequest(regionRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedLibraryElementToLibrary();
            
        }


    }
}
