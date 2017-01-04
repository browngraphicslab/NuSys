using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class PlaceHolderTextBox : ScrollableTextboxUIElement
    {
        private string _placeholderText { get; set; }

        public Color PlaceHolderTextColor { get; set; } = UIDefaults.PlaceHolderTextColor;

        private Color _originalTextcolor { get; set; }


        /// <summary>
        /// The placeholder text to display on the Scrollable textbox
        /// </summary>
        public string PlaceHolderText
        {
            get { return _placeholderText; }
            set
            {
                _placeholderText = value;
                ShowPlaceHolderText();
            }
        }

        public PlaceHolderTextBox(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, bool scrollVert, bool showScrollBar) : base(parent, resourceCreator, scrollVert, showScrollBar)
        {
            OnFocusGained += PlaceHolderTextBox_OnFocusGained;
            TextChanged += PlaceHolderTextBox_TextChanged;
            OnFocusLost += PlaceHolderTextBox_OnFocusLost;
        }

        private void PlaceHolderTextBox_OnFocusLost(BaseRenderItem item)
        {
            ShowPlaceHolderText();
        }

        public override void Dispose()
        {
            OnFocusGained -= PlaceHolderTextBox_OnFocusGained;
            TextChanged -= PlaceHolderTextBox_TextChanged;

            base.Dispose();
        }

        private void PlaceHolderTextBox_TextChanged(InteractiveBaseRenderItem item, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                ShowPlaceHolderText();
            }
        }

        private void PlaceHolderTextBox_OnFocusGained(BaseRenderItem item)
        {
            HidePlaceHolderText();
        }

        private void ShowPlaceHolderText()
        {
            if (string.IsNullOrEmpty(Text))
            {
                _originalTextcolor = TextColor;
                TextChanged -= PlaceHolderTextBox_TextChanged;
                Text = PlaceHolderText;
                TextChanged += PlaceHolderTextBox_TextChanged;
                TextColor = PlaceHolderTextColor;
            }
        }

        private void HidePlaceHolderText()
        {

            if (Text == PlaceHolderText)
            {
                TextChanged -= PlaceHolderTextBox_TextChanged;
                Text = string.Empty;
                TextChanged += PlaceHolderTextBox_TextChanged;
                TextColor = _originalTextcolor;
            }
        }
    }
}
