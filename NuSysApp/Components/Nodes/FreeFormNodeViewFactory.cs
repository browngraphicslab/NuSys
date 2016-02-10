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

            if (model is NodeModel)
                return await CreateFromNodeType((NodeModel)model);

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

            var viewModel = new LinkViewModel(model, atom1Vm, atom2Vm);
            var view = new BezierLinkView(viewModel);
            atom1Vm.AddLink(viewModel);
            atom2Vm.AddLink(viewModel);
            return view;
        }

        private async Task<FrameworkElement> CreateFromNodeType(NodeModel model)
        {
            UserControl view = null;

            if (model.ContentId != null && SessionController.Instance.ContentController.Get(model.ContentId) == null)
            {
                view = new LoadNodeView(new LoadNodeViewModel(model));
                SessionController.Instance.LoadingNodeDictionary[model.ContentId] = new Tuple<AtomModel, LoadNodeView>(model,(LoadNodeView)view);
                ((LoadNodeView)view).StartBar();
                return view;
            }

            switch (model.NodeType)
            {
                case NodeType.Text:
                    view = new TextNodeView(new TextNodeViewModel((TextNodeModel)model));
                    break;
                case NodeType.Group:
                    view = new GroupNodeView(new GroupNodeViewModel((NodeContainerModel)model));
                    break;
                case NodeType.Tag:
                    view = new LabelNodeView(new LabelNodeViewModel((TagNodeModel)model));
                    break;
                case NodeType.Image:
                    view = new ImageNodeView(new ImageNodeViewModel((ImageNodeModel)model));
                    break;
                case NodeType.Word:
                    view = new WordNodeView(new WordNodeViewModel((WordNodeModel)model));
                    break;
                case NodeType.Powerpoint:
                    view = new PowerpointNodeView(new PowerpointNodeViewModel((PowerpointNodeModel)model));
                    break;
                case NodeType.Audio:
                    view = new AudioNodeView(new AudioNodeViewModel((AudioNodeModel)model));
                    break;
                case NodeType.PDF:
                    view = new PdfNodeView(new PdfNodeViewModel((PdfNodeModel)model));
                    break;
                case NodeType.Video:
                    view = new VideoNodeView(new VideoNodeViewModel((VideoNodeModel)model));
                    break;
                case NodeType.Workspace:
                    view = new WorkspaceView(new WorkspaceViewModel((WorkspaceModel)model));
                    break;
                case NodeType.Web:
                    view = new WebNodeView(new WebNodeViewModel((WebNodeModel)model));
                    break;
                case NodeType.Area:
                    view = new AreaNodeView(new AreaNodeViewModel((AreaModel)model));
                    break;
            }
            await ((AtomViewModel) view.DataContext).Init();

            return view;
        }
    }
}
