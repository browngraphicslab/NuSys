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
        public async Task<FrameworkElement> CreateFromSendable(Sendable model, List<FrameworkElement> AtomViewList)
        {
            UserControl view = null;

            if (model is ElementInstanceModel)
                return await CreateFromNodeType((ElementInstanceModel)model);

            if (model is LinkModel)
                return CreateLinkView((LinkModel) model, AtomViewList);

            if (model is PinModel)
            {
                var vm = new PinViewModel((PinModel)model);
                return vm.View;
            }   

            return null;
        }


        private UserControl CreateLinkView(LinkModel model, List<FrameworkElement> AtomViewList)
        {
            var atom1Vm = (AtomViewModel)AtomViewList.First(s => ((AtomViewModel)s.DataContext).Model == model.Atom1).DataContext;
            var atom2Vm = (AtomViewModel)AtomViewList.First(s => ((AtomViewModel)s.DataContext).Model == model.Atom2).DataContext;

            var viewModel = new LinkViewModel(new ElementInstanceController(model), atom1Vm, atom2Vm);
            var view = new BezierLinkView(viewModel);
            atom1Vm.AddLink(viewModel);
            atom2Vm.AddLink(viewModel);
            return view;
        }

        private async Task<FrameworkElement> CreateFromNodeType(ElementInstanceModel model)
        {
            UserControl view = null;

            if (model.ContentId != null && SessionController.Instance.ContentController.Get(model.ContentId) == null)
            {
                view = new LoadNodeView(new LoadNodeViewModel(new ElementInstanceController(model)));
                if (SessionController.Instance.LoadingNodeDictionary.ContainsKey(model.ContentId))
                {
                    SessionController.Instance.LoadingNodeDictionary[model.ContentId]?.Add(
                        new Tuple<ElementInstanceModel, LoadNodeView>(model, (LoadNodeView) view));
                }
                else
                {
                    SessionController.Instance.LoadingNodeDictionary[model.ContentId] =
                        new List<Tuple<ElementInstanceModel, LoadNodeView>>()
                        {
                            new Tuple<ElementInstanceModel, LoadNodeView>(model, (LoadNodeView) view)
                        };

                }
                ((LoadNodeView)view).StartBar();
                return view;
            }

            switch (model.ElementType)
            {
                case ElementType.Text:
                    view = new TextNodeView(new TextNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Group:
                    view = new GroupNodeView(new GroupNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Tag:
                    view = new LabelNodeView(new LabelNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Image:
                    view = new ImageNodeView(new ImageNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Word:
                    view = new WordNodeView(new WordNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Powerpoint:
                    view = new PowerpointNodeView(new PowerpointNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Audio:
                    view = new AudioNodeView(new AudioNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.PDF:
                    view = new PdfNodeView(new PdfNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Video:
                    view = new VideoNodeView(new VideoNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Workspace:
                    view = new FreeFormViewer(new FreeFormViewerViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Web:
                    view = new WebNodeView(new WebNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Area:
                    view = new AreaNodeView(new AreaNodeViewModel(new ElementInstanceController(model)));
                    break;
            }
            await ((AtomViewModel) view.DataContext).Init();

            return view;
        }
    }
}
