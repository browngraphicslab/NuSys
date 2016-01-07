using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class FullScreenViewerViewModel : BaseINPC
    {
        private NodeModel _nodeModel;
        private DetailNodeViewFactory _viewFactory = new DetailNodeViewFactory();

        public UserControl View { get; set; }

        public ObservableCollection<TextBlock> Tags { get; set; }
 
        public FullScreenViewerViewModel()
        {
            
        }

        public async void SetNodeModel(NodeModel model)
        {
            _nodeModel = model;
            View = await _viewFactory.CreateFromSendable(_nodeModel);
            RaisePropertyChanged("View");
        }

        public void MakeTagList()
        {
            if (_nodeModel != null)
            {
                Tags = new ObservableCollection<TextBlock>();
                List<string> tags = (List<string>) _nodeModel.GetMetaData("tags");
                foreach (string tag in tags)
                {
                    TextBlock tagBlock = new TextBlock();
                    tagBlock.Text = tag;
                    tagBlock.Height = 50;
                    tagBlock.Width = 50;
                    tagBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    Tags.Add(tagBlock);
                }
            }

        }
    }
}
