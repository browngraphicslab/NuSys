using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class TextboxUIElement : RectangleUIElement
    {
        public delegate void TapEventHandler(TextboxUIElement sender, Vector2 position);
        public event TapEventHandler Tapped;
        public event TapEventHandler DoubleTapped;

        /// <summary>
        /// private helper for the public property TextHorizontalAlignment
        /// </summary>
        private CanvasHorizontalAlignment _textHorizontalAlignment { get; set; }

        /// <summary>
        /// private helper for the public property TextVerticalAlignment
        /// </summary>
        private CanvasVerticalAlignment _textVerticalAlignment { get; set; }

        /// <summary>
        /// private helper for public property WordWrapping
        /// </summary>
        private CanvasWordWrapping _wordWrapping { get; set; }

        /// <summary>
        /// private helper for public property TrimmingGranularity
        /// </summary>
        private CanvasTextTrimmingGranularity _trimmingGranularity { get; set; }

        /// <summary>
        /// private helper for public property TrimmingSign
        /// </summary>
        private CanvasTrimmingSign _trimmingSign { get; set; }

        /// <summary>
        /// private helper for public property FontFamily
        /// </summary>
        private string _fontFamily { get; set; }

        /// <summary>
        /// private helper for public property FontSize
        /// </summary>
        private float _fontSize { get; set; }

        /// <summary>
        /// private helper for public property FontStyle
        /// </summary>
        private FontStyle _fontStyle { get; set; }

        /// <summary>
        /// private helper for public property FontWeight
        /// </summary>
        private FontWeight _fontWeight { get; set; }


        /// <summary>
        /// The text to be displayed in the textbox
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// The horizontal alignment of the text within the textbox
        /// </summary>
        public CanvasHorizontalAlignment TextHorizontalAlignment
        {
            get { return _textHorizontalAlignment; }
            set
            {
                _textHorizontalAlignment = value;
                UpdateCanvasTextFormat();
            }
        }

        /// <summary>
        /// The vertical alignment of the text within the textbox
        /// </summary>
        public CanvasVerticalAlignment TextVerticalAlignment
        {
            get { return _textVerticalAlignment;}
            set
            {
                _textVerticalAlignment = value;
                UpdateCanvasTextFormat();
            }
        }

        /// <summary>
        /// The color of the text within the text box
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// The style of the text within the text box, normal or italic. oblique is not bold. 
        /// </summary>
        public FontStyle FontStyle
        {
            get { return _fontStyle; }
            set
            {
                _fontStyle = value;
                UpdateCanvasTextFormat();
            }
        }

        /// <summary>
        /// The font weight class set this using FontWeights.Bold or something along those lines
        /// </summary>
        public FontWeight FontWeight
        {
            get { return _fontWeight; }
            set
            {
                _fontWeight = value;
                UpdateCanvasTextFormat();
            }
        }

        /// <summary>
        /// The size of the text in the textbox. 
        /// </summary>
        public float FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                UpdateCanvasTextFormat();
            }
        }

        /// <summary>
        /// The font of the text in the textbox
        /// </summary>
        public string FontFamily
        {
            get { return _fontFamily; }
            set
            {
                _fontFamily = value;
                UpdateCanvasTextFormat();
            }
        }

        /// <summary>
        /// The default break point at which text moves to a new line to avoid overflow
        /// </summary>
        public CanvasWordWrapping Wrapping
        {
            get { return _wordWrapping; }
            set
            {
                _wordWrapping = value;
                UpdateCanvasTextFormat();
            }
        }

        /// <summary>
        /// The sign used to show that text has overflown the end of the text box
        /// </summary>
        public CanvasTrimmingSign TrimmingSign
        {
            get { return _trimmingSign; }
            set
            {
                _trimmingSign = value;
                UpdateCanvasTextFormat();
            }
        }

        /// <summary>
        /// The granularity chosen to break off the end of text if text has overflown the end of the text box
        /// </summary>
        public CanvasTextTrimmingGranularity TrimmingGranularity
        {
            get { return _trimmingGranularity; }
            set
            {
                _trimmingGranularity = value;
                UpdateCanvasTextFormat();
            }
        }

        public virtual CanvasTextFormat CanvasTextFormat { get; protected set; }

        private bool _constructed;

        public TextboxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set default values
            TextHorizontalAlignment = UIDefaults.TextHorizontalAlignment;
            TextVerticalAlignment = UIDefaults.TextVerticalAlignment;
            TextColor = UIDefaults.TextColor;
            FontStyle = UIDefaults.FontStyle;
            FontSize = UIDefaults.FontSize;
            FontFamily = UIDefaults.TextFont;
            Wrapping = UIDefaults.Wrapping;
            TrimmingSign = UIDefaults.TrimmingSign;
            TrimmingGranularity = UIDefaults.TrimmingGranularity;
            FontWeight = UIDefaults.FontWeight;
            Background = Colors.Transparent;
            BorderWidth = 0;
            Text = "";
            _constructed = true;

            UpdateCanvasTextFormat();

            var tapRecognizer = new TapGestureRecognizer();
            this.GestureRecognizers.Add(tapRecognizer);
            tapRecognizer.OnTapped += TapRecognizer_OnTapped;
        }

        private void TapRecognizer_OnTapped(TapGestureRecognizer sender, TapEventArgs args)
        {
            if (args.TapType == TapEventArgs.Tap.SingleTap)
            {
                Tapped?.Invoke(this, args.Position);
            }
            else if (args.TapType == TapEventArgs.Tap.DoubleTap)
            {
                DoubleTapped?.Invoke(this, args.Position);
            }
        }

        protected virtual void UpdateCanvasTextFormat()
        {
            if (!_constructed)
            {
                return;
            }

            CanvasTextFormat = new CanvasTextFormat
            {
                HorizontalAlignment = TextHorizontalAlignment,
                VerticalAlignment = TextVerticalAlignment,
                WordWrapping = Wrapping,
                TrimmingGranularity = TrimmingGranularity,
                TrimmingSign = TrimmingSign,
                FontFamily = FontFamily,
                FontSize = FontSize * (float)SessionController.Instance.SessionSettings.TextScale,
                FontStyle = FontStyle,
                FontWeight = FontWeight                
            };


        }

        /// <summary>
        /// Draws the text within the textbox
        /// </summary>
        /// <param name="ds"></param>
        protected override void DrawText(CanvasDrawingSession ds)
        {
            // save the current transform of the drawing session
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            if (Text != null)
            {
                var validSize = Width - 2*(BorderWidth + UIDefaults.XTextPadding) >= 0 &&
                                Height - 2*(BorderWidth + UIDefaults.YTextPadding) >= 0;

                Debug.Assert(validSize, "these must be greater than zero or drawText crashes below");

                if (validSize)
                {
                    // update the font size based on the accessibility settings
                    CanvasTextFormat.FontSize = FontSize * (float)SessionController.Instance.SessionSettings.TextScale;

                    // draw the text within the proper bounds
                    var x = BorderWidth + UIDefaults.XTextPadding;
                    var y = BorderWidth + UIDefaults.YTextPadding;
                    var width = Width - 2 * (BorderWidth + UIDefaults.XTextPadding);
                    var height = Height - 2 * (BorderWidth + UIDefaults.YTextPadding);
                    ds.DrawText(Text, new Rect(x, y, width, height), TextColor, CanvasTextFormat);
                }
            }
            ds.Transform = orgTransform;
        }
    }
}
