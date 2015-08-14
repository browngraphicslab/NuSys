using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    
    public enum Options {
        Select, GlobalInk, AddTextNode, AddInkNode, Document, PromoteInk, Cortana, Erase, Highlight, Save
    }


    public sealed partial class FloatingMenu : UserControl
    {
        public event OnModeChangeHandler ModeChange;
        public delegate void OnModeChangeHandler(Options mode);

        private bool _subMenuOpen;

        public FloatingMenu()
        {
            this.InitializeComponent();
            SetOpacityActive(idleButton);
        }

        public void SetOpacityActive(Button btnToActivate)
        {
            // Buttons to deactivate
            inkButton.Opacity      = Constants.ButtonDeactivatedOpacity;
            linkButton.Opacity     = Constants.ButtonDeactivatedOpacity;
            textButton.Opacity     = Constants.ButtonDeactivatedOpacity;
            scribbleButton.Opacity = Constants.ButtonDeactivatedOpacity;
            docButton.Opacity      = Constants.ButtonDeactivatedOpacity;
            idleButton.Opacity     = Constants.ButtonDeactivatedOpacity;
            saveButton.Opacity     = Constants.ButtonDeactivatedOpacity;
            // Button to activate
            btnToActivate.Opacity  = Constants.ButtonActivatedOpacity;
            // Close all open submenus
            if (!_subMenuOpen) return;
            slidein.Begin();
            _subMenuOpen = false;
        }

        private void GlobalInkButton_Click(object sender, RoutedEventArgs e)
        {
            SetOpacityActive((Button)sender);
            ModeChange?.Invoke(Options.GlobalInk);
            if (_subMenuOpen) return;
            slideout.Begin();
            _subMenuOpen = true;
        }

        private void LinkButton_Click(object sender, TappedRoutedEventArgs e)
        {
            SetOpacityActive((Button)sender);
            ModeChange?.Invoke(Options.PromoteInk);
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            SetOpacityActive((Button)sender);
            ModeChange?.Invoke(Options.AddTextNode);
        }


        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            SetOpacityActive((Button)sender);
            ModeChange?.Invoke((Options.Erase));
        }

        private void InkNodeButton_Click(object sender, RoutedEventArgs e)
        {
            SetOpacityActive((Button)sender);
            ModeChange?.Invoke(Options.AddInkNode);
        }

        private async void DocumentButton_Click(object sender, RoutedEventArgs e)
        {
            SetOpacityActive((Button)sender);
            ModeChange?.Invoke(Options.Document);
        }

        private async void CortanaButton_Click(object sender, RoutedEventArgs e)
        {
            SetOpacityActive((Button) sender);
            ModeChange?.Invoke(Options.Cortana);
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SetOpacityActive((Button)sender);
            ModeChange?.Invoke(Options.Save);
        }

        private void Erase_OnTapped(object sender, RoutedEventArgs e)
        {
            ModeChange?.Invoke((Options.Erase));
        }

        private void Highlight_OnTapped(object sender, RoutedEventArgs e)
        {
            ModeChange?.Invoke((Options.Highlight));
        }

        private void Idle_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SetOpacityActive((Button)sender);
            ModeChange?.Invoke(Options.Select);
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            var compositeTransform = vm.FMTransform;

            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;

            /*
            vm.FMTransform = compositeTransform;
            if (compositeTransform.TranslateX < -85 || compositeTransform.TranslateX > this.ActualWidth
                || compositeTransform.TranslateY < -85 + FM.Children.Count * -100 || compositeTransform.TranslateY > this.ActualHeight)
            {
                FM.Visibility = Visibility.Collapsed;
                e.Complete();
            }
            */
            
            e.Handled = true;
        }
    }
}
