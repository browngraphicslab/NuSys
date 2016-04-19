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
using Windows.UI.Xaml.Input;
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

        private ElementViewModel _currentElementViewModel;
        public ElementController CurrentElementController { get; set; }

        public delegate void TitleChangedHandler(object source, string newTitle);
        public event TitleChangedHandler TitleChanged;

        public DetailViewerViewModel()

        {
            Tags = new ObservableCollection<FrameworkElement>();
            Metadata = new ObservableCollection<StackPanel>();
        }

        public void Dispose()
        {
            var tempvm = (ElementViewModel)View.DataContext;
            tempvm.PropertyChanged -= NodeVMPropertChanged;
            _nodeModel = null;

        }

        public async Task<bool> ShowElement(ElementController controller)
        {
            CurrentElementController = controller;
            View = await _viewFactory.CreateFromSendable(controller);
            if (View == null)
                return false;
            _nodeModel = controller.Model;
            Title = controller.LibraryElementModel.Title;
            this.ChangeTitle(this, controller.LibraryElementModel.Title);

            controller.MetadataChange += ControllerOnMetadataChange;
            controller.LibraryElementModel.OnTitleChanged += ChangeTitle;
            
            var tempvm = (ElementViewModel) View.DataContext;
            tempvm.PropertyChanged += NodeVMPropertChanged;
            MakeTagList();
            RaisePropertyChanged("Title");
            RaisePropertyChanged("View");
            RaisePropertyChanged("Tags");
            RaisePropertyChanged("Metadata");
            this.MakeMetadataList();
            return true;
        }


        public void ChangeTitle(object sender, string title)
        {
            TitleChanged?.Invoke(this, title);
            Title = title;
        }



        private void ControllerOnMetadataChange(object source, string key)
        {
            if (key == "tags")
            {
                MakeTagList();
            }
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
            RaisePropertyChanged("Tags");
        }

        public async void AddTag(string tag)
        {
            List<string> tags = (List<string>) _nodeModel.GetMetaData("tags");
            tags.Add(tag);

            var contentVm = (ElementViewModel) View.DataContext;
            contentVm.Controller.LibraryElementModel.Keywords.Add(tag);

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SetTagsRequest(_nodeModel.Id, tags));
            /*
            //this should be refactored later
            var tagBlock = this.MakeTagBlock(tag);
            Tags.Add(tagBlock);

            RaisePropertyChanged("Tags");

            //makes the entire metadata list again to update new tags
            MakeMetadataList();
            */
        }

        //this is an ugly method, refactor later so not making a UI element in viewmodel
        public FrameworkElement MakeTagBlock(string text)
        {
            var deleteButton = new TextBlock() { Text = "X" };
            deleteButton.Foreground = new SolidColorBrush(Constants.foreground6);
            deleteButton.FontSize = 15;
            deleteButton.FontWeight = FontWeights.Bold;
            deleteButton.Margin = new Thickness(0,0,3,0);
            
            var deleteGrid = new Grid();
            deleteGrid.Tag = text;
            deleteGrid.Children.Add(deleteButton);
            deleteGrid.Tapped += DeleteGridOnTapped;

            var tagContent = new TextBlock() { Text = text };
            tagContent.Foreground = new SolidColorBrush(Constants.foreground6);
            tagContent.FontStyle = FontStyle.Italic;
            tagContent.HorizontalAlignment = HorizontalAlignment.Stretch;

            var stackPanel = new Grid();
            stackPanel.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(20)});
            stackPanel.ColumnDefinitions.Add(new ColumnDefinition{Width = GridLength.Auto});
            stackPanel.Children.Add(deleteGrid);
            stackPanel.Children.Add(tagContent);
            Grid.SetColumn(deleteGrid, 0);
            Grid.SetColumn(tagContent,1);

            Button tagBlock = new Button();
            tagBlock.Tapped += TagBlock_Tapped;
            tagBlock.Background = new SolidColorBrush(Constants.salmonColor);
            tagBlock.Content = stackPanel;
            tagBlock.Height = 30;
            tagBlock.Padding = new Thickness(5);
            tagBlock.BorderThickness = new Thickness(0);
            tagBlock.Foreground = new SolidColorBrush(Constants.foreground6);
            tagBlock.Margin = new Thickness(5, 2, 2, 5);///
            tagBlock.Opacity = 0.75;
            tagBlock.FontStyle = FontStyle.Italic;
           // tagBlock.IsHitTestVisible = false;

            return tagBlock;
        }

        private async void DeleteGridOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            List<string> tags = (List<string>)_nodeModel.GetMetaData("tags");
            var t = ((FrameworkElement) sender).Tag as string;
            if (t == null)
                return;
            tags.Remove(t);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SetTagsRequest(_nodeModel.Id, tags));
            MakeTagList();
            RaisePropertyChanged("Tags");
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
            keyBox.Foreground = new SolidColorBrush(Constants.foreground6);
            keyBox.FontSize = 18;
            keyBox.Height = 30;

            var colon = new TextBlock {Text = ":"};
            colon.Foreground = new SolidColorBrush(Constants.foreground6);
            colon.FontSize = 18;
            colon.Height = 30;
            colon.Padding = new Thickness(2, 0, 2, 0);
            
            TextBox valBox = new TextBox {Text = val};
            valBox.Foreground = new SolidColorBrush(Constants.foreground6);
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
      
        }


    }

    internal class T
    {
    }
}
