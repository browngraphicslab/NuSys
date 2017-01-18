using System;
using System.Collections.Generic;
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
    public class CheckBoxUIElement : RectangleUIElement
    {

        /// <summary>
        /// The position of the label relative to the checkbox
        /// </summary>
        public enum CheckBoxLabelPosition
        {
            Right,
            Left
        }

        /// <summary>
        /// The position of the label relative to the checkbox, the default is right
        /// </summary>
        public CheckBoxLabelPosition LabelPosition { get; set; }

        /// <summary>
        /// True if the checkbox is selected false otherwise, use SetCheckBoxSelection() to change the selection programatically
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        /// RectangleUIElement used to display to the user that the checkbox is selected
        /// </summary>
        private RectangleUIElement _selectionIndicatorRect;

        /// <summary>
        /// the width of the checkbox, must be less than or equal to the width of the checkboxuielement plus the spacing between
        /// the checkbox and the label
        /// </summary>
        public float CheckBoxWidth
        {
            get { return _checkBoxButton.Width; }
            set { _checkBoxButton.Width = Math.Min(Width - SpaceBetweenCheckboxAndLabel, value); }
        }

        /// <summary>
        /// the space between the checkbox and the label in pixel coordinates
        /// </summary>
        public float SpaceBetweenCheckboxAndLabel { get; set; }

        /// <summary>
        /// the height of the checkbox, must be less than or equal to the overall height of the checkboxuielement
        /// </summary>
        public float CheckBoxHeight
        {
            get { return _checkBoxButton.Height; }
            set { _checkBoxButton.Height = Math.Min(Height, value); }
        }

        /// <summary>
        /// The color of the rectangle indicator used to show that the checkbox is selected
        /// </summary>
        public Color SelectionIndicatorColor
        {
            get { return _selectionIndicatorRect.Background; }
            set { _selectionIndicatorRect.Background = value; }
        }

        /// <summary>
        /// The color of the border of the checkbox
        /// </summary>
        public Color CheckBoxBorderColor
        {
            get { return _checkBoxButton.BorderColor; }
            set
            {
                _checkBoxButton.BorderColor = value;
                _checkBoxButton.SelectedBorder = value; // we don't want the border to flash on tapped
            }
        }

        /// <summary>
        /// The color of the background of the checkbox
        /// </summary>
        public Color CheckBoxBackground
        {
            get { return _checkBoxButton.Background; }
            set
            {
                _checkBoxButton.Background = value;
                _checkBoxButton.SelectedBackground = value; // we don't want the background to flash on tapped
            }
        }

        /// <summary>
        /// helper delegate for the on selection changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="SelectionValue"></param>
        public delegate void OnSelectionChangedHandler(CheckBoxUIElement sender, bool SelectionValue);

        /// <summary>
        /// Event called whenever the selection is changed, if you programmtically change the selection
        /// but the selection is already set to that value, this event will not be fired
        /// </summary>
        public event OnSelectionChangedHandler Selected;

        /// <summary>
        /// The margin between the edge of the CheckboxUIelement and the indicator rect
        /// </summary>
        private float _indicatorRectMargins = 2;

        /// <summary>
        /// The margin of error for touch events, affects the localbounds in every direction from the checkbox itself
        /// </summary>
        public float ErrorMargin { get; set; }

        /// <summary>
        /// The actual checkbox itself
        /// </summary>
        private ButtonUIElement _checkBoxButton;

        /// <summary>
        /// the label associated with the checkbox
        /// </summary>
        private TextboxUIElement _labelElement;

        /// <summary>
        /// The sign used to show that the text has overflow the end of the label
        /// </summary>
        public CanvasTrimmingSign LabelTrimmingSign
        {
            get { return _labelElement.TrimmingSign; }
            set { _labelElement.TrimmingSign = value; }
        }

        /// <summary>
        /// specifies at what granularity the trimming sign will be used upon text overflow
        /// </summary>
        public CanvasTextTrimmingGranularity LabelTrimmingGranularity
        {
            get { return _labelElement.TrimmingGranularity; }
            set { _labelElement.TrimmingGranularity = value; }
        }

        /// <summary>
        /// the vertical alignment of the text within the label
        /// </summary>
        public CanvasVerticalAlignment LabelTextVerticalAlignment
        {
            get { return _labelElement.TextVerticalAlignment; }
            set { _labelElement.TextVerticalAlignment = value; }
        }

        /// <summary>
        /// the horizontal alignment of the text within the label
        /// </summary>
        public CanvasHorizontalAlignment LabelTextHorizontalAlignment
        {
            get { return _labelElement.TextHorizontalAlignment; }
            set { _labelElement.TextHorizontalAlignment = value; }
        }

        /// <summary>
        /// The color of the text on the label
        /// </summary>
        public Color LabelTextColor
        {
            get { return _labelElement.TextColor; }
            set { _labelElement.TextColor = value; }
        }

        /// <summary>
        /// the actual text displayed on the label
        /// </summary>
        public string LabelText
        {
            get { return _labelElement.Text; }
            set { _labelElement.Text = value; }
        }

        /// <summary>
        /// the fontsize of the text displayed on the label
        /// </summary>
        public float LabelFontSize
        {
            get { return _labelElement.FontSize; }
            set { _labelElement.FontSize = value; }
        }

        /// <summary>
        /// the fontstyle used for the text displayed on the label, normal, oblique, and italic. note that oblique is not bold
        /// </summary>
        public FontStyle LabelFontStyle
        {
            get { return _labelElement.FontStyle; }
            set { _labelElement.FontStyle = value; }
        }

        /// <summary>
        /// the font family used for the text 
        /// </summary>
        public string LabelFontFamily
        {
            get { return _labelElement.FontFamily; }
            set { _labelElement.FontFamily = value; }
        }

        /// <summary>
        /// the background color of the label
        /// </summary>
        public Color LabelBackground
        {
            get { return _labelElement.Background; }
            set { _labelElement.Background = value; }
        }

        /// <summary>
        /// True if we want the checkbox to only be changed programmatically
        /// </summary>
        public bool DisableSelectionOnTap { get; set; }

        public CheckBoxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, bool initialSelectionValue = false) : base(parent, resourceCreator)
        {
            // set the initial selection to the passed in value
            IsSelected = initialSelectionValue;

            // set some default ui values, others are below
            ErrorMargin = 10;
            Height = UIDefaults.CheckBoxUIElementHeight;
            Width = UIDefaults.CheckBoxUIElementWidth;
            LabelPosition = UIDefaults.CheckBoxLabelPosition;

            // add the checkbox - OK to make ButtonUIElement here since this is a custom button
            _checkBoxButton = new ButtonUIElement(this, ResourceCreator, new RectangleUIElement(this, ResourceCreator))
            {
                BorderWidth = 1,
                IsHitTestVisible = false,
            };
            AddChild(_checkBoxButton);

            // set the default ui values for the new checkbox, do this after initializing checkbox because properties change checkbox values
            CheckBoxHeight = UIDefaults.CheckBoxHeight;
            SpaceBetweenCheckboxAndLabel = 10; // set this before checkboxwidth since the checkboxWidth uses this in its calculations
            CheckBoxWidth = UIDefaults.CheckBoxWidth;
            CheckBoxBorderColor = Constants.ALMOST_BLACK;
            CheckBoxBackground = Colors.White;

            // we'll just accept all the default TextBoxUIElementValuesforNow
            _labelElement = new TextboxUIElement(this, ResourceCreator)
            {
                IsHitTestVisible = false,
            };
            LabelTextHorizontalAlignment = UIDefaults.CheckBoxLabelTextHorizontalAlignmentAlignment;
            AddChild(_labelElement);


            // add the selection indicator rect
            _selectionIndicatorRect = new RectangleUIElement(this, Canvas)
            {
                IsHitTestVisible = false,
                IsVisible = false
            };
            AddChild(_selectionIndicatorRect);
            
            // set the default selection indicator color, do this after initializing the _selectionIndicatorRect because property changes rect values
            SelectionIndicatorColor = Constants.ALMOST_BLACK;


            // add the proper events
            Tapped += SetCheckboxSelectionOnTapped;

        }

        public override void Dispose()
        {
            Tapped -= SetCheckboxSelectionOnTapped;

            base.Dispose();
        }

        /// <summary>
        /// Fired when the checkbox is tapped, sets the textbox selection based on the current IsSelected value
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void SetCheckboxSelectionOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (!DisableSelectionOnTap)
            {
                SetCheckBoxSelection(!IsSelected);
            }
        }

        /// <summary>
        /// Set the selection of the checkbox to the newSelection value, does nothing if the checkbox is already set for that value
        /// </summary>
        /// <param name="newSelection"></param>
        public void SetCheckBoxSelection(bool newSelection)
        {
            // return if we already have that value set
            if (newSelection == IsSelected)
            {
                return;
            }

            // set is selected to the new value
            IsSelected = newSelection;

            // set the indicator rect visibility based on the value of IsSelected
            _selectionIndicatorRect.IsVisible = IsSelected;

            // fire the method to tell the user that the value has changed
            Selected?.Invoke(this, IsSelected);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {

            // set the default border color and selected border color of the checkbox button
            _checkBoxButton.BorderColor = CheckBoxBorderColor;
            _checkBoxButton.SelectedBorder = CheckBoxBorderColor;

            // set the background and size and location of the selectionIndicatorRect
            if (IsSelected)
            {
                _selectionIndicatorRect.Background = SelectionIndicatorColor;
                _selectionIndicatorRect.Transform.LocalPosition = _checkBoxButton.Transform.LocalPosition + new Vector2(BorderWidth + _indicatorRectMargins);
                _selectionIndicatorRect.Width = _checkBoxButton.Width - 2*_indicatorRectMargins;
                _selectionIndicatorRect.Height = _checkBoxButton.Height - 2 * _indicatorRectMargins;
            }


            // set the height and width of the Label
            _labelElement.Width = Width - CheckBoxWidth - SpaceBetweenCheckboxAndLabel - 2 * BorderWidth;
            _labelElement.Height = Height - 2 * BorderWidth;
            _labelElement.BorderWidth = 0;
            _labelElement.TextHorizontalAlignment = CanvasHorizontalAlignment.Center;

            // position the checkbox and label based on the LabelPosition
            switch (LabelPosition)
            {
                case CheckBoxLabelPosition.Right:
                    // center the _checkBoxButton Vertically
                    _checkBoxButton.Transform.LocalPosition = new Vector2(BorderWidth, Height/2 - _checkBoxButton.Height/2);
                    // put the label next to the checkbox
                    _labelElement.Transform.LocalPosition = new Vector2(BorderWidth + CheckBoxWidth + SpaceBetweenCheckboxAndLabel, BorderWidth);
                    break;
                case CheckBoxLabelPosition.Left:
                    // center the _checkBoxButton Vertically
                    _checkBoxButton.Transform.LocalPosition = new Vector2(Width-BorderWidth-CheckBoxWidth, Height / 2 - _checkBoxButton.Height / 2);
                    // put the label next to the checkbox
                    _labelElement.Transform.LocalPosition = new Vector2(BorderWidth);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Background = Colors.Transparent;

            base.Update(parentLocalToScreenTransform);
        }
    }
}
