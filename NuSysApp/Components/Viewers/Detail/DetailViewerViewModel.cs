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
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }
        public string Date { get; set; }

        public UserControl View { get; set; }

        public UserControl RegionView
        {
            get
            {
                return _regionableRegionTabViewModel?.View;
            }
        }

        public ObservableCollection<FrameworkElement> Tags { get; set; }
        private ObservableCollection<IMetadatable> _tabs;

        public ObservableCollection<IMetadatable> Tabs
        {
            get { return _tabs; }
            set
            {
                _tabs = value;
                RaisePropertyChanged("Tabs");
            }
        }

        private Visibility _tabVisibility;

        public Visibility TabVisibility
        {
            get {  return _tabVisibility; }
            set
            {
                _tabVisibility = value; 
                RaisePropertyChanged("TabVisibility");
            }
        }
        public double TabPaneHeight { get; set; }
        private double _tabHeight;
        public double TabHeight
        {
            get { return _tabHeight; }
            set
            {
                _tabHeight = value; 
                RaisePropertyChanged("TabHeight");
            }
        }

        public ObservableCollection<StackPanel> Metadata { get; set; }

        public ObservableCollection<Region> RegionCollection { set; get; }
        private DetailHomeTabViewModel _regionableRegionTabViewModel;
        private DetailHomeTabViewModel _regionableHomeTabViewModel;

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
            RegionCollection = new ObservableCollection<Region>();
            Tabs = new ObservableCollection<IMetadatable>();
          //  TabVisibility = Visibility.Collapsed;
            
        }

        private void AddRegionToList(object source, RegionController regionController)
        {
            RegionCollection.Add(regionController.Model);
        }

        public void Dispose()
        {
            CurrentElementController.TitleChanged -= ControllerTitleChanged;

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
        public async Task<bool> ShowElement(IMetadatable metadatable)
        {
            if (metadatable.MetadatableType() == MetadatableType.Content)
            {
                var controller = metadatable as LibraryElementController;
                if (controller == null)
                {
                    return false;
                }
                if (CurrentElementController != null)
                {
                    CurrentElementController.KeywordsChanged -= KeywordsChanged;
                }
                CurrentElementController = controller;
                CurrentElementController.KeywordsChanged += KeywordsChanged;
                CurrentElementController.RegionAdded += AddRegionToList;
                CurrentElementController.RegionRemoved += RemoveRegionFromList;

                RegionCollection.Clear();

                var regions = CurrentElementController.LibraryElementModel.Regions;

                if (regions?.Count > 0)
                {
                    foreach (var region in CurrentElementController.LibraryElementModel.Regions)
                    {
                        RegionCollection.Add(region);
                    }
                }
                
                View = await _viewHomeTabViewFactory.CreateFromSendable(controller);
                if (View == null)
                {
                    return false;
                }

                var regionView = await _viewHomeTabViewFactory.CreateFromSendable(controller);
                if (regionView == null)
                {
                    return false;
                }


                _regionableRegionTabViewModel = regionView.DataContext as DetailHomeTabViewModel;
                _regionableRegionTabViewModel.Editable = true;
                _regionableHomeTabViewModel = View.DataContext as DetailHomeTabViewModel;
                _regionableHomeTabViewModel.Editable = false;

                RaisePropertyChanged("View");
                RaisePropertyChanged("RegionView");



                View.Loaded += delegate
                {
                    _regionableHomeTabViewModel.SetExistingRegions(controller.LibraryElementModel.Regions);

                };


                regionView.Loaded += delegate
                {

                    _regionableRegionTabViewModel.SetExistingRegions(controller.LibraryElementModel.Regions);

                };
                SizeChanged += (sender, left, width, height) => _regionableRegionTabViewModel.SizeChanged(sender, width, height);
                SizeChanged += (sender, left, width, height) => _regionableHomeTabViewModel.SizeChanged(sender, width, height);

                //_nodeModel = controller.LibraryElementModel;

                Title = controller.LibraryElementModel.Title;
                //this.ChangeTitle(this, controller.LibraryElementModel.Title);

                controller.TitleChanged += ControllerTitleChanged;
                MakeTagList();
                RaisePropertyChanged("View");
                RaisePropertyChanged("Tags");
                RaisePropertyChanged("Metadata");
                RaisePropertyChanged("RegionView");
                RaisePropertyChanged("View");

                AddTab(metadatable);
                return true;
            } else if (metadatable.MetadatableType() == MetadatableType.Region)
            {
                var controller = metadatable as RegionController;
                if (controller == null)
                {
                    return false;
                }
                var regionModel = controller.Model;
                View = await _viewHomeTabViewFactory.CreateFromSendable(CurrentElementController);
                if (View == null)
                {
                    return false;
                }

                var regionSet = new HashSet<Region>();
                regionSet.Add(regionModel);
                View.Loaded += delegate
                {
                    _regionableHomeTabViewModel.SetExistingRegions(regionSet);

                };

                _regionableHomeTabViewModel = View.DataContext as DetailHomeTabViewModel;
                _regionableHomeTabViewModel.Editable = false;

                RaisePropertyChanged("View");
                
                //regionView.Loaded += delegate
                //{

                //    _regionableRegionTabViewModel.SetExistingRegions(controller.LibraryElementModel.Regions);

                //};
                SizeChanged += (sender, left, width, height) => _regionableHomeTabViewModel.SizeChanged(sender, width, height);
                
                Title = regionModel.Name;
                //this.ChangeTitle(this, regionModel.Name);
                
                //var tempvm = (DetailHomeTabViewModel)View.DataContext;
                //tempvm.TitleChanged += ControllerTitleChanged;
                
                RaisePropertyChanged("Title");
                RaisePropertyChanged("View");
                RaisePropertyChanged("Tags");
                RaisePropertyChanged("Metadata");
                RaisePropertyChanged("RegionView");
                RaisePropertyChanged("View");

                AddTab(metadatable);
                return true;
            } else
            {
                return false;
            }
            
        }

        public void AddTab(IMetadatable metadatable)
        {
            if (_tabs.Contains(metadatable))
            {
                return;
            }
            if (_tabs.Count < 6)
            {
                _tabs.Add(metadatable);
            }
            else
            {
                _tabs.RemoveAt(0);
                _tabs.Add(metadatable);
            }
            TabVisibility = Visibility.Visible;

            if (_tabs.Count > 1)
            {
                TabVisibility = Visibility.Visible;
            }
            else
            {
                TabVisibility = Visibility.Collapsed;
            }
            TabHeight = TabPaneHeight/Tabs.Count;
            Tabs = _tabs;
        }


        private void RemoveRegionFromList(object source, Region region)
        {
            if (RegionCollection.Contains(region))
                RegionCollection.Remove(region);
        }

        private void KeywordsChanged(object sender, HashSet<Keyword> keywords)
        {
            MakeTagList();
        }

        /*
        public void ChangeTitle(object sender, string title)
        {
            TitleChanged?.Invoke(this, title);
            Title = title;
        }
        */

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

        private void ControllerTitleChanged(object sender, string title)
        {
            Title = title;
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
            tagBlock.Background = new SolidColorBrush(Constants.color4);
            tagBlock.Content = stackPanel;
            tagBlock.Height = 30;
            tagBlock.Padding = new Thickness(5);
            tagBlock.BorderThickness = new Thickness(0);
            tagBlock.Foreground = new SolidColorBrush(Constants.foreground6);
            tagBlock.Margin = new Thickness(5, 2, 2, 5);///
            tagBlock.FontStyle = FontStyle.Italic;
           
            return tagBlock;
        }

        private async void DeleteGridOnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            var t = ((FrameworkElement) sender).Tag as string;
            if (t == null)
                return;
            CurrentElementController?.RemoveKeyword(new Keyword(t));
        }

        public void ChangeControllersTitle(string title)
        {
            CurrentElementController.TitleChanged -= ControllerTitleChanged;
            CurrentElementController.SetTitle(title);
            CurrentElementController.TitleChanged += ControllerTitleChanged;
        }

    }

}
