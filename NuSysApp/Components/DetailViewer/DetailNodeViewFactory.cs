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

            if (model is LinkModel)
            {
                List<UserControl> list = await CreateLinkAtomList((LinkModel)model);
                return CreateLinkView((LinkModel) model, list);
            }

            return null;
        }


        private UserControl CreateLinkView(LinkModel model, List<UserControl> AtomViewList)
        {

            var atom1Vm = (AtomViewModel)AtomViewList.First(s => ((AtomViewModel)s.DataContext).Model == model.Atom1).DataContext;
            var atom2Vm = (AtomViewModel)AtomViewList.First(s => ((AtomViewModel)s.DataContext).Model == model.Atom2).DataContext;

            var viewModel = new LinkViewModel(model, atom1Vm, atom2Vm);
            //var view = new BezierLinkView(viewModel);
            atom1Vm.AddLink(viewModel);
            atom2Vm.AddLink(viewModel);
            var view = new LinkDetailView(viewModel);
            return view;

        }

        private async Task<List<UserControl>> CreateLinkAtomList(LinkModel model)
        {
            AtomModel atom1 = model.Atom1;
            AtomModel atom2 = model.Atom2;
            var factory = new FreeFormNodeViewFactory();
            var atomview1 = (UserControl)await factory.CreateFromSendable(atom1, null);
            var atomview2 = (UserControl)await factory.CreateFromSendable(atom2, null);
            List<UserControl> list = new List<UserControl>();
            list.Add(atomview1);
            list.Add(atomview2);
            return list;
        }

        private async Task<UserControl> CreateFromNodeType(NodeModel model)
        {
            UserControl view = null;

            switch (model.NodeType)
            {
                case NodeType.Text:
                    var tvm = new TextNodeViewModel((TextNodeModel) model);
                    view = new TextDetailView(tvm);
                    await tvm.Init();
                    break;
                case NodeType.Image:
                    var ivm = new ImageNodeViewModel((ImageNodeModel)model);
                    await ivm.Init();
                    view = new ImageFullScreenView(ivm);
                    break;
                case NodeType.Word:
                    view = new WordDetailView(new WordNodeViewModel((WordNodeModel)model));
                    break;
                case NodeType.Powerpoint:
                    view = new PowerpointDetailView(new PowerpointNodeViewModel((PowerpointNodeModel)model));
                    break;
                case NodeType.PDF:
                    view = new PdfDetailView(new PdfNodeViewModel((PdfNodeModel)model));
                    break;
                case NodeType.Web:
                    view = new WebDetailView(new WebNodeViewModel((WebNodeModel)model));
                    break;
                case NodeType.Video:
                    view = new VideoDetailView(new VideoNodeViewModel((VideoNodeModel) model));
                    break;
                case NodeType.Audio:
                    AudioNodeViewModel audioVM = new AudioNodeViewModel((AudioNodeModel) model);
                    await audioVM.Init();
                    view = new AudioDetailView(audioVM);
                    break;
                case NodeType.Group:
                    view = new GroupDetailView(new NodeContainerViewModel((NodeContainerModel)model));
                    break;
            }

            var tpl = view.FindName("nodeTpl") as NodeTemplate;
            if (tpl != null)
            {
                tpl.OnTemplateReady += async delegate {
                    var inqVm = new InqCanvasViewModel(model.InqCanvas, new Size(model.Width, model.Height));
                };
            }

            return view;
        }
    }
}
