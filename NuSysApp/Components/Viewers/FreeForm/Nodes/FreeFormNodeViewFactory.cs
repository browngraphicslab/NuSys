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
    public class FreeFormNodeViewFactory : INodeViewFactory
    {
        public async Task<FrameworkElement> CreateFromSendable(ElementController controller, List<FrameworkElement> AtomViewList)
        {
            UserControl view = null;

            if (controller.Model is LinkModel)
                return CreateLinkView((LinkModel)controller.Model, AtomViewList);

            var model = controller.Model;

            switch (model.ElementType)
            {
                case ElementType.Text:
                    view = new TextNodeView(new TextNodeViewModel(controller));
                    break;
                case ElementType.Group:
                    view = new GroupNodeView(new GroupNodeViewModel((ElementCollectionController)controller));
                    break;
                case ElementType.Tag:
                    view = new LabelNodeView(new LabelNodeViewModel((ElementCollectionController)controller));
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
                    break;
            }
            await ((ElementViewModel)view.DataContext).Init();

            return view;
        }


        private UserControl CreateLinkView(LinkModel model, List<FrameworkElement> AtomViewList)
        {
            var atom1Vm =
                (ElementViewModel)
                    AtomViewList.First(s => ((ElementViewModel) s.DataContext).Model == model.Atom1).DataContext;
            var atom2Vm =
                (ElementViewModel)
                    AtomViewList.First(s => ((ElementViewModel) s.DataContext).Model == model.Atom2).DataContext;

            var viewModel = new LinkViewModel(new ElementController(model), atom1Vm, atom2Vm);
            var view = new BezierLinkView(viewModel);
            atom1Vm.AddLink(viewModel);
            atom2Vm.AddLink(viewModel);
            return view;
        }
    }
}
