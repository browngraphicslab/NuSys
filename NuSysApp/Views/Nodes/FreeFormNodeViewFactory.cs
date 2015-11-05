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
        public UserControl CreateFromModel(NodeModel model)
        {
            UserControl view = null;

            switch (model.NodeType)
            {
                case NodeType.Text:
                    view = new TextNodeView( new TextNodeViewModel((TextNodeModel)model));
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
            return view;
        }
    }
}
