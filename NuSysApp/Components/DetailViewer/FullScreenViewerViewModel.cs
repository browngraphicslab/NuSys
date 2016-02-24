using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private String _tagToDelete;
        public Boolean DeleteOnFocus;
        public string Title { get; set; }
        public string Date { get; set; }

        public UserControl View { get; set; }
            
        public ObservableCollection<Button> Tags { get; set; }
        public FullScreenViewerViewModel()
        {
            Tags = new ObservableCollection<Button>();
        }

        public async void SetNodeModel(AtomModel model)
        {
            _nodeModel = model;
            View = await _viewFactory.CreateFromSendable(_nodeModel);
            var tempvm = (AtomViewModel) View.DataContext;
            tempvm.PropertyChanged += NodeVMPropertChanged;
            RaisePropertyChanged("View");
            RaisePropertyChanged("Tags");
        }

        private void NodeVMPropertChanged(object sender, PropertyChangedEventArgs e)
        {
            var tempvm = (AtomViewModel)View.DataContext;
            switch (e.PropertyName.ToLower())
            {
                case "title":
                    Title = tempvm.Title;
                    RaisePropertyChanged("Title");
                    break;
                default:
                    break;
            }
        }
        public void MakeTagList()
        {
            Tags.Clear();
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
            deleteButton.PointerExited += DeleteButton_PointerExited;
            deleteButton.PointerEntered += DeleteButton_PointerEntered;
            deleteButton.Name = text;
            
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

        private async void TagBlock_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (DeleteOnFocus)
            {
                List<string> tags = (List<string>)_nodeModel.GetMetaData("tags");
                tags.Remove(_tagToDelete);
                _nodeModel.SetMetaData("tags", tags);

                Tags.Remove((Button)sender);
                RaisePropertyChanged("Tags");
            }
        }

        private void DeleteButton_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            DeleteOnFocus = false;
        }
        private void DeleteButton_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            DeleteOnFocus = true;
            _tagToDelete = ((TextBlock)sender).Name;
            Debug.WriteLine(((TextBlock)sender).Name);
        }
    }
}
