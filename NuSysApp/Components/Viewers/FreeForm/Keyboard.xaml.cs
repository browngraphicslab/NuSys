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
            CommentThisOutIfYouAreTesting();


        }

        private void CommentThisOutIfYouAreTesting()
        {
            xDraggableBar.Background = new SolidColorBrush(Color.FromArgb(255, 26, 26, 26));
            xTestingText.Visibility = Visibility.Collapsed;
            xTestingText2.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Stolen from github. Iterates through alphabet keys and switches them to lowercase letters
        /// </summary>
        private void ChangeAlphabeticalToLower()
            {
                Regex upperCaseRegex = new Regex("[A-Z]");
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
        /// <summary>
        /// Stolen from github. Iterates through alphabet keys and switches them to uppercase letters
        /// </summary>
        private void ChangeAlphabeticalToUpper()
        {
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
            //TODO: prevent keyboard from exiting viewable screen
        }

        private void Keyboard_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var compositeTransform = this.RenderTransform as CompositeTransform;
            Debug.Assert(compositeTransform != null);


            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;

        }

        private void Keyboard_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            ReturnToScreen();
        }

        private void ReturnToScreen()
        {
            var compositeTransform = this.RenderTransform as CompositeTransform;

            var x = compositeTransform.TranslateX;
            var y = compositeTransform.TranslateY;

            if (x < 0)
            {
                x = 0;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (x + Width > SessionController.Instance.ScreenWidth)
            {
                x = SessionController.Instance.ScreenWidth - Width;
            }
            if (y + Height > SessionController.Instance.ScreenHeight)
            {
                y = SessionController.Instance.ScreenHeight - Height;
            }

            compositeTransform.TranslateX = x;
            compositeTransform.TranslateY = y;


        }

        /// <summary>
        /// Makes the keyboard appear
        /// </summary>
        public void GainPseudoFocus()
        {
            UITask.Run(delegate
            {
                SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager.ClearAllPointers();
                Visibility = Visibility.Visible;
                var normalizedWidth = 920.0/1920;
                var normalizedHeight = 337.0/1080;
                Width = SessionController.Instance.ScreenWidth* normalizedWidth;
                Height = SessionController.Instance.ScreenHeight* normalizedHeight;
            });
        }
        /// <summary>
        /// Makes the keyboard disappear.
        /// </summary>
        public void LosePseudoFocus()
        {
            UITask.Run(delegate
            {
                Visibility = Visibility.Collapsed;
                SessionController.Instance.ShiftHeld = false; //Important line; otherwise, when we first type, we will type in caps
            });
        }
        /// <summary>
        /// Selects key and adds it to list of pressed keys
        /// </summary>
        /// <param name="key"></param>
        private void SelectKey(KeyboardKey key)
        {
            key.Select(); //Visually selects key
            _pressedKeys.Add(key);
            xTestingText2.Text = string.Join(", ", _pressedKeys.Select(a => a.KeyText)); //TODO: remove this line after done testing
        }
        /// <summary>
        /// Unselects key and removes it from list of pressed keys
        /// </summary>
        /// <param name="key"></param>
        private void UnselectKey(KeyboardKey key)
        {
            key.Unselect(); //Visually unselects key
            if (_pressedKeys.Contains(key))
            {
                _pressedKeys.Remove(key); 
            }
            xTestingText2.Text = string.Join(", ", _pressedKeys.Select(a => a.KeyText)); //TODO: remove this line after done testing

        }
        /// <summary>
        /// Activates key -- makes it hittest visible and visually clickable
        /// </summary>
        /// <param name="key"></param>
        private void ActivateKey(KeyboardKey key)
        {
            key.Activate();
        }
        /// <summary>
        /// Deactivates key --makes it hittest invisible and visually unclickable
        /// </summary>
        /// <param name="key"></param>
        private void DeactivateKey(KeyboardKey key)
        {
            key.Deactivate();
        }

            #endregion misc/appearence

        #region generic key handlers
        /// <summary>
        /// Selects key if pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Key_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var key = sender as KeyboardKey;
            SelectKey(key);
        }
        /// <summary>
        /// If poitner released, checks that the key was pressed and calls keyclicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Key_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var key = sender as KeyboardKey;
            //We want to make sure that the key we fire events with was originally pressed!
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
        /// <summary>
        /// Takes in a Keyboardkey's value and calls Keypressed event depending on the value
        /// </summary>
        /// <param name="command"></param>
        private void KeyClicked(string value)
        {

            VirtualKey key;
            //If a valid value passed in, handle accordingly
            if (_charsToKeys.ContainsKey(value))
            {
                //Updates shiftheld bool depending on value
                if (ShiftKeyToChars.Values.Contains(value))
                {
                    SessionController.Instance.ShiftHeld = true;
                }
                else if (NoShiftKeyToChars.Values.Contains(value))
                {
                    SessionController.Instance.ShiftHeld = false;
                }
                else if (CurrentMode.IsUpperCase())
                {
                    SessionController.Instance.ShiftHeld = true;
                }
                else
                {
                    SessionController.Instance.ShiftHeld = false;
                }
                //Gets virtual key corresponding to value and fires KeyboardKeyPressed event
                key = _charsToKeys[value];
                KeyboardKeyPressed?.Invoke(this, new KeyArgs() { Key = key, Pressed = true });

            }
            //Cancel out of uppercase tapped mode if key clicked
            if (CurrentMode == KeyboardMode.UpperCaseAlphabeticalTapped)
            {
                SwitchFromUpperCaseAlphabeticalMode();
                SwitchToLowerCaseAlphabeticalMode();
            }
            //Cancel out of alphabetical control mode if key clicked and value is not control
            else if (CurrentMode == KeyboardMode.AlphabeticalControl && value != "CONTROL")
            {
                SwitchFromAlphabeticalControlMode();
                SwitchToLowerCaseAlphabeticalMode();
            }

        }
        #endregion generic key handlers

        #region shift
        /// <summary>
        /// Called when we press shift. 
        /// If on alphabetical mode, switches to uppercasetapped mode
        /// If already in uppercase tapped mode or uppercase held mode, switch to lowercase
        /// Or if in alphabetical control mode, make sure to switch out of control mode before switching to lwoer case
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            if (CurrentMode == KeyboardMode.UpperCaseAlphabeticalTapped)
            {
                SwitchToUpperCaseAlphabeticalHeldMode();
            }

        }


        #endregion shift

        #region ctrl

        /// <summary>
        /// On pointerpressed, select control key if on alphabetical keyboard and switch to alphabetical control mode
        /// If in alphabetical control mode already, unselect and switch back to lowercase (regardless if we were first in uppercase mode)
        ///  </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            /// <summary>
            /// Unselects control key when pointer released
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void ControlKey_OnPointerReleased(object sender, PointerRoutedEventArgs e)
            {
                var key = sender as KeyboardKey;
                UnselectKey(key);
            }
            /// <summary>
            /// Unselects control key when pointer exited
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void ControlKey_OnPointerExited(object sender, PointerRoutedEventArgs e)
            {
                var key = sender as KeyboardKey;
                UnselectKey(key);
            }
            /// <summary>
            /// Changes the superscript text of the keys with special control functions 
            /// </summary>
            /// <param name="showsuperscript"></param>
            private void ShowSuperscriptsOnControl(bool showsuperscript)
            {
                if (showsuperscript)
                {
                    xAKey.SuperscriptText = "Select All";
                    xZKey.SuperscriptText = "Undo";
                    xXKey.SuperscriptText = "Cut";
                    xCKey.SuperscriptText = "Copy";
                    xVKey.SuperscriptText = "Paste";
                }
                else
                {
                    xAKey.SuperscriptText = String.Empty;
                    xZKey.SuperscriptText = String.Empty;
                    xXKey.SuperscriptText = String.Empty;
                    xCKey.SuperscriptText = String.Empty;
                    xVKey.SuperscriptText = String.Empty;
            }
        }
        #endregion ctrl

        #region other keys
        /// <summary>
        /// Close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void X_OnClick(object sender, RoutedEventArgs e)
        {
            LosePseudoFocus();
        }

        /// <summary>
        /// When the special key toggle is pressed, switch to special mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// When  the alphabetical key toggle is pressed, switch to alphabetical mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Alphabetical_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SwitchToLowerCaseAlphabeticalMode();

        }
        #endregion other keys

        #region switch methods
        /// <summary>
        /// Switches from alphabetical control mode by releasing control and updating the superscript for certain letters(a,z,x,c,v)
        /// </summary>
        private void SwitchFromAlphabeticalControlMode()
        {
            KeyboardKeyReleased?.Invoke(this, new KeyArgs
            {
                 Key = VirtualKey.Control, Pressed = true 
            });
            ShowSuperscriptsOnControl(false);
        }
        /// <summary>
        /// Switches from uppercase mode by releasing shift key and changing keys to lower
        /// </summary>
        private void SwitchFromUpperCaseAlphabeticalMode()
        {
            KeyboardKeyReleased?.Invoke(this, new KeyArgs
            {
                Key = VirtualKey.Shift,
                Pressed = true
            });
            ChangeAlphabeticalToLower();
        }
        /// <summary>
        /// Switches to specialcontrol mode
        /// </summary>
        private void SwitchToSpecialControlMode()
        {
            CurrentMode = KeyboardMode.SpecialControl;

        }
        /// <summary>
        /// Switches to alphabetical control mode by showing the superscript text control special key
        /// </summary>
        private void SwitchToAlphabeticalControlMode()
        {
            ShowSuperscriptsOnControl(true);
            CurrentMode = KeyboardMode.AlphabeticalControl;
        }
        /// <summary>
        /// Switches to lower case alphabetical mode by unselecting shift and hiding the numkeyboard
        /// </summary>
        private void SwitchToLowerCaseAlphabeticalMode()
        {
            xSpecialKeyboard.Visibility = Visibility.Collapsed;
            xABCKeyboard.Visibility = Visibility.Visible;

            UnselectKey(xLShift);

            CurrentMode = KeyboardMode.LowerCaseAlphabetical;
        }


        /// <summary>
        /// Switches to upper case alphabetical mode by select shift and hiding the numkeyboard
        /// </summary>
        private void SwitchToUpperCaseAlphabeticalTappedMode()
        {
            xSpecialKeyboard.Visibility = Visibility.Collapsed;
            xABCKeyboard.Visibility = Visibility.Visible;

            SelectKey(xLShift);

            ChangeAlphabeticalToUpper();

            CurrentMode = KeyboardMode.UpperCaseAlphabeticalTapped;

        }
        /// <summary>
        /// Switches to uppercase held mode by changing color of shift key
        /// </summary>
        private void SwitchToUpperCaseAlphabeticalHeldMode()
        {
            //TODO: get rid of the color hard code and make it a better color
            xLShift.KeyColor = new SolidColorBrush(Colors.BlueViolet); 

            CurrentMode = KeyboardMode.UpperCaseAlphabeticalHeld;
        }
        /// <summary>
        /// Switches to special keys by hiding the alphabetical keyboard
        /// </summary>
        private void SwitchToSpecialMode()
        {
            xSpecialKeyboard.Visibility = Visibility.Visible;
            xABCKeyboard.Visibility = Visibility.Collapsed;

            CurrentMode = KeyboardMode.Special;
        }

        #endregion switch methods

    }
}