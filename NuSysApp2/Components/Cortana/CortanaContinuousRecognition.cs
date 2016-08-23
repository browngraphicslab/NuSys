using System;
using System.Linq;
using System.Threading.Tasks;

namespace NuSysApp2
{
    class CortanaContinuousRecognition : Cortana
    {
        private CortanaContinuousRecognition() { } // This class only contains static methods.

        /// <summary>
        /// Initializes speech recognition, continuously listens until the stop command is issued,
        /// then returns the dictated text.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> RunContinuousRecognizerAndReturnResult()
        {
            await ResetRecognizer();
            while (true)
            {
                try
                {
                    var dictationChunk = await RunRecognizerChunk();
                    if (dictationChunk == "recognizerfailed")
                    {
                        return null;
                    }
                    if (dictationChunk == null) continue;
                    DictatedStringBuilder.Append(" " + dictationChunk);
                    if (dictationChunk.Contains(StopListeningCommand))
                    {
                        //var raw = DictatedStringBuilder.ToString();
                        //var result = RemoveStopCommand(raw);
                        return RemoveStopCommand(DictatedStringBuilder.ToString());
                    }
                    // if a command is detected, return the command
                    if (SpeechCommands.Contains(dictationChunk))
                    {
                        var command = dictationChunk;
                        return command;
                    }
                }
                catch (Exception exception)
                {
                    HandleSpeechException(exception);
                    return null;
                }
            }
        }
    }
}
