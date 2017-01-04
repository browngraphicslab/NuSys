using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class PlaceHolderTextBox : ScrollableTextboxUIElement
    {
        private string _placeholderText { get; set; }


        /// <summary>
        /// The placeholder text to display on the Scrollable textbox
        /// </summary>
        public string PlaceHolderText
        {
            get { return _placeholderText; }
            set
            {
                _placeholderText = value;
                Text = _placeholderText;
            }
        }

        public PlaceHolderTextBox(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, bool scrollVert, bool showScrollBar) : base(parent, resourceCreator, scrollVert, showScrollBar)
        {
            OnFocusGained += PlaceHolderTextBox_OnFocusGained;
            OnFocusLost += PlaceHolderTextBox_OnFocusLost;
        }

        public override void Dispose()
        {
            OnFocusGained -= PlaceHolderTextBox_OnFocusGained;
            OnFocusLost -= PlaceHolderTextBox_OnFocusLost;
            base.Dispose();
        }

        private void PlaceHolderTextBox_OnFocusLost(BaseRenderItem item)
        {
            if (Text == string.Empty)
            {
                Text = PlaceHolderText;
            }
        }

        private void PlaceHolderTextBox_OnFocusGained(BaseRenderItem item)
        {
            if (Text == PlaceHolderText)
            {
                Text = string.Empty;
            }
        }
    }
}
