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
        public async Task<UserControl> CreateFromSendable(LibraryElementModel model)
        {
            UserControl view = null;

            switch (model.Type)
            {
                case ElementType.Text:
                    var nodeModel = new TextElementModel(model.Id);
                    nodeModel.LibraryId = model.Id;
                    view = new TextDetailView(new TextNodeViewModel(new TextNodeController(nodeModel)));
                    break;
                case ElementType.Image:
                    var imageModel = new ImageElementModel(model.Id);
                    imageModel.LibraryId = model.Id;
                    view = new ImageFullScreenView(new ImageElementViewModel(new ImageElementIntanceController(imageModel)));
                    break;
                case ElementType.Word:
                    //view = new WordDetailView(new WordNodeViewModel(controller));
                    break;
                case ElementType.Powerpoint:
                    //view = new PowerpointDetailView(new PowerpointNodeViewModel(controller));
                    break;
                case ElementType.PDF:
                    view = new PdfDetailView(new PdfNodeViewModel(new ElementController(new PdfNodeModel(model.Id))));
                    break;
                case ElementType.Web:
                    //view = new WebDetailView(new WebNodeViewModel(controller));
                    break;
                case ElementType.Video:
                    view = new VideoDetailView(new VideoNodeViewModel(new ElementController(new VideoNodeModel(model.Id))));
                    break;
                case ElementType.Audio:
                    view = new AudioDetailView(new AudioNodeViewModel(new ElementController(new AudioNodeModel(model.Id))));
                    break;
                case ElementType.Collection:
                    view = new GroupDetailView(new ElementCollectionViewModel(new ElementCollectionController(new CollectionElementModel(model.Id))));
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
        //            view = new TextDetailView(tvm);
        //            break;
        //        case ElementType.Image:
        //            var ivm = new ImageElementViewModel(controller);
        //            view = new ImageFullScreenView(ivm);
        //            break;
        //        case ElementType.Word:
        //            view = new WordDetailView(new WordNodeViewModel(controller));
        //            break;
        //        case ElementType.Powerpoint:
        //            view = new PowerpointDetailView(new PowerpointNodeViewModel(controller));
        //            break;
        //        case ElementType.PDF:
        //            view = new PdfDetailView(new PdfNodeViewModel(controller));
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
