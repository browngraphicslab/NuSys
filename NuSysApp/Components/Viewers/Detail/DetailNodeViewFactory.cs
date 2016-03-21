using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class DetailNodeViewFactory
    {
        public async Task<UserControl> CreateFromSendable(ElementController controller)
        {
            UserControl view = null; 

            switch (controller.Model.ElementType)
            {
                case ElementType.Text:
                    var tvm = new TextNodeViewModel(controller);
                    view = new TextDetailView(tvm);
                    break;
                case ElementType.Image:
                    var ivm = new ImageElementViewModel(controller);
                    view = new ImageFullScreenView(ivm);
                    break;
                case ElementType.Word:
                    view = new WordDetailView(new WordNodeViewModel(controller));
                    break;
                case ElementType.Powerpoint:
                    view = new PowerpointDetailView(new PowerpointNodeViewModel(controller));
                    break;
                case ElementType.PDF:
                    view = new PdfDetailView(new PdfNodeViewModel(controller));
                    break;
                case ElementType.Web:
                    view = new WebDetailView(new WebNodeViewModel(controller));
                    break;
                case ElementType.Video:
                    view = new VideoDetailView(new VideoNodeViewModel(controller));
                    break;
                case ElementType.Audio:
                    AudioNodeViewModel audioVM = new AudioNodeViewModel(controller);
                    view = new AudioDetailView(audioVM);
                    break;
                case ElementType.Collection:
                    view = new GroupDetailView(new ElementCollectionViewModel((ElementCollectionController)controller));
                    break;
            }

            await ((ElementViewModel)view.DataContext).Init();
            return view;
        }

    }
}
