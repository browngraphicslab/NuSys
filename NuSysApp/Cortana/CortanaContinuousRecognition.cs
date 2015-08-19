using System;
using System.Linq;
using System.Threading.Tasks;

namespace NuSysApp
{
    sealed class CortanaContinuousRecognition : Cortana
    {
        private CortanaContinuousRecognition() { }

        /// <summary>
        /// Raises events when commands are encountered, instead of returning anything (or maybe not .__.)
        /// </summary>
        /// <returns></returns>
        public static async Task<string> RunRecognizerAndReturnResult()
        {
            await ResetRecognizer();
            while (WorkspaceView.CortanaRunning)
            {
                try
                {
                    var dictationChunk = await RunRecognizerChunk(); // bleh (this is where stuff screws up)
                    DictatedStringBuilder.Append(" " + dictationChunk);
                    var processedResult = ProcessDictationChunk(dictationChunk);
                    if (processedResult != null)
                    {
                        return processedResult.Trim();
                    }
                }
                catch (Exception exception)
                {
                    HandleSpeechException(exception);
                    return null;
                }
            }
            return null;
        }

        private static string ProcessDictationChunk(string chunk)
        {
            if (string.IsNullOrWhiteSpace(chunk))
            {
                return null;
            }
            if (chunk.Contains(ResetStringBuilderCommand))
            {
                DictatedStringBuilder.Clear();
                DictatedStringBuilder.Append(GetSubstringFollowingResetCommand(chunk));
            }
            if (chunk.Contains(StopListeningCommand))
            {
                return RemoveStopCommand(DictatedStringBuilder.ToString()).Trim();
            }
            if (NodeCreationCommands.Contains(chunk.ToLower().Trim()))
            {
                var command = chunk.ToLower().Trim();
                return command;
            }
            return null;
        }
    }
}
