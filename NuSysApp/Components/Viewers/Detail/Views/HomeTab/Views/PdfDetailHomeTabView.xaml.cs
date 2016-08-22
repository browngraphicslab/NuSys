using NuSysApp.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NusysIntermediate;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfDetailHomeTabView : UserControl
    {

        private NusysPdfAnalysisModel _analysisModel;
        public PdfDetailHomeTabView(PdfDetailHomeTabViewModel vm)
        {
            InitializeComponent();
            //Show/hide regions buttons need reference to rectangle wrapper for methods to work.
            xShowHideRegionButtons.Wrapper = xClippingWrapper;

            vm.LibraryElementController.Disposed += ControllerOnDisposed;

            // disable page left and page right buttons for pdf regions
            if (vm.LibraryElementController.LibraryElementModel.Type == NusysConstants.ElementType.PdfRegion)
            {
                pageLeft.Visibility = Visibility.Collapsed;
                pageRight.Visibility = Visibility.Collapsed;
            }

            DataContext = vm;
            vm.PageLocationChanged += Vm_PageLocationChanged;
            Loaded += PdfDetailHomeTabView_Loaded;

            xClippingWrapper.Controller = vm.LibraryElementController;
            xClippingWrapper.ProcessLibraryElementController();

            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed += DetailViewerView_Disposed;

            Task.Run(async delegate
            {
                _analysisModel = await SessionController.Instance.NuSysNetworkSession.FetchAnalysisModelAsync(vm.LibraryElementController.LibraryElementModel.ContentDataModelId) as NusysPdfAnalysisModel;
                UITask.Run(async delegate {
                    SetPdfSuggestions(vm.CurrentPageNumber);
                });
            });
        }

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;
            Dispose(); 
        }

        private void Vm_PageLocationChanged(object sender, int pageLocation)
        {
            UpdateRegionViews(pageLocation);
            SetPdfSuggestions(pageLocation);
        }

        /// <summary>
        /// sets the page info via the suggestion box.  THe info is gathered from the server-given Analysis Model
        /// </summary>
        /// <param name="pageNumber"></param>
        private void SetPdfSuggestions(int pageNumber)
        {
            xPageNumberBox.Text = pageNumber.ToString();
            if (_analysisModel != null)
            {
                if (_analysisModel.DocumentAnalysisModel.Segments.Any(segment => segment.pageNumber == pageNumber))
                {
                    xSentimentBox.Text = Math.Round( _analysisModel.DocumentAnalysisModel.Segments.Where(segment => segment.pageNumber == pageNumber).Average(segment => segment.SentimentRating)*100, 3) + " %";
                }
                else
                {
                    xSentimentBox.Text = "None found";
                }
                xKeyPhrasesBox.Text = string.Join(", ", _analysisModel.DocumentAnalysisModel.Segments.Where(segment => segment.pageNumber == pageNumber).Select(segment => string.Join(", ", segment.KeyPhrases)));
            }
            else
            {
                xSentimentBox.Text = "...";
                xKeyPhrasesBox.Text = "...";
            }
        }

        private async void PdfDetailHomeTabView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PdfDetailHomeTabViewModel;
            xClippingWrapper.Controller = vm.LibraryElementController;
            await xClippingWrapper.ProcessLibraryElementController();
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (PdfDetailHomeTabViewModel)DataContext;
            vm.LibraryElementController.Disposed -= ControllerOnDisposed;
            DataContext = null;
        }
        
        private async void OnPageLeftClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfDetailHomeTabViewModel)this.DataContext;
            if (vm == null)
                return;
            await vm.FlipLeft();
        }

        private async void OnPageRightClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfDetailHomeTabViewModel)this.DataContext;
            if (vm == null)
                return;
            await vm.FlipRight();
        }

        private async void UpdateRegionViews(int currentPageNumber)
        {
            // takes care of visibility of temporary regions
            foreach(var item in xClippingWrapper.GetTemporaryRegionItems())
            {
                var tempRegion = item as TemporaryImageRegionView;
                var tempRegionDC = tempRegion.DataContext as TemporaryImageRegionViewModel;

                if (tempRegionDC.PageLocation != null)
                {
                    await UITask.Run(() =>
                    {
                        tempRegion.Visibility = tempRegionDC.PageLocation == currentPageNumber ? Visibility.Visible : Visibility.Collapsed;

                    });
                }

            }

            // takes care of visibility of normal regions
            foreach (var item in xClippingWrapper.GetRegionItems())
            {
                var regionView = item as PDFRegionView;
                var model = (regionView?.DataContext as PdfRegionViewModel)?.Model as PdfRegionModel;
                Debug.Assert(regionView != null);
                await UITask.Run(() =>
                {
                    regionView.Visibility = model?.PageLocation == currentPageNumber ? Visibility.Visible : Visibility.Collapsed;
                });
            }
        }

        public void Dispose()
        {
            var vm = DataContext as PdfDetailHomeTabViewModel;
            if (vm != null) // because delete library element request can remove the view model outside of this
            {
                vm.PageLocationChanged -= Vm_PageLocationChanged;
            }

            xClippingWrapper.Dispose();
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (PdfDetailHomeTabViewModel)DataContext;
            if (vm == null)
            {
                return;
            }

            if (_analysisModel != null)
            {
                var suggestedRegions = _analysisModel.PageImageAnalysisModels.SelectMany(item => item?.Regions ?? new List<CognitiveApiRegionModel>());//.Where(i => i.MarkedImportant);

                foreach (var region in suggestedRegions)
                {
                    var rect = region.Rectangle;
                    if (rect.Height == null || rect.Left == null || rect.Top == null || rect.Width == null)
                    {
                        continue;
                    }
                    var tempvm = new TemporaryImageRegionViewModel(new Point(rect.Left.Value, rect.Top.Value), rect.Width.Value, rect.Height.Value, this.xClippingWrapper, this.DataContext as DetailHomeTabViewModel, region.PageNumber);
                    var tempview = new TemporaryImageRegionView(tempvm);
                    xClippingWrapper.AddTemporaryRegion(tempview);
                }
                vm.Goto(vm.CurrentPageNumber);
            }

            /*
            var contentDataModelId = vm.LibraryElementController.LibraryElementModel.ContentDataModelId;
            Task.Run(async delegate
            {
                //create the request to get the analysis model
                var request = new GetAnalysisModelRequest(contentDataModelId);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                var analysisModel = request.GetReturnedAnalysisModel() as NusysImageAnalysisModel;

                //switch back to UI thread for adding the regions
                await UITask.Run(delegate
                {
                    if (analysisModel != null && analysisModel.Faces != null && analysisModel.Faces.Length > 0)
                    {
                        //iterate through each suggestion
                        foreach (var suggestedRegion in analysisModel.Faces)
                        {
                            var rect = suggestedRegion.FaceRectangle;

                            var metadataDict = new List<MetadataEntry>();

                            if (suggestedRegion.Age != null)//to add the age to the future region
                            {
                                metadataDict.Add(new MetadataEntry("suggested_age", new List<string>() { suggestedRegion.Age.Value.ToString() }, MetadataMutability.MUTABLE));
                            }
                            if (!string.IsNullOrEmpty(suggestedRegion.Gender))//to add the gender to the future region
                            {
                                metadataDict.Add(new MetadataEntry("suggested_gender", new List<string>() { suggestedRegion.Gender }, MetadataMutability.MUTABLE));
                            }

                            if (rect == null || rect.Left == null || rect.Top == null || rect.Height == null || rect.Width == null)
                            {
                                continue;
                            }
                            //create a temp region for every face
                            var tempvm = new TemporaryImageRegionViewModel(new Point(rect.Left.Value, rect.Top.Value), rect.Width.Value, rect.Height.Value, this.xClippingWrapper, this.DataContext as DetailHomeTabViewModel);
                            var tempview = new TemporaryImageRegionView(tempvm);
                            tempvm.MetadataToAddUponBeingFullRegion = metadataDict;
                            xClippingWrapper.AddTemporaryRegion(tempview);
                        }
                    }
                });

            });
        */
        }
}
}