using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NuSysApp
{
    sealed class CortanaContinuousRecognition : Cortana
    {
        private CortanaContinuousRecognition() { }
        /// <summary>
        /// Initializes speech recognition, continuously listens until the stop command is issued,
        /// then returns the dictated text.
        /// </summary>
        /// <returns></returns>
        //public static async Task<string> RunContinuousRecognizerAndReturnResult()
        //{
        //    await ResetRecognizer();
        //    while (WorkspaceView.CortanaRunning)
        //    {
        //        try
        //        {
        //            var dictationChunk = await RunRecognizerChunk();
        //            switch (dictationChunk)
        //            {
        //                case RecognizerFailedIndicator:
        //                    return null;
        //                case null:
        //                case "":
        //                    continue;
        //            }
        //            if (dictationChunk.Contains(ResetStringBuilderCommand))
        //            {
        //                DictatedStringBuilder.Clear();
        //                DictatedStringBuilder.Append(GetAllAfterResetCommand(dictationChunk));
        //            }
        //            DictatedStringBuilder.Append(" " + dictationChunk);
        //            if (dictationChunk.Contains(StopListeningCommand))
        //            {
        //                return RemoveStopCommand(DictatedStringBuilder.ToString()).Trim();
        //            }
        //            // if a node creation command is detected, return the command
        //            if (NodeCreationCommands.Contains(dictationChunk.ToLower().Trim()))
        //            {
        //                var command = dictationChunk.ToLower().Trim();
        //                return command;
        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            HandleSpeechException(exception);
        //            return null;
        //        }
        //    }
        //    return null;
        //}

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
                    var dictationChunk = await RunRecognizerChunk();
                    DictatedStringBuilder.Append(" " + dictationChunk); // output will be trimmed
                    var processedResult = ProcessDictationChunk(dictationChunk);
                    if (processedResult != null)
                    {
                        return processedResult;
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
                DictatedStringBuilder.Append(GetAllAfterResetCommand(chunk));
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
