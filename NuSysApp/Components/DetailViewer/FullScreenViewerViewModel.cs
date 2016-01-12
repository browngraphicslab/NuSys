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

        public ObservableCollection<Button> Tags { get; set; }
 
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
                Tags = new ObservableCollection<Button>();
                List<string> tags = (List<string>) _nodeModel.GetMetaData("tags");
                foreach (string tag in tags)
                {
                    Button tagBlock = this.MakeTagBlock(tag);
                    Tags.Add(tagBlock);
                }
            }
        }

        public async void AddTag(string tag)
        {
            //add to model
            List<string> tags = (List<string>) _nodeModel.GetMetaData("tags");
            tags.Add(tag);
            _nodeModel.SetMetaData("tags", tags);

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SetTagsRequest(_nodeModel.Id, tags));

            //this should be refactored later
            Button tagBlock = this.MakeTagBlock(tag);
            Tags.Add(tagBlock);

            RaisePropertyChanged("Tags");
        }

        //this is an ugly method, refactor later so not making a UI element in viewmodel
        public Button MakeTagBlock(string text)
        {

            Button tagBlock = new Button();
            tagBlock.Content = text;
            tagBlock.HorizontalAlignment = HorizontalAlignment.Right;
            tagBlock.Height = 40;
            tagBlock.Margin = new Thickness(0, 2,0,0);
            tagBlock.FontStyle = FontStyle.Italic;
            tagBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            tagBlock.Background = new SolidColorBrush(Colors.DarkSalmon);
            return tagBlock;
        }
    }
}
