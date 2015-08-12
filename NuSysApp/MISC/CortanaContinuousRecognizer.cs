using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;

namespace NuSysApp
{
    class CortanaContinuousRecognizer
    {
        private SpeechRecognizer _speechRecognizer;
        private CoreDispatcher _dispatcher;
        private StringBuilder _dictatedTextBuilder;

        public async Task RunRecognizer()
        {
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            _speechRecognizer = new SpeechRecognizer();
            var result = await _speechRecognizer.CompileConstraintsAsync();
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated +=
                ContinuousRecognitionSession_ResultGenerated;
            _speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender,
            SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                _dictatedTextBuilder.Append(args.Result.Text + " ");
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var text = _dictatedTextBuilder.ToString();
                });
            }
            else
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var text = _dictatedTextBuilder.ToString();
                });
            }
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender,
            SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Debug.WriteLine("Automatic timeout of dictation");
                        var text = _dictatedTextBuilder.ToString();
                    });
                }
                else
                {
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Debug.WriteLine("Continuous Recognition Completed: " + args.Status.ToString());
                    });
                }
            }
        }
    }
}
