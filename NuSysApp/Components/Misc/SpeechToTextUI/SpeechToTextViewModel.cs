using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NuSysApp
{

    class SpeechToTextViewModel : BaseINPC
    {
        // whether the speech to text box is currently listening
        // also controls visibility of stop and record buttons, and status text
        private bool _isListening;
        public bool IsListening
        {
            get { return _isListening; }
            set
            {
                _isListening = value;
                if (_isListening)
                {
                    RecordButtonVisibility = Visibility.Collapsed;
                    StopButtonVisibility = Visibility.Visible;
                    IsTextBoxEnabled = false;
                    StatusText = "Tap Stop and Await Results";
                }
                else
                {
                    StopButtonVisibility = Visibility.Collapsed;
                    RecordButtonVisibility = Visibility.Visible;
                    StatusText = "Tap the Microphone and Speak";
                    IsTextBoxEnabled = true;
                }
                RaisePropertyChanged("IsListening");
            }

        }

        private bool _isTextBoxEnabled;
        public bool IsTextBoxEnabled
        {
            get { return _isTextBoxEnabled; }
            set
            {
                _isTextBoxEnabled = value;
                RaisePropertyChanged("IsTextBoxEnabled");
            }
            
        }

        // the visibility of the record button
        private Visibility _recordButtonVisibility;
        public Visibility RecordButtonVisibility
        {
            get { return _recordButtonVisibility; }
            set
            {
                _recordButtonVisibility = value;
                RaisePropertyChanged("RecordButtonVisibility");
            }
        }

        // the visibility of the stop button
        private Visibility _stopButtonVisibility;
        public Visibility StopButtonVisibility
        {
            get { return _stopButtonVisibility; }
            set
            {
                _stopButtonVisibility = value;
                RaisePropertyChanged("StopButtonVisibility");
            }
        }

        //
        private Visibility _errorButtonVisiblity;

        public Visibility ErrorButtonVisibility
        {
            get { return _errorButtonVisiblity; }
            set
            {
                _errorButtonVisiblity = value;
                if (_errorButtonVisiblity == Visibility.Visible)
                {
                    StopButtonVisibility = Visibility.Collapsed;
                    RecordButtonVisibility = Visibility.Collapsed;
                }

                RaisePropertyChanged("ErrorButtonVisibility");
            }
        }

        // status text under record and stop buttons, give user textual feedback
        private string _statusText;
        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value;
                RaisePropertyChanged("StatusText");
            }
        }

        // the width of the root grid
        private double _width;
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        // the height of the root grid
        private double _height;
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged("Text");
            }
        }

        private Visibility _isEnabled;

        public Visibility IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }

        private List<WordAlternates> _prevWordAlternates;
        private List<WordAlternates> _newWordAlternates;
        private StringBuilder _prevText;
        private StringBuilder _newText;

        public void PrepareForAlternates()
        {
            _prevWordAlternates = new List<WordAlternates>();
            _newWordAlternates = new List<WordAlternates>();
            _prevText = new StringBuilder();
            _newText = new StringBuilder(Text);


            if (string.IsNullOrEmpty(_newText.ToString())) return;
            var words = _newText.ToString().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                _newWordAlternates.Add(new WordAlternates(word, new HashSet<string>()));
            }
        }

        public void UpdateWordsByIndex()
        {
            _prevText.Clear();
            _prevText.Append(_newText);
            _newText.Clear();
            _newText.Append(Text);
            _prevWordAlternates = _newWordAlternates;
            _newWordAlternates = new List<WordAlternates>();

            var prevWords = _prevText.ToString().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            var newWords = _newText.ToString().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

            int prevWordIndex;
            int newWordIndex;

            for (prevWordIndex = 0, newWordIndex = 0;
                prevWordIndex < prevWords.Length && newWordIndex < newWords.Length;)
            {
                if (string.Compare(prevWords[prevWordIndex], newWords[newWordIndex]) == 0)
                {
                    _newWordAlternates.Add(_prevWordAlternates[prevWordIndex]);
                    prevWordIndex++;
                    newWordIndex++;
                }
                else
                {
                    _newWordAlternates.Add(new WordAlternates(newWords[newWordIndex], new HashSet<string>()));
                    newWordIndex++;
                }
            }

            while (newWordIndex < newWords.Length)
            {

                _newWordAlternates.Add(new WordAlternates(newWords[newWordIndex], new HashSet<string>()));
                newWordIndex++;
            }

            printWordsByIndex();
        }

        public void UpdateWordsByIndex(List<WordAlternates> alternates, int alternatesStartIndex)
        {
            _prevText.Clear();
            _prevText.Append(_newText);
            _newText.Clear();
            _newText.Append(Text);
            _prevWordAlternates = _newWordAlternates;
            _newWordAlternates = new List<WordAlternates>();                       

            var prevWords = _prevText.ToString().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
            var newWords = _newText.ToString().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

            int prevWordIndex;
            int newWordIndex;

            // take care of words prior to start Index
            for (prevWordIndex = 0, newWordIndex = 0;
                prevWordIndex < prevWords.Length && newWordIndex < newWords.Length && newWordIndex < alternatesStartIndex;)
            {
                if (string.Compare(prevWords[prevWordIndex], newWords[newWordIndex]) == 0)
                {
                    _newWordAlternates.Add(_prevWordAlternates[prevWordIndex]);
                    prevWordIndex++;
                    newWordIndex++;
                }
                else
                {
                    _newWordAlternates.Add(new WordAlternates(newWords[newWordIndex], new HashSet<string>()));
                }
            }

            // take care of words within the new alternates
            for (int i = 0; i < alternates.Count; i++, newWordIndex++)
            {
                _newWordAlternates.Add(alternates[i]);
            }

            // skip any deleted text in prevWords
            while (prevWordIndex < prevWords.Length && newWordIndex < newWords.Length && 
                string.Compare(prevWords[prevWordIndex], newWords[newWordIndex]) != 0)
            {
                prevWordIndex++;
            }

            // take care of words after new alternates
            for (prevWordIndex = prevWordIndex, newWordIndex = newWordIndex;
                prevWordIndex < prevWords.Length && newWordIndex < newWords.Length;)
            {
                if (string.Compare(prevWords[prevWordIndex], newWords[newWordIndex]) == 0)
                {
                    _newWordAlternates.Add(_prevWordAlternates[prevWordIndex]);
                    prevWordIndex++;
                    newWordIndex++;
                }
                else
                {
                    _newWordAlternates.Add(new WordAlternates(newWords[newWordIndex], new HashSet<string>()));
                }
            }

            while (newWordIndex < newWords.Length)
            {
                _newWordAlternates.Add(new WordAlternates(newWords[newWordIndex], new HashSet<string>()));
                newWordIndex++;
            }

            printWordsByIndex();

        }

        public void printWordsByIndex()
        {
            foreach (var word in _newWordAlternates)
            {
                Debug.WriteLine(word.ToString());
            }
        }

        public List<string> GetAlternatesByIndex(int index)
        {
            if (index < _newWordAlternates.Count)
            {
                return _newWordAlternates[index].Alternates.ToList();
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
