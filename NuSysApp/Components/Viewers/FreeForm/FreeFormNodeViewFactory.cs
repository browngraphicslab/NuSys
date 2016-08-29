using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NuSysApp.Controller;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;

namespace NuSysApp
{
    public class FreeFormNodeViewFactory : INodeViewFactory
    {
        public async Task<FrameworkElement> CreateFromSendable(ElementController controller)
        {
            UserControl view = null;

            var model = controller.Model;

            switch (model.ElementType)
            {
                case NusysConstants.ElementType.Text:
                    view = new TextNodeView(new TextNodeViewModel(controller));
                    break;
                case NusysConstants.ElementType.Collection:

                    view = new GroupNodeView(new GroupNodeViewModel((ElementCollectionController)controller));
                    break;
                case NusysConstants.ElementType.Tag:
                    view = new LabelNodeView(new LabelNodeViewModel((ElementController)controller));
                    break;
                case NusysConstants.ElementType.ImageRegion:
                case NusysConstants.ElementType.Image:
                    view = new ImageNodeView(new ImageElementViewModel(controller));
                    break;
                case NusysConstants.ElementType.Word:
                    view = new WordNodeView(new WordNodeViewModel(controller));
                    break;
                case NusysConstants.ElementType.Powerpoint:
                    view = new PowerpointNodeView(new PowerpointNodeViewModel(controller));
                    break;
                case NusysConstants.ElementType.AudioRegion:
                case NusysConstants.ElementType.Audio:
                    view = new AudioNodeView(new AudioNodeViewModel(controller));
                    break;
                case NusysConstants.ElementType.PdfRegion:
                case NusysConstants.ElementType.PDF:
                    view = new PdfNodeView(new PdfNodeViewModel(controller));
                    break;
                case NusysConstants.ElementType.VideoRegion:
                case NusysConstants.ElementType.Video:
                    view = new VideoNodeView(new VideoNodeViewModel(controller));
                    break;
                case NusysConstants.ElementType.Web:
                    view = new WebNodeView(new WebNodeViewModel(controller));
                    break;
                case NusysConstants.ElementType.Area:
                    view = new AreaNodeView(new AreaNodeViewModel((ElementCollectionController)controller));
                    break;/*
                case ElementType.Link:
                    var linkModel = (LinkModel)controller.Model;
                    if (linkModel.IsPresentationLink)
                        view = new PresentationLinkView(new LinkViewModel((LinkController)controller));
                    else
                        view = new BezierLinkView(new LinkViewModel((LinkController)controller));
                    break;*/
            }
            Debug.Assert(view != null, "this should never return null");
            await ((ElementViewModel)view.DataContext).Init();

            return view;
        }


       
    }
}
