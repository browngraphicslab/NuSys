using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        private AtomModel _nodeModel;
        private DetailNodeViewFactory _viewFactory = new DetailNodeViewFactory();
        private String TagToDelete;
        public Boolean DeleteOnFocus;
        //public String Title = (String)_nodeModel.GetMetaData("title");

        public UserControl View { get; set; }
            
        public ObservableCollection<Button> Tags { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public FullScreenViewerViewModel()
        {

        }

        public async void SetNodeModel(AtomModel model)
        {
            _nodeModel = model;
            model.TitleChanged += (value, title) =>
            {
                Title = title;
                RaisePropertyChanged("Title");
            };
            View = await _viewFactory.CreateFromSendable(_nodeModel);
            View.DataContext = this;
            RaisePropertyChanged("View");
        }

        public void MakeTagList()
        {
            Tags = new ObservableCollection<Button>();
            if (_nodeModel != null)
            {
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
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SetTagsRequest(_nodeModel.Id, tags));

            //this should be refactored later
            Button tagBlock = this.MakeTagBlock(tag);
            Tags.Add(tagBlock);

            RaisePropertyChanged("Tags");
        }

        //this is an ugly method, refactor later so not making a UI element in viewmodel
        public Button MakeTagBlock(string text)
        {
            var deleteButton = new TextBlock() { Text = "X" };
            deleteButton.Foreground = new SolidColorBrush(Colors.White);
            deleteButton.FontSize = 15;
            deleteButton.FontWeight = FontWeights.Bold;
            deleteButton.Margin = new Thickness(0,0,3,0);
            deleteButton.Tapped += DeleteButton_Tapped;
            deleteButton.PointerExited += DeleteButton_PointerExited;
            TagToDelete = text;
            
            var tagContent = new TextBlock() { Text = text };
            tagContent.Foreground = new SolidColorBrush(Colors.White);
            tagContent.FontStyle = FontStyle.Italic;

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(deleteButton);
            stackPanel.Children.Add(tagContent);
            stackPanel.Orientation = Orientation.Horizontal;

            Button tagBlock = new Button();
            tagBlock.Content = stackPanel;
            tagBlock.Height = 40;
            tagBlock.Margin = new Thickness(2, 2, 2, 2);
            tagBlock.Padding = new Thickness(5);
            tagBlock.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            tagBlock.Background = new SolidColorBrush(Colors.Transparent);
            tagBlock.Tapped += TagBlock_Tapped; 

            return tagBlock;
        }

        private void TagBlock_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (DeleteOnFocus)
            {
                List<string> tags = (List<string>)_nodeModel.GetMetaData("tags");
                tags.Remove(TagToDelete);
                _nodeModel.SetMetaData("tags", tags);
                Tags.Remove((Button)sender);
                RaisePropertyChanged("Tags");
            }
        }

        private void DeleteButton_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            DeleteOnFocus = false;
        }

        private void DeleteButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DeleteOnFocus = true;
        }
    }
}
