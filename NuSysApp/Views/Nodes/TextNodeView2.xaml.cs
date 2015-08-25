using Windows.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class TextNodeView2 : UserControl
    {
        private List<Image> _images = new List<Image>();

        public TextNodeView2(TextNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;


            rtfTextBox.SizeChanged += delegate
            {
                RearrangeImagePlaceHolders();
            };

            rtfTextBox.TextChanging += delegate
            {
                RearrangeImagePlaceHolders();
            };

            rtfTextBox.TextChanged += delegate
            {
                RearrangeImagePlaceHolders();
            };

            vm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                try
                {
                    if (e.PropertyName == "Rtf")
                    {
                        imgPlaceholderContainer.Children.Clear();
                        _images.Clear();

                        foreach (var vmImg in vm.InlineImages)
                        {
                            var imageOverlay = new Image();
                            imageOverlay.Tapped += delegate { Debug.WriteLine("click."); };
                            imageOverlay.Source = vmImg;
                            imageOverlay.Width = vmImg.PixelWidth;
                            imageOverlay.Height = vmImg.PixelHeight;
                            _images.Add(imageOverlay);
                            imgPlaceholderContainer.Children.Add(imageOverlay);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception caught");
                }
            };
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {

        }


        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            var vm = (TextNodeViewModel)this.DataContext;

            if (vm.IsEditingInk == true)
            {
                nodeTpl.ToggleInkMode();
            }

            vm.ToggleEditing();
            
            if (!vm.IsEditing)
            {
                await vm.UpdateRtf();

                RearrangeImagePlaceHolders();
            }

            if (mdTextBox.Visibility == Visibility.Visible)
            {
                mdTextBox.Visibility = Visibility.Collapsed;
                rtfTextBox.Visibility = Visibility.Visible;
                imgPlaceholderContainer.Visibility = Visibility.Visible;
                rtfTextBox.Document.ApplyDisplayUpdates();
            }
            else
            {
                mdTextBox.Visibility = Visibility.Visible;
                rtfTextBox.Visibility = Visibility.Collapsed;
                imgPlaceholderContainer.Visibility = Visibility.Collapsed;
            }

        }

        private void OnInkClick(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();
        }

        private void RearrangeImagePlaceHolders()
        {

            string str2;
            rtfTextBox.Document.GetText(TextGetOptions.None, out str2);
            rtfTextBox.Document.Selection.SetRange(0, str2.Length);

            Rect rect2;
            int hit2;
            rtfTextBox.Document.Selection.GetRect(PointOptions.None, out rect2, out hit2);


            grid.Height = rect2.Height > this.Height ? rect2.Height : this.Height;

            var currentSelectionStart = rtfTextBox.Document.Selection.StartPosition;
            var currentSelectionEnd = rtfTextBox.Document.Selection.EndPosition;

            var vm = (TextNodeViewModel)this.DataContext;

            var objPos = 0;
            var startPos = 0;
            for (var i = 0; i < _images.Count; i++)
            {
                string str;
                rtfTextBox.Document.GetText(TextGetOptions.None, out str);
                rtfTextBox.Document.Selection.SetRange(startPos, str.Length);
                var findPos = rtfTextBox.Document.Selection.FindText("￼", TextConstants.MaxUnitCount, FindOptions.Word);

                if (findPos == 0)
                    throw new Exception("Couldn't find image in RichText");

                objPos = GetNthIndex(str, '￼', i + 1);

                int hit;
                Rect rect;
                rtfTextBox.Document.Selection.GetRect(PointOptions.None, out rect, out hit);

                var posX = rect.Left + rtfTextBox.Padding.Left;
                var posY = rect.Top + rtfTextBox.Padding.Top;
                Canvas.SetLeft(_images[i], posX);
                Canvas.SetTop(_images[i], posY);

                startPos = objPos + 1;
            }

            rtfTextBox.Document.Selection.SetRange(currentSelectionStart, currentSelectionEnd);
        }

        private int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private void FormatText(object sender, RoutedEventArgs e)
        {
            /*
            ITextSelection selected = rtfTextBox.Document.Selection;
            if (selected != null)
            {
                ITextCharacterFormat characterFormat = selected.CharacterFormat;
                if (sender == Bold)
                {
                    characterFormat.Bold = FormatEffect.Toggle;
                }
                if (sender == Italic)
                {
                    characterFormat.Italic = FormatEffect.Toggle;
                }
                if (sender == Underline)
                {
                    if (characterFormat.Underline == UnderlineType.Single)
                    {
                        characterFormat.Underline = UnderlineType.None;
                    }
                    else
                    {
                        characterFormat.Underline = UnderlineType.Single;
                    }
                }
                if (sender == Size8)
                {
                    characterFormat.Size = 8;
                }
                if (sender == Size12)
                {
                    characterFormat.Size = 12;
                }
                if (sender == Size14)
                {
                    characterFormat.Size = 14;
                }
                if (sender == Size18)
                {
                    characterFormat.Size = 18;
                }
                if (sender == Size24)
                {
                    characterFormat.Size = 24;
                }
                if (sender == Red)
                {
                    characterFormat.ForegroundColor = Color.FromArgb(100, 255, 0, 0);
                }
                if (sender == Orange)
                {
                    characterFormat.ForegroundColor = Color.FromArgb(100, 255, 128, 0);
                }
                if (sender == Yellow)
                {
                    characterFormat.ForegroundColor = Color.FromArgb(100, 255, 255, 0);
                }
                if (sender == Green)
                {
                    characterFormat.ForegroundColor = Color.FromArgb(100, 0, 255, 0);
                }
                if (sender == Blue)
                {
                    characterFormat.ForegroundColor = Color.FromArgb(100, 0, 0, 255);
                }
                if (sender == Purple)
                {
                    characterFormat.ForegroundColor = Color.FromArgb(100, 127, 0, 255);
                }
                if (sender == Black)
                {
                    characterFormat.ForegroundColor = Color.FromArgb(100, 0, 0, 0);
                }
                if (sender == White)
                {
                    characterFormat.ForegroundColor = Color.FromArgb(100, 255, 255, 255);
                }

    

                selected.CharacterFormat = characterFormat;
          

        }
              */
        }
    }
}