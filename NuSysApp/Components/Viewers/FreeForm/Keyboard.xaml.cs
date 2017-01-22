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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class Keyboard : UserControl
    {

        private VirtualKey _key;



        /// <summary>
        /// the event that will be fired whenever a key is pressed that should add a new character to the listener.
        /// It passes the character pressed.
        /// </summary>
        public event EventHandler<KeyArgs> KeyboardKeyPressed;


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
            {"-", VirtualKey.Subtract},
            {"+", VirtualKey.Add},
            {"/", (VirtualKey) 191},
            {"=", VirtualKey.Add},
            {"'", (VirtualKey) 222},
            {"DOUBLEAPOSTOPHRE", (VirtualKey) 222},
            {":", (VirtualKey) 186},
            {";", (VirtualKey) 186},
            {"\\", (VirtualKey) 220},
             {"[", (VirtualKey) 219},
            {"]", (VirtualKey) 221},









        };

        public static BiDictionary<VirtualKey, string> NoShiftKeyToChars = new BiDictionary<VirtualKey, string>()
        {
            {(VirtualKey) 191, "/"},
            {VirtualKey.Subtract, "-"},
            {VirtualKey.Add, "+"},
            {(VirtualKey) 188,","},
            {(VirtualKey) 190,"." },
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
            {(VirtualKey)191, "?"},
            {(VirtualKey) 220, "|"},
            {(VirtualKey) 186, ":"},
            {(VirtualKey) 188,"<"},
            {(VirtualKey) 190,">" },
            {(VirtualKey) 219, "{"},
            {(VirtualKey) 221, "}"},


        };

        private bool _showNumericKeyboard;

        public bool ShowNumericKeyboard
        {
            get { return _showNumericKeyboard; }
            set { _showNumericKeyboard = value; }
        }


        private bool _showCapitalAlphabeticalKeyboard;

        public bool ShowCapitalAlphabeticalKeyboard
        {
            get { return _showCapitalAlphabeticalKeyboard; }
            set { _showCapitalAlphabeticalKeyboard = value; }
        }

        public Keyboard()
        {
            this.InitializeComponent();

            this.RenderTransform = new CompositeTransform();

            ShowNumericKeyboard = false;
            ShowCapitalAlphabeticalKeyboard = false;

        }

        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                KeyClicked(button.CommandParameter.ToString());
            }

        }

        private void FunctionButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                switch (button.CommandParameter.ToString())
                {
                    case "LSHIFT":
                        ChangeAlphabeticalCase(button.Content?.ToString());
                        break;

                    case "ALT":
                    case "CTRL":
                        break;

                    case "RETURN":
                        break;

                    case "BACK":
                        break;

                    default:
                        break;
                }
            }
        }

    private void ChangeAlphabeticalCase(string buttonString)
        {
            Regex upperCaseRegex = new Regex("[A-Z]");
            Regex lowerCaseRegex = new Regex("[a-z]");
            Button btn;
            foreach (UIElement elem in xABCKeyboard.Children) //iterate the main grid
            {
                Grid grid = elem as Grid;
                if (grid != null)
                {
                    foreach (UIElement uiElement in grid.Children)  //iterate the single rows
                    {
                        btn = uiElement as Button;
                        if (btn != null) // if button contains only 1 character
                        {
                            if (btn.Content.ToString().Length == 1)
                            {
                                if (upperCaseRegex.Match(btn.Content.ToString()).Success) // if the char is a letter and uppercase
                                    btn.Content = btn.Content.ToString().ToLower();
                                else if (lowerCaseRegex.Match(buttonString).Success) // if the char is a letter and lower case
                                    btn.Content = btn.Content.ToString().ToUpper();
                            }

                        }
                    }
                }
            }

            ShowCapitalAlphabeticalKeyboard = !ShowCapitalAlphabeticalKeyboard;

        }

        private void KeyClicked(string command)
        {
            
            if (command == null)
            {
                Debug.Fail("Woops");
            }
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
                    SessionController.Instance.ShiftHeld = ShowCapitalAlphabeticalKeyboard;
                }


                key = _charsToKeys[command];

                
                KeyboardKeyPressed?.Invoke(this, new KeyArgs() { Key = key, Pressed = true });

            }
        }



        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShowNumericKeyboard)
            {
                xNumKeyboard.Visibility = Visibility.Collapsed;
                xABCKeyboard.Visibility = Visibility.Visible;

                x123Toggle1.Visibility = Visibility.Visible;
                x123Toggle2.Visibility = Visibility.Visible;

                x123Toggle1.IsChecked = false;
                x123Toggle2.IsChecked = false;

                xabcToggle1.Visibility = Visibility.Collapsed;
                xabcToggle2.Visibility = Visibility.Collapsed;

                xabcToggle1.IsChecked = true;
                xabcToggle2.IsChecked = true;

            }
            else
            {
                xNumKeyboard.Visibility = Visibility.Visible;
                xABCKeyboard.Visibility = Visibility.Collapsed;
                x123Toggle1.Visibility = Visibility.Collapsed;
                x123Toggle2.Visibility = Visibility.Collapsed;
                xabcToggle1.Visibility = Visibility.Visible;
                xabcToggle2.Visibility = Visibility.Visible;

                x123Toggle1.IsChecked = true;
                x123Toggle2.IsChecked = true;


                xabcToggle1.IsChecked = false;
                xabcToggle2.IsChecked = false;
            }

            ShowNumericKeyboard = !ShowNumericKeyboard;


        }


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

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {

        }

        public void LosePseudoFocus()
        {
            Visibility = Visibility.Collapsed;
            SessionController.Instance.ShiftHeld = false;
        }

        public void GainPseudoFocus()
        {
            SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager.ClearAllPointers();
            Visibility = Visibility.Visible;
        }

        private void Button_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                KeyClicked(button.CommandParameter.ToString());
            }
        }

        private void Button_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                KeyClicked(button.CommandParameter.ToString());
            }
        }

        private void XLShift_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            ChangeAlphabeticalCase(button.Content?.ToString());
            
            SessionController.Instance.ShiftHeld = ShowCapitalAlphabeticalKeyboard;
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
