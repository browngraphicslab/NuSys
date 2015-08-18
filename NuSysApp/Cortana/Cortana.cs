using System;
using System.Collections.Generic;
using System.Text;
using Windows.Media.SpeechRecognition;
using System.Threading.Tasks;

namespace NuSysApp
{
    class Cortana
    {
        protected Cortana() { }

        // This error is raised if "Getting to know you" is disabled in Win10 privacy settings.
        protected const uint HResultPrivacyStatementDeclined = 0x80045509;

        protected static SpeechRecognizer Recognizer;
        protected static StringBuilder DictatedStringBuilder;
        protected const string StopListeningCommand = "stop";
        protected const string ResetStringBuilderCommand = "reset";
        protected const string BeginDictationCommand = "dictate";
        protected const string RecognizerFailedIndicator = "recognizerfailed";

        // Speech commands should be accessible from CortanaMode.
        public static readonly IReadOnlyCollection<string> NodeCreationCommands = new List<string>
        {
            "open document",
            "create text",
            "create ink",
        };

        /// <summary>
        /// Reset all variables required for speech recognition
        /// </summary>
        /// <returns></returns>
        protected static async Task ResetRecognizer()
        {
            DictatedStringBuilder = new StringBuilder();
            Recognizer = new SpeechRecognizer();
            await Recognizer.CompileConstraintsAsync();
        }

        /// <summary>
        /// Runs one iteration of Cortana recognizer. If a phrase is detected, then that phrase is returned.
        /// If not, then the method returns null.
        /// </summary>
        protected static async Task<string> RunRecognizerChunk()
        {
            try
            {
                var speechRecognitionResult = await Recognizer.RecognizeAsync();
                var succeeded = SpeechRecognitionSucceeded(speechRecognitionResult);
                return succeeded ? speechRecognitionResult.Text : null;
            }
            catch
            {
                return RecognizerFailedIndicator;
            }
        }
        private static bool SpeechRecognitionSucceeded(SpeechRecognitionResult result)
        {
            return result.Status == SpeechRecognitionResultStatus.Success;
        }

        /// <summary>
        /// Returns the substring preceding the first instance of the predefined stop command from a string.
        /// </summary>
        protected static string RemoveStopCommand(string dictatedString)
        {
            if (!dictatedString.Contains(StopListeningCommand)) return dictatedString;
            var index = dictatedString.IndexOf(StopListeningCommand, StringComparison.OrdinalIgnoreCase);
            return dictatedString.Substring(0, index-1);
        }

        protected static string GetAllAfterResetCommand(string dictatedString)
        {
            if (!dictatedString.Contains(ResetStringBuilderCommand)) return dictatedString;
            var index = dictatedString.IndexOf(ResetStringBuilderCommand, StringComparison.OrdinalIgnoreCase);
            return dictatedString.Substring(index + ResetStringBuilderCommand.Length + 1);
        }

        protected static async Task HandleSpeechException(Exception exception)
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
