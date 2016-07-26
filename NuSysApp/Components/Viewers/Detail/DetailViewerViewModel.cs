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
using MyToolkit.UI;
using NusysIntermediate;

namespace NuSysApp
{
    public class DetailViewerViewModel : BaseINPC
    {
        private ElementModel _nodeModel;
        private DetailViewHomeTabViewFactory _viewHomeTabViewFactory = new DetailViewHomeTabViewFactory();
        private string _tagToDelete;
        public bool DeleteOnFocus;
        private string _title;
        public Dictionary<string, DetailViewTabType> TabDictionary = new Dictionary<string, DetailViewTabType>();
         
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

        public UserControl RegionView { set; get; }

        public ObservableCollection<FrameworkElement> Tags { get; set; }

        public ObservableCollection<FrameworkElement> SuggestedTags { get; set; }

        // Tabs keeps track of which tabs are open in the DV
        private ObservableCollection<IDetailViewable> _tabs;
        public ObservableCollection<IDetailViewable> Tabs
        {
            get { return _tabs; }
            set
            {
                _tabs = value;
                RaisePropertyChanged("Tabs");
            }
        }




        //Visibility binding for the DV tabs. Visible when 2+ tabs.
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
        // Tab Pane Height is a reference to the height of the Tab pane 
        public double TabPaneHeight { get; set; }
        private double _tabHeight;

        // TabHeight controls the standard height that tabs have. It is some factor of the TabPaneHeight
        public double TabHeight
        {
            get { return _tabHeight; }
            set
            {
                _tabHeight = value; 
                RaisePropertyChanged("TabHeight");
                RaisePropertyChanged("TextHeight");
            }
        }
        public double TextHeight { get { return _tabHeight - 25; } }

        public ObservableCollection<StackPanel> Metadata { get; set; }

        public ObservableCollection<Region> RegionCollection { set; get; }

        public ObservableCollection<Region> OrderedRegionCollection
        {
            get
            {
                if (CurrentElementController.LibraryElementModel.Type == NusysConstants.ElementType.PDF)
                {
                    var list = RegionCollection.ToList<Region>();
                    var orderedList = (list.OrderBy(a => (a as PdfRegionModel).PageLocation)).ToList<Region>();
                    var collection = new ObservableCollection<Region>();
                    foreach (var region in orderedList)
                    {
                        //(region as PdfRegionModel).PageLocation += 1;
                        collection.Add(region);
                        

                    }
                    return collection;
                }
                else
                {
                    return new ObservableCollection<Region>();
                }
            }
        }
        private DetailHomeTabViewModel _regionableRegionTabViewModel;
        private DetailHomeTabViewModel _regionableHomeTabViewModel;

        private ElementViewModel _currentElementViewModel;
        public LibraryElementController CurrentElementController { get; set; }

        public IDetailViewable CurrentDetailViewable { get; set; }

        public delegate void TitleChangedHandler(object source, string newTitle);
        public event TitleChangedHandler TitleChanged;

        public delegate void SizeChangedEventHandler(object source, double left, double width, double height);
        public event SizeChangedEventHandler SizeChanged;
        
        public DetailViewerViewModel()

        {
            Tags = new ObservableCollection<FrameworkElement>();
            SuggestedTags = new ObservableCollection<FrameworkElement>();
            Metadata = new ObservableCollection<StackPanel>();
            RegionCollection = new ObservableCollection<Region>();
            Tabs = new ObservableCollection<IDetailViewable>();
            //  TabVisibility = Visibility.Collapsed;

            SizeChanged += OnSizeChanged_InvokeTabVMSizeChanged;
        }

        private void OnSizeChanged_InvokeTabVMSizeChanged(object source, double left, double width, double height)
        {
            _regionableRegionTabViewModel?.SizeChanged(source, width, height);
            _regionableHomeTabViewModel?.SizeChanged(source, width, height);
        }

