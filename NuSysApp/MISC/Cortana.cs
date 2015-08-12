using System;
using Windows.Media.SpeechRecognition;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NuSysApp
{
    class Cortana
    {
        private const uint HResultPrivacyStatementDeclined = 0x80045509;
        private Cortana() { } // prevent generation of default constructor
        public async static Task<string> RunRecognizer()
        {
            try
            {
                // Create an instance of SpeechRecognizer.
                var speechRecognizer = new SpeechRecognizer();

                var webSearchGrammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.WebSearch,
                    "webSearch");

                speechRecognizer.UIOptions.AudiblePrompt = "Say a command...";
                speechRecognizer.UIOptions.ExampleText = @"Ex. 'open document' or 'create [node type]'";
                speechRecognizer.Constraints.Add(webSearchGrammar);

                // Compile the dictation grammar by default.
                await speechRecognizer.CompileConstraintsAsync();

                // Start recognition.
                var speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync(); //????!!!??!?!?!?!?!?!??? (crashes after 2-3 successful commands)
                // Do something with the recognition result.
                var result = speechRecognitionResult.Text;
                return result;
            }
            catch (Exception exception)
            {
                // Handle the speech privacy policy error.
                if ((uint)exception.HResult == HResultPrivacyStatementDeclined)
                {
                    Debug.WriteLine("The privacy statement was declined.");
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
    }
}
