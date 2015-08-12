using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    
    public enum Options {
        SELECT, GLOBAL_INK, ADD_TEXT_NODE, ADD_INK_NODE, DOCUMENT, PROMOTE_INK, SAVE
    }


    public sealed partial class FloatingMenu : UserControl
    {
        public event OnModeChangeHandler ModeChange;
        public delegate void OnModeChangeHandler(Options mode);

        private bool _subMenuOpen;

        public FloatingMenu()
        {
            this.InitializeComponent();
            SetActive(idleButton);
        }

        public void SetActive(Button btn)
        {
            inkButton.Opacity = 1;
            linkButton.Opacity = 1;
            textButton.Opacity = 1;
            scribbleButton.Opacity = 1;
            docButton.Opacity = 1;
            idleButton.Opacity = 1;
            saveButton.Opacity = 1;

            btn.Opacity = 0.75;

            if (_subMenuOpen == true)
            {
                slidein.Begin();
                _subMenuOpen = false;
            }
        }

        private void GlobalInkButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.GLOBAL_INK);
            if (_subMenuOpen == false)
            {
                slideout.Begin();
                _subMenuOpen = true;
            }
        }

        private void LinkButton_Click(object sender, TappedRoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.GLOBAL_INK);
        }

        private void TextButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.ADD_TEXT_NODE);
        }


        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void InkNodeButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.ADD_INK_NODE);
        }

        private async void DocumentButton_Click(object sender, RoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.DOCUMENT);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Save on tapped");
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.SAVE);
        }

        private void Erase_OnTapped(object sender, RoutedEventArgs e)
        {
        }

        private void Highlight_OnTapped(object sender, RoutedEventArgs e)
        {
        }

        private void Idle_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SetActive((Button)sender);
            ModeChange?.Invoke(Options.SELECT);
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)this.DataContext;
            var compositeTransform = vm.FMTransform;

            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;

            /*
            vm.FMTransform = compositeTransform;
            if (compositeTransform.TranslateX < -85 || compositeTransform.TranslateX > this.ActualWidth || compositeTransform.TranslateY < -85 + FM.Children.Count * -100 || compositeTransform.TranslateY > this.ActualHeight)
            {
                FM.Visibility = Visibility.Collapsed;
                e.Complete();
            }
            */
            
            e.Handled = true;
        }
    }
}
