using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Components.Misc.SpeechToTextUI;
using NuSysApp.Misc.SpeechToTextUI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class SpeechToTextBox : UserControl
    {
        private SpeechToTextViewModel _vm;
        private object _pairedController;


        // speech recognition variables

        // the number of characters selected
        private int SelectionLength { get; set; }
        // the index of the first character selected or the index of the character the cursor is in front of
        private int SelectionStart { get; set; }
        // the amount of text, discounting any white space at the end
        private int TextLength { get; set; }
        // the text up to the first character selected, or up to the first character after the cursor
        private string First { get; set; }
        // the text after the last character selected or after the cursor
        private string Second { get; set; }
        // The speech recognizer used throughout this sample.
        private SpeechRecognizer _speechRecognizer;
        // Keep track of whether the continuous recognizer is currently running, so it can be cleaned up appropriately.
        private bool _isListening;
        // _dispatcher, so we can update the UI in a thread-safe manner.
        private CoreDispatcher _dispatcher;

        // holds the list of hypotheses that have been generated so far
        private List<string> _hypothesesList;
        private CorrectionSuggester _myCorrectionSuggester;
        private bool _textChanged;

        public SpeechToTextBox()
        {
            this.InitializeComponent();
            _vm = new SpeechToTextViewModel();
            // Keep track of the UI thread _dispatcher, as speech events will come in on a separate thread.
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            DataContextChanged += delegate(FrameworkElement sender, DataContextChangedEventArgs args)
            {
                // await the sessionController.instance.sessionview.maincanvas onloaded event to set the data context
                _vm = DataContext as SpeechToTextViewModel;
                if (_vm == null)
                {
                    return;
                }

                // set _vm variables
                _vm.IsListening = false;
                _vm.Width = 400;
                _vm.Height = 300;
                _vm.IsEnabled = Visibility.Collapsed;

                // place the box in the center of the screen
                Canvas.SetTop(this, (SessionController.Instance.SessionView.MainCanvas.ActualHeight / 2.0) - _vm.Height / 2.0);
                Canvas.SetLeft(this, (SessionController.Instance.SessionView.MainCanvas.ActualWidth / 2.0) - _vm.Width / 2.0);

                
                // for recentering when the canvas changes
                SessionController.Instance.SessionView.MainCanvas.SizeChanged += MainCanvas_SizeChanged;
                // for closing the speech to text box
                //SessionController.Instance.SessionView.MainCanvas.Tapped += MainCanvas_Tapped;
                
            };
        }

        private void MainCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // return if the pointer is pressed inside the grid
            var position = e.GetCurrentPoint(RootGrid).Position;
            if (position.X >= 0 && position.Y >= 0 && position.X <= RootGrid.Width & position.Y <= RootGrid.Height)
                return;

            if (_vm.IsEnabled == Visibility.Visible)
            {
                if ((_pairedController as TextNodeController) != null)
                {
                    var textNodeController = _pairedController as TextNodeController;
                    textNodeController.LibraryElementController.SetContentData(_vm.Text);
                    _pairedController = null;
                } else if (_pairedController as TextInputBlock != null)
                {
                    var textInputBlock = _pairedController as TextInputBlock;
                    textInputBlock.Text = _vm.Text;
                    _pairedController = null;
                }

                CleanlyCloseSpeechRecognizer();


            }

        }

        private async void CleanlyCloseSpeechRecognizer()
        {
            if (this._speechRecognizer != null)
            {
                if (_isListening)
                {
                    await this._speechRecognizer.ContinuousRecognitionSession.CancelAsync();
                    _isListening = false;
                }

                _speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                _speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;

                this._speechRecognizer.Dispose();
                this._speechRecognizer = null;

                _vm.IsEnabled = Visibility.Collapsed;
                _vm.Text = string.Empty;
                SessionController.Instance.SessionView.MainCanvas.PointerPressed -= MainCanvas_PointerPressed;
            }
        }

        // instantiation for use with textnodes
        public async void Instantiate(TextNodeController controller, string text="")
        {
            if (_vm.IsEnabled == Visibility.Collapsed)
            {
                _vm.IsEnabled = Visibility.Visible;
                _vm.IsListening = false;
                _pairedController = controller;
                _vm.ErrorButtonVisibility = Visibility.Collapsed;
                _hypothesesList = new List<string>();
                _myCorrectionSuggester = new CorrectionSuggester();

                
                _vm.Text = HtmlRemoval.StripTagsReplaceDivCloseWithNewLines(text);
                _vm.PrepareForAlternates(); // make sure to call this after setting _vm.Text
                _textChanged = true;
                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
                SessionController.Instance.SessionView.MainCanvas.PointerPressed += MainCanvas_PointerPressed;
            }
        }

        // instantiate for use with textInputBlcoks
        public async void Instantiate(TextInputBlock controller, string text)
        {
            if (_vm.IsEnabled == Visibility.Collapsed)
            {
                _vm.IsEnabled = Visibility.Visible;
                _vm.IsListening = false;
                _pairedController = controller;
                _vm.ErrorButtonVisibility = Visibility.Collapsed;
                _hypothesesList = new List<string>();
                _myCorrectionSuggester = new CorrectionSuggester();


                _vm.Text = HtmlRemoval.StripTagsReplaceDivCloseWithNewLines(text);
                _vm.PrepareForAlternates(); // make sure to call this after setting _vm.Text
                _textChanged = true;
                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
                SessionController.Instance.SessionView.MainCanvas.PointerPressed += MainCanvas_PointerPressed;
            }
        }

        private async Task InitializeRecognizer(Language recognizerLanguage)
        {
            if (_speechRecognizer != null)
            {
                // cleanup prior to re-initializing this scenario.
                _speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
                _speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;

                this._speechRecognizer.Dispose();
                this._speechRecognizer = null;
            }

            this._speechRecognizer = new SpeechRecognizer(recognizerLanguage);

            // Apply the dictation topic constraint to optimize for dictated freeform speech.
            var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
            _speechRecognizer.Constraints.Add(dictationConstraint);
            SpeechRecognitionCompilationResult result = await _speechRecognizer.CompileConstraintsAsync();
            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                _vm.ErrorButtonVisibility = Visibility.Visible;
                _vm.StatusText = $"Error: close window and try again, (error - {result.Status}";
            }

            // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
            // some recognized phrases occur, or the garbage rule is hit. HypothesisGenerated fires during recognition, and
            // allows us to provide incremental feedback based on what the user's currently saying.
            _speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            _speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                // If TimeoutExceeded occurs, the user has been silent for too long. We can use this to 
                // cancel recognition if the user in dictation mode and walks away from their device, etc.
                // In a global-command type scenario, this timeout won't apply automatically.
                // With dictation (no grammar in place) modes, the default timeout is 20 seconds.
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        _vm.IsListening = false;
                        _vm.Text = First + DictationTextbox.SelectedText + Second;
                    });
                }
                else
                {
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        _vm.IsListening = false;
                        _vm.StatusText = $"Continuous Recognition Completed: Speech Recognition Status {args.Status}";
                    });
                }
            }
        }

        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            var hypothesis = args.Hypothesis.Text;
            _hypothesesList.Add(hypothesis);

            await UpdateSpeechRecogntionProperties();


            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _vm.Text = First + " " + hypothesis + "..." + Second;
                DictationTextbox.Focus(FocusState.Programmatic);
                DictationTextbox.Select(First.Length, hypothesis.Length + 4);
                _textChanged = true;
            });

            await UpdateSpeechRecogntionProperties();
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // We may choose to discard content that has low confidence, as that could indicate that we're picking up
            // noise via the microphone, or someone could be talking out of earshot.
            //if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
            //    args.Result.Confidence == SpeechRecognitionConfidence.High)

            await UpdateSpeechRecogntionProperties();

            var corrections = _myCorrectionSuggester.GetAlternates(_hypothesesList, args.Result.Text);
            _hypothesesList.Clear();

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _vm.Text = First + " " + args.Result.Text + " " + Second;
                DictationTextbox.Select(First.Length + args.Result.Text.Length + 2, 0); // move selection to end of input text
                var startIndex = First.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).Length;
                _vm.UpdateWordsByIndex(corrections, startIndex);
            });

            await UpdateSpeechRecogntionProperties();
        }

        /// <summary>
        /// Sets SelectionLength = to the number of characters currently selected in the dictationTextBox
        /// Sets SelectionStart = to the index of the first character selected or the index of the character the cursor is in front of\n
        /// Sets TextLength = to the total text in the dictationTextBox minus any white space on the end\n
        /// Sets First = to the text up to the first character selected, or the first character after the cursor\n
        /// Sets Second = to the text after the last character selected or from the first character after the cursor\n
        /// </summary>
        /// <returns></returns>
        private async Task UpdateSpeechRecogntionProperties()
        {

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // the number of characters selected
                SelectionLength = DictationTextbox.SelectionLength;
                // the index of the first character selected or the index of the character the cursor is in front of
                SelectionStart = DictationTextbox.SelectionStart;
                // the amount of text, discounting any white space at the end
                TextLength = DictationTextbox.Text.TrimEnd().Length;
                // the text up to the first character selected, or up to the first character after the cursor
                First = DictationTextbox.Text.Substring(0, SelectionStart);
                // the text after the last character selected or after the cursor
                Second = DictationTextbox.Text.Substring(SelectionStart + SelectionLength);
            });
        }


        private void MainCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetTop(this, (SessionController.Instance.SessionView.MainCanvas.ActualHeight / 2.0) - _vm.Height / 2.0);
            Canvas.SetLeft(this, (SessionController.Instance.SessionView.MainCanvas.ActualWidth / 2.0) - _vm.Width / 2.0);
        }

        // when the rootgrid is manipulated, move the entired box
        private void RootGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            xMatrixTransform.Matrix = xTransformGroup.Value;
            xCompositeTransform.TranslateX = e.Delta.Translation.X;
            xCompositeTransform.TranslateY = e.Delta.Translation.Y;

            var transform = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(RootGrid);
            var point = transform.TransformPoint(new Point(0, 0));
            if (point.X > 0)
            {
                xCompositeTransform.TranslateX = -e.Delta.Translation.X;
                e.Complete();
                Debug.WriteLine("Hi Left");
            }
            if (point.Y > 0)
            {
                xCompositeTransform.TranslateY = -e.Delta.Translation.Y;
                e.Complete();
                Debug.WriteLine("Hi Top");
            }
            if (point.X - _vm.Width < -SessionController.Instance.SessionView.MainCanvas.ActualWidth)
            {
                xCompositeTransform.TranslateX = -e.Delta.Translation.X;
                e.Complete();
                Debug.WriteLine("Hi right");
            }
            if (point.Y - _vm.Height < -SessionController.Instance.SessionView.MainCanvas.ActualHeight)
            {
                xCompositeTransform.TranslateY = -e.Delta.Translation.Y;
                e.Complete();
                Debug.WriteLine("Hi Bottom");
            }
            

            //SessionController.Instance.SessionView.MainCanvas.

            e.Handled = true;
        }

        // stop bubbling when manipulation occurs on the dictation textbox, this is so user text selection works
        private void DictationTextBox_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            e.Handled = true;
        }

        private async void Record_OnTapped(object sender, TappedRoutedEventArgs e)
        {

            if (_vm.IsListening == false) // occurs if you click the button while the speech recognizer is not listening
            {

                if (_textChanged = true)
                {
                    _vm.UpdateWordsByIndex();
                    _textChanged = false;
                }

                // The recognizer can only start listening in a continuous fashion if the recognizer is currently idle.
                // This prevents an exception from occurring.
                if (_speechRecognizer.State == SpeechRecognizerState.Idle)
                {
                    try
                    {
                        _vm.IsListening = true;
                        await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        _vm.IsListening = false;
                        await _speechRecognizer.StopRecognitionAsync();
                        _vm.StatusText = ex.Message;
                    }
                }
            }

        }

        private async void Stop_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _vm.IsListening = false;

            if (_speechRecognizer.State != SpeechRecognizerState.Idle)
            {
                // StopAsync() allows the speech recognizer to finish its final result
                try
                {
                    // this calls the on result genereated code
                    await _speechRecognizer.ContinuousRecognitionSession.StopAsync();
                }
                catch (Exception exception)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog(exception.Message, "Exception");
                    await messageDialog.ShowAsync();
                }
            }
        }

        private void DictationTextbox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _vm.Text = DictationTextbox.Text;
            _textChanged = true;
            
        }

        private async void DictationTextBox_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
            var textBox = sender as TextBox;

            // only implement custom menu if one word is selected
            if (textBox.SelectionLength > 0 && CountWords(textBox.SelectedText, textBox.SelectedText.Length) == 1)
            {
                // get the index of the selected word in the text
                var textBoxContent = textBox.Text;
                var index = CountWords(textBoxContent, textBox.SelectionStart);

                if (_textChanged)
                {
                    _vm.UpdateWordsByIndex();
                    _textChanged = false;
                }

                // get the list of alternateSuggestions
                var alternates = _vm.GetAlternatesByIndex(index);

                // create menu
                var menu = new PopupMenu();
                for (int i = 0; i < alternates.Count && i < 6; i++)
                {
                    menu.Commands.Add(new UICommand(alternates[i], null, i));
                }

                // create selection rectangle
                var rect = this.GetTextboxSelectionRect(textBox);

                // show the menu and await command to be selected
                var chosenCommand = await menu.ShowForSelectionAsync(rect, Placement.Below);

                // if command is selected replace the text
                if (chosenCommand != null)
                {
                    if (textBox.SelectedText.EndsWith(" "))
                    {
                        textBox.SelectedText = chosenCommand.Label + " ";
                    }
                    else
                    {
                        textBox.SelectedText = chosenCommand.Label;
                    }

                }
            }
        }

        public static int CountWords(string s, int length)
        {
            MatchCollection collection = Regex.Matches(s.Substring(0, length), @"[\S]+");
            return collection.Count;
        }

        // returns a rect for selected text
        // if no text is selected, returns caret location
        // textbox should not be empty
        private Rect GetTextboxSelectionRect(TextBox textbox)
        {
            Rect rectFirst, rectLast;
            if (textbox.SelectionStart == textbox.Text.Length)
            {
                rectFirst = textbox.GetRectFromCharacterIndex(textbox.SelectionStart - 1, true);
            }
            else
            {
                rectFirst = textbox.GetRectFromCharacterIndex(textbox.SelectionStart, false);
            }

            int lastIndex = textbox.SelectionStart + textbox.SelectionLength;
            if (lastIndex == textbox.Text.Length)
            {
                rectLast = textbox.GetRectFromCharacterIndex(lastIndex - 1, true);
            }
            else
            {
                rectLast = textbox.GetRectFromCharacterIndex(lastIndex, false);
            }

            GeneralTransform buttonTransform = textbox.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());

            // Make sure that we return a valid rect if selection is on multiple lines
            // and end of the selection is to the left of the start of the selection.
            double x, y, dx, dy;
            y = point.Y + rectFirst.Top;
            dy = rectLast.Bottom - rectFirst.Top;
            if (rectLast.Right > rectFirst.Left)
            {
                x = point.X + rectFirst.Left;
                dx = rectLast.Right - rectFirst.Left;
            }
            else
            {
                x = point.X + rectLast.Right;
                dx = rectFirst.Left - rectLast.Right;
            }

            return new Rect(x, y, dx, dy);
        }


    }
}
