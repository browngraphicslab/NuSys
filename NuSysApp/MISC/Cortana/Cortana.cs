using System;
using System.Collections.Generic;
using System.Text;
using Windows.Media.SpeechRecognition;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace NuSysApp
{
    class Cortana
    {
        // This error is raised if "Getting to know you" is disabled in Win10 privacy settings
        protected const uint HResultPrivacyStatementDeclined = 0x80045509;
        protected SpeechRecognizer Recognizer;
        protected CoreDispatcher Dispatcher;
        protected StringBuilder DictatedStringBuilder;

        public static IEnumerable<string> SpeechCommands = new List<string>
        {
            "open document",
            "create text",
            "create ink"
        };

        public async Task InitializeRecognizer()
        {
            DictatedStringBuilder = new StringBuilder();
            Recognizer = new SpeechRecognizer();
            await Recognizer.CompileConstraintsAsync();
        }

        //public static async Task<string> RunRecognizer()
        //{
        //    try
        //    {
        //        // Create an instance of SpeechRecognizer.
        //        var speechRecognizer = new SpeechRecognizer();

        //        //var webSearchGrammar = new SpeechRecognitionTopicConstraint(
        //        //    SpeechRecognitionScenario.WebSearch, "webSearch");

        //        speechRecognizer.UIOptions.AudiblePrompt = "Say a command...";
        //        speechRecognizer.UIOptions.ExampleText = @"Ex. 'open document' or 'create [node type]'";
        //        //speechRecognizer.Constraints.Add(webSearchGrammar);

        //        // Compile the dictation grammar by default.
        //        await speechRecognizer.CompileConstraintsAsync();

        //        // Start recognition.
        //        var speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync(); //????!!!??!?!?!?!?!?!??? (crashes after 2 successful commands)
        //        // Check if recognition was successful
        //        if (speechRecognitionResult.Status != SpeechRecognitionResultStatus.Success) return null;
        //        // Do something with the recognition result.
        //        var result = speechRecognitionResult.Text;
        //        return result;
        //    }
        //    catch (Exception exception)
        //    {
        //        //// Handle the speech privacy policy error.
        //        //if ((uint)exception.HResult == HResultPrivacyStatementDeclined)
        //        //{
        //        //    Debug.WriteLine("The privacy statement was declined.");
        //        //    await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-accounts"));
        //        //}
        //        //else
        //        //{
        //        //    var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "Exception");
        //        //    await messageDialog.ShowAsync();
        //        //}
        //        //return null;
        //        await CatchSpeechException(exception);
        //        return null;
        //    }
        //}

        protected static async Task CatchSpeechException(Exception exception)
        {
            if ((uint)exception.HResult == HResultPrivacyStatementDeclined)
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(
                    "You must accept the speech privacy policy to continue.",
                    "Speech Exception");
                messageDialog.ShowAsync().GetResults();

            }
            else
            {
                var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "Exception");
                await messageDialog.ShowAsync();
            }
        }
    }
}
