using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace NuSysApp
{
    class CortanaContinuousRecognition : Cortana
    {
        public async Task<string> RunContinuousRecognizerAndReturnResult(WorkspaceView view)
        {
            await InitializeRecognizer();
            while (true)
            {
                try
                {
                    // run recognizer "chunk"
                    var speechRecognitionResult = await Recognizer.RecognizeAsync();
                    if (speechRecognitionResult.Status != SpeechRecognitionResultStatus.Success) continue;
                    // get result from recognition
                    var dictationResult = speechRecognitionResult.Text;
                    if (dictationResult == "stop")
                    {
                        return DictatedStringBuilder.ToString();
                    }
                    if (SpeechCommands.Contains(dictationResult)) // if a command is detected, pass the command to CortanaMode
                    {
                        var command = dictationResult;
                        return command;
                    }
                    if (!string.IsNullOrWhiteSpace(dictationResult))
                    {
                        DictatedStringBuilder.Append(" " + dictationResult); // append phrase to stringbuilder
                    }
                }
                catch (Exception exception)
                {
                    CatchSpeechException(exception);
                    return null;
                }
            }
        }
    }
}
