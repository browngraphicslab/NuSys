using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class FreeFormNodeViewFactory : INodeViewFactory
    {
        public UserControl CreateFromSendable(Sendable model, List<UserControl> AtomViewList)
        {
            UserControl view = null;

            if (model is NodeModel)
                return CreateFromNodeType((NodeModel)model);
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

        private UserControl CreateFromNodeType(NodeModel model)
        {
            UserControl view = null;

            switch (model.NodeType)
            {
                case NodeType.Text:
                    view = new TextNodeView(new TextNodeViewModel((TextNodeModel)model));
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
            }

            var tpl = view.FindName("nodeTpl") as NodeTemplate;
            if (tpl != null)
            {
                tpl.OnTemplateReady += delegate {
                    tpl.inkCanvas.ViewModel = new InqCanvasViewModel(model.InqCanvas);
                };
            }

            ((NodeViewModel)view.DataContext).Init(view);
            return view;
        }
    }
}
