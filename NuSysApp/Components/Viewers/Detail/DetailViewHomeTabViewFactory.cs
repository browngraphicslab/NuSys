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
    public class DetailViewHomeTabViewFactory
    {
        public async Task<UserControl> CreateFromSendable(LibraryElementController controller)
        {
            UserControl view = null;

            switch (controller.LibraryElementModel.Type)
            {
                case ElementType.Text:
                    view = new TextDetailHomeTabView(new TextDetailHomeTabViewModel(controller));
                    break;
                case ElementType.Image:
                    view = new ImageDetailHomeTabView(new ImageDetailHomeTabViewModel(controller));
                    break;
                case ElementType.Word:
                    view = new WordDetailHomeTabView(new WordDetailHomeTabViewModel(controller));
                    break;
                case ElementType.Powerpoint:
                    //view = new PowerpointDetailView(new PowerpointNodeViewModel(controller));
                    break;
                case ElementType.PDF:
                    view = new PdfDetailHomeTabView(new PdfDetailHomeTabViewModel(controller));
                    break;
                case ElementType.Web:
                    //view = new WebDetailView(new WebNodeViewModel(controller));
                    break;
                case ElementType.Video:
                    view = new VideoDetailHomeTabView(new VideoDetailHomeTabViewModel(controller));
                    break;
                case ElementType.Audio:
                    view = new AudioDetailHomeTabView(new AudioDetailHomeTabViewModel(controller));
                    break;
                case ElementType.Collection:
                    view = new GroupDetailHomeTabView(new GroupDetailHomeTabViewModel(controller));
                    break;
            }

            if (view == null)
                return null;
            await ((DetailHomeTabViewModel)view.DataContext).Init();
            return view;
        }
        //public async Task<UserControl> CreateFromSendable(ElementController controller)
        //{
        //    UserControl view = null; 

        //    switch (controller.Model.ElementType)
        //    {
        //        case ElementType.Text:
        //            var tvm = new TextNodeViewModel(controller);
        //            view = new TextDetailHomeTabView(tvm);
        //            break;
        //        case ElementType.Image:
        //            var ivm = new ImageElementViewModel(controller);
        //            view = new ImageDetailHomeTabView(ivm);
        //            break;
        //        case ElementType.Word:
        //            view = new WordDetailView(new WordNodeViewModel(controller));
        //            break;
        //        case ElementType.Powerpoint:
        //            view = new PowerpointDetailView(new PowerpointNodeViewModel(controller));
        //            break;
        //        case ElementType.PDF:
        //            view = new PdfDetailHomeTabView(new PdfNodeViewModel(controller));
        //            break;
        //        case ElementType.Web:
        //            view = new WebDetailView(new WebNodeViewModel(controller));
        //            break;
        //        case ElementType.Video:
        //            view = new VideoDetailHomeTabView(new VideoNodeViewModel(controller));
        //            break;
        //        case ElementType.Audio:
        //            AudioNodeViewModel audioVM = new AudioNodeViewModel(controller);
        //            view = new AudioDetailHomeTabView(audioVM);
        //            break;
        //        case ElementType.Collection:
        //            view = new GroupDetailHomeTabView(new ElementCollectionViewModel((ElementCollectionController)controller));
        //            break;
        //    }

        //    if (view == null)
        //        return null; 

        //    await ((ElementViewModel)view.DataContext).Init();
        //    return view;
        //}


    }
}
