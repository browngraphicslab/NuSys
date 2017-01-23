using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyToolkit.UI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

    namespace NuSysApp
    {
        public sealed partial class Keyboard : UserControl
        {


        /// <summary>
        /// the event that will be fired whenever a key is pressed that should add a new character to the listener.
        /// It passes the character pressed.
        /// </summary>
        public event EventHandler<KeyArgs> KeyboardKeyPressed;

        public event EventHandler<KeyArgs> KeyboardKeyReleased;


        #region dicts
            private static BiDictionary<string, VirtualKey> _charsToKeys = new BiDictionary<string, VirtualKey>()
        {
            {"A", VirtualKey.A},
            {"B", VirtualKey.B},
            {"C", VirtualKey.C},
            {"D", VirtualKey.D},
            {"E", VirtualKey.E},
            {"F", VirtualKey.F},
            {"G", VirtualKey.G},
            {"H", VirtualKey.H},
            {"I", VirtualKey.I},
            {"J", VirtualKey.J},
            {"K", VirtualKey.K},
            {"L", VirtualKey.L},
            {"M", VirtualKey.M},
            {"N", VirtualKey.N},
            {"O", VirtualKey.O},
            {"P", VirtualKey.P},
            {"Q", VirtualKey.Q},
            {"R", VirtualKey.R},
            {"S", VirtualKey.S},
            {"T", VirtualKey.T},
            {"U", VirtualKey.U},
            {"V", VirtualKey.V},
            {"W", VirtualKey.W},
            {"X", VirtualKey.X},
            {"Y", VirtualKey.Y},
            {"Z", VirtualKey.Z},
            {",", (VirtualKey) 188},
            {".", (VirtualKey) 190},
            {"?", (VirtualKey) 191},
            {")", VirtualKey.Number0},
            {"!", VirtualKey.Number1},
            {"@", VirtualKey.Number2},
            {"#", VirtualKey.Number3},
            {"$", VirtualKey.Number4},
            {"%", VirtualKey.Number5},
            {"^", VirtualKey.Number6},
            {"&", VirtualKey.Number7},
            {"*", VirtualKey.Number8},
            {"(", VirtualKey.Number9},
            {"0", VirtualKey.Number0},
            {"1", VirtualKey.Number1},
            {"2", VirtualKey.Number2},
            {"3", VirtualKey.Number3},
            {"4", VirtualKey.Number4},
            {"5", VirtualKey.Number5},
            {"6", VirtualKey.Number6},
            {"7", VirtualKey.Number7},
            {"8", VirtualKey.Number8},
            {"9", VirtualKey.Number9},
            {"SPACE", VirtualKey.Space},
            {"BACK", VirtualKey.Back},
            {"ENTER", VirtualKey.Enter},
            {"CONTROL", VirtualKey.Control},
            {"-", VirtualKey.Subtract},
            {"+", VirtualKey.Add},
            {"/", (VirtualKey) 191},
            {"=", VirtualKey.Add},
            {"'", (VirtualKey) 222},
            {"\"", (VirtualKey) 222},
            {":", (VirtualKey) 186},
            {";", (VirtualKey) 186},
            {"\\", (VirtualKey) 220},
            {"[", (VirtualKey) 219},
            {"]", (VirtualKey) 221},
            {"LEFT", VirtualKey.Left},
            {"RIGHT", VirtualKey.Right},










        };

            public static BiDictionary<VirtualKey, string> NoShiftKeyToChars = new BiDictionary<VirtualKey, string>()
        {
            {(VirtualKey) 191, "/"},
            {VirtualKey.Subtract, "-"},
            {VirtualKey.Add, "+"},
            {(VirtualKey) 188, ","},
            {(VirtualKey) 190, "."},
            {(VirtualKey) 220, "\\"},
            {(VirtualKey) 186, ";"},
            {(VirtualKey) 222, "'"},
            {(VirtualKey) 219, "["},
            {(VirtualKey) 221, "]"},


        };

            public static BiDictionary<VirtualKey, string> ShiftKeyToChars = new BiDictionary<VirtualKey, string>()
        {
            {VirtualKey.Number0, ")"},
            {VirtualKey.Number1, "!"},
            {VirtualKey.Number2, "@"},
            {VirtualKey.Number3, "#"},
            {VirtualKey.Number4, "$"},
            {VirtualKey.Number5, "%"},
            {VirtualKey.Number6, "^"},
            {VirtualKey.Number7, "&"},
            {VirtualKey.Number8, "*"},
            {VirtualKey.Number9, "("},
            {VirtualKey.Subtract, "_"},
            {VirtualKey.Add, "="},
            {(VirtualKey) 191, "?"},
            {(VirtualKey) 220, "|"},
            {(VirtualKey) 186, ":"},
            {(VirtualKey) 188, "<"},
            {(VirtualKey) 190, ">"},
            {(VirtualKey) 219, "{"},
            {(VirtualKey) 221, "}"},
                {(VirtualKey) 222, "\""},



        };
        #endregion dicts

        #region keyboardmode enum
        public enum KeyboardMode
            {
                LowerCaseAlphabetical,
                UpperCaseAlphabeticalTapped,
                UpperCaseAlphabeticalHeld,
                Special,
                AlphabeticalControl,
                SpecialControl
            }


        #endregion keyboardmode enum
        private KeyboardMode _currentMode;

            public KeyboardMode CurrentMode
            {
                set
                {
                    xTestingText.Text = value.ToString();
                    _currentMode = value;
                }
                get { return _currentMode; }
            }


         private List<KeyboardKey> _pressedKeys;

        /// <summary>
        /// Constructor
        /// </summary>
        public Keyboard()
        {
            this.InitializeComponent();

            this.RenderTransform = new CompositeTransform();
            CurrentMode = KeyboardMode.LowerCaseAlphabetical;
            _pressedKeys = new List<KeyboardKey>();


        }

        private void ChangeAlphabeticalToLower()
            {
                Regex upperCaseRegex = new Regex("[A-Z]");
                Regex lowerCaseRegex = new Regex("[a-z]");
                KeyboardKey key;
                foreach (UIElement elem in xABCKeyboard.Children) //iterate the main grid
                {
                    Grid grid = elem as Grid;
                    if (grid != null)
                    {
                        foreach (UIElement uiElement in grid.Children) //iterate the single rows
                        {
                            key = uiElement as KeyboardKey;
                            if (key != null) // if button contains only 1 character
                            {
                                if (key.KeyText.Length == 1)
                                {
                                    if (upperCaseRegex.Match(key.KeyText).Success)
                                        // if the char is a letter and uppercase
                                        key.KeyText = key.KeyText.ToLower();

                                }

                            }
                        }
                    }
                }

            }
        private void ChangeAlphabeticalToUpper()
        {
            Regex upperCaseRegex = new Regex("[A-Z]");
            Regex lowerCaseRegex = new Regex("[a-z]");
            KeyboardKey key;
            foreach (UIElement elem in xABCKeyboard.Children) //iterate the main grid
            {
                Grid grid = elem as Grid;
                if (grid != null)
                {
                    foreach (UIElement uiElement in grid.Children) //iterate the single rows
                    {
                        key = uiElement as KeyboardKey;
                        if (key != null) // if button contains only 1 character
                        {
                            if (key.KeyText.Length == 1)
                            {
                                if (lowerCaseRegex.Match(key.KeyText).Success)
                                    // if the char is a letter and lower case
                                    key.KeyText = key.KeyText.ToUpper();
                            }

                        }
                    }
                }
            }

        }






        #region misc/appearence
        private void Keyboard_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

        }

        private void Keyboard_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var transform = this.RenderTransform as CompositeTransform;
            Debug.Assert(transform != null);

            var compositeTransform = transform;

            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;


        }

        private void Keyboard_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
        }

        public void GainPseudoFocus()
            {
                UITask.Run(delegate
                {


                    SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager.ClearAllPointers();
                    Visibility = Visibility.Visible;
                });
            }

            public void LosePseudoFocus()
            {
                UITask.Run(delegate
                {
                    Visibility = Visibility.Collapsed;
                    SessionController.Instance.ShiftHeld = false;
                });
            }
            private void SelectKey(KeyboardKey key)
            {
                key.Select();
                _pressedKeys.Add(key);
                xTestingText2.Text = string.Join(", ", _pressedKeys.Select(a => a.KeyText));
            }

            private void UnselectKey(KeyboardKey key)
            {
                key.Unselect();
                if (_pressedKeys.Contains(key))
                {
                    _pressedKeys.Remove(key);

                }
                xTestingText2.Text = string.Join(", ", _pressedKeys.Select(a => a.KeyText));

        }

        private void ActivateKey(KeyboardKey key)
            {
                key.Activate();
            }

            private void DeactivateKey(KeyboardKey key)
            {
                key.Deactivate();
            }

            #endregion misc/appearence

        #region generic key handlers

        private void Key_OnPointerPressed(object sender, PointerRoutedEventArgs e)
            {
                var key = sender as KeyboardKey;
                SelectKey(key);

            }


            private void Key_OnPointerReleased(object sender, PointerRoutedEventArgs e)
            {
                var key = sender as KeyboardKey;
                if (_pressedKeys.Contains(key))
                {
                    KeyClicked(key.KeyValue);

                }
                UnselectKey(key);

            }

            private void Key_OnPointerExited(object sender, PointerRoutedEventArgs e)
            {
                var key = sender as KeyboardKey;
                UnselectKey(key);
            }

        private void KeyClicked(string command)
        {


            VirtualKey key;

            if (_charsToKeys.ContainsKey(command))
            {

                if (ShiftKeyToChars.Values.Contains(command))
                {
                    SessionController.Instance.ShiftHeld = true;
                }
                else if (NoShiftKeyToChars.Values.Contains(command))
                {
                    SessionController.Instance.ShiftHeld = false;
                }
                else
                {
                    if ((CurrentMode == KeyboardMode.LowerCaseAlphabetical) || (CurrentMode == KeyboardMode.Special))
                    {
                        SessionController.Instance.ShiftHeld = false;
                    }
                    else if ((CurrentMode == KeyboardMode.UpperCaseAlphabeticalHeld) ||
                             (CurrentMode == KeyboardMode.UpperCaseAlphabeticalTapped))
                    {
                        SessionController.Instance.ShiftHeld = true;

                    }
                }

                key = _charsToKeys[command];
                KeyboardKeyPressed?.Invoke(this, new KeyArgs() { Key = key, Pressed = true });

            }

            if (CurrentMode == KeyboardMode.UpperCaseAlphabeticalTapped)
            {
                SwitchFromUpperCaseAlphabeticalMode();
                SwitchToLowerCaseAlphabeticalMode();
            }

            if (CurrentMode == KeyboardMode.AlphabeticalControl && command != "CONTROL")
            {
                SwitchFromAlphabeticalControlMode();
                SwitchToLowerCaseAlphabeticalMode();
            }

        }

        #endregion generic key handlers

        #region shift

        private void Shift_OnPointerPressed(object sender, PointerRoutedEventArgs e)
            {
                var key = sender as KeyboardKey;

                if (CurrentMode == KeyboardMode.LowerCaseAlphabetical)
                {
                     SwitchToUpperCaseAlphabeticalTappedMode();
                }
                else if(CurrentMode.IsUpperCase())
                {
                    SwitchFromUpperCaseAlphabeticalMode();
                    SwitchToLowerCaseAlphabeticalMode();
                }else if (CurrentMode == KeyboardMode.AlphabeticalControl)
                {
                    SwitchFromAlphabeticalControlMode();
                    SwitchToLowerCaseAlphabeticalMode();

            }



        }

        private void Shift_OnHolding(object sender, HoldingRoutedEventArgs e)
        {
            var key = sender as KeyboardKey;
            //ChangeAlphabeticalCase();

            if (CurrentMode == KeyboardMode.UpperCaseAlphabeticalTapped)
            {
                SwitchToUpperCaseAlphabeticalHeldMode();
            }

        }


        #endregion shift

        #region ctrl
        private void ControlKey_OnPointerPressed(object sender, PointerRoutedEventArgs e)
            {
                var key = sender as KeyboardKey;

                if (CurrentMode.IsAlphabetical())
                {
                    SelectKey(key);
                    SwitchToAlphabeticalControlMode();
                }
                else if (CurrentMode == KeyboardMode.Special)
                {
                    SwitchToSpecialControlMode();
                }
                else if (CurrentMode == KeyboardMode.AlphabeticalControl)
                {
                    UnselectKey(key);
                    SwitchFromAlphabeticalControlMode();
                    SwitchToLowerCaseAlphabeticalMode();
                }
                KeyClicked(key.KeyValue);




            }


            private void ControlKey_OnPointerReleased(object sender, PointerRoutedEventArgs e)
            {
                var key = sender as KeyboardKey;
                UnselectKey(key);
            }
            private void ControlKey_OnPointerExited(object sender, PointerRoutedEventArgs e)
            {
                var key = sender as KeyboardKey;
                UnselectKey(key);
            }

            private void ShowSuperscriptsOnControl(bool showsuperscript)
            {

                if (showsuperscript)
                {
                    xAKey.SuperscriptText = "Select All";
                    xZKey.SuperscriptText = "Undo";
                    xXKey.SuperscriptText = "Cut";
                    xCKey.SuperscriptText = "Copy";
                }
                else
                {
                    xAKey.SuperscriptText = String.Empty;
                    xZKey.SuperscriptText = String.Empty;
                    xXKey.SuperscriptText = String.Empty;
                    xCKey.SuperscriptText = String.Empty;
                }


            }
            #endregion ctrl





        private void X_OnClick(object sender, RoutedEventArgs e)
        {
            LosePseudoFocus();
        }

        private void Special_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (CurrentMode.IsUpperCase())
            {
                SwitchFromUpperCaseAlphabeticalMode();
            }else if (CurrentMode == KeyboardMode.AlphabeticalControl)
            {
                SwitchFromAlphabeticalControlMode();
            }
            SwitchToSpecialMode();
        }


        private void Alphabetical_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SwitchToLowerCaseAlphabeticalMode();

        }

        #region switch methods

        private void SwitchFromAlphabeticalControlMode()
        {
            ShowSuperscriptsOnControl(false);
        }

        private void SwitchFromUpperCaseAlphabeticalMode()
        {
            ChangeAlphabeticalToLower();
        }

        private void SwitchToSpecialControlMode()
        {
            CurrentMode = KeyboardMode.SpecialControl;

        }

        private void SwitchToAlphabeticalControlMode()
        {
            ShowSuperscriptsOnControl(true);
            CurrentMode = KeyboardMode.AlphabeticalControl;
        }

        private void SwitchToLowerCaseAlphabeticalMode()
        {
            xNumKeyboard.Visibility = Visibility.Collapsed;
            xABCKeyboard.Visibility = Visibility.Visible;

            UnselectKey(xLShift);

            CurrentMode = KeyboardMode.LowerCaseAlphabetical;
        }



        private void SwitchToUpperCaseAlphabeticalTappedMode()
        {
            xNumKeyboard.Visibility = Visibility.Collapsed;
            xABCKeyboard.Visibility = Visibility.Visible;

            SelectKey(xLShift);

            ChangeAlphabeticalToUpper();

            CurrentMode = KeyboardMode.UpperCaseAlphabeticalTapped;

        }

        private void SwitchToUpperCaseAlphabeticalHeldMode()
        {
            xLShift.KeyColor = new SolidColorBrush(Colors.BlueViolet);

            CurrentMode = KeyboardMode.UpperCaseAlphabeticalHeld;
        }
        private void SwitchToSpecialMode()
        {
            xNumKeyboard.Visibility = Visibility.Visible;
            xABCKeyboard.Visibility = Visibility.Collapsed;

            CurrentMode = KeyboardMode.Special;
        }




        #endregion switch methods


    }
}