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
    public class DetailViewerViewModel : BaseINPC
    {
        private ElementModel _nodeModel;
        private DetailNodeViewFactory _viewFactory = new DetailNodeViewFactory();
        private String _tagToDelete;
        public Boolean DeleteOnFocus;
        public string Title { get; set; }
        public string Date { get; set; }

        public UserControl View { get; set; }
            
        public ObservableCollection<FrameworkElement> Tags { get; set; }

        public ObservableCollection<StackPanel> Metadata { get; set; }
 
        public DetailViewerViewModel()

        {
            Tags = new ObservableCollection<FrameworkElement>();
            Metadata = new ObservableCollection<StackPanel>();
        }

        public async void ShowElement(ElementController controller)
        {
            _nodeModel = controller.Model;
            Title = _nodeModel.Title;
            View = await _viewFactory.CreateFromSendable(controller);
            var tempvm = (ElementViewModel) View.DataContext;
            tempvm.PropertyChanged += NodeVMPropertChanged;
            MakeTagList();
            RaisePropertyChanged("Title");
            RaisePropertyChanged("View");
            RaisePropertyChanged("Tags");
            RaisePropertyChanged("Metadata");
            this.MakeMetadataList();
        }

        private void NodeVMPropertChanged(object sender, PropertyChangedEventArgs e)
        {
            var tempvm = (ElementViewModel)View.DataContext;
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

        public void MakeMetadataList()
        {
            Metadata.Clear();
            if (_nodeModel != null)
            {
                string[] keys = _nodeModel.GetMetaDataKeys();
                foreach (string key in keys)
                {
                    bool editable = true;
                    if (key == "tags" || key == "node_type" || key == "node_creation_date")
                    {
                        editable = false;
                    }

                    var val = _nodeModel.GetMetaData(key);
                    string valString;
                    if (val is System.Collections.Generic.List<string>)
                    {
                        valString = string.Join(", ", (List<string>)val);
                    }
                    else
                    {
                        valString = val.ToString();
                    }
                    StackPanel metadata = this.MakeMetaDataBlock(key, valString, editable);
                    Metadata.Add(metadata);
                }
            }
        }

        public async void AddMetadata(string key, string val, bool update)
        {
            _nodeModel.SetMetaData(key, val);
            if (!update)
            {
                StackPanel mdBlock = MakeMetaDataBlock(key, val, true);
                Metadata.Add(mdBlock);
            }
            RaisePropertyChanged("Metadata");
        }

        public void MakeTagList()
        {
            Tags.Clear();
            if (_nodeModel != null)
            {
                List<string> tags = (List<string>) _nodeModel.GetMetaData("tags");
                foreach (string tag in tags)
                {
                    var tagBlock = this.MakeTagBlock(tag);
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
            var tagBlock = this.MakeTagBlock(tag);
            Tags.Add(tagBlock);

            RaisePropertyChanged("Tags");

            //makes the entire metadata list again to update new tags
            MakeMetadataList();
        }

        //this is an ugly method, refactor later so not making a UI element in viewmodel
        public FrameworkElement MakeTagBlock(string text)
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
            tagContent.HorizontalAlignment = HorizontalAlignment.Stretch;

            var stackPanel = new Grid();
            stackPanel.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(20)});
            stackPanel.ColumnDefinitions.Add(new ColumnDefinition{Width = GridLength.Auto});
            stackPanel.Children.Add(deleteButton);
            stackPanel.Children.Add(tagContent);
            Grid.SetColumn(deleteButton,0);
            Grid.SetColumn(tagContent,1);

            Button tagBlock = new Button();
            tagBlock.Tapped += TagBlock_Tapped;
            tagBlock.Background = new SolidColorBrush(Colors.DarkSalmon);
            tagBlock.Content = stackPanel;
            tagBlock.Height = 30;
            tagBlock.Padding = new Thickness(5);
            tagBlock.BorderThickness = new Thickness(0);
            tagBlock.Foreground = new SolidColorBrush(Colors.White);
            tagBlock.Margin = new Thickness(5, 2, 2, 5);///
            tagBlock.Opacity = 0.75;
            tagBlock.FontStyle = FontStyle.Italic;
            tagBlock.IsHitTestVisible = false;

            return tagBlock;
        }

        private async void MetaDataBox_OnKeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.Enter)
            {
                await UpdateMetadataVal(e);
            }
        }

        private async Task UpdateMetadataVal(Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            TextBox valBox = ((TextBox) e.OriginalSource);
            string newVal = valBox.Text.Trim();
            if (newVal != "")
            {
                TextBlock keyBox = (TextBlock) ((StackPanel) valBox.Parent).Children[0];
                this.AddMetadata(keyBox.Text, newVal, true);
            }

            RaisePropertyChanged("Metadata");
        }

        public StackPanel MakeMetaDataBlock(string key, string val, bool editable)
        {
            var keyBox = new TextBlock {Text = key};
            keyBox.Foreground = new SolidColorBrush(Colors.White);
            keyBox.FontSize = 18;
            keyBox.Height = 30;

            var colon = new TextBlock {Text = ":"};
            colon.Foreground = new SolidColorBrush(Colors.White);
            colon.FontSize = 18;
            colon.Height = 30;
            colon.Padding = new Thickness(2, 0, 2, 0);
            
            TextBox valBox = new TextBox {Text = val};
            valBox.Foreground = new SolidColorBrush(Colors.White);
            valBox.FontSize = 18;
            valBox.MinWidth = 80;
            valBox.Height = 20;
            valBox.Margin = new Thickness(5, 0, 0, 0);
            valBox.BorderThickness = new Thickness(0);
            valBox.KeyUp += MetaDataBox_OnKeyUp;
            if (!editable)
            {
                valBox.IsReadOnly = true;
            }

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(keyBox);
            stackPanel.Children.Add(colon);
            stackPanel.Children.Add(valBox);
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Height = 40;
            stackPanel.Margin = new Thickness(2, 2, 2, 2);

            return stackPanel;
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

    internal class T
    {
    }
}
