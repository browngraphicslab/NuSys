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

            if (model is ElementInstanceModel)
                return await CreateFromNodeType((ElementInstanceModel)model);


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

            var atom1Vm = (ElementInstanceViewModel)AtomViewList.First(s => ((ElementInstanceViewModel)s.DataContext).Model == model.Atom1).DataContext;
            var atom2Vm = (ElementInstanceViewModel)AtomViewList.First(s => ((ElementInstanceViewModel)s.DataContext).Model == model.Atom2).DataContext;

            var viewModel = new LinkViewModel(new ElementInstanceController(model), atom1Vm, atom2Vm);
            //var view = new BezierLinkView(viewModel);
            atom1Vm.AddLink(viewModel);
            atom2Vm.AddLink(viewModel);
            var view = new LinkDetailView(viewModel);
            return view;

        }

        private async Task<List<UserControl>> CreateLinkAtomList(LinkModel model)
        {
            ElementInstanceModel atom1 = model.Atom1;
            ElementInstanceModel atom2 = model.Atom2;
            var factory = new FreeFormNodeViewFactory();
            var atomview1 = (UserControl)await factory.CreateFromSendable(atom1, null);
            var atomview2 = (UserControl)await factory.CreateFromSendable(atom2, null);
            List<UserControl> list = new List<UserControl>();
            list.Add(atomview1);
            list.Add(atomview2);
            return list;
        }

        private async Task<UserControl> CreateFromNodeType(ElementInstanceModel model)
        {
            UserControl view = null;

            switch (model.ElementType)
            {
                case ElementType.Text:
                    var tvm = new TextNodeViewModel(new ElementInstanceController(model));
                    view = new TextDetailView(tvm);
                    await tvm.Init();
                    break;
                case ElementType.Image:
                    var ivm = new ImageNodeViewModel((new ElementInstanceController(model)));
                    await ivm.Init();
                    view = new ImageFullScreenView(ivm);
                    break;
                case ElementType.Word:
                    view = new WordDetailView(new WordNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Powerpoint:
                    view = new PowerpointDetailView(new PowerpointNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.PDF:
                    view = new PdfDetailView(new PdfNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Web:
                    view = new WebDetailView(new WebNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Video:
                    view = new VideoDetailView(new VideoNodeViewModel(new ElementInstanceController(model)));
                    break;
                case ElementType.Audio:
                    AudioNodeViewModel audioVM = new AudioNodeViewModel(new ElementInstanceController(model));
                    await audioVM.Init();
                    view = new AudioDetailView(audioVM);
                    break;
                case ElementType.Group:
                    view = new GroupDetailView(new ElementInstanceCollectionViewModel(new ElementInstanceController(model)));
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
