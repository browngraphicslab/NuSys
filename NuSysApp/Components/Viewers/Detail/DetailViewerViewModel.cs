using System;
using System.Collections.Concurrent;
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
        // Private Variables

        /// <summary>
        /// Visibility binding for the DV tabs. Visible when 2+ tabs.
        /// </summary>
        private Visibility _tabVisibility;
        private string _title;
        private DetailViewHomeTabViewFactory _viewHomeTabViewFactory = new DetailViewHomeTabViewFactory();
        private double _tabWidth;
        private DetailHomeTabViewModel _regionableRegionTabViewModel;
        private DetailHomeTabViewModel _regionableHomeTabViewModel;
        private ElementViewModel _currentElementViewModel;

        // Public Variables

        public Dictionary<string, DetailViewTabType> TabDictionary = new Dictionary<string, DetailViewTabType>();
        public event TitleChangedHandler OnTitleChanged;
        public UserControl View { get; set; }
        public UserControl RegionView { set; get; }
        public ObservableCollection<FrameworkElement> Tags { get; set; }
        public ObservableCollection<FrameworkElement> SuggestedTags { get; set; }
        public ObservableCollection<DetailViewTabTemplate> Tabs { get; private set; }
        public ObservableCollection<StackPanel> Metadata { get; set; }
        public LibraryElementController CurrentElementController { get; set; }
        public delegate void TitleChangedHandler(object source, string newTitle);
        public event TitleChangedHandler TitleChanged;
        // Tab Pane Width is a reference to the width of the Tab pane 
        public double TabPaneWidth { get; set; }


        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnTitleChanged?.Invoke(this, _title);
            }
        }

        public Visibility TabVisibility
        {
            get {  return _tabVisibility; }
            set
            {
                _tabVisibility = value; 
                RaisePropertyChanged("TabVisibility");
            }
        }

        // TabWidth controls the standard height that tabs have. It is some factor of the TabPaneWidth
        public double TabWidth
        {
            get { return _tabWidth; }
            set
            {
                _tabWidth = value; 
                RaisePropertyChanged("TabWidth");
            }
        }
        
        public DetailViewerViewModel()

        {
            Tags = new ObservableCollection<FrameworkElement>();
            SuggestedTags = new ObservableCollection<FrameworkElement>();
            Metadata = new ObservableCollection<StackPanel>();
            Tabs = new ObservableCollection<DetailViewTabTemplate>();
            SessionController.Instance.ContentController.OnElementDelete += ContentController_OnElementDelete;
        }

        private void ContentController_OnElementDelete(LibraryElementModel element)
        {
            RemoveTab(element.LibraryElementId);
            // set tab visibility to true if there is more than one
            TabVisibility = Tabs.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            TabWidth = TabPaneWidth / Tabs.Count;
        }

        public void Dispose()
        {

            // If this is null remove it 
            CurrentElementController.KeywordsChanged -= KeywordsChanged;
        }
        public async Task<bool> ShowElement(LibraryElementController controller)
        {                     
            if (!controller.ContentLoaded)
            {
                await
                    SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(
                        controller.LibraryElementModel.ContentDataModelId);
            }
            if (CurrentElementController != null)
            {
                CurrentElementController.KeywordsChanged -= KeywordsChanged;
                CurrentElementController.TitleChanged -= ControllerTitleChanged;
            }
            CurrentElementController = controller;
            CurrentElementController.KeywordsChanged += KeywordsChanged;

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
            Debug.Assert(_regionableRegionTabViewModel != null);
            _regionableRegionTabViewModel.Editable = true;
            _regionableHomeTabViewModel = View.DataContext as DetailHomeTabViewModel;
            Debug.Assert(_regionableHomeTabViewModel != null);
            _regionableHomeTabViewModel.Editable = false;

            Title = CurrentElementController.LibraryElementModel.Title;

            CurrentElementController.TitleChanged += ControllerTitleChanged;
            MakeTagList();
            MakeSuggestedTagList();

            Debug.Assert(View != null);
            Debug.Assert(RegionView != null);
            Debug.Assert(SuggestedTags != null);
            Debug.Assert(Tags != null);
            Debug.Assert(Metadata != null);

            RaisePropertyChanged("View");
            RaisePropertyChanged("RegionView");
            RaisePropertyChanged("SuggestedTags");
            RaisePropertyChanged("Tags");
            RaisePropertyChanged("Metadata");

            AddTab(CurrentElementController);
            return true;
        }


        public void AddTab(LibraryElementController controller)
        {
            Debug.Assert(Tabs != null);
            // return if the tab is already in the detail view
            if (Tabs.Any(tab => tab.LibraryElementId == controller.LibraryElementModel.LibraryElementId))
            {
                return;
            }

            if (Tabs.Count < 6)
            {
                Tabs.Add(new DetailViewTabTemplate(controller));
            }
            else
            {
                var controllerId = Tabs[0].LibraryElementId;
                RemoveTab(controllerId);
                Tabs.Add(new DetailViewTabTemplate(controller));
            }
            TabVisibility = Visibility.Visible;

            TabVisibility = Tabs.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            TabWidth = TabPaneWidth/Tabs.Count;
        }
        

        private void KeywordsChanged(object sender, HashSet<Keyword> keywords)
        {
            MakeTagList();
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
                foreach (var tag in tags ?? new HashSet<Keyword>())
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
                // if this is null we set LibraryElementModel.Metadata to the new metadata at the end of the method
                var metaDataDict = CurrentElementController?.LibraryElementModel.Metadata ?? new ConcurrentDictionary<string, MetadataEntry>();
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

                foreach (var kvp in CurrentElementController.FullMetadata ?? new Dictionary<string, MetadataEntry>())
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
                    foreach (var kvp in opposite.FullMetadata ?? new Dictionary<string, MetadataEntry>())
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
                var currentTags = CurrentElementController?.LibraryElementModel.Keywords ?? new HashSet<Keyword>();
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
            CurrentElementController.TitleChanged -= ControllerTitleChanged;
            CurrentElementController.SetTitle(title);
            CurrentElementController.TitleChanged += ControllerTitleChanged;
        }

        public void RemoveTab(string libraryElementControllerId)
        {
            var tabToRemove = Tabs.FirstOrDefault(item => item.LibraryElementId == libraryElementControllerId);
            if (tabToRemove == null)
            {
                return;
            }
            tabToRemove.Dispose();
            Tabs?.Remove(tabToRemove);

            if (Tabs?.Count < 2)
            {
                TabVisibility = Visibility.Collapsed;
            }
            if (Tabs?.Count > 0)
            {
                var viewable = Tabs[Tabs.Count - 1];
                DetailViewTabType tabToOpenTo = DetailViewTabType.Home;
                var controller =
                    SessionController.Instance.ContentController.GetLibraryElementController(viewable?.LibraryElementId);
                if (TabDictionary.ContainsKey(viewable?.LibraryElementId))
                {
                    tabToOpenTo = TabDictionary[viewable?.LibraryElementId];
                }
                SessionController.Instance.SessionView.DetailViewerView.ShowElement(controller, tabToOpenTo);

            }
            else
            {
                SessionController.Instance.SessionView.DetailViewerView.CloseDv();
            }
            TabWidth = TabPaneWidth / Tabs.Count;
        }

    }
}
