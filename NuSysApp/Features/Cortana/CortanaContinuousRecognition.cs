using System;
using System.Linq;
using System.Threading.Tasks;

namespace NuSysApp
{
    partial class CortanaContinuousRecognition : Cortana
    {
        private CortanaContinuousRecognition() { }

        /// <summary>
        /// Initializes speech recognition, continuously listens until the stop command is issued,
        /// then returns the dictated text.
        /// </summary>
        /// <returns></returns>
        private static async Task<string> RunRecognizerAndReturnResult()
        {
            await ResetRecognizer();
            while (WorkspaceView.CortanaRunning)
            {
                try
                {
                    var dictationChunk = await RunRecognizerChunk(); // bleh (this is where stuff screws up)
                    DictatedStringBuilder.Append(" " + dictationChunk);
                    CortanaPopupDialog.ModifyPopupText("hi");
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

        /// <summary>
        /// Determines if a dictated phrase is a "special" phrase. If so, an appropriate result
        /// is returned. If not, null is returned so RunRecognizerAndReturnResult can continue
        /// recognizing more speech.
        /// </summary>
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
