using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public enum Options
    {
        Select, GlobalInk, AddTextNode, AddInkNode, Document, PromoteInk, Cortana, AudioCapture, Erase, Color, Save, Load, Pin
    }


    public sealed partial class FloatingMenu : UserControl
    {
        public event OnModeChangeHandler ModeChange;
        public delegate void OnModeChangeHandler(Options mode);

        private bool _subMenuOpen;
        private bool _subMenuSelectOpen;
        private bool _subMenuNodesOpen;
        private bool _subMenuAdditionalOpen;

        private readonly List<Button> _buttons;
        private static readonly SolidColorBrush BorderColor = new SolidColorBrush(Color.FromArgb(255, 194, 251, 255));

        public FloatingMenu()
        {
            this.InitializeComponent();
            _buttons = new List<Button>
            {
                inkButton,
                //audioCaptureButton,
                NewNode,
                NewMedia,
                Bucket,
                Erase,
                Highlight,
                MultiSelect,
                CortanaButton,
                Export,
                idleButton,
                Load
            };
            SetActive(idleButton);
        }

        private static void AddBorder(Button btn)
        {
            btn.BorderBrush = BorderColor;
        }

        private static void RemoveBorder(Button btn)
        {
            btn.BorderBrush = null;
        }

        public void SetActive(Options option)
        {
            // TODO: Add support for all other options
            switch (option)
            {
                case Options.Select:
                    SetActive(idleButton);
                    break;
            }
        }

        public void SetActive(Button btnToActivate)
        {
            // set all buttons to no border
            foreach (var btn in _buttons)
            {
                RemoveBorder(btn);
            }
            // set clicked inactive to activated border
            if (btnToActivate.Name == "inkButton" || btnToActivate.Name == "idleButton")
            {
                AddBorder(btnToActivate);
            }
            // Close any open submenus
            if (!_subMenuOpen && !_subMenuSelectOpen && !_subMenuNodesOpen && !_subMenuAdditionalOpen) return;
            slidein.Begin();
            slideinSelect.Begin();
            slideinNodes.Begin();
            slideinAdditional.Begin();
            _subMenuOpen = false;
            _subMenuSelectOpen = false;
            _subMenuNodesOpen = false;
            _subMenuAdditionalOpen = false;
        }

        private void Expandable(object sender, RoutedEventArgs e)
        {
            //expand
            if (ExpandImage.Visibility == Visibility.Visible)
            {
                bucketClose.Begin();
                bucketWindow.IsHitTestVisible = false;
                pinClose.Begin();
                pinWindow.IsHitTestVisible = false;
                expand.Begin();
                CollapseImage.Visibility = Visibility.Visible;
                ExpandImage.Visibility = Visibility.Collapsed;
            }
            else
            //collapse
            {
                slidein.Begin();
                slideinSelect.Begin();
                slideinNodes.Begin();
                slideinAdditional.Begin();
                _subMenuOpen = false;
                _subMenuSelectOpen = false;
                _subMenuNodesOpen = false;
                _subMenuAdditionalOpen = false;
                bucketClose.Begin();
                bucketWindow.IsHitTestVisible = false;
                pinClose.Begin();
                pinWindow.IsHitTestVisible = false;
                collapse.Begin();
                CollapseImage.Visibility = Visibility.Collapsed;
                ExpandImage.Visibility = Visibility.Visible;
            }
        }

        private void GlobalInkButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.GlobalInk);
            if (_subMenuOpen)
            {
                slidein.Begin();
                _subMenuOpen = false;
            }
            slideout.Begin();
            _subMenuOpen = true;
        }

        private void LinkButton_Click(object sender, TappedRoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.PromoteInk);
            ShowActive(idleButton, (Button)sender);
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.AddTextNode);
            ShowActive(addNodeButton, (Button)sender);
        }

        private void InkNodeButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.AddInkNode);
            ShowActive(addNodeButton, (Button)sender);
        }

        private async void DocumentButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.Document);
            ShowActive(addNodeButton, (Button)sender);
        }

        private async void AudioCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.AudioCapture);
            ShowActive(addNodeButton, (Button)sender);
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.Save);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.Load);
        }

        private void Erase_OnTapped(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke((Options.Erase));
            ShowActive(inkButton, (Button)sender);

        }

        private void Color_OnTapped(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke((Options.Color));
        }

        private void Idle_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SetActive(idleButton);
            ModeChange?.Invoke(Options.Select);
            if (_subMenuSelectOpen)
            {
                slideinSelect.Begin();
                _subMenuSelectOpen = false;
            }
            slideoutSelect.Begin();
            _subMenuSelectOpen = true;
        }

        private void Nodes_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            slidein.Begin();
            slideinSelect.Begin();
            slideinAdditional.Begin();
            _subMenuOpen = false;
            _subMenuSelectOpen = false;
            _subMenuAdditionalOpen = false;


            if (_subMenuNodesOpen)
            {
                slideinNodes.Begin();
                _subMenuNodesOpen = false;
            }
            slideoutNodes.Begin();
            _subMenuNodesOpen = true;
        }

        private async void CortanaButton_Click(object sender, TappedRoutedEventArgs e)
        {
            SetActive((Button)sender);
            //if (!WorkspaceView.CortanaRunning)
            //{
            //    AddBorder((Button)sender);
            //    ModeChange?.Invoke(Options.Cortana);
            //}
            //else
            //{
            //    RemoveBorder((Button)sender);
            //    ModeChange?.Invoke(Options.Cortana);
            //}
        }

        private void Additional_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            slidein.Begin();
            slideinSelect.Begin();
            slideinNodes.Begin();
            _subMenuOpen = false;
            _subMenuSelectOpen = false;
            _subMenuNodesOpen = false;

            if (_subMenuAdditionalOpen)
            {
                slideinAdditional.Begin();
                _subMenuAdditionalOpen = false;
            }
            slideoutAdditional.Begin();
            _subMenuAdditionalOpen = true;

        }
        private async void PinButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.Pin);
            if (pinWindow.Opacity == 0)
            {
                pinOpen.Begin();
                pinWindow.IsHitTestVisible = true;
                collapse.Begin();
                slidein.Begin();
                slideinSelect.Begin();
                slideinNodes.Begin();
                slideinAdditional.Begin();
                _subMenuOpen = false;
                _subMenuSelectOpen = false;
                _subMenuNodesOpen = false;
                _subMenuAdditionalOpen = false;
                CollapseImage.Visibility = Visibility.Collapsed;
                ExpandImage.Visibility = Visibility.Visible;
            }
        }

        private void Bucket_Click(object sender, RoutedEventArgs e)
        {
            if (bucketWindow.Opacity == 0)
            {
                bucketOpen.Begin();
                bucketWindow.IsHitTestVisible = true;
                collapse.Begin();
                slidein.Begin();
                slideinSelect.Begin();
                slideinNodes.Begin();
                slideinAdditional.Begin();
                _subMenuOpen = false;
                _subMenuSelectOpen = false;
                _subMenuNodesOpen = false;
                _subMenuAdditionalOpen = false;
                CollapseImage.Visibility = Visibility.Collapsed;
                ExpandImage.Visibility = Visibility.Visible;
            }
        }

        private void ShowActive(Button modeButton, Button setButton)
        {
            //switch locations of modeButton and setButton
            //add border to setButton
        }
            

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            var compositeTransform = vm.FMTransform;

            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;

            e.Handled = true;
        }
    }
}