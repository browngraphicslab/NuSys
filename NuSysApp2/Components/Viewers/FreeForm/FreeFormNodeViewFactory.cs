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
                case ElementType.Text:
                    view = new TextNodeView(new TextNodeViewModel(controller));
                    break;
                case ElementType.Collection:
                    view = new GroupNodeView(new GroupNodeViewModel((ElementCollectionController)controller));
                    break;
                case ElementType.Tag:
                    view = new LabelNodeView(new LabelNodeViewModel((ElementController)controller));
                    break;
                case ElementType.Image:
                    view = new ImageNodeView(new ImageElementViewModel(controller));
                    break;
                case ElementType.Word:
                    view = new WordNodeView(new WordNodeViewModel(controller));
                    break;
                case ElementType.Powerpoint:
                    view = new PowerpointNodeView(new PowerpointNodeViewModel(controller));
                    break;
                case ElementType.Audio:
                    view = new AudioNodeView(new AudioNodeViewModel(controller));
                    break;
                case ElementType.PDF:
                    view = new PdfNodeView(new PdfNodeViewModel(controller));
                    break;
                case ElementType.Video:
                    view = new VideoNodeView(new VideoNodeViewModel(controller));
                    break;
                case ElementType.Web:
                    view = new WebNodeView(new WebNodeViewModel(controller));
                    break;
                case ElementType.Area:
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
            await ((ElementViewModel)view.DataContext).Init();

            return view;
        }


       
    }
}
