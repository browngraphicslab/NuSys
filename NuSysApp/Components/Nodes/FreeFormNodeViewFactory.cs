using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;


namespace NuSysApp
{
    public class FreeFormNodeViewFactory : INodeViewFactory
    {
        public async Task<UserControl> CreateFromSendable(Sendable model, List<UserControl> AtomViewList)
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


        private UserControl CreateLinkView(LinkModel model, List<UserControl> AtomViewList)
        {
            var atom1Vm = (AtomViewModel)AtomViewList.First(s => ((AtomViewModel)s.DataContext).Model == model.Atom1).DataContext;
            var atom2Vm = (AtomViewModel)AtomViewList.First(s => ((AtomViewModel)s.DataContext).Model == model.Atom2).DataContext;

            var viewModel = new LinkViewModel(model, atom1Vm, atom2Vm);
            var view = new BezierLinkView(viewModel);
            atom1Vm.AddLink(viewModel);
            atom2Vm.AddLink(viewModel);
            return view;
        }

        private async Task<UserControl> CreateFromNodeType(NodeModel model)
        {
            UserControl view = null;

            switch (model.NodeType)
            {
                case NodeType.Text:
                    var tvm = new TextNodeViewModel((TextNodeModel) model);
                    view = new TextNodeView(tvm);
                     await tvm.UpdateRtf();
                    break;
                case NodeType.Group:
                    view = new GroupNodeView(new GroupNodeViewModel((NodeContainerModel)model));
                    break;
                case NodeType.GroupTag:
                    view = new LabelNodeView(new LabelNodeViewModel((NodeContainerModel)model));
                    break;
                case NodeType.Image:
                    view = new ImageNodeView(new ImageNodeViewModel((ImageNodeModel)model));
                    break;
                case NodeType.Audio:
                    view = new AudioNodeView(new AudioNodeViewModel((AudioNodeModel)model));
                    break;
                case NodeType.PDF:
                    view = new PdfNodeView(new PdfNodeViewModel((PdfNodeModel)model));
                    break;
                case NodeType.Workspace:
                    view = new WorkspaceView(new WorkspaceViewModel((WorkspaceModel)model));
                    break;
                case NodeType.Web:
                    view = new WebNodeView(new WebNodeViewModel((WebNodeModel)model));
                    break;
            }

            var tpl = view.FindName("nodeTpl") as NodeTemplate;
            if (tpl != null)
            {
                tpl.OnTemplateReady += async delegate {
                    var inqVm = new InqCanvasViewModel(model.InqCanvas, new Size(model.Width, model.Height));
                    if (tpl.inkCanvas != null) { 
                    //    tpl.inkCanvas.ViewModel = inqVm;
                    }
                };
            }

            return view;
        }
    }
}
