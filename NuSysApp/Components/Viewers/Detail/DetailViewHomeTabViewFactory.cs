using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NusysIntermediate;


namespace NuSysApp
{
    public class DetailViewHomeTabViewFactory
    {
        public async Task<UserControl> CreateFromSendable(LibraryElementController controller)
        {
            UserControl view = null;

            switch (controller.LibraryElementModel.Type)
            {
                case NusysConstants.ElementType.Text:
                    view = new TextDetailHomeTabView(new TextDetailHomeTabViewModel(controller));
                    break;
                case NusysConstants.ElementType.Image:
                    view = new ImageDetailHomeTabView(new ImageDetailHomeTabViewModel(controller));
                    break;
                case NusysConstants.ElementType.Word:
                    view = new WordDetailHomeTabView(new WordDetailHomeTabViewModel(controller));
                    break;
                case NusysConstants.ElementType.Powerpoint:
                    //view = new PowerpointDetailView(new PowerpointNodeViewModel(controller));
                    break;
                case NusysConstants.ElementType.PDF:
                    view = new PdfDetailHomeTabView(new PdfDetailHomeTabViewModel(controller));
                    break;
                case NusysConstants.ElementType.Web:
                    //view = new WebDetailView(new WebNodeViewModel(controller));
                    break;
                case NusysConstants.ElementType.Video:
                    view = new VideoDetailHomeTabView(new VideoDetailHomeTabViewModel(controller));
                    break;
                case NusysConstants.ElementType.Audio:
                    view = new AudioDetailHomeTabView(new AudioDetailHomeTabViewModel(controller));
                    break;
                case NusysConstants.ElementType.Collection:
                    view = new GroupDetailHomeTabView(new GroupDetailHomeTabViewModel(controller));
                    break;
                case NusysConstants.ElementType.Link:
                    view = new LinkDetailHomeTabView(new LinkHomeTabViewModel(controller as LinkLibraryElementController));
                    break;
            }

            if (view == null)
                return null;
            await ((DetailHomeTabViewModel)view.DataContext).Init();
            return view;
        }
    }
}
