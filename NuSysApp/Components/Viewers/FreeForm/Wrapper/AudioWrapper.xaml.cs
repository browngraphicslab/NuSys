﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
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
using NusysIntermediate;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NuSysApp
{

    public sealed partial class AudioWrapper : Page, INuSysDisposable
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

        public event EventHandler Disposed;

        private LibraryElementController _contentController;


        /// <summary>
        /// The content of the wrapper, a media element in this case
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
            }
        }
        //denormalized start of audio
        public double AudioStart { set; get; }

        //denormalized end of audio
        public double AudioEnd { set; get; }

        //Currently selected region.
        private FrameworkElement _selectedRegion;


        public delegate void RegionsUpdatedEventHandler(object sender, List<double> regionMarkers);
        public event RegionsUpdatedEventHandler OnRegionsUpdated;

        public delegate void RegionSeekedEventHandler(double normalizedTime);
        public event RegionSeekedEventHandler OnRegionSeeked;

        public delegate void IntervalChangedEventHandler(object sender, double start, double end);
        public event IntervalChangedEventHandler OnIntervalChanged;

        public AudioWrapper()
        {
            this.InitializeComponent();
        }

        public void ProcessLibraryElementController()
        {
            Debug.Assert(Controller != null);
            var type = Controller.LibraryElementModel.Type;

            if (NusysConstants.IsRegionType(type))
            {
                RegionLibraryElementController regionController = null;
                switch (Controller.LibraryElementModel.Type)
                {
                    //AudioStart and AudioEnd must be stored so that the media element knows the bounds of the region
                    case NusysConstants.ElementType.AudioRegion:
                        regionController = Controller as AudioRegionLibraryElementController;
                        AudioStart = (regionController as AudioRegionLibraryElementController).AudioRegionModel.Start;
                        AudioEnd = (regionController as AudioRegionLibraryElementController).AudioRegionModel.End;
                        (regionController as AudioRegionLibraryElementController).TimeChanged += AudioWrapper_IntervalUpdated;
                        break;
                    case NusysConstants.ElementType.VideoRegion:
                        regionController = Controller as VideoRegionLibraryElementController;
                        AudioStart = (regionController as VideoRegionLibraryElementController).VideoRegionModel.Start;
                        AudioEnd = (regionController as VideoRegionLibraryElementController).VideoRegionModel.End;
                        (regionController as VideoRegionLibraryElementController).IntervalChanged += AudioWrapper_IntervalUpdated;
                        break;
                }


            }
            else
            {
                AudioStart = 0;
                AudioEnd = 1;
            }

            // clear the items control
            xClippingCanvas.Items.Clear();


            //clear our reference to the selected region
            _selectedRegion = null;

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

            var compositeTransform = new CompositeTransform();
            compositeTransform.ScaleX = 1 / (AudioEnd - AudioStart);
            //    compositeTransform.CenterX = this.ActualWidth * (AudioStart + (AudioEnd - AudioStart) / 2.0);
            compositeTransform.TranslateX = -AudioStart * this.ActualWidth / (AudioEnd - AudioStart);
            xClippingCanvas.RenderTransform = compositeTransform;
        }

        private void AudioWrapper_IntervalUpdated(object sender, double start, double end)
        {
            AudioStart = start;
            AudioEnd = end;
            OnIntervalChanged?.Invoke(sender, start, end);
        }

        public double GetWidth()
        {
            return this.ActualWidth;
        }
        public double GetHeight()
        {
            return this.ActualHeight;
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

                Debug.Assert(regionLibraryElementController != null);
                //  Debug.Assert(regionLibraryElementController.LibraryElementModel is RectangleRegion);
                if (regionLibraryElementController.LibraryElementModel.LibraryElementId == Controller.LibraryElementModel.LibraryElementId)
                {
                    return;
                }
                // create the view and vm based on the region type
                FrameworkElement view = null;
                RegionViewModel vm = null;
                var renderTransform = xClippingCanvas.RenderTransform as CompositeTransform ?? new CompositeTransform();
                switch (regionLibraryElementController.LibraryElementModel.Type)
                {
                    case NusysConstants.ElementType.AudioRegion:
                        vm = new AudioRegionViewModel(regionLibraryElementController.LibraryElementModel as AudioRegionModel,
                                regionLibraryElementController as AudioRegionLibraryElementController, this);
                        view = new AudioRegionView(vm as AudioRegionViewModel);
                        var audioRegionView = view as AudioRegionView;
                        audioRegionView.RescaleComponents(renderTransform.ScaleX);
                        audioRegionView.OnRegionSeek += AudioWrapper_OnRegionSeek;
                        audioRegionView.OnSelectedOrDeselected += Region_OnSelectedOrDeselected;
                        break;
                    case NusysConstants.ElementType.VideoRegion:
                        vm = new VideoRegionViewModel(regionLibraryElementController.LibraryElementModel as VideoRegionModel,
                                regionLibraryElementController as VideoRegionLibraryElementController, this);
                        view = new VideoRegionView(vm as VideoRegionViewModel);
                        var videoRegionView = view as VideoRegionView;
                        videoRegionView.RescaleComponents(renderTransform.ScaleX);
                        videoRegionView.OnRegionSeek += AudioWrapper_OnRegionSeek;
                        videoRegionView.OnSelectedOrDeselected += Region_OnSelectedOrDeselected;

                        break;
                    default:
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

                //Fires RegionsUpdated event so that timeline markers of AudioMediaPlayer's MediaElement are accurate.
                FireRegionsUpdated();
            });
            return null;
        }

        /// <summary>
        /// Called when progress bar is tapped. Checks time tapped to see if any regions
        /// were hit. If so, the region is selected. If not, the region is deselected.
        /// </summary>
        /// <param name="normalizedMediaElementPosition"></param>
        public void CheckTimeForRegions(double normalizedMediaElementPosition)
        {
            foreach (var item in xClippingCanvas.Items)
            {
                var regionViewModel = (item as FrameworkElement).DataContext as RegionViewModel;
                switch (regionViewModel.Model.Type)
                {
                    case NusysConstants.ElementType.AudioRegion:
                        var audioRegionmodel = regionViewModel.Model as AudioRegionModel;
                        if(normalizedMediaElementPosition < audioRegionmodel.End && normalizedMediaElementPosition > audioRegionmodel.Start)
                        {
                            (item as AudioRegionView).FireSelection();

                        }
                        else
                        {
                            (item as AudioRegionView).FireDeselection();
                        }

                        break;
                    case NusysConstants.ElementType.VideoRegion:
                        var videoRegionModel = regionViewModel.Model as VideoRegionModel;
                        if (normalizedMediaElementPosition < videoRegionModel.End && normalizedMediaElementPosition > videoRegionModel.Start)
                        {
                            (item as VideoRegionView).FireSelection();

                        }
                        else
                        {
                            (item as VideoRegionView).FireDeselection();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Called when audio region view is tapped. Invokes event listened to by AudioMediaPlayer/VideoMediaPlayer
        /// </summary>
        /// <param name="time"></param>
        public void AudioWrapper_OnRegionSeek(double time)
        {
            OnRegionSeeked?.Invoke(time);
        }

        /// <summary>
        /// Fired when time (start and/or end) is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void AudioWrapper_TimeChanged(object sender, double start, double end)
        {
            FireRegionsUpdated();
        }
        /// <summary>
        /// Gets a list of doubles representing normalized timeline markers and fires event listened to by audiomediaplayer.
        /// </summary>
        private void FireRegionsUpdated()
        {

            var timelineMarkers = GetTimelineMarkers();
            OnRegionsUpdated?.Invoke(this, timelineMarkers);
        }

        
        public void RemoveRegionView(string regionLibraryElementId)
        {

            foreach (var item in xClippingCanvas.Items)
            {
                var regionVM = (item as FrameworkElement).DataContext as RegionViewModel;
                Debug.Assert(regionVM != null);
                if (regionVM.Model.LibraryElementId == regionLibraryElementId)
                {
                    xClippingCanvas.Items.Remove(item);
                    //Fires ONRegionsUpdated event so that the parent AudioMediaPlayer's MediaElement will have a correct list
                    //of TimelineMarkers.
                    FireRegionsUpdated();
                    regionVM.Dispose(null, EventArgs.Empty);
                    return;
                }
            }
        }

        /// <summary>
        /// Returns a list of normalized doubles representing start and end of the audio region. Both of these doubles will be added
        /// as a timeline marker to the AudioMediaPlayer's MediaElement's Markers.
        /// </summary>
        /// <returns></returns>
        public List<double> GetTimelineMarkers()
        {
            var timelineMarkers = new List<double>();
            foreach (var item in xClippingCanvas.Items)
            {
                var regionViewModel = (item as FrameworkElement).DataContext as RegionViewModel;
                switch (regionViewModel.Model.Type)
                {
                    case NusysConstants.ElementType.AudioRegion:
                        var audioModel = regionViewModel.Model as AudioRegionModel;
                        timelineMarkers.Add(audioModel.Start);
                        timelineMarkers.Add(audioModel.End);
                        break;
                    case NusysConstants.ElementType.VideoRegion:
                        var videoModel = regionViewModel.Model as VideoRegionModel;
                        timelineMarkers.Add(videoModel.Start);
                        timelineMarkers.Add(videoModel.End);
                        break;
                    default:
                        break;
                }
            }
            return timelineMarkers;
        }

        private void Region_OnSelectedOrDeselected(object sender, bool selected)
        {
            if (selected)
            {
                var region = sender as FrameworkElement;
                Debug.Assert(region != null);
                DeselectRegion(_selectedRegion);
                _selectedRegion = region;
            }
            else
            {
                var region = sender as FrameworkElement;
                Debug.Assert(region != null);
                _selectedRegion = null;
            }
        }
        /// <summary>
        /// Helper method for deselecting regoins based on type
        /// </summary>
        private void DeselectRegion(FrameworkElement item)
        {
            if (item == null)
            {
                return;
            }
            var regionViewModel = (item as FrameworkElement).DataContext as RegionViewModel;
            switch (regionViewModel.Model.Type)
            {
                case NusysConstants.ElementType.AudioRegion:
                    var audioRegionView = item as AudioRegionView;
                    audioRegionView.FireDeselection();
                    break;
                case NusysConstants.ElementType.VideoRegion:
                    var videoRegionView = item as VideoRegionView;
                    videoRegionView.FireDeselection();
                    break;
                default:
                    break;
            }
        }

        private void xClippingGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            if (AudioEnd != 0 || AudioStart != 0)
            {
                         var compositeTransform = new CompositeTransform();
                         compositeTransform.ScaleX = 1 / (AudioEnd - AudioStart);
                compositeTransform.TranslateX = -AudioStart * this.ActualWidth / (AudioEnd - AudioStart);

                //   compositeTransform.CenterX = this.ActualWidth * (AudioStart + (AudioEnd - AudioStart) / 2.0);
                xClippingCanvas.RenderTransform = compositeTransform;
                foreach (var item in xClippingCanvas.Items)
                {
                    var regionViewModel = (item as FrameworkElement).DataContext as RegionViewModel;
                    FrameworkElement region;
                    switch (regionViewModel.Model.Type)
                    {
                        case NusysConstants.ElementType.AudioRegion:
                            region = item as AudioRegionView;
                            (region as AudioRegionView).RescaleComponents(compositeTransform.ScaleX);
                            break;
                        case NusysConstants.ElementType.VideoRegion:
                            region = item as VideoRegionView;
                            (region as VideoRegionView).RescaleComponents(compositeTransform.ScaleX);
                            break;
                        default:
                            break;
                    }
                }
            }

    
        }


        /// <summary>
        /// Checks marker's time to see if it coincides with the start or end of ALL audio regions.
        /// If coincides with start, view is selected.
        /// If coincides with end, view is deselected.
        /// </summary>
        /// <param name="denormalizedMarkerTime"></param>
        /// <param name="totalDuration"></param>
        public void CheckMarker(double denormalizedMarkerTime, double totalDuration)
        {
            foreach (var item in xClippingCanvas.Items)
            {
                var regionViewModel = (item as FrameworkElement).DataContext as RegionViewModel;
                //FrameworkElement region;
                switch (regionViewModel.Model.Type)
                {
                    case NusysConstants.ElementType.AudioRegion:
                        var audioRegionView = item as AudioRegionView;
                        var audioRegionViewModel = regionViewModel as AudioRegionViewModel;
                        var audioRegionModel = regionViewModel.Model as AudioRegionModel;

                        if ((int)(audioRegionModel.Start * totalDuration) == denormalizedMarkerTime)
                        {
                            audioRegionView.FireSelection();
                        }
                        if ((int)(audioRegionModel.End * totalDuration) == denormalizedMarkerTime)
                        {
                            audioRegionView.FireDeselection();

                        }
                        break;
                    case NusysConstants.ElementType.VideoRegion:
                        var videoRegionView = item as VideoRegionView;
                        var videoRegionViewModel = regionViewModel as VideoRegionViewModel;
                        var videoRegionModel = regionViewModel.Model as VideoRegionModel;

                        if ((int)(videoRegionModel.Start * totalDuration) == denormalizedMarkerTime)
                        {
                            videoRegionView.FireSelection();
                        }
                        if ((int)(videoRegionModel.End * totalDuration) == denormalizedMarkerTime)
                        {
                            videoRegionView.FireDeselection();

                        }
                        break;
                }
            }
        }

        public void Dispose()
        {
            if (Controller != null)
            {
                var contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(Controller.LibraryElementModel.ContentDataModelId);
                contentDataModel.OnRegionAdded -= AddRegionView;
                contentDataModel.OnRegionRemoved -= RemoveRegionView;
            }
            Disposed?.Invoke(this, EventArgs.Empty);
        }

    }
}