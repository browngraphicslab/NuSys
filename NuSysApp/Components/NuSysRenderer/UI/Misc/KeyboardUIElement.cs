using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    /// <summary>
    /// this class will represent the on-screen keyboard
    /// </summary>
    public class KeyboardUIElement : ResizeableWindowUIElement
    {
        /// <summary>
        /// the event that will be fired whenever a key is pressed that should add a new character to the listener.
        /// It passes the character pressed.
        /// </summary>
        public event EventHandler<KeyArgs> KeyboardKeyPressed; 

        private static BiDictionary<string, VirtualKey> _charsToKeys = new BiDictionary<string, VirtualKey>()
        {
            {"a",VirtualKey.A },
            {"b",VirtualKey.B },
            {"c",VirtualKey.C },
            {"d",VirtualKey.D },
            {"e",VirtualKey.E },
            {"f",VirtualKey.F },
            {"g",VirtualKey.G },
            {"h",VirtualKey.H },
            {"i",VirtualKey.I },
            {"j",VirtualKey.J },
            {"k",VirtualKey.K },
            {"l",VirtualKey.L },
            {"m",VirtualKey.M },
            {"n",VirtualKey.N },
            {"o",VirtualKey.O },
            {"p",VirtualKey.P },
            {"q",VirtualKey.Q },
            {"r",VirtualKey.R },
            {"s",VirtualKey.S },
            {"t",VirtualKey.T },
            {"u",VirtualKey.U },
            {"v",VirtualKey.V },
            {"w",VirtualKey.W },
            {"x",VirtualKey.X },
            {"y",VirtualKey.Y },
            {"z",VirtualKey.Z },
            {"\"",(VirtualKey)222 },
            {",",(VirtualKey)188 },
            {".",(VirtualKey)190 },
            {"?",(VirtualKey)191 },
        };

        /// <summary>
        /// private dict mapping each character to the NORMALIZED coordinates of the button
        /// </summary>
        private static Dictionary<string,Vector2> _characterToLocations = new Dictionary<string, Vector2>()
        {
            {"q",new Vector2(.02f,.1f) },
            {"w",new Vector2(.02f +.08f * 1,.1f) },
            {"e",new Vector2(.02f +.08f * 2,.1f) },
            {"r",new Vector2(.02f +.08f * 3,.1f) },
            {"t",new Vector2(.02f +.08f * 4,.1f) },
            {"y",new Vector2(.02f +.08f * 5,.1f) },
            {"u",new Vector2(.02f +.08f * 6,.1f) },
            {"i",new Vector2(.02f +.08f * 7,.1f) },
            {"o",new Vector2(.02f +.08f * 8,.1f) },
            {"p",new Vector2(.02f +.08f * 9,.1f) },
            {"a",new Vector2(.08f * 1 - .0265f, .32f) },
            {"s",new Vector2(.08f * 2 - .0265f, .32f) },
            {"d",new Vector2(.08f * 3 - .0265f, .32f) },
            {"f",new Vector2(.08f * 4 - .0265f, .32f) },
            {"g",new Vector2(.08f * 5 - .0265f, .32f) },
            {"h",new Vector2(.08f * 6 - .0265f, .32f) },
            {"j",new Vector2(.08f * 7 - .0265f, .32f) },
            {"k",new Vector2(.08f * 8 - .0265f, .32f) },
            {"l",new Vector2(.08f * 9 - .0265f, .32f) },
            {"\"",new Vector2(.08f * 10 - .0265f, .32f) },
            {"z",new Vector2(-.055f +.08f * 1,.54f) },
            {"x",new Vector2(-.055f +.08f * 2,.54f) },
            {"c",new Vector2(-.055f +.08f * 3,.54f) },
            {"v",new Vector2(-.055f +.08f * 4,.54f) },
            {"b",new Vector2(-.055f +.08f * 5,.54f) },
            {"n",new Vector2(-.055f +.08f * 6,.54f) },
            {"m",new Vector2(-.055f +.08f * 7,.54f) },
            {",",new Vector2(-.055f +.08f * 8,.54f) },
            {".",new Vector2(-.055f +.08f * 9,.54f) },
            {"?",new Vector2(-.055f +.08f * 10,.54f) },

        };

        /// <summary>
        /// The enum to represent the current page
        /// </summary>
        private enum KeyboardPage
        {
            Letters,
            NonAlphabetical,
            All
        }

        /// <summary>
        /// bool representing if the shift button has been pressed
        /// </summary>
        public bool  ShiftEnabled { get; private set; } = false;

        /// <summary>
        /// lists the buttons by page so we know which ones to hide and show.
        /// This indices of this list should correspond to the KeyboardPage enum above
        /// </summary>
        private Dictionary<KeyboardPage,HashSet<CharacterButton>> _buttonsByPage;

        /// <summary>
        /// the private instance referring to the current page of the keyboard
        /// </summary>
        private KeyboardPage _currentPage;

        /// <summary>
        /// This constrcutor will create all the buttons for the keyboard
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public KeyboardUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _buttonsByPage = new Dictionary<KeyboardPage, HashSet<CharacterButton>>()
            {
                {KeyboardPage.Letters, new HashSet<CharacterButton>() },
                {KeyboardPage.NonAlphabetical, new HashSet<CharacterButton>() },
                {KeyboardPage.All, new HashSet<CharacterButton>()}
            };
            foreach (var kvp in _characterToLocations)
            {
                var button = new CharacterButton(this,resourceCreator,_charsToKeys[kvp.Key],this);
                button.Pressed += ButtonOnPressed;
                if ("qwertyuiopasdfghjklzxcvbnm?,.'".Contains(kvp.Key.ToString())) //todo have a better page-determining condition than this
                {
                    _buttonsByPage[KeyboardPage.Letters].Add(button);
                }
                else //tODO have another else if to account for more than two pages
                {
                    _buttonsByPage[KeyboardPage.NonAlphabetical].Add(button);
                    //button.IsVisible = false;
                }
                AddChild(button);
            }



            _currentPage = KeyboardPage.Letters;
        }

        /// <summary>
        /// The most important feature of this dispose is to remove the pressed handlers for all buttons
        /// </summary>
        public override void Dispose()
        {
            foreach (var kvp in _buttonsByPage)
            {
                foreach (var button in kvp.Value)
                {
                    button.Pressed -= ButtonOnPressed;
                }
            }
            base.Dispose();
        }

        /// <summary>
        /// Event handler called whenever any button is pressed on this keyboard
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ButtonOnPressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var charButton = item as CharacterButton;
            Debug.Assert(charButton != null);
            KeyboardKeyPressed?.Invoke(this, new KeyArgs() {Key = charButton.Key,Pressed = true});
            SessionController.Instance.SessionView.FreeFormViewer.CanvasInteractionManager.ClearPointer(pointer);
            item.OnReleased(pointer);
        }

        /// <summary>
        /// this update function will update the coordinates and size of every button
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {

            if (!IsVisible)
            {
                return;
            }
            foreach (var button in _buttonsByPage[_currentPage])
            {
                button.Height = Height/5;
                button.Width = Width * .07f;
                var normalVec = _characterToLocations[_charsToKeys.GetKeyByValue(button.Key)];
                button.Transform.LocalPosition = new Vector2(normalVec.X*Width,normalVec.Y*Height);
            }
            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Private button UI element class used to hold a certain character
        /// </summary>
        private class CharacterButton : RectangleButtonUIElement
        {
            /// <summary>
            /// the character for this button
            /// </summary>
            public VirtualKey Key { get; private set; }

            /// <summary>
            /// the reference to the keyboard that holds this class
            /// </summary>
            private KeyboardUIElement _keyboard;

            /// <summary>
            /// button class takes in the character for this button
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="resourceCreator"></param>
            /// <param name="character"></param>
            public CharacterButton(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, VirtualKey key, KeyboardUIElement keyboard)
                : base(parent, resourceCreator, 0, _charsToKeys.GetKeyByValue(key).ToString())
            {
                Key = key;
                _keyboard = keyboard;
            }

            /// <summary>
            /// this override will update the character on this page
            /// </summary>
            /// <param name="parentLocalToScreenTransform"></param>
            public override void Update(Matrix3x2 parentLocalToScreenTransform)
            {
                ButtonText = _keyboard.ShiftEnabled ? ButtonText.ToUpper() : ButtonText.ToLower();
                base.Update(parentLocalToScreenTransform);
            }
        }
    }
}
