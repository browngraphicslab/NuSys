using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp.Components.NuSysRenderer.UI.BaseUIElements
{
    class CheckBoxUIElement : ButtonUIElement
    {
        /// <summary>
        /// True if the checkbox is selected false otherwise, use SetCheckBoxSelection() to change the selection programatically
        /// </summary>
        public bool IsSelected { get; private set; }

        /// <summary>
        /// RectangleUIElement used to display to the user that the checkbox is selected
        /// </summary>
        private RectangleUIElement _selectionIndicatorRect;

        /// <summary>
        /// The color of the rectangle indicator used to show that the checkbox is selected
        /// </summary>
        public Color SelectionIndicatorColor { get; set; }

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
        /// The margin of error for touch events
        /// </summary>
        private float _errorMargin = 10;

        public CheckBoxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, bool initialSelectionValue = false) : base(parent, resourceCreator, new RectangleUIElement(parent, resourceCreator))
        {
            // set the initial selection to the passed in value
            IsSelected = initialSelectionValue;

            // set the default ui values
            Height = 15;
            Width = 15;
            BorderWidth = 1;
            Bordercolor = Colors.Black;
            SelectedBorder = Colors.Black;
            SelectedBackground = Colors.White;
            Background = Colors.White;

            // set default selection indicator color
            SelectionIndicatorColor = Colors.Black;


            _selectionIndicatorRect = new RectangleUIElement(this, Canvas)
            {
                IsHitTestVisible = false,
                IsVisible = false
            };
            AddChild(_selectionIndicatorRect);
            
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
            SetCheckBoxSelection(!IsSelected);
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
            // set the background and size of the selectionIndicatorRect
            if (IsSelected)
            {
                _selectionIndicatorRect.Background = SelectionIndicatorColor;
                _selectionIndicatorRect.Transform.LocalPosition = new Vector2(BorderWidth + _indicatorRectMargins);
                _selectionIndicatorRect.Width = Width - 2*(BorderWidth + _indicatorRectMargins);
                _selectionIndicatorRect.Height = Height - 2*(BorderWidth + _indicatorRectMargins);
            }
            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Override the local bounds with an error margin so that touch users are comfortable
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            return new Rect(-_errorMargin, -_errorMargin, Width + 2* _errorMargin, Height + 2* _errorMargin);
        }
    }
}