        public void Dispose()
        {
            CurrentDetailViewable.TitleChanged -= ControllerTitleChanged;
            

            _nodeModel = null;

        }
        public async Task<bool> ShowElement(IDetailViewable viewable)
        {      
            if (viewable is LibraryElementController)
            {
                var controller = viewable as LibraryElementController;
                
                if (!controller.IsLoaded)
                {
                    await
                        SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(
                            controller.LibraryElementModel.LibraryElementId);
                }
                if (CurrentElementController != null)
                {
                    CurrentElementController.KeywordsChanged -= KeywordsChanged;
                    if (CurrentDetailViewable != null)
                    {
                        CurrentDetailViewable.TitleChanged -= ControllerTitleChanged;
                    }
                }
                CurrentElementController = controller;
                CurrentDetailViewable = controller;
                CurrentElementController.KeywordsChanged += KeywordsChanged;

                RegionCollection.Clear();

                var regions = SessionController.Instance.RegionsController.GetRegionLibraryElementIds(CurrentElementController.LibraryElementModel.LibraryElementId);

                if (regions?.Count > 0)
                {
                    foreach (var regionLibraryId in regions)
                    {
                        Debug.Assert(SessionController.Instance.ContentController.GetContent(regionLibraryId) is Region);
                        RegionCollection.Add(SessionController.Instance.ContentController.GetContent(regionLibraryId) as Region);
                    }
                }
                RaisePropertyChanged("OrderedRegionCollection");

                View = await _viewHomeTabViewFactory.CreateFromSendable(controller);
                if (View == null)
                {
                    return false;
                }

                RegionView = await _viewHomeTabViewFactory.CreateFromSendable(controller);
                if (RegionView == null)
                {
                    return false;
                }


                _regionableRegionTabViewModel = RegionView.DataContext as DetailHomeTabViewModel;
                _regionableRegionTabViewModel.Editable = true;
                _regionableHomeTabViewModel = View.DataContext as DetailHomeTabViewModel;
                _regionableHomeTabViewModel.Editable = false;

                RaisePropertyChanged("View");
                RaisePropertyChanged("RegionView");

                RegionView.Loaded += delegate
                {

                    _regionableRegionTabViewModel.SetExistingRegions();

                };

                View.Loaded += delegate
                {
                    _regionableHomeTabViewModel.SetExistingRegions();
                };


                //SizeChanged += (sender, left, width, height) => _regionableRegionTabViewModel.SizeChanged(sender, width, height);
                //SizeChanged += (sender, left, width, height) => _regionableHomeTabViewModel.SizeChanged(sender, width, height);
                
                Title = controller.LibraryElementModel.Title;

                controller.TitleChanged += ControllerTitleChanged;
                MakeTagList();
                MakeSuggestedTagList();
                RaisePropertyChanged("View");
                RaisePropertyChanged("SuggestedTags");
                RaisePropertyChanged("Tags");
                RaisePropertyChanged("Metadata");
                RaisePropertyChanged("RegionView");

                AddTab(viewable);
                return true;
            }/* else if (viewable is RegionLibraryElementController)
            {
                var controller = viewable as RegionLibraryElementController;
                CurrentDetailViewable = controller;
                var regionModel = controller.Model;
                if (regionModel == null)
                {
                    return false;
                }
                CurrentElementController =
                    SessionController.Instance.ContentController.GetLibraryElementController(controller.ContentId);
                if (CurrentElementController == null)
                {
                    return false;
                }
                View = await _viewHomeTabViewFactory.CreateFromSendable(CurrentElementController, CurrentElementController.LibraryElementModel.Regions);
                if (View == null)
                {
                    return false;
                }

                var regionSet = new HashSet<Region>();
                regionSet.Add(regionModel);
                
                _regionableHomeTabViewModel = View.DataContext as DetailHomeTabViewModel;
                _regionableHomeTabViewModel.Editable = false;

                RaisePropertyChanged("View");
                
                SizeChanged += (sender, left, width, height) => _regionableHomeTabViewModel.SizeChanged(sender, width, height);
                
                Title = regionModel.Name;
                _regionableHomeTabViewModel.RegionsToLoad = regionSet; // Only one region (the one selected) will appear in the DV

                if (View is PdfDetailHomeTabView)
                {
                    (View as PdfDetailHomeTabView).ContentLoaded += delegate
                    {
                        _regionableHomeTabViewModel.SetExistingRegions();
                        (_regionableHomeTabViewModel as PdfDetailHomeTabViewModel).Goto((regionModel as PdfRegionModel).PageLocation, regionModel);
                    };
                }else if(View is ImageDetailHomeTabView)
                {
                    (View as ImageDetailHomeTabView).ContentLoaded += delegate
                    {
                        _regionableHomeTabViewModel.SetExistingRegions();
                        (_regionableHomeTabViewModel as ImageDetailHomeTabViewModel).HighlightRegion(regionModel as RectangleRegion);
                    };
                }else if(View is AudioDetailHomeTabView)
                {
                    (View as AudioDetailHomeTabView).ContentLoaded += delegate
                    {
                        _regionableHomeTabViewModel.SetExistingRegions();
                        (_regionableHomeTabViewModel as AudioDetailHomeTabViewModel).OnRegionSeek((regionModel as AudioRegionModel).Start + 0.01);
                    };
                }else if (View is VideoDetailHomeTabView)
                {
                    (View as VideoDetailHomeTabView).ContentLoaded += delegate
                    {
                        _regionableHomeTabViewModel.SetExistingRegions();
                        (_regionableHomeTabViewModel as VideoDetailHomeTabViewModel).OnRegionSeek((regionModel as VideoRegionModel).Start + 0.01);

                    };
                }

                


                RaisePropertyChanged("Title");
                RaisePropertyChanged("View");
                RaisePropertyChanged("Tags");
                RaisePropertyChanged("Metadata");

                AddTab(viewable);
                return true;
            } else
            {
                return false;
            }*/
            return false;
        }


