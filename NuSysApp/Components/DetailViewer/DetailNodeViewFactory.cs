using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class DetailNodeViewFactory
    {
        public async Task<UserControl> CreateFromSendable(Sendable model)
        {
            UserControl view = null;

            if (model is NodeModel)
                return await CreateFromNodeType((NodeModel)model);


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
                    view = new TextDetailView(tvm);
                    await tvm.UpdateRtf();
                    break;
                case NodeType.Image:
                    view = new ImageFullScreenView(new ImageNodeViewModel((ImageNodeModel)model));
                    break;
                case NodeType.PDF:
                    view = new PdfDetailView(new PdfNodeViewModel((PdfNodeModel)model));
                    break;
                case NodeType.Web:
                    view = new WebDetailView(new WebNodeViewModel((WebNodeModel)model));
                    break;
            }

            var tpl = view.FindName("nodeTpl") as NodeTemplate;
            if (tpl != null)
            {
                tpl.OnTemplateReady += async delegate {
                    var inqVm = new InqCanvasViewModel(model.InqCanvas);
                        tpl.inkCanvas.ViewModel = inqVm;
                };
            }

            return view;
        }
    }
}
