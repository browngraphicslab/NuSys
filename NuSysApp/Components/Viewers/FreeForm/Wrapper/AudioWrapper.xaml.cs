using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NuSysApp
{
    /// 
    ///  xaml - wrap your content like this
    /// 
    ///             <local:AudioWrapper x:Name="xClippingWrapper">
    ///                 <local:AudioWrapper.Content>
    ///                    <MediaElement... />
    ///                 </local:AudioWrapper.Content>
    ///             </local:AudioWrapper>
    /// 
    ///  code behind -  place this in on loaded
    ///     xClippingWrapper.Controller = _vm.LibraryElementController;
    /// 
    /// </summary>
    public sealed partial class AudioWrapper : Page
    {


        /// <summary>
        /// Registers the ContentProperty as a value called contet which you can access through xaml
        /// 
        /// Content - the name of the property in xaml ie.  <Wrapper Content="">
        ///                                                     or
        ///                                                     <Wrapper.Content></Wrapper.Content>
        /// typeof(FrameWorkElement) - what the type of the content is
        /// typeof(object) - the sender that calls the ContentProperty, dont worry about it
        /// new PropertyMetaData() - where you would put methods which are called if the content were bound using {binding: name} 
        ///                         if the binding changes, default is null, but you can add your own so use (null, callbacks)
        /// </summary>
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                "Content", typeof(FrameworkElement), typeof(object), new PropertyMetadata(null));

        private LibraryElementController _contentController;


        /// <summary>
        /// The content of the wrapper, basically any rectangle based format, ideally images
        /// </summary>
        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set
            {
                SetValue(ContentProperty, value);
                xClippingContent.Content = value;
            }
        }

        /// <summary>
        /// The library element controller of the node this is on, calls processlibraryelementController when it is set, should only happen once
        /// </summary>
        public LibraryElementController Controller
        {
            get { return _contentController; }
            set
            {
                _contentController = value;
                ProcessLibraryElementController();
            }
        }


        public AudioWrapper()
        {
            this.InitializeComponent();
        }

        private void ProcessLibraryElementController()
        {
            Debug.Assert(Controller != null);
            var type = Controller.LibraryElementModel.Type;

            if (Constants.IsRegionType(type))
            {
                // rectangle region width and height are normalized so this is something like scaleX = 1 / .5
            }

            // clear the items control
            xClippingCanvas.Items.Clear();

            // get the region ids for the wrapper
            var regionsLibraryElementIds =
                SessionController.Instance.RegionsController.GetContentDataModelRegionLibraryElementIds(
                    Controller.LibraryElementModel.ContentDataModelId);
            Debug.Assert(regionsLibraryElementIds != null);

            // for each region id create a new view and put it into the canvas
            foreach (var regionId in regionsLibraryElementIds)
            {
                AddRegionView(regionId);
            }

            // Add the OnRegionAdded and OnRegionRemoved events so the view is updated
            var contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(Controller.LibraryElementModel.ContentDataModelId);
            contentDataModel.OnRegionAdded += AddRegionView;
            contentDataModel.OnRegionRemoved += RemoveRegionView;
        }

        /// <summary>
        /// Adds a new region view to the wrapper
        /// </summary>
        public Task AddRegionView(string regionLibraryElementId)
        {
            UITask.Run(async delegate
            {
                // used to check if the wrapper is in an editable detailhometabviewmodel
                var ParentDC = DataContext as DetailHomeTabViewModel;

                // get the region from the id
                var regionLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(regionLibraryElementId);
                if (regionLibraryElementController.LibraryElementModel.Type == ElementType.VideoRegion)
                {
                    return;
                }
                Debug.Assert(regionLibraryElementController != null);
       //         Debug.Assert(regionLibraryElementController.LibraryElementModel is RectangleRegion);
                if (regionLibraryElementController.LibraryElementModel.LibraryElementId == Controller.LibraryElementModel.LibraryElementId)
                {
                    return;
                }
                // create the view and vm based on the region type
                // todo add pdf, and video functionality
                FrameworkElement view = null;
                RegionViewModel vm = null;
                switch (regionLibraryElementController.LibraryElementModel.Type)
                {
                    case ElementType.AudioRegion:
                        vm = new AudioRegionViewModel(regionLibraryElementController.LibraryElementModel as AudioRegionModel,
                                regionLibraryElementController as AudioRegionLibraryElementController, this);
                        view = new AudioRegionView(vm as AudioRegionViewModel);
                        break;
                    default:
                        vm = null;
                        view = null;
                        break;
                }

                // set editable based on the parent data context
                vm.Editable = false;
                if (ParentDC != null)
                {
                    vm.Editable = ParentDC.Editable;
                }

                // add the region to thew view

                xClippingCanvas.Items.Add(view);
            });
            return null;
        }

        public void RemoveRegionView(string regionLibraryElementId)
        {


            foreach (var item in xClippingCanvas.Items)
            {
                var region = (item as FrameworkElement).DataContext as RegionViewModel;
                Debug.Assert(region != null);

                if (region.Model.LibraryElementId == regionLibraryElementId)
                {
                    xClippingCanvas.Items.Remove(item);
                    return;
                }
            }
        }

        private void xClippingGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
