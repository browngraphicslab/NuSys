using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

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

        private readonly List<Button> _buttons;

        public FloatingMenu()
        {
            this.InitializeComponent();
            _buttons = new List<Button>
            {
                inkButton,
                linkButton,
                textButton,
                scribbleButton,
                docButton,
                cortanaButton,
                idleButton,
                saveButton
            };
            SetOpacityActive(idleButton, cortanaButton);
        }

        public void SetOpacityActive(params Button[] btnsToActivate)
        {
            // set all buttons to deactivated opacity
            foreach (var btn in _buttons)
            {
                btn.Opacity = Constants.ButtonDeactivatedOpacity;
            }
            //cortanaButton.Opacity = WorkspaceView.CortanaRunning
            //    ? Constants.ButtonDeactivatedOpacity
            //    : Constants.ButtonActivatedOpacity;
            foreach (var btnToActivate in btnsToActivate)
            {
                btnToActivate.Opacity = Constants.ButtonActivatedOpacity;
            }
            // Close any open submenus
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
            ModeChange?.Invoke(Options.Cortana);
            if (!WorkspaceView.CortanaRunning)
            {
                SetOpacityActive(cortanaButton);
            }
            else
            {
                SetOpacityActive(idleButton);
                ModeChange?.Invoke(Options.Select);
            }
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
