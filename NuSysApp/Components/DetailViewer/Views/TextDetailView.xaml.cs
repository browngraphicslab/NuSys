using Windows.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.Media.SpeechSynthesis;
using Windows.Media.SpeechRecognition;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    
    public sealed partial class TextDetailView : AnimatableNodeView
    {
        //private SpeechRecognizer _recognizer;
        //private bool _isRecording;
        private ObservableCollection<String> sizes = new ObservableCollection<String>();
        private ObservableCollection<FontFamily> fonts = new ObservableCollection<FontFamily>();

        public TextDetailView(TextNodeViewModel vm)
        {

            InitializeComponent();
            DataContext = vm;

            var model = (TextNodeModel)vm.Model;

            if (model.Text != "") { 
                rtfTextBox.SetRtfText(model.Text);
            }

            model.TextChanged += delegate
            {
                rtfTextBox.SetRtfText(model.Text);
            };

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {
                await SessionController.Instance.InitializeRecog();
            };
            rtfTextBox.TextChanged += delegate (object s, RoutedEventArgs a)
            {
                var request = new ChangeContentRequest(model.Id, model.ContentId, rtfTextBox.GetRtfText());
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request, NetworkClient.PacketType.UDP);
            };
            sizes.Add("8");
            sizes.Add("12");
            sizes.Add("16");
            sizes.Add("20");
            sizes.Add("24");

            fonts.Add(new FontFamily("Arial"));
            fonts.Add(new FontFamily("Courier New"));
            fonts.Add(new FontFamily("Times New Roman"));
            fonts.Add(new FontFamily("Verdana"));
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
            var model = (TextNodeModel)vm.Model;
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
                ((TextNodeModel) vm.Model).Text = session.SpeechString;
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
        }

    }
}