using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Viewers;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class NodeMenuRight : UserControl
    {

        private Image _dragItem;

        private enum DragMode { Duplicate, Tag, Link, PresentationLink };
        private DragMode _currenDragMode = DragMode.Duplicate;

        public NodeMenuRight()
        {
            this.InitializeComponent();
                      
            DuplicateElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            DuplicateElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            Link.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            Link.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            PresentationLink.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            PresentationLink.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            PresentationMode.Click += OnPresentationClick; 

        }

        private void OnBtnDeleteClick(object sender, RoutedEventArgs e)
        {
            var model = (ElementModel)((ElementViewModel)this.DataContext).Model;
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(model.Id));
        }

        private void OnPresentationClick(object sender, RoutedEventArgs e)
        {

            var vm = ((ElementViewModel)this.DataContext);
            var sv = SessionController.Instance.SessionView;
            // unselect start element
            vm.IsSelected = false;
            vm.IsEditing = false;
            sv.EnterPresentationMode(vm);
        }

        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));
            var send = (FrameworkElement)sender;

            var vm = wvm.Selections.First();
            if (_currenDragMode == DragMode.Duplicate)
            {

      


                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitsStart = hitsStart.Where(uiElem => (uiElem as FrameworkElement) is GroupNodeView).ToList();

                if (hitsStart.Any())
                {
                    var first = (FrameworkElement)hitsStart.First();
                    var vm1 = (GroupNodeViewModel)first.DataContext;
                    var groupnode = (GroupNodeView)first;
                    var np = new Point(p.X - vm1.Model.Width / 2, p.Y - vm1.Model.Height / 2);
                    var canvas = groupnode.FreeFormView.AtomContainer;
                    var targetPoint = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(canvas).TransformPoint(p);
                    p = args.GetCurrentPoint(first).Position; ;

                    vm.Controller.RequestDuplicate(targetPoint.X, targetPoint.Y, new Message(await vm.Model.Pack()));
                }
                else
                {
                    vm.Controller.RequestDuplicate(r.X, r.Y, new Message(await vm.Model.Pack()));
                }
            }



            if (_currenDragMode == DragMode.Link || _currenDragMode == DragMode.PresentationLink)
            {
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitsStart = hitsStart.Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel).ToList();

                var hitsStart2 = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitsStart2 = hitsStart2.Where(uiElem => (uiElem as FrameworkElement).DataContext is RegionViewModel).ToList();

                var hitRectangleView = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitRectangleView = hitRectangleView.Where(uiElem => (uiElem as FrameworkElement).DataContext is RectangleViewModel).ToList();

                if (hitsStart.Any())
                {
                    var first = (FrameworkElement)hitsStart.First();
                    var dc = (ElementViewModel)first.DataContext;

                    if (vm == dc || (dc is FreeFormViewerViewModel) || dc is LinkViewModel)
                    {
                        return;
                    }

                    if (hitRectangleView.Any())
                    {
                        foreach (var element in hitRectangleView)
                        {
                            if (element is RectangleView)
                            {
                                Dictionary<string, object> inFgDictionary = vm.Controller.CreateTextDictionary(200, 100,
                                    100,
                                    200);
                                Dictionary<string, object> outFgDictionary = vm.Controller.CreateTextDictionary(100, 100,
                                    100,
                                    100);
                                if (_currenDragMode == DragMode.PresentationLink)
                                {
                                    vm.Controller.RequestPresentationLinkTo(dc.Id, (RectangleView)element, null, inFgDictionary,
                                        outFgDictionary);
                                }
                                else
                                {
                                    SessionController.Instance.LinkController.RequestLink(new LinkId(dc.ContentId), new LinkId(vm.ContentId));
                                }
                            }
                        }
                    }
                    else if (hitsStart2.Any())
                    {
                        foreach (var element in hitsStart2)
                        {
                            if (element is AudioRegionView)
                            {

                            }

                            if (element is VideoRegionView)
                            {

                            }

                            if (element is PDFRegionView)
                            {

                            }
                            if (element is ImageRegionView)
                            {
                                Dictionary<string, object> inFgDictionary = vm.Controller.CreateTextDictionary(200, 100, 100, 200);
                                Dictionary<string, object> outFgDictionary = vm.Controller.CreateTextDictionary(100, 100, 100, 100);
                                if (_currenDragMode == DragMode.PresentationLink)
                                {
                                    // vm.Controller.RequestPresentationLinkTo(dc.Id, null, element as ImageRegionView, inFgDictionary, outFgDictionary);
                                }
                                else
                                {
                                    SessionController.Instance.LinkController.RequestLink(new LinkId(dc.ContentId), new LinkId(vm.ContentId));
                                }
                            }

                            /*
                            if (element is LinkedTimeBlock)
                            {
                                Dictionary<string, object> inFgDictionary = vm.Controller.CreateTextDictionary(200, 100,
                                    100,
                                    200);
                                Dictionary<string, object> outFgDictionary = vm.Controller.CreateTextDictionary(100, 100,
                                    100,
                                    100);
                                if (_currenDragMode == DragMode.PresentationLink)
                                {
                                    vm.Controller.RequestPresentationLinkTo(dc.Id, null, (LinkedTimeBlock)element, inFgDictionary,
                                           outFgDictionary);
                                }
                                else
                                {
                                    vm.Controller.RequestLinkTo(dc.Id, null, (LinkedTimeBlock) element, inFgDictionary,
                                        outFgDictionary);
                                }
                                //(element as LinkedTimeBlock).changeColor();
                                //vm.Controller.RequestLinkTo(dc.Id, (LinkedTimeBlock)element);

                                */



                        }
                    }
                    else
                    {
                        if (dc.LinkList.Where(c => c.OutElement.Model.Id == vm.Id).Count() > 0 || vm.LinkList.Where(c => c.OutElement.Model.Id == dc.Id).Count() > 0)
                        {
                            return;
                        }


                        if (_currenDragMode == DragMode.Link)
                        {
                            SessionController.Instance.LinkController.RequestLink(new LinkId(dc.ContentId), new LinkId(vm.ContentId));
                            vm.Controller.RequestVisualLinkTo();
                        }
                        if (_currenDragMode == DragMode.PresentationLink)
                        {
                            vm.Controller.RequestPresentationLinkTo(dc.Id);
                        }
                    }
                }
            }

            ReleasePointerCaptures();
            (sender as FrameworkElement).RemoveHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta));
            args.Handled = true;
        }

        private void BtnAddOnManipulationDelta(object sender, PointerRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            var p = args.GetCurrentPoint(xCanvas).Position;
            t.TranslateX = p.X - _dragItem.ActualWidth / 2;
            t.TranslateY = p.Y - _dragItem.ActualHeight / 2;
            args.Handled = true;
        }


        private async void BtnAddOnManipulationStarting(object sender, PointerRoutedEventArgs args)
        {
            Debug.WriteLine("Starting once!");

            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);

            CapturePointer(args.Pointer);

            
            if (sender == DuplicateElement)
            {
                _currenDragMode = DragMode.Duplicate;
            }

            if (sender == Link)
            {
                _currenDragMode = DragMode.Link;
            }

            if (sender == PresentationLink)
            {
                _currenDragMode = DragMode.PresentationLink;
            }
            


            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            _dragItem = new Image();
            _dragItem.Source = bmp;
            _dragItem.Width = 50;
            _dragItem.Height = 50;
            xCanvas.Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            (sender as FrameworkElement).AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta), true);

            args.Handled = true;
        }
    }
}
