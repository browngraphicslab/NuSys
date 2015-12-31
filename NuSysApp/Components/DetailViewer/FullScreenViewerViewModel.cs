using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class FullScreenViewerViewModel : BaseINPC
    {
        private NodeModel _nodeModel;
        private DetailNodeViewFactory _viewFactory = new DetailNodeViewFactory();

        public UserControl View { get; set; }

        public FullScreenViewerViewModel()
        {
            
        }

        public async void SetNodeModel(NodeModel model)
        {
            _nodeModel = model;
            View = await _viewFactory.CreateFromSendable(_nodeModel);
            RaisePropertyChanged("View");
        }
        
    }
}
