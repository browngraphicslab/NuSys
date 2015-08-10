using System;
using Windows.Media.SpeechRecognition;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace NuSysApp
{
    class Cortana
    {
        private const uint HResultPrivacyStatementDeclined = 0x80045509;
        private SpeechRecognizer _speechRecognizer;
        // Speech events may originate from a thread other than the UI thread.
        // Keep track of the UI thread dispatcher so that we can update the
        // UI in a thread-safe manner.
        private CoreDispatcher _dispatcher;
        private StringBuilder _dictatedTextBuilder;
        public Cortana()
        {
            
        }

        public static async void ShowMessage(string message)
        {
            var messageDialog = new Windows.UI.Popups.MessageDialog(message, "Text spoken");
            await messageDialog.ShowAsync();
        }

        public static async Task<string> RunRecognizer()
        {
            try
            {
                // Create an instance of SpeechRecognizer.
                var speechRecognizer = new SpeechRecognizer();

                var webSearchGrammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.WebSearch,
                    "webSearch");

                speechRecognizer.UIOptions.AudiblePrompt = "Say what you want to search for...";
                speechRecognizer.UIOptions.ExampleText = @"Ex. 'weather for London'";
                speechRecognizer.Constraints.Add(webSearchGrammar);

                // Compile the dictation grammar by default.
                await speechRecognizer.CompileConstraintsAsync();

                // Start recognition.
                var speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();

                // Do something with the recognition result.
                var result = speechRecognitionResult.Text;
                //var messageDialog = new Windows.UI.Popups.MessageDialog(result, "Text spoken");
                //await messageDialog.ShowAsync();
                return result;
            }
            catch (Exception exception)
            {
                // Handle the speech privacy policy error.
                if ((uint)exception.HResult == HResultPrivacyStatementDeclined)
                {
                    Debug.WriteLine(
                        "The privacy statement was declined. Go to Settings->Privacy->Speech, inking and typing, and ensure you have viewed the privacy policy, and 'Get To Know You' is enabled.");
                    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-accounts"));
                }
                else
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "Exception");
                    await messageDialog.ShowAsync();
                }
                return null;
            }
        }

        public async void ContinuousInput()
        {
            this._dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            this._speechRecognizer = new SpeechRecognizer();
            var result = await _speechRecognizer.CompileConstraintsAsync();
            this._speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            this._speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
        }
        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {

            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
              args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                _dictatedTextBuilder.Append(args.Result.Text + " ");

                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ShowMessage(_dictatedTextBuilder.ToString());
                    //btnClearText.IsEnabled = true;
                });
            }
            else
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ShowMessage(_dictatedTextBuilder.ToString());
                });
            }
        }
        private async void ContinuousRecognitionSession_Completed(
  SpeechContinuousRecognitionSession sender,
  SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status == SpeechRecognitionResultStatus.Success) return;
            if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //rootPage.NotifyUser(
                    //  "Automatic Time Out of Dictation",
                    //  NotifyType.StatusMessage);

                    //DictationButtonText.Text = " Continuous Recognition";
                    ShowMessage(_dictatedTextBuilder.ToString());
                });
            }
            else
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    //rootPage.NotifyUser(
                    //  "Continuous Recognition Completed: " + args.Status.ToString(),
                    //  NotifyType.StatusMessage);

                    //DictationButtonText.Text = " Continuous Recognition";
                });
            }
        }
    }
}
