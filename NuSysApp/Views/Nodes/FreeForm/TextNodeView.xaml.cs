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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class TextNodeView : UserControl
    {
        private List<Image> _images = new List<Image>();

        public TextNodeView(TextNodeViewModel vm)
        {

            this.InitializeComponent();
            this.DataContext = vm;

            var model = (TextNodeModel)vm.Model;

            mdTextBox.Text = model.Text;
            mdTextBox.TextChanging += delegate
            {
                model.Text = mdTextBox.Text;
                AdjustScrollHeight();
            };
            

            mdTextBox.SizeChanged += delegate
            {
                RearrangeImagePlaceHolders();
                AdjustScrollHeight();
            };

            rtfTextBox.SizeChanged += delegate
            {
                RearrangeImagePlaceHolders();
                AdjustScrollHeight();
            };

            rtfTextBox.TextChanging += delegate
            {
                RearrangeImagePlaceHolders();
                AdjustScrollHeight();
            };

            rtfTextBox.TextChanged += delegate
            {
                RearrangeImagePlaceHolders();
                AdjustScrollHeight();
            };

            grid.IsDoubleTapEnabled = true;
            grid.DoubleTapped += delegate(object sender, DoubleTappedRoutedEventArgs e)
            {
                Debug.WriteLine("double tapppped");
                var pos = e.GetPosition(rtfTextBox);
                var range = rtfTextBox.Document.GetRangeFromPoint(pos, PointOptions.ClientCoordinates);
                range.StartOf(TextRangeUnit.Link, true);
                var str = string.Empty;
                range.GetText(TextGetOptions.UseCrlf, out str);


                if (!str.StartsWith("HYPERLINK"))
                    return;

                var groups = Regex.Match(str, "\"(.*?)\"").Groups;
                var url = groups[1].Value;
                Launcher.LaunchUriAsync(new Uri("http://en.wikipedia.org" + url));
                e.Handled = true;
            };

            vm.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                try
                {
                    if (e.PropertyName == "RtfText")
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

        private async void OnRecordClick(object sender, RoutedEventArgs e)
        {
            TextNodeViewModel vm = (TextNodeViewModel) DataContext;
            vm.Model.SetMetaData("bling", "poof");
            return;
            var oldColor = this.RecordVoice.Background;
            Color c = new Color();
            c.A = 255;
            c.R = 199;
            c.G = 84;
            c.B = 82;
            this.RecordVoice.Background = new SolidColorBrush(c);
            await TranscribeVoice();
            this.RecordVoice.Background = oldColor;
        }

        private async Task TranscribeVoice()
        {
            // Create an instance of SpeechRecognizer. 
            var speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();
            //speechRecognizer.
            //speechRecognizer.UIOptions.ShowConfirmation = false;
            //speechRecognizer.UIOptions.IsReadBackEnabled = false;
            //speechRecognizer.UIOptions.AudiblePrompt = "";
            // Compile the dictation grammar that is loaded by default. = ""; 
            await speechRecognizer.CompileConstraintsAsync();
            string spokenString = "";
            // Start recognition. 
            try
            {
                Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                // If successful, display the recognition result. 
                if (speechRecognitionResult.Status == Windows.Media.SpeechRecognition.SpeechRecognitionResultStatus.Success)
                {
                    spokenString = speechRecognitionResult.Text;
                }
            }
            catch (Exception exception)
            {
            }
            speechRecognizer.Dispose();
            this.mdTextBox.Text = spokenString;
            var vm = (TextNodeViewModel)this.DataContext;
            vm.UpdateRtf();
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
                mdTextBox.Focus(FocusState.Keyboard);
            }
            
            AdjustScrollHeight();        
        }

        private void OnInkClick(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();
        }

        private void RearrangeImagePlaceHolders()
        {
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

        private void AdjustScrollHeight()
        {
            TextNodeViewModel vm = (TextNodeViewModel)DataContext;

            if (!vm.IsEditing)
            {
                var contentHeight = rtfTextBox.ComputeRtfHeight();
                grid.Height = contentHeight > this.MinHeight ? contentHeight : this.MinHeight;
            } else
            {
                grid.Height = mdTextBox.ActualHeight;
            }
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

        public NodeTemplate NodeTpl
        {
            get { return nodeTpl; }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Remove();
        }

        /// <summary>
        /// Catches the double-tap event so that the floating menus can't be lost.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FloatingButton_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;            
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            if (Colors.Opacity == 0)
            {
                colorout.Begin();
            }
            else
            {
                colorin.Begin();
            }
        }

        private void Change_Color(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            Button colorButton = sender as Button;
            if (colorButton.Name == "Red")
            {
                vm.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 152, 149));
            }
            else if (colorButton.Name == "Green")
            {
                vm.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 190, 240, 142));
            }
            else if (colorButton.Name == "Parchment")
            {
                vm.Color = new SolidColorBrush(Windows.UI.Colors.BlanchedAlmond);
            } else if (colorButton.Name == "Blue")
            {
                vm.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 173, 216, 230));
            }
            colorin.Begin();
        }
    }
}