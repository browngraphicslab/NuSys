using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LibraryElementPropertiesWindow : UserControl
    {
        private int _count;
        private LibraryElementModel _currentElementModel;
        public LibraryElementPropertiesWindow()
        {
            this.InitializeComponent();
            _count = 0;
        }

        private async void LoadThumbnails(int numRows, int numCols, LibraryElementModel newItem)
        {

            StackPanel itemPanel = new StackPanel();
            itemPanel.Orientation = Orientation.Vertical;
            
            if (newItem.Type == ElementType.Image)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://wiki.tripwireinteractive.com/images/4/47/Placeholder.png", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }
            else if (newItem.Type == ElementType.Text)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://findicons.com/files/icons/1580/devine_icons_part_2/512/defult_text.png", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }
            else if (newItem.Type == ElementType.Web)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://www.clker.com/cliparts/I/Y/4/e/m/C/internet-icon-md.png", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }
            else if (newItem.Type == ElementType.PDF)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://iconizer.net/files/Devine_icons/orig/PDF.png", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }
            else if (newItem.Type == ElementType.Audio)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://icons.iconarchive.com/icons/icons8/windows-8/512/Music-Audio-Wave-icon.png", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }
            else if (newItem.Type == ElementType.Video)
            {
                Image icon = new Image();
                icon.Source = new BitmapImage(new Uri("http://www.veryicon.com/icon/ico/System/Icons8%20Metro%20Style/Photo%20Video%20Camcoder%20pro.ico", UriKind.Absolute));
                icon.MaxWidth = 125;
                itemPanel.Children.Add(icon);
            }

            if (newItem.Title != null)
            {
                TextBlock title = new TextBlock();
                title.Text = newItem.Title;
                itemPanel.Children.Add(title);
            }

            TextBlock WorkSpace = new TextBlock();
            WorkSpace.Text = newItem.Type.ToString();
            itemPanel.Children.Add(WorkSpace);


            var wrappedView = new Border();
            wrappedView.Padding = new Thickness(10);
            wrappedView.Child = itemPanel;
            Grid.SetRow(wrappedView, _count / numCols);
            Grid.SetColumn(wrappedView, _count % numCols);
            AliasGrid.Children.Add(wrappedView);
            _count++;
        }

        public void SetElement(LibraryElementModel element)
        {
            if (element == null)
            {
                Debug.WriteLine("tried to see properties window of a null element");
                return;
            }
            _currentElementModel = element;

            Title.Text = element.Title ?? "";
            Type.Text = element.Type.ToString() ?? "";
            ID.Text = element.Id ?? "";
            Creator.Text = element.Creator ?? "";

            if (element.Type == ElementType.Collection)
            {
                EnterCollectionButton.Visibility = Visibility.Visible;
            }
            else
            {
                EnterCollectionButton.Visibility = Visibility.Collapsed;
            }
        }
        public void setLastEdited(string lastedited)
        {
            LastEdited.Text = lastedited;
        }

        public void setHistory(string history)
        {
            LastEdited.Text = history;
        }

        private void CollapseArrow_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        public void setPreviewSource(BitmapImage image)
        {
            Preview.Source = image;
        }

        private void OnTitleTextChanged(object sender, TextChangedEventArgs e)
        {
            _currentElementModel?.SetTitle(Title.Text);
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteLibraryElementRequest(_currentElementModel.Id));
        }

        private async void EnterCollection_OnClick(object sender, RoutedEventArgs e)
        {
            var id = _currentElementModel.Id;
            UITask.Run(async delegate
            {
                var content = SessionController.Instance.ContentController.Get(id);
                if (content != null && content.Type == ElementType.Collection)
                {
                    List<Message> messages = new List<Message>();
                    await Task.Run(async delegate
                    {
                        messages = await SessionController.Instance.NuSysNetworkSession.GetCollectionAsElementMessages(id);
                    });
                    Visibility = Visibility.Collapsed;
                    SessionController.Instance.FireEnterNewCollectionEvent();
                    await
                        SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                            new UnsubscribeFromCollectionRequest(
                                SessionController.Instance.ActiveFreeFormViewer.ContentId));
                    await SessionController.Instance.SessionView.LoadWorkspaceFromServer(messages, id);
                }
            });
        }
    }
}
