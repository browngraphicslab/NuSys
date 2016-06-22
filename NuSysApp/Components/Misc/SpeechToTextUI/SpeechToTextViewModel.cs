using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace NuSysApp.Misc.SpeechToTextUI
{
    class SpeechToTextViewModel : INotifyPropertyChanged
    {
        // used to store the text which has not been updated yet
        private string _previousText;

        private string _textBoxText;
        public string TextBoxText
        {
            get { return _textBoxText; }
            set
            {
                _textBoxText = value;
                OnPropertyChanged();
            }
        }

        private List<string> _wordsByIndex;
        private List<HashSet<string>> _correctionsByIndex;

        public SpeechToTextViewModel()
        {
            _wordsByIndex = new List<string>();
            _correctionsByIndex = new List<HashSet<string>>();
            TextBoxText = string.Empty;
            _previousText = string.Empty;
        }

        public void TextChanged(object sender, string currentTextBoxText)
        {
            TextBoxText = currentTextBoxText;
            OnPropertyChanged();
        }

        public void UpdateWords(object sender, string currentTextBoxText, List<HashSet<string>> correctionsList)
        {
            UpdateWordsByIndex(_previousText, currentTextBoxText, correctionsList);
            // update _previousText
            _previousText = currentTextBoxText;
        }

        void UpdateWordsByIndex(string previousText, string newText, List<HashSet<string>> correctionsList)
        {
            // get the changes from the previous text to the new text
            IEnumerable<MyLcsAlgo.DiffSection> changes = MyLcsAlgo.Diff(previousText, 0, previousText.Length, newText, 0, newText.Length);

            // variables used for updating the word indexes
            bool inWhiteSpace = true;
            bool inWord= false;
            int currWordIndex = 0;
            StringBuilder currWord = new StringBuilder();
            int prevIndex = 0;

            // update the word indexes based on the changes
            foreach (var change in changes)
            {
                switch (change.Type)
                {
                    case MyLcsAlgo.DiffSectionType.Copy:
                        foreach (var c in change.Content)
                        {
                            // if the character is white space
                            if (char.IsWhiteSpace(c))
                            {
                                // check that inWhiteSpace is set correctly
                                if (inWhiteSpace == false)
                                {
                                    inWhiteSpace = true;
                                }
                                // if we are currently in a word then we have arrived at the end
                                if (inWord)
                                {
                                    // if _wordsByIndex does not contain the proper string then update it
                                    if (string.Compare(_wordsByIndex[currWordIndex], currWord.ToString()) != 0)
                                    {
                                        _wordsByIndex[currWordIndex] = currWord.ToString();
                                        _correctionsByIndex[currWordIndex] = new HashSet<string>();
                                    }
                                        

                                    // update currWord, inWord, and currWord index
                                    currWord.Clear();
                                    inWord = false;
                                    currWordIndex++;
                                }
                            }
                            else // the character is not whitespace
                            {
                                // if we are currently in whitespace we're in the beginning of a word
                                if (inWhiteSpace)
                                {
                                    // set inWord to true, inWhiteSpace to false, and update currWordIndex and currWrod
                                    inWord = true;
                                    inWhiteSpace = false;
                                    currWord.Append(c);
                                }
                                else
                                {
                                    // the character is not whitespace but update the currWord with the new character
                                    currWord.Append(c);
                                }
                            }
                            // update the prevIndex to reflect the character we are at
                            prevIndex++;
                        }
                        break;
                    case MyLcsAlgo.DiffSectionType.Insert:
                        foreach (var c in change.Content)
                        {
                            // if the character is white space
                            if (char.IsWhiteSpace(c))
                            {
                                // check that inWhiteSpace is set correctly
                                if (inWhiteSpace == false)
                                {
                                    inWhiteSpace = true;
                                }
                                // if we are currently in a word then the white space either splits the word or
                                // implies that the word has ended
                                if (inWord)
                                {
                                    // if we are splitting a word in two
                                    if (_wordsByIndex[currWordIndex].Length > currWord.Length)
                                    {
                                        // get the second part of the word that was split in two
                                        var newWord = _wordsByIndex[currWordIndex].Substring(currWord.Length);
                                        // set the currWordIndex to the first part of the word
                                        _wordsByIndex[currWordIndex] = currWord.ToString();
                                        _correctionsByIndex[currWordIndex] = new HashSet<string>();
                                        // insert the second part of the word at the next index of _wordsByIndex
                                        _wordsByIndex.Insert(currWordIndex + 1, newWord);
                                        _correctionsByIndex.Insert(currWordIndex + 1, new HashSet<string>());
                                    }
                                    else // if the word has ended
                                    {
                                        // if _wordsByIndex does not contain the proper string then update it
                                        if (string.Compare(_wordsByIndex[currWordIndex], currWord.ToString()) != 0)
                                        {
                                            _wordsByIndex[currWordIndex] = currWord.ToString();
                                            _correctionsByIndex[currWordIndex] = new HashSet<string>();
                                        }
                                            
                                    }

                                    // update currWord, inWord, and currWord index
                                    currWord.Clear();
                                    inWord = false;
                                    currWordIndex++;
                                }
                            }
                            else // the character is not whitespace
                            {
                                // if we are currently in whitespace we're in the beginning of a word
                                if (inWhiteSpace)
                                {
                                    // update currWord to contain the new character
                                    currWord.Append(c);
                                    // if we are inserting a character at the start of a currentWord
                                    if (_previousText.Length > prevIndex && char.IsLetterOrDigit(_previousText[prevIndex]))
                                    {
                                        // update the word in _wordsByIndex
                                        _wordsByIndex[currWordIndex] = currWord + _wordsByIndex[currWordIndex];
                                        _correctionsByIndex[currWordIndex] = new HashSet<string>();
                                    }
                                    else
                                    {
                                        // insert the new word in _wordsByIndex
                                        _wordsByIndex.Insert(currWordIndex, currWord.ToString());
                                        if (correctionsList != null)
                                        {
                                            _correctionsByIndex.Insert(currWordIndex, correctionsList[0]);
                                            correctionsList.RemoveAt(0);
                                        }
                                        else
                                        {
                                            _correctionsByIndex.Insert(currWordIndex, new HashSet<string>());
                                        }
                                        
                                    }

                                    // set inWord to true, inWhiteSpace to false
                                    inWord = true;
                                    inWhiteSpace = false;
                                    
                                }
                                // if we are inserting a character
                                else
                                {
                                    // update the word in _wordsByIndex to contain the new character
                                    _wordsByIndex[currWordIndex] = _wordsByIndex[currWordIndex].Insert(currWord.Length,
                                        c.ToString());
                                    // the character is not whitespace but update the currWord with the new character
                                    currWord.Append(c);
                                    // we don't update corrections by index here because insertion occurs on each char
                                    // when the word is initially inserted
                                }
                            }
                        }
                        break;
                    case MyLcsAlgo.DiffSectionType.Delete:

                        // combine words in _wordsByIndex if whitepsace between them is deleted
                        var numWhiteSpaceDeleted = 0;
                        foreach (var c in change.Content)
                        {
                            // if we delete a white space character
                            if (char.IsWhiteSpace(c))
                            {
                                numWhiteSpaceDeleted++;

                                // check if gap has been closed between two words
                                if (prevIndex + 1 < _previousText.Length && char.IsLetterOrDigit(_previousText[prevIndex + 1]) 
                                    && prevIndex - numWhiteSpaceDeleted > 0 && !char.IsWhiteSpace(_previousText[prevIndex - numWhiteSpaceDeleted]))
                                {
                                    // if the gap has been closed, combine the word in the current word index at _wordsByIndex 
                                    // with the next word
                                    _wordsByIndex[currWordIndex] = _wordsByIndex[currWordIndex] +
                                                                   _wordsByIndex[currWordIndex + 1];
                                    _wordsByIndex.RemoveAt(currWordIndex + 1);
                                    _correctionsByIndex[currWordIndex] = new HashSet<string>();
                                    _correctionsByIndex.RemoveAt(currWordIndex + 1);
                                    numWhiteSpaceDeleted = 0;
;                                }
                            }
                            prevIndex++;
                        }

                        // from here to break statement we are just removing characters from _wordsByIndex
                        // count the number of characters that are being deleted that are not whitespace
                        int numCharToDelete = Regex.Matches(change.Content, @"[\S]").Count;

                        int lettersLeftInCurrWord = inWord
                            ? _wordsByIndex[currWordIndex].Substring(currWord.Length).Length
                            : _wordsByIndex[currWordIndex].Length;

                        // if we are only removing letters from the current word
                        if (numCharToDelete <= lettersLeftInCurrWord)
                        {
                            // update the currentword in _wordsByIndex to remove the old character
                            _wordsByIndex[currWordIndex] = _wordsByIndex[currWordIndex].Remove(currWord.Length,
                                numCharToDelete);
                            if (string.IsNullOrEmpty(_wordsByIndex[currWordIndex]))
                            {
                                _wordsByIndex.RemoveAt(currWordIndex);
                                _correctionsByIndex.RemoveAt(currWordIndex);
                            }
                        }
                        else
                        {
                            int tempWordIndex = currWordIndex;
                            // remove characters from the current word
                            _wordsByIndex[currWordIndex] = _wordsByIndex[currWordIndex].Remove(currWord.Length,
                                lettersLeftInCurrWord);
                            if (string.IsNullOrEmpty(_wordsByIndex[tempWordIndex]))
                            {
                                _wordsByIndex.RemoveAt(tempWordIndex);
                                _correctionsByIndex.RemoveAt(tempWordIndex);
                            }
                            else
                            {
                                tempWordIndex++;
                            }

                            // update numCharToDelete
                            numCharToDelete -= lettersLeftInCurrWord;

                            // remove characters from next words while there are still characters to delete                          
                            while (numCharToDelete > 0)
                            {
                                // remove the entire word
                                if (numCharToDelete >= _wordsByIndex[tempWordIndex].Length)
                                {
                                    numCharToDelete -= _wordsByIndex[tempWordIndex].Length;
                                    _wordsByIndex.RemoveAt(tempWordIndex);
                                    _correctionsByIndex.RemoveAt(tempWordIndex);
                                }
                                // remove the part of the word remaining
                                else
                                {
                                    _wordsByIndex[currWordIndex] = _wordsByIndex[currWordIndex].Remove(0, numCharToDelete);
                                    _correctionsByIndex[currWordIndex] = new HashSet<string>();
                                    break;
                                }
                            }
                        }
                        
                        break;
                    default:
                        break;
                }
            }
            PrintWordsByIndex(_wordsByIndex);
            PrintCorrectionsByIndex(_correctionsByIndex); 
        }

        public static void PrintWordsByIndex(List<string> wordsByIndex )
        {
            if (wordsByIndex != null)
            {
                Debug.Write("{ ");
                foreach (var word in wordsByIndex)
                {
                    Debug.Write($"{word},");
                }
                Debug.WriteLine("}");
            }
        }

        private static void PrintCorrectionsByIndex(List<HashSet<string>> list)
        {
            var index = 0;
            foreach (var wordSet in list)
            {
                var output = "{" + index + ": ";
                foreach (var word in wordSet)
                {
                    output = output + word + ", ";
                }
                output = output + "}";
                Debug.WriteLine(output);
                index++;
            }
        }

        internal List<string> GetAlternatesByIndex(int index)
        {
            // error checking for index out of bounds exceptions
            if (index < 0 || index >= _correctionsByIndex.Count)
            {
                throw new ArgumentOutOfRangeException("The given index is out of bounds.");
            }

            return new List<string>(_correctionsByIndex[index]);
        }

        /// <summary>
        /// Count words with Regex.
        /// </summary>
        public static int CountWords(string s)
        {
            MatchCollection collection = Regex.Matches(s, @"[\S]+");
            return collection.Count;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
