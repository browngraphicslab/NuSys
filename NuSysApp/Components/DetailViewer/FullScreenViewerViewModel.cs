using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
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
                    TextBlock tagBlock = this.MakeTagBlock(tag);
                    Tags.Add(tagBlock);
                }
            }
        }

        public void AddTag(string tag)
        {
            //add to model
            List<string> tags = (List<string>) _nodeModel.GetMetaData("tags");
            tags.Add(tag);
            _nodeModel.SetMetaData("tags", tags);

            //this should be refactored later
            TextBlock tagBlock = this.MakeTagBlock(tag);
            Tags.Add(tagBlock);

            RaisePropertyChanged("Tags");
        }

        //this is an ugly method, refactor later so not making a UI element in viewmodel
        public TextBlock MakeTagBlock(string text)
        {
            TextBlock tagBlock = new TextBlock();
            tagBlock.Text = text;
            tagBlock.Height = 20;
            tagBlock.TextWrapping = TextWrapping.Wrap;
            tagBlock.TextAlignment = TextAlignment.Right;
            tagBlock.FontStyle = FontStyle.Italic;
            tagBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            return tagBlock;
        }
    }
}
