using NuSysApp.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class TextDetailView : UserControl
    {
        //private SpeechRecognizer _recognizer;
        //private bool _isRecording;
        private ObservableCollection<String> sizes = new ObservableCollection<String>();
        private ObservableCollection<FontFamily> fonts = new ObservableCollection<FontFamily>();
        private string _modelContentId;
        private string _modelId;
        public TextDetailView(TextNodeViewModel vm)
        {

            InitializeComponent();

            rtfTextBox.Focus(Windows.UI.Xaml.FocusState.Keyboard);

            DataContext = vm;

            var model = (TextElementInstanceModel)vm.Model;

            var token = model.GetMetaData("Token");
            if (token == null || String.IsNullOrEmpty(token?.ToString()))
            {
                SourceBttn.Visibility = Visibility.Collapsed;
            }
            else if (!Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token?.ToString()))
            {
                SourceBttn.Visibility = Visibility.Collapsed;
            }

            var txt = SessionController.Instance.ContentController.Get((DataContext as TextNodeViewModel).ContentId).Data;
            if (txt != "") { 
                rtfTextBox.SetRtfText(txt);
            }

            model.TextChanged += delegate
            {
                var text = SessionController.Instance.ContentController.Get((DataContext as TextNodeViewModel).ContentId).Data;
                rtfTextBox.SetRtfText(text);
            };

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {
              //  await SessionController.Instance.InitializeRecog();
            };
            rtfTextBox.KeyUp += delegate
            {
                UpdateText();
            };

            _modelContentId = model.ContentId;
            _modelId = model.Id;
            sizes.Add("8");
            sizes.Add("12");
            sizes.Add("16");
            sizes.Add("20");
            sizes.Add("24");

            fonts.Add(new FontFamily("Arial"));
            fonts.Add(new FontFamily("Courier New"));
            fonts.Add(new FontFamily("Times New Roman"));
            fonts.Add(new FontFamily("Verdana"));

            controlPanel.Width = grid.Width;
        }

        //private async Task InitializeRecog()
        //{
        //    await Task.Run( async () =>
        //    {
        //        _recognizer = new SpeechRecognizer();
        //        // Compile the dictation grammar that is loaded by default. = ""; 
        //        await _recognizer.CompileConstraintsAsync();
        //    });
        //}



        public void Dispose()
        {
            var vm = DataContext as TextNodeViewModel;
            var model = (TextElementInstanceModel)vm.Model;
            model.Text = rtfTextBox.GetRtfText();
        }

        private async void OnRecordClick(object sender, RoutedEventArgs e)
        {
            //if (!_isRecording)

            var session = SessionController.Instance;
            if (!session.IsRecording)
            {
                //var oldColor = this.RecordVoice.Background;
                Color c = new Color();
                c.A = 255;
                c.R = 199;
                c.G = 84;
                c.B = 82;
           //     this.RecordVoice.Background = new SolidColorBrush(c);
                //await TranscribeVoice();
                await session.TranscribeVoice();
                //     this.RecordVoice.Background = oldColor;
                var vm = (TextNodeViewModel) DataContext;
                ((TextElementInstanceModel) vm.Model).Text = session.SpeechString;
            }
            else
            {
                var vm = this.DataContext as TextNodeViewModel;
             //   this.RecordVoice.Background = vm.Color;
            }
        }

        //private async Task TranscribeVoice()
        //{
        //    string spokenString = "";
        //    // Create an instance of SpeechRecognizer. 
        //    // Start recognition. 

        //    try
        //    {
        //       // this.RecordVoice.Click += stopTranscribing;
        //        _isRecording = true;
        //        SpeechRecognitionResult speechRecognitionResult = await _recognizer.RecognizeAsync();
        //        _isRecording = false;
        //      //  this.RecordVoice.Click -= stopTranscribing;
        //        // If successful, display the recognition result. 
        //        if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
        //        {
        //            spokenString = speechRecognitionResult.Text;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        const int privacyPolicyHResult = unchecked((int)0x80045509);
        //        const int networkNotAvailable = unchecked((int)0x80045504);

        //        if (ex.HResult == privacyPolicyHResult)
        //        {
        //            // User has not accepted the speech privacy policy
        //            string error = "In order to use dictation features, we need you to agree to Microsoft's speech privacy policy. To do this, go to your Windows 10 Settings and go to Privacy - Speech, inking, & typing, and enable data collection.";
        //            var messageDialog = new Windows.UI.Popups.MessageDialog(error);
        //            messageDialog.ShowAsync();

        //        }
        //        else if (ex.HResult == networkNotAvailable)
        //        {
        //            string error = "In order to use dictation features, NuSys requires an internet connection";
        //            var messageDialog = new Windows.UI.Popups.MessageDialog(error);
        //            messageDialog.ShowAsync();
        //        }
        //    }
        //    //_recognizer.Dispose();
        //   // this.mdTextBox.Text = spokenString;
            
        //    Debug.WriteLine(spokenString);
            
        //    var vm = (TextNodeViewModel)DataContext;
        //    (vm.Model as TextNodeModel).Text = spokenString;
        //}

        //private async void stopTranscribing(object o, RoutedEventArgs e)
        //{
        //    _recognizer.StopRecognitionAsync();
        //    _isRecording = false;
        //   // this.RecordVoice.Click -= stopTranscribing;
        //}

        private void BoldButton_OnClick(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = rtfTextBox.Document.Selection;
            if (selectedText != null)
            {
                ITextCharacterFormat format = selectedText.CharacterFormat;
                format.Bold = FormatEffect.Toggle;
                selectedText.CharacterFormat = format;
            }
            UpdateText();
        }

        private void ItalicButton_OnClick(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = rtfTextBox.Document.Selection;
            if (selectedText != null)
            {
                ITextCharacterFormat format = selectedText.CharacterFormat;
                format.Italic = FormatEffect.Toggle;
                selectedText.CharacterFormat = format;
            }
            UpdateText();
        }

        private void UnderlineButton_OnClick(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = rtfTextBox.Document.Selection;
            if (selectedText != null)
            {
                ITextCharacterFormat format = selectedText.CharacterFormat;
                if (format.Underline == UnderlineType.None)
                {
                    format.Underline = UnderlineType.Single;
                }
                else
                {
                    format.Underline = UnderlineType.None;
                }
                selectedText.CharacterFormat = format;
            }
            UpdateText();
        }

        private void SizeChanged(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = rtfTextBox.Document.Selection;
            if (selectedText != null)
            {
                if (SizeBox.SelectedItem != null)
                {
                    float size = (float)Convert.ToDouble(SizeBox.SelectedItem);
                    ITextCharacterFormat format = selectedText.CharacterFormat;
                    format.Size = size;
                    selectedText.CharacterFormat = format;
                }
                
            }
            UpdateText();
        }

        private void FontChanged(object sender, RoutedEventArgs e)
        {
            ITextSelection selectedText = rtfTextBox.Document.Selection;
            if (selectedText != null)
            {
                if (FontBox.SelectedItem != null)
                {
                    FontFamily font = (FontFamily)FontBox.SelectedItem;
                    ITextCharacterFormat format = selectedText.CharacterFormat;
                    format.Name = font.Source;
                    selectedText.CharacterFormat = format;
                }
            }
            UpdateText();
        }

        private void UpdateText()
        {
            var request = new ChangeContentRequest(_modelId, _modelContentId, rtfTextBox.GetRtfText());
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request, NetworkClient.PacketType.UDP);
        }

        private async void OnGoToSource(object sender, RoutedEventArgs e)
        {
            var model = (TextElementInstanceModel)((TextNodeViewModel)DataContext).Model;
            string token = model.GetMetaData("Token")?.ToString();

            if (!Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token?.ToString()))
            {
                return;
            }

            string ext = Path.GetExtension(model.GetMetaData("FilePath").ToString());
            StorageFolder toWriteFolder = NuSysStorages.OpenDocParamsFolder;

            if (Constants.WordFileTypes.Contains(ext))
            {
                string bookmarkId = model.GetMetaData("BookmarkId").ToString();
                StorageFile writeBookmarkFile = await StorageUtil.CreateFileIfNotExists(NuSysStorages.OpenDocParamsFolder, token);

                using (StreamWriter writer = new StreamWriter(await writeBookmarkFile.OpenStreamForWriteAsync()))
                {
                    writer.WriteLineAsync(bookmarkId);
                }

                using (StreamWriter writer = new StreamWriter(await NuSysStorages.FirstTimeWord.OpenStreamForWriteAsync()))
                {
                    writer.WriteLineAsync(token);
                }
            }
            else if (Constants.PowerpointFileTypes.Contains(ext))
            {
                using (StreamWriter writer = new StreamWriter(await NuSysStorages.FirstTimePowerpoint.OpenStreamForWriteAsync()))
                {
                    writer.WriteLineAsync(token);
                }
            }

            await AccessList.OpenFile(token);
        }

        /*
        private void RtfTextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            Dispose();
            String cont = (String)(rtfTextBox.GetRtfText());
            var newPos = cont.Length - 1;
            rtfTextBox.Document.Selection.SetRange(newPos, newPos);
            rtfTextBox.Focus(FocusState.Keyboard);
        }*/
    }
}