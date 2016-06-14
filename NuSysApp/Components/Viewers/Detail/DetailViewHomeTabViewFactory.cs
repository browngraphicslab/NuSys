using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NuSysApp.Components.Viewers.Detail.Views;
using NuSysApp.Components.Viewers.Detail.Views.HomeTab.ViewModels;

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
                    //view = new WordDetailView(new WordNodeViewModel(controller));
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
                    view = new VideoDetailView(new VideoNodeViewModel(new ElementController(new VideoNodeModel(model.LibraryElementId))));
                    break;
                case ElementType.Audio:
                    view = new AudioDetailView(new AudioNodeViewModel(new ElementController(new AudioNodeModel(model.LibraryElementId))));
                    break;
                case ElementType.Collection:
                    view = new GroupDetailView(new ElementCollectionViewModel(new ElementCollectionController(new CollectionElementModel(model.LibraryElementId))));
                    break;
            }

            if (view == null)
                return null;

            await ((ElementViewModel)view.DataContext).Init();
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
        //            view = new VideoDetailView(new VideoNodeViewModel(controller));
        //            break;
        //        case ElementType.Audio:
        //            AudioNodeViewModel audioVM = new AudioNodeViewModel(controller);
        //            view = new AudioDetailView(audioVM);
        //            break;
        //        case ElementType.Collection:
        //            view = new GroupDetailView(new ElementCollectionViewModel((ElementCollectionController)controller));
        //            break;
        //    }

        //    if (view == null)
        //        return null; 

        //    await ((ElementViewModel)view.DataContext).Init();
        //    return view;
        //}


    }
}
