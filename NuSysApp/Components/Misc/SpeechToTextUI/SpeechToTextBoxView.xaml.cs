using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp.Misc.SpeechToTextUI
{
    public sealed partial class SpeechToTextBoxView : UserControl
    {
        // Speech events may come in on a thread other than the UI thread, keep track of the UI thread's
        // _dispatcher, so we can update the UI in a thread-safe manner.
        private CoreDispatcher _dispatcher;

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

        // correction suggester variables

        // holds the list of hypotheses that have been generated so far
        private List<string> _hypothesesList;
        // Our custom correctionSuggester
        private CorrectionSuggester _myCorrectionSuggester;
        // if this is true, we update our words by index
        private bool textChanged = false;

        // events

        public delegate void MyTextChangedEventHandler(object sender, string newText);
        public event MyTextChangedEventHandler OnTextChanged;
        public delegate void UpdateWordsByIndexEventHandler(object sender, string currentText, List<HashSet<string>> corrections);
        public event UpdateWordsByIndexEventHandler OnUpdateWords;


        /// <summary>
        /// This HResult represents the scenario where a user is prompted to allow in-app speech, but 
        /// declines. This should only happen on a Phone device, where speech is enabled for the entire device,
        /// not per-app.
        /// </summary>
        private static uint HResultPrivacyStatementDeclined = 0x80045509;

        #region constructors
        // constructor called when dictation box is supposed to be initialized as empty
        public SpeechToTextBoxView(double width, double height)
        {
            this.InitializeComponent();

            RootGrid.Width = width;
            RootGrid.Height = height;

            _isListening = false;
            _hypothesesList = new List<string>();
            _myCorrectionSuggester = new CorrectionSuggester();

            // add events
            this.Loaded += SpeechToTextBoxView_Loaded;
            this.Unloaded += SpeechToTextBoxView_Unloaded;

            // supply the data context
            DataContext = new SpeechToTextViewModel();
            OnTextChanged += (DataContext as SpeechToTextViewModel).TextChanged;
            OnUpdateWords += (DataContext as SpeechToTextViewModel).UpdateWords;
        }  

        // constructor called when dictation box is initialized with text
        public SpeechToTextBoxView(double width, double height, string text) : this(width, height)
        {
            // populate the textbox with string stripped of html
            dictationTextBox.Text = HtmlRemoval.StripTagsReplaceDivCloseWithNewLines(text);
            // add the current text to the currently confirmed text
            First = dictationTextBox.Text;
            //_myCorrectionSuggester = new CorrectionSuggester(text);
            textChanged = true;
        }
        #endregion

        #region canvas manipulation code
        // when the rootgrid is manipulated, move the entired box
        private void RootGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            xMatrixTransform.Matrix = xTransformGroup.Value;
            xCompositeTransform.TranslateX = e.Delta.Translation.X;
            xCompositeTransform.TranslateY = e.Delta.Translation.Y;

            e.Handled = true;
        }   

        // stop bubbling when manipulation occurs on the dictation textbox, this is so user text selection works
        private void dictationTextBox_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            
            e.Handled = true;
        }
        #endregion


        #region speech recognition
        #region initialization code
        private async void SpeechToTextBoxView_Loaded(object sender, RoutedEventArgs e)
        {
            // Keep track of the UI thread _dispatcher, as speech events will come in on a separate thread.
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            // Prompt the user for permission to access the microphone. This request will only happen
            // once, it will not re-prompt if the user rejects the permission.
            bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();
            if (permissionGained)
            {
                btnContinuousRecognize.IsEnabled = true;
                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
            }
            else
            {
                this.dictationTextBox.Text = "Permission to access capture resources was not given by the user, reset the application setting in Settings->Privacy->Microphone.";
                btnContinuousRecognize.IsEnabled = false;
            }

        }

        /// <summary>
        /// Initialize Speech Recognizer and compile constraints.
        /// </summary>
        /// <param name="recognizerLanguage">Language to use for the speech recognizer</param>
        /// <returns>Awaitable task.</returns>
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
                btnContinuousRecognize.IsEnabled = false;
                StatusBlock.Visibility = Visibility.Visible;
                StatusBlock.Text = $"Error: close window and try again, (error - {result.Status}";
            }

            // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
            // some recognized phrases occur, or the garbage rule is hit. HypothesisGenerated fires during recognition, and
            // allows us to provide incremental feedback based on what the user's currently saying.
            _speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            _speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
        }
        #endregion

        #region shutdown code
        /// <summary>
        /// Upon leaving, clean up the speech recognizer. Ensure we aren't still listening, and disable the event 
        /// handlers to prevent leaks.
        /// </summary>
        /// <param name="e">Unused navigation parameters.</param>
        private async void SpeechToTextBoxView_Unloaded(object sender, RoutedEventArgs e)
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
            }
        }

        // removes the speech to text box from the panel its on
        private void ButtonClose_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var parent = this.Parent as Panel;
            if (parent != null)
                parent.Children.Remove(this);
            else
            {
                throw new Exception("The SpeechToTextBox Can only close when placed on a panel :'(");
            }
        }
        #endregion

        #region error during speech recognition or speech recognition timeout code
        /// <summary>
        /// Handle events fired when error conditions occur, such as the microphone becoming unavailable, or if
        /// some transient issues occur.
        /// </summary>
        /// <param name="sender">The continuous recognition session</param>
        /// <param name="args">The state of the recognizer</param>
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
                        DictationButtonText.Text = " Dictate";
                        var textBoxContent = First + dictationTextBox.SelectedText + Second;
                        dictationTextBox.Text = textBoxContent;
                        _isListening = false;
                    });
                }
                else
                {
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        DictationButtonText.Text = " Dictate";
                        StatusBlock.Visibility = Visibility.Visible;
                        StatusBlock.Text = $"Continuous Recognition Completed: Speech Recognition Status {args.Status.ToString()}";
                        _isListening = false;
                    });
                }
            }
        }
        #endregion

        /// <summary>
        /// Called continuously while the speech recognizer is running.
        /// Replaces any selected text, or adds in new hypotheses at the position of the cursor
        /// After the first call, the latest hypothesis is continuously selected and replaced until ContinuousRecognitionSession_ResultGenerated is called
        /// </summary>
        /// <param name="sender">The recognizer that has generated the hypothesis</param>
        /// <param name="args">The hypothesis formed</param>
        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            var hypothesis = args.Hypothesis.Text;
            _hypothesesList.Add(hypothesis);

            await UpdateSpeechRecogntionProperties();         

            var textboxContent = First + " " + hypothesis + "..." + Second;
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                dictationTextBox.Text = textboxContent;
                dictationTextBox.Focus(FocusState.Programmatic);
                dictationTextBox.Select(First.Length, hypothesis.Length + 4);
            });

            await UpdateSpeechRecogntionProperties();
        }

        /// <summary>
        /// Handle events fired when a result is generated. Check for high to medium confidence, and then append the
        /// string to the end of the stringbuffer, and replace the content of the textbox with the string buffer, to
        /// remove any hypothesis text that may be present.
        /// </summary>
        /// <param name="sender">The Recognition session that generated this result</param>
        /// <param name="args">Details about the recognized speech</param>
        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            // We may choose to discard content that has low confidence, as that could indicate that we're picking up
            // noise via the microphone, or someone could be talking out of earshot.
            //if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
            //    args.Result.Confidence == SpeechRecognitionConfidence.High)

            await UpdateSpeechRecogntionProperties();

            var textBoxContent = First + " " + args.Result.Text + " " + Second;

            var corrections = _myCorrectionSuggester.GetAlternates(_hypothesesList);
            _hypothesesList.Clear();

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                dictationTextBox.Text = textBoxContent;
                dictationTextBox.Select(First.Length + args.Result.Text.Length + 2, 0); // move selection to end of input text
                if (textChanged)
                {
                    OnUpdateWords?.Invoke(this, textBoxContent, corrections);
                    textChanged = false;
                }
                               
            });

            await UpdateSpeechRecogntionProperties();
        }  

        /// <summary>
        /// Begin recognition, or finish the recognition session. 
        /// </summary>
        /// <param name="sender">The button that generated this event</param>
        /// <param name="e">Unused event details</param>
        public async void ContinuousRecognize_Click(object sender, RoutedEventArgs e)
        {
            btnContinuousRecognize.IsEnabled = false;
            if (_isListening == false) // occurs if you click the button while the speech recognizer is not listening
            {
                // send all current changes to the speech recognizer
                // before the user has started speaking
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var textBoxContent = dictationTextBox.Text;
                    if (textChanged)
                    {
                        OnUpdateWords?.Invoke(this, textBoxContent, null);
                        textChanged = false;
                    }
                    
                });
                

                // The recognizer can only start listening in a continuous fashion if the recognizer is currently idle.
                // This prevents an exception from occurring.
                if (_speechRecognizer.State == SpeechRecognizerState.Idle)
                {
                    DictationButtonText.Text = " Stop Dictation";
                    hlOpenPrivacySettings.Visibility = Visibility.Collapsed;

                    try
                    {
                        _isListening = true;
                        await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        if ((uint)ex.HResult == HResultPrivacyStatementDeclined)
                        {
                            // Show a UI link to the privacy settings.
                            hlOpenPrivacySettings.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            var messageDialog = new Windows.UI.Popups.MessageDialog(ex.Message, "Exception");
                            await messageDialog.ShowAsync();
                        }

                        _isListening = false;
                        DictationButtonText.Text = " Dictate";
                    }
                }
            }
            else // occurs if you tap the stop dictate button while dictation is still occuring
            {
                _isListening = false;
                DictationButtonText.Text = " Dictate";

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
            btnContinuousRecognize.IsEnabled = true;
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
                SelectionLength = dictationTextBox.SelectionLength;
                // the index of the first character selected or the index of the character the cursor is in front of
                SelectionStart = dictationTextBox.SelectionStart;
                // the amount of text, discounting any white space at the end
                TextLength = dictationTextBox.Text.TrimEnd().Length;
                // the text up to the first character selected, or up to the first character after the cursor
                First = dictationTextBox.Text.Substring(0, SelectionStart);
                // the text after the last character selected or after the cursor
                Second = dictationTextBox.Text.Substring(SelectionStart + SelectionLength);
            });
        }

        #endregion speech recognition

        #region CorrectionSuggester
        private void PrintList(List<string> list, SpeechRecognitionConfidence confidence)
        {
            Debug.WriteLine("---NEW CHAIN---");
            foreach (var str in list)
            {
                Debug.WriteLine(str);
            }
            Debug.WriteLine("Confidence: " + confidence);
            Debug.WriteLine("---END CHAIN---");
        }

        /// <summary>
        /// Automatically scroll the textbox down to the bottom whenever new dictated text arrives
        /// </summary>
        /// <param name="sender">The dictation textbox</param>
        /// <param name="e">Unused text changed arguments</param>
        private void dictationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // update the textbox in the viewmodel
            OnTextChanged?.Invoke(sender, dictationTextBox.Text);

            textChanged = true;

            // The automatic scroll down is currently breaking scrolling functionality
            /*
            var grid = (Grid)VisualTreeHelper.GetChild(dictationTextBox, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer))
                {
                    continue;
                }

                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                break;
            }
            */

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

                if (textChanged)
                {
                    OnUpdateWords?.Invoke(this, textBoxContent, null);
                    textChanged = false;
                }
                
                // get the list of alternateSuggestions
                var alternates = (DataContext as SpeechToTextViewModel).GetAlternatesByIndex(index);

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
            MatchCollection collection = Regex.Matches(s.Substring(0,length), @"[\S]+");
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
#endregion CorrectionSuggester



        /// <summary>
        /// Open the Speech, Inking and Typing page under Settings -> Privacy, enabling a user to accept the 
        /// Microsoft Privacy Policy, and enable personalization.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="args">Ignored</param>
        private async void openPrivacySettings_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-speechtyping"));
        }

        
    }
}