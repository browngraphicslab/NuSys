using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class PivotContainer : RectangleUIElement
    {

        #region private variables
        /// <summary>
        /// Each pivot is associated with a button, these are those buttons
        /// </summary>
        private List<ButtonUIElement> _pivotButtons;

        /// <summary>
        /// Mapping of buttons to specific pivots
        /// </summary>
        private Dictionary<ButtonUIElement, Pivot> _buttonToPivot;

        /// <summary>
        /// The pivot that is currently being displayed
        /// </summary>
        private Pivot _currentlyDisplayedPivot;

        /// <summary>
        /// The display rect used to display the RectangleUIElement associated with the pivot
        /// </summary>
        private RectangleUIElement _displayRect;
        #endregion private variables

        #region events
        /// <summary>
        /// delegate for use with the pivot changed event
        /// </summary>
        /// <param name="pivot"></param>
        public delegate void OnPivotChangedHandler(Pivot pivot);

        /// <summary>
        /// Fired whenever the current pivot that is being displayed is changed, 
        /// </summary>
        public event OnPivotChangedHandler PivotChanged;

        #endregion events

        public PivotContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _pivotButtons = new List<ButtonUIElement>();
            _buttonToPivot = new Dictionary<ButtonUIElement, Pivot>();
        }

        /// <summary>
        /// Add a new pivot to the pivot container
        /// </summary>
        /// <param name="pivot"></param>
        public void AddPivot(Pivot pivot)
        {
            // initailize a new button for the pivot
            var newButton = InitializeNewButton(pivot);

            // add the button to the list of pivot buttons
            _pivotButtons.Add(newButton);

            // add the button and pivot to the mapping of buttons to pivots
            _buttonToPivot.Add(newButton, pivot);

            // add the button as a child of the element
            AddChild(newButton);
        }

        /// <summary>
        /// Adds a range of pivots to the pivot container
        /// </summary>
        /// <param name="pivots"></param>
        public void AddPivotRange(IEnumerable<Pivot> pivots)
        {
            foreach (var pivot in pivots)
            {
                AddPivot(pivot);
            }
        }

        /// <summary>
        /// Initializes a new button UI, but does not add it as a child, it does add all the proper events and sets the colors
        /// does not add it to lists or dictionaries
        /// </summary>
        /// <param name="pivot"></param>
        /// <returns></returns>
        private ButtonUIElement InitializeNewButton(Pivot pivot)
        {
            // create the button and initialize its ui
            var newButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas));
            newButton.ButtonText = pivot.Title;

            // add the events to the button
            AddPivotButtonEvents(newButton);

            return newButton;
        }

        /// <summary>
        /// Add all the button events for a pivot button
        /// </summary>
        /// <param name="pivotButton"></param>
        private void AddPivotButtonEvents(ButtonUIElement pivotButton)
        {
            pivotButton.Tapped += OnPivotButtonTapped;
        }

        /// <summary>
        /// Remove all the button events from a pivot button
        /// </summary>
        /// <param name="pivotButton"></param>
        private void RemovePivotButtonEvents(ButtonUIElement pivotButton)
        {
            pivotButton.Tapped -= OnPivotButtonTapped;
        }

        /// <summary>
        /// Called when a pivot button is tapped, if the pivot button is associated with a pivot that is not currently being displayed
        /// then it displays that pivot, otherwise it simply returns and no side effects occur
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnPivotButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var button = item as ButtonUIElement;
            Debug.Assert(button != null);

            Debug.Assert(_buttonToPivot.ContainsKey(button));
            var pivot = _buttonToPivot[button];

            if (_currentlyDisplayedPivot == pivot)
            {
                return; // we don't care if the pivot is already being displayed
            }

            DisplayPivot(pivot);
        }

        /// <summary>
        /// Displays the passed in pivot on the pivot container, fires the proper events
        /// </summary>
        /// <param name="pivot"></param>
        private void DisplayPivot(Pivot pivot)
        {
            if (_displayRect != null)
            {
                RemoveChild(_displayRect);
            }
            _displayRect = pivot.DisplayObject;

            PivotChanged?.Invoke(pivot);
        }

    }
}
