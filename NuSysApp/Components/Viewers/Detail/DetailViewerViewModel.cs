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
        private DetailViewHomeTabViewFactory _viewHomeTabViewFactory = new DetailViewHomeTabViewFactory();
        private string _tagToDelete;
        public bool DeleteOnFocus;
        public string Title { get; set; }
        public string Date { get; set; }

        public UserControl View { get; set; }

        public UserControl RegionView
        {
            get
            {
                return _regionableViewModel?.View;
            }
        }

        public ObservableCollection<FrameworkElement> Tags { get; set; }

        public ObservableCollection<StackPanel> Metadata { get; set; }

        //public ObservableCollection<Region> Regions { get; set; }
        private Regionable<Region> _regionableViewModel;

        private ElementViewModel _currentElementViewModel;
        public LibraryElementController CurrentElementController { get; set; }

        public delegate void TitleChangedHandler(object source, string newTitle);
        public event TitleChangedHandler TitleChanged;

        public delegate void SizeChangedEventHandler(object source, double left, double width, double height);
        public event SizeChangedEventHandler SizeChanged;
        
        public DetailViewerViewModel()

        {
            Tags = new ObservableCollection<FrameworkElement>();
            Metadata = new ObservableCollection<StackPanel>();

        }

        public void Dispose()
        {
            var tempvm = (DetailHomeTabViewModel)View.DataContext;
            tempvm.TitleChanged -= NodeVMTitleChanged;

            _nodeModel = null;

        }

        public async Task<bool> ShowElement(ElementController controller)
        {
            if (!await ShowElement(controller.LibraryElementController))
            {
                return false;
            }
            
            //Create non-libraryelementcontroller tabs
            return true;
        }
        public async Task<bool> ShowElement(LibraryElementController controller)
        {
            if (CurrentElementController != null)
            {
                CurrentElementController.KeywordsChanged -= KeywordsChanged;
            }
            CurrentElementController = controller;
            CurrentElementController.KeywordsChanged += KeywordsChanged;
            View = await _viewHomeTabViewFactory.CreateFromSendable(controller);
            if (View == null)
            {
                return false;
            }

            var regionView  = await _viewHomeTabViewFactory.CreateFromSendable(controller);
            if (regionView == null)
            {
                return false;
            }
            _regionableViewModel = regionView.DataContext as Regionable<Region>;
            RaisePropertyChanged("RegionView");
            regionView.Loaded += delegate
            {
                _regionableViewModel.SetExistingRegions(controller.LibraryElementModel.Regions);
                
            };
            SizeChanged += (sender, left, width, height) => _regionableViewModel.SizeChanged(sender, width, height);
            //_nodeModel = controller.LibraryElementModel;

            Title = controller.LibraryElementModel.Title;
            this.ChangeTitle(this, controller.LibraryElementModel.Title);
            
            //vm.Controller.MetadataChange += ControllerOnMetadataChange;
            //vm.Controller.LibraryElementModel.OnTitleChanged += ChangeTitle;

            
            var tempvm = (DetailHomeTabViewModel)View.DataContext;
            tempvm.TitleChanged += NodeVMTitleChanged;
            MakeTagList();
            RaisePropertyChanged("Title");
            RaisePropertyChanged("View");
            RaisePropertyChanged("Tags");
            RaisePropertyChanged("Metadata");
            RaisePropertyChanged("RegionView");

            return true;
        }

        private void KeywordsChanged(object sender, HashSet<Keyword> keywords)
        {
            MakeTagList();
        }


        public void ChangeTitle(object sender, string title)
        {
            TitleChanged?.Invoke(this, title);
            Title = title;
        }

        public void ChangeSize(object sender, double left, double width, double height)
        {
            SizeChanged?.Invoke(sender, left, width, height);
        }

        private void ControllerOnMetadataChange(object source, string key)
        {
            if (key == "tags")
            {
                MakeTagList();
            }
        }

        private void NodeVMTitleChanged(object sender, string title)
        {
            Title = title;
            RaisePropertyChanged("Title");
        }

        
        public void MakeTagList()
        {
            Tags.Clear();
            if (CurrentElementController != null)
            {
                var tags = CurrentElementController?.LibraryElementModel.Keywords;
                foreach (var tag in tags)
                {
                    var tagBlock = this.MakeTagBlock(tag.Text);
                    Tags.Add(tagBlock);
                }
            }
            RaisePropertyChanged("Tags");
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
            tagBlock.Background = new SolidColorBrush(Constants.color4);
            tagBlock.Content = stackPanel;
            tagBlock.Height = 30;
            tagBlock.Padding = new Thickness(5);
            tagBlock.BorderThickness = new Thickness(0);
            tagBlock.Foreground = new SolidColorBrush(Constants.foreground6);
            tagBlock.Margin = new Thickness(5, 2, 2, 5);///
            tagBlock.FontStyle = FontStyle.Italic;
           // tagBlock.IsHitTestVisible = false;

            return tagBlock;
        }

        private async void DeleteGridOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            var t = ((FrameworkElement) sender).Tag as string;
            if (t == null)
                return;
            CurrentElementController?.RemoveKeyword(new Keyword(t));
        }
        /*
        private async void MetaDataBox_OnKeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.Enter)
            {
                await UpdateMetadataVal(e);
            }
        }*/
        /*
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
        }*/
        /*
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
        */
        private async void TagBlock_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
      
        }


    }

    internal class T
    {
    }
}
