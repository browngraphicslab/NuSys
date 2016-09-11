using NuSysApp.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NusysIntermediate;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfDetailHomeTabView : UserControl
    {
        private NusysImageAnalysisModel _analysisModel;

        private CanvasRenderEngine _renderEngine;
        private RenderItemInteractionManager _interactionManager;
        private ImageDetailRenderItem _imageDetailRenderItem;

        public bool ShowRegions;

        public PdfDetailHomeTabView(PdfDetailHomeTabViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();

            xImgCanvas.CreateResources += XImgCanvasOnCreateResources;
            vm.LibraryElementController.Disposed += ControllerOnDisposed;

            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed += DetailViewerView_Disposed;
            SizeChanged += OnSizeChanged;

            Task.Run(async delegate
            {
                _analysisModel = await SessionController.Instance.NuSysNetworkSession.FetchAnalysisModelAsync(vm.LibraryElementController.LibraryElementModel.ContentDataModelId) as NusysImageAnalysisModel;
                UITask.Run(async delegate {
                    SetImageAnalysis();
                });
            });
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (_imageDetailRenderItem == null)
                return;
            xImgCanvas.Width = totalStackPanel.ActualWidth;
            xImgCanvas.Height = totalStackPanel.ActualHeight;
            _imageDetailRenderItem.CanvasSize = new Size(totalStackPanel.ActualWidth, totalStackPanel.ActualHeight);

        }

        private async void XImgCanvasOnCreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            var vm = (PdfDetailHomeTabViewModel)DataContext;
            var libElemController = vm.LibraryElementController as PdfLibraryElementController;
            libElemController.ContentDataController.ContentDataModel.OnRegionAdded += ContentDataModelOnOnRegionAdded;
            libElemController.ContentDataController.ContentDataModel.OnRegionRemoved += ContentDataModelOnOnRegionRemoved;

            _renderEngine = new CanvasRenderEngine();
            var root = new BaseRenderItem(null, xImgCanvas);
            _imageDetailRenderItem = new ImageDetailRenderItem(libElemController, new Size(xImgCanvas.Width, xImgCanvas.Height), root, xImgCanvas);
            _imageDetailRenderItem.IsRegionsVisible = ShowRegions;
            _imageDetailRenderItem.CanvasSize = new Size(totalStackPanel.ActualWidth, totalStackPanel.ActualHeight);

            xImgCanvas.Width = totalStackPanel.ActualWidth;
            xImgCanvas.Height = totalStackPanel.ActualHeight;

            if (ShowRegions)
            {
                _imageDetailRenderItem.IsRegionsModifiable = true;
            }
            _imageDetailRenderItem.NeedsRedraw += ImageOnNeedsRedraw;

            await _imageDetailRenderItem.Load();
            root.Children.Add(_imageDetailRenderItem);



            _renderEngine.Init(xImgCanvas, root);
            _interactionManager = new RenderItemInteractionManager(_renderEngine, xImgCanvas);
            xImgCanvas.Invalidate();
        }

        private void ContentDataModelOnOnRegionRemoved(string regionLibraryElementModelId)
        {
            xImgCanvas.Invalidate();
        }

        private void ContentDataModelOnOnRegionAdded(string regionLibraryElementModelId)
        {
            xImgCanvas.Invalidate();
        }

        private void ImageOnNeedsRedraw()
        {
            xImgCanvas.Invalidate();
        }

        /// <summary>
        /// Set information gained from cognitive image analysis.
        /// </summary>
        private void SetImageAnalysis()
        {
            if (_analysisModel != null)
            {
                //set description to caption with highest confidence
                var descriptionlist = _analysisModel.Description.Captions.ToList();
                var bestDescription = descriptionlist.OrderByDescending(x => x.Confidence).FirstOrDefault();
                xDescription.Text = bestDescription.Text;

                if (_analysisModel.Categories != null && _analysisModel.Categories.Any())
                {
                    //get categories and add the category if the score meets min confidence level
                    var categorylist = _analysisModel.Categories.ToList();
                    var categories =
                        categorylist.Where(x => x.Score > Constants.MinConfidence).OrderByDescending(x => x.Score);
                    foreach (var i in categories)
                    {
                        i.Name = i.Name.Replace("_", " ");
                        i.Name.Trim();
                    }
                    xCategories.Text = string.Join(", ", categories.Select(category => string.Join(", ", category.Name)));
                }


                //get tag list and order them in order of confidence
                var taglist = _analysisModel.Tags?.ToList().OrderByDescending(x => x.Confidence);
                //add to items control of suggested tags
                foreach (var i in taglist)
                {
                    var tag = MakeSuggestedTag(i.Name);
                    xTags?.Items?.Add(tag);
                }
            }

        }
        /// <summary>
        /// hyperlink button that represents an auto generated tag
        /// on tapped they are added to the element's taglist
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private FrameworkElement MakeSuggestedTag(string text)
        {
            HyperlinkButton tag;
            tag = new HyperlinkButton();
            tag.Margin = new Thickness(3);
            tag.Content = text;
            tag.FontStyle = FontStyle.Italic;
            tag.Foreground = new SolidColorBrush(Constants.color3);
            tag.Tapped += SuggestedTag_OnTapped;

            return tag;
        }
        /// <summary>
        /// handler to add tag to element's taglist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SuggestedTag_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var tag = (HyperlinkButton)sender;
            var keyword = new Keyword((string)tag.Content, Keyword.KeywordSource.UserInput);
            var vm = (PdfDetailHomeTabViewModel)DataContext;
            vm.LibraryElementController.AddKeyword(keyword);

            //remove from suggested tag list
            foreach (var i in xTags?.Items)
            {
                var currTag = (HyperlinkButton)i;
                if (currTag.Content == tag.Content)
                {
                    xTags?.Items?.Remove(i);
                }
            }
        }

        /// <summary>
        /// dispose suggestedtag_ontapped method
        /// </summary>
        private void DisposeTags()
        {
            foreach (var i in xTags?.Items)
            {
                var currTag = (HyperlinkButton)i;
                currTag.Tapped -= SuggestedTag_OnTapped;
            }
        }

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {

            Dispose();
        }

        private void Dispose()
        {
            var vm = (PdfDetailHomeTabViewModel)DataContext;
            var libElemController = vm.LibraryElementController as PdfLibraryElementController;
            libElemController.ContentDataController.ContentDataModel.OnRegionAdded -= ContentDataModelOnOnRegionAdded;
            _imageDetailRenderItem?.Dispose();
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;
            _interactionManager?.Dispose();
            _interactionManager = null;
            SizeChanged -= OnSizeChanged;
            DisposeTags();
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (ImageDetailHomeTabViewModel)DataContext;
            vm.LibraryElementController.Disposed -= ControllerOnDisposed;

            DataContext = null;
        }

        /// <summary>
        /// collapse image analysis for more space if necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CollapseIcon_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (ContentScrollViewer.Visibility == Visibility.Visible)
            {
                ContentScrollViewer.Visibility = Visibility.Collapsed;
                CollapseIcon.Source = new BitmapImage(new Uri("ms-appx:///Assets/open up.png"));
            }
            else
            {
                ContentScrollViewer.Visibility = Visibility.Visible;
                CollapseIcon.Source = new BitmapImage(new Uri("ms-appx:///Assets/collapse down.png"));
            }
        }

        /// <summary>
        /// when the suggest Temp Regions button is pressen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (PdfDetailHomeTabViewModel)DataContext;
            if (vm == null)
            {
                return;
            }

            var contentDataModelId = vm.LibraryElementController.LibraryElementModel.ContentDataModelId;
            Task.Run(async delegate
            {
                var analysisModel = await SessionController.Instance.NuSysNetworkSession.FetchAnalysisModelAsync(contentDataModelId) as NusysImageAnalysisModel;

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

                            //   var tempvm = new TemporaryImageRegionViewModel(new Point(rect.Left.Value, rect.Top.Value), rect.Width.Value, rect.Height.Value, this.xClippingWrapper, this.DataContext as DetailHomeTabViewModel,null,vm.LibraryElementController.LibraryElementModel.AccessType);
                            //     var tempview = new TemporaryImageRegionView(tempvm);
                            //       tempvm.MetadataToAddUponBeingFullRegion = metadataDict;
                            //  xClippingWrapper.AddTemporaryRegion(tempview);

                        }
                    }
                });

            });
        }

    }
}
