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
    public class FreeFormElementViewModelFactory
    {
        public async Task<ElementViewModel> CreateFromSendable(ElementController controller)
        {
            ElementViewModel vm = null;

            var model = controller.Model;

            switch (model.ElementType)
            {
                case ElementType.Text:
                    vm = new TextNodeViewModel(controller);
                    break;
                case ElementType.Collection:
                    vm =new GroupNodeViewModel((ElementCollectionController)controller);
                    break;
                case ElementType.Tag:
                    vm =new LabelNodeViewModel((ElementController)controller);
                    break;
                case ElementType.Image:
                    vm = new ImageElementViewModel(controller);
                    break;
                case ElementType.Word:
                    vm = new WordNodeViewModel(controller);
                    break;
                case ElementType.Powerpoint:
                    vm = new PowerpointNodeViewModel(controller);
                    break;
                case ElementType.Audio:
                    vm = new AudioNodeViewModel(controller);
                    break;
                case ElementType.PDF:
                    vm = new PdfNodeViewModel(controller);
                    break;
                case ElementType.Video:
                    vm = new VideoNodeViewModel(controller);
                    break;
                case ElementType.Web:
                    vm =new WebNodeViewModel(controller);
                    break;
                case ElementType.Area:
                    vm = new AreaNodeViewModel((ElementCollectionController)controller);
                    break;/*
                case ElementType.Link:
                    var linkModel = (LinkModel)controller.Model;
                    if (linkModel.IsPresentationLink)
                        view = new PresentationLinkView(new LinkViewModel((LinkController)controller));
                    else
                        view = new BezierLinkView(new LinkViewModel((LinkController)controller));
                    break;*/
            }
            await vm.Init();

            return vm;
        }


       
    }
}
