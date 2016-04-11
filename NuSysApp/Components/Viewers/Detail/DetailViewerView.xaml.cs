using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LdaLibrary;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class DetailViewerView : AnimatableUserControl
    {

        private ElementViewModel _activeVm;

        public DetailViewerView()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            //NewTagBox.Activate();


            DataContextChanged += delegate(FrameworkElement sender, DataContextChangedEventArgs args)
              {
                  if (!(DataContext is DetailViewerViewModel))
                      return;
              

                  var vm = (DetailViewerViewModel)DataContext;

                  vm.PropertyChanged += OnPropertyChanged;
                  vm.TitleChanged += LibraryElementModelTitleChanged;
                  Tags.ItemsSource = vm.Tags;
                  vm.MakeTagList();

                  this.Width = SessionController.Instance.SessionView.ActualWidth / 2;
                  this.Height = SessionController.Instance.SessionView.ActualHeight;
                  this.MaxHeight = SessionController.Instance.SessionView.ActualHeight;
                  this.MaxWidth = SessionController.Instance.SessionView.ActualWidth - resizer.ActualWidth-30;
                  Canvas.SetTop(this, 0);
                  Canvas.SetLeft(this, SessionController.Instance.SessionView.ActualWidth - Width);
                  // Metadata.ItemsSource = vm.Metadata;
              };

            SuggestButton.Click += delegate(object sender, RoutedEventArgs args)
            {
                var dvm = (DetailViewerViewModel)DataContext;
                var cvm = (ElementViewModel)dvm.View.DataContext;
                if (cvm is PdfNodeViewModel)
                {
                    var pvm = (PdfNodeViewModel) cvm;
                    LaunchLDA(pvm.GetAllText());
                }
                if (cvm is TextNodeViewModel)
                {
                    var tvm = (TextNodeViewModel)cvm;
                    LaunchLDA(tvm.Controller.LibraryElementModel.Data);
                }
            };



        }

        private void LibraryElementModelTitleChanged(object sender, string newTitle)
        {
            if (sender!=this && TitleBox.Text != newTitle)
            {
                TitleBox.Text = newTitle;
            }
        }

        public async Task LaunchLDA(string text)
        {
            if (text == null || text == "")
                return;
            var dvm = (DetailViewerViewModel)DataContext;
            var cvm = (ElementViewModel) dvm.View.DataContext;

            Task.Run(async () =>
            {
                var test = new List<string>();

                // parameters for our LDA algorithm
                string filename = cvm.Title;
                test.Add(filename);
                test.Add("niters 10");
                test.Add("ntopics 1");
                test.Add("twords 10");
                test.Add("dir ");
                test.Add("est true");
                test.Add("alpha 12.5");
                test.Add("beta .1");
                test.Add("model model-final");
                
                DieStopWords ds = new DieStopWords();
                text = await ds.removeStopWords(text);
                List<string> topics = await TagExtractor.launch(test, new List<string>() { text });
                await UITask.Run(() =>
                {
                    cvm.Controller.SetMetadata("tags", topics);
                });
            });
        }

        public async void ShowElement(ElementController controller)
        {
            var vm = (DetailViewerViewModel)DataContext;
            if (await vm.ShowElement(controller))
                Visibility = Visibility.Visible;

            if (controller.Model is TextElementModel || controller.Model is PdfNodeModel)
            {
                SuggestButton.Visibility = Visibility.Visible;
            }
            else
            {
                SuggestButton.Visibility = Visibility.Collapsed;
            }

        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {

            var vm = (DetailViewerViewModel) DataContext;
            Tags.ItemsSource = vm.Tags;

        }

        private async void NewTagBox_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == VirtualKey.Enter)
            {
                await AddTag();
                e.Handled = true;
            }
        }

        private void TitleChanged(object sender, KeyRoutedEventArgs e)
        {
            var vm = (DetailViewerViewModel)DataContext;
            vm.CurrentElementController.LibraryElementModel.Title = TitleBox.Text;
            //vm.LibraryElementModelOnOnTitleChanged(this, TitleBox.Text);
        }

        private async void AddTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            await AddTag();
        }

        private async Task AddTag()
        {
            //tagLine.Opacity = 1;
            var vm = (DetailViewerViewModel)DataContext;
            string newTag = NewTagBox.Text.Trim();
            var str = newTag.Replace(", ", ",");
            var tags = str.Split(',');
            foreach (var tag in tags)
            {
                if (tag != "")
                {
                    vm.AddTag(tag);
                   // Tags.ItemsSource = vm.Tags;
                }
            }
            
            NewTagBox.Text = "";
        }

        private async void NewKey_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == VirtualKey.Enter)
            {
                await AddMetadataKey(e);
                e.Handled = true;
            }
        }

        private async Task AddMetadataKey(KeyRoutedEventArgs e)
        {
            var vm = (DetailViewerViewModel)DataContext;
            string newKey = ((TextBox)e.OriginalSource).Text.Trim();
            if (newKey != "")
            {
              vm.AddMetadata(newKey, "", false);
            }
          //  NewMetadataBox.Text = "";
        }

        private void topBar_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if ((e.OriginalSource as UIElement) == (UIElement)exitButton)
            {
                return;
            }
            if ((Canvas.GetLeft(this) + this.ActualWidth < SessionController.Instance.SessionView.ActualWidth || e.Delta.Translation.X < 0)
                && (Canvas.GetLeft(this) > 0 || e.Delta.Translation.X > 0))
            {
                Canvas.SetLeft(this, Canvas.GetLeft(this) + e.Delta.Translation.X);
            }
            e.Handled = true;
        }

        private async void closeDV_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            var vm = (DetailViewerViewModel)DataContext;
            var textview = (vm.View as TextDetailView);
            textview?.Dispose();
        }

        private void metaData_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var toggle = (TitleEnter.Visibility == Visibility.Collapsed);
            TitleEnter.Visibility = toggle ? Visibility.Visible : Visibility.Collapsed;
            TagContainer.Visibility = toggle ? Visibility.Visible : Visibility.Collapsed;
            nodeContent.Visibility = toggle ? Visibility.Visible : Visibility.Collapsed;
           // MetadataContainer.Visibility = toggle ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TitleEnter_OnTextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
      //      ((ElementViewModel) ((DetailViewerViewModel) DataContext).View.DataContext).Model.Title = TitleEnter.Text;
        }

        private void Resizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
      //      if (!_allowResize)
        //        return;

            double rightCoord = Canvas.GetLeft(this) + this.Width;
       
            if ((this.Width > 250 || e.Delta.Translation.X < 0) && (Canvas.GetLeft(this) > 0 || e.Delta.Translation.X > 0) && (Canvas.GetLeft(this) > 30 || e.Delta.Translation.X > 0))
            {
                this.Width -= e.Delta.Translation.X;
               // xContainer.Width = this.Width - 30;

               // exitButtonContainer.Width = xContainer.Width;

                if (nodeContent.Content is ImageFullScreenView)
                {
                   // ((ImageFullScreenView) nodeContent.Content).SetDimension(xContainer.Width, SessionController.Instance.SessionView.ActualHeight);
                } else if (nodeContent.Content is TextDetailView)
                {
                 //   ((TextDetailView)nodeContent.Content).SetDimension(xContainer.Width);
                } else if (nodeContent.Content is WebDetailView)
                {
                    ((WebDetailView)nodeContent.Content).SetDimension(xContainer.Width, SessionController.Instance.SessionView.ActualHeight);
                    Canvas.SetTop(nodeContent, (SessionController.Instance.SessionView.ActualHeight - nodeContent.Height) / 2);
                }

                Canvas.SetLeft(this, rightCoord - this.Width);

                e.Handled = true;
            }

            if (Canvas.GetLeft(this) <= 30)
            {
                Canvas.SetLeft(this,30);
            }
        }
        
    }
}