        public void AddTab(IDetailViewable viewable)
        {
            if (_tabs.Contains(viewable))
            {
                return;
            }
            if (_tabs.Count < 6)
            {
                _tabs.Add(viewable);
            }
            else
            {
                _tabs.RemoveAt(0);
                _tabs.Add(viewable);
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
        

        private void KeywordsChanged(object sender, HashSet<Keyword> keywords)
        {
            MakeTagList();
            //MakeSuggestedTagList();
        }
        
        public void ChangeSize(object sender, double left, double width, double height)
        {
            //Debug.WriteLine("DetailViewerViewModel ChangeSize being called");
            SizeChanged?.Invoke(sender, left, width, height);
        }

        public void ChangeRegionsSize(object sender, double width, double height)
        {
            _regionableRegionTabViewModel?.SizeChanged(sender, width, height);
            _regionableHomeTabViewModel?.SizeChanged(sender, width, height);
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

        public void MakeSuggestedTagList()
        {
            // clear the current suggested tags
            SuggestedTags.Clear();
            if (CurrentElementController != null)
            {
                //TODO remove debug asserts, if statements are ugly but needed because otherwise produced async crash on key not found

                // get the metaDataDictionary for the currentelementController
                var metaDataDict = CurrentElementController?.LibraryElementModel.Metadata;
                var suggestedTags = new List<string>();
                // get a list of the suggested tags from the metadataentry for system suggested names
                if (metaDataDict.ContainsKey("system_suggested_names"))
                {
                    suggestedTags = metaDataDict["system_suggested_names"].Values;
                }
                if (metaDataDict.ContainsKey("system_suggested_topics"))
                {
                    suggestedTags.AddRange(metaDataDict["system_suggested_topics"].Values);
                }
                //HACKY solution to add suggested tags to all types, can remove later --Trent

                suggestedTags.AddRange(suggestedTags);//doubles the importance of all system suggested tags added so far

                if (metaDataDict.ContainsKey("system_suggested_dates"))
                {
                    suggestedTags.AddRange(metaDataDict["system_suggested_dates"].Values);
                }

                foreach (var kvp in CurrentElementController.LibraryElementModel.FullMetadata ?? new Dictionary<string, MetadataEntry>())
                {
                    suggestedTags.AddRange(new HashSet<string>(kvp.Value.Values));
                }
                var linksController = SessionController.Instance.LinksController;
                foreach (var linkId in linksController.GetLinkedIds(CurrentElementController?.ContentId))
                {
                    var linkController = linksController.GetLinkLibraryElementControllerFromLibraryElementId(linkId);
                    if (linkController == null)
                    {
                        continue;
                    }
                    var opposite = linksController.GetOppositeLibraryElementModel(CurrentElementController?.ContentId, linkController);
                    if (opposite?.LibraryElementModel == null)
                    {
                        continue;
                    }
                    suggestedTags.AddRange(opposite.LibraryElementModel.Keywords.Select(key => key.Text));
                    foreach (var kvp in opposite.LibraryElementModel.FullMetadata ?? new Dictionary<string, MetadataEntry>())
                    {
                        if (kvp.Key != "system_suggested_dates")
                        {
                            suggestedTags.AddRange(kvp.Value.Values);
                        }
                    }
                }
                //END HACKY REGION


                // count the number of times each tag appears in the suggested tags using a <tag, count> dictionary
                var tagCountDictionary = new Dictionary<string, int>();
                foreach (var suggestedTag in suggestedTags)
                {
                    if (suggestedTag == null)
                    {
                        continue;
                    }
                    var lowerTag = suggestedTag.ToLower();
                    if (tagCountDictionary.ContainsKey(lowerTag))
                    {
                        tagCountDictionary[lowerTag] += 1;
                    }
                    else
                    {
                        tagCountDictionary.Add(lowerTag, 1);
                    }                  
                }

                // remove the tags from the <tag, count> dictionary which are already set as keywords
                var currentTags = CurrentElementController?.LibraryElementModel.Keywords;
                foreach (var currentTag in currentTags)
                {
                    var lowerTag = currentTag.Text.ToLower();
                    if (tagCountDictionary.ContainsKey(lowerTag))
                    {
                        tagCountDictionary.Remove(lowerTag);
                    }
                }

                // now use the tagCountDictionary to order the tags by their importance
                var suggestions = from entry in tagCountDictionary
                                orderby entry.Value descending
                                select entry.Key;

                // create a limited number of tag blocks
                int numSuggestions = 0;
                foreach (var suggestion in suggestions)
                {
                    // this is the limiter
                    if (numSuggestions == 50)
                    {
                        break;
                    }
                    var suggestedTagBlock = MakeSuggestedTagBlock(suggestion);
                    SuggestedTags.Add(suggestedTagBlock);
                    numSuggestions++;                   
                }
            }
            RaisePropertyChanged("SuggestedTags");
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
           // tagBlock.IsHitTestVisible = false;
            return tagBlock;
        }

        //this is an ugly method, refactor later so not making a UI element in viewmodel
        public FrameworkElement MakeSuggestedTagBlock(string text)
        {
            var tagContent = new TextBlock() { Text = text };
            tagContent.Foreground = new SolidColorBrush(Constants.color3);
            tagContent.FontStyle = FontStyle.Italic;
            tagContent.HorizontalAlignment = HorizontalAlignment.Stretch;

            var stackPanel = new Grid();
            stackPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            stackPanel.Children.Add(tagContent);
            Grid.SetColumn(tagContent, 0);

            Button suggestedTagBlock = new Button();
            suggestedTagBlock.Background = new SolidColorBrush(Colors.Transparent);
            suggestedTagBlock.Content = stackPanel;
            suggestedTagBlock.Height = 30;
            suggestedTagBlock.Padding = new Thickness(5);
            suggestedTagBlock.BorderThickness = new Thickness(0);
            suggestedTagBlock.Foreground = new SolidColorBrush(Constants.color3);
            suggestedTagBlock.Margin = new Thickness(5, 2, 2, 5);
            suggestedTagBlock.FontStyle = FontStyle.Italic;
            suggestedTagBlock.Tapped += SuggestedTagBlock_Tapped;
            
            // This is Super Important! adds the string to the button so Tapped method can access the string!
            suggestedTagBlock.Tag = text;
            return suggestedTagBlock;
        }

        private void SuggestedTagBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var text = (sender as Button)?.Tag as string;
            Debug.Assert(text != null);
            // not sure if topic modeling is the right source for keywordSource
            var keyword = new Keyword(text, Keyword.KeywordSource.TopicModeling);
            CurrentElementController?.AddKeyword(keyword);
            SuggestedTags.Remove(sender as FrameworkElement);
            // when we run out of tags, try to make more
            if (SuggestedTags.Count == 0)
            {
                MakeSuggestedTagList();
            }
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
            CurrentDetailViewable.TitleChanged -= ControllerTitleChanged;
            CurrentDetailViewable.SetTitle(title);
            CurrentDetailViewable.TitleChanged += ControllerTitleChanged;

            Tabs.Remove(CurrentDetailViewable);
            Tabs.Add(CurrentDetailViewable);

            /*
            // TODO make the exploration mode related list box show up
            var button = sender as Button;
            var panel = button.Content as StackPanel;
            //var block = panel.FindVisualChild("tagContent") as TextBlock;
           // var tag = block.Text;
           // Debug.WriteLine(tag);
           */


        }

    }
}
