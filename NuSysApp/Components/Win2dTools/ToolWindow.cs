using System;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    public abstract class ToolWindow : ResizeableWindowUIElement
    {
        /// <summary>
        /// The delete to delete the tool
        /// </summary>
        private ButtonUIElement _deleteButton;

        /// <summary>
        /// The button to refresh the tool
        /// </summary>
        private ButtonUIElement _refreshButton;

        /// <summary>
        /// This functions as a simple list that is hidden or shown depending on whether 
        /// _filterChooserDropdownButton is clicked effectively creating a dropdown list
        /// </summary>
        private DropdownUIElement _filterChooser;

        /// <summary>
        /// The button for showing the dropdown list for choosing filter type
        /// </summary>
        private ButtonUIElement _filterChooserDropdownButton;

        /// <summary>
        /// The rectangle UI element that can be dragged to create a new collection
        /// </summary>
        private RectangleUIElement _draggableCollectionElement;

        /// <summary>
        /// The rectangle UI element that can be dragged to create a stack
        /// </summary>
        private RectangleUIElement _draggableStackElement;

        /// <summary>
        /// The height of the rows and button for the filter chooser dropdown menu
        /// </summary>
        protected const int FILTER_CHOOSER_HEIGHT = 60;

        /// <summary>
        ///Height of the button bar at the bottom of the tool
        /// </summary>
        private const int BUTTON_BAR_HEIGHT = 60;

        /// <summary>
        /// The rectangle at the bottom of the tool window
        /// </summary>
        protected RectangleUIElement ButtonBarRectangle;

        private const int BUTTON_MARGIN = 10;
        public ToolWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            SetUpButtons();
            SetUpFilterDropDown();
            SetUpDraggableIcons();
            SetUpBottomButtonBar();
        }

        private void SetUpBottomButtonBar()
        {
            ButtonBarRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Azure,
                Height = BUTTON_BAR_HEIGHT,
                Width = this.Width
            };
            ButtonBarRectangle.Transform.LocalPosition = new Vector2(0, this.Height - BUTTON_BAR_HEIGHT);
            AddChild(ButtonBarRectangle);
        }

        /// <summary>
        /// This sets up the collection and stack draggable elements 
        /// </summary>
        private void SetUpDraggableIcons()
        {
            _draggableCollectionElement = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Green,
                Height = 50,
                Width = 50,
            };
            _draggableCollectionElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN + _draggableCollectionElement.Width / 2), BUTTON_MARGIN);
            AddChild(_draggableCollectionElement);

            _draggableStackElement = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Green,
                Height = 50,
                Width = 50,
            };
            _draggableStackElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN + _draggableStackElement.Width / 2), _draggableCollectionElement.Height  + BUTTON_MARGIN);
            AddChild(_draggableStackElement);

        }

        /// <summary>
        /// Sets up the button and the basic list so that they work together like a dropdown list
        /// </summary>
        private void SetUpFilterDropDown()
        {
            _filterChooserDropdownButton = new ButtonUIElement(this, ResourceCreator, new RectangleUIElement(this, ResourceCreator));
            _filterChooserDropdownButton.ButtonText = "fdsafd";
            _filterChooserDropdownButton.Width = Width;
            _filterChooserDropdownButton.Height = FILTER_CHOOSER_HEIGHT;
            _filterChooserDropdownButton.ButtonTextColor = Colors.Black;
            _filterChooserDropdownButton.BorderWidth = 2;
            _filterChooserDropdownButton.Bordercolor = Colors.Black;
            _filterChooserDropdownButton.ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            _filterChooserDropdownButton.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            _filterChooserDropdownButton.Tapped += _dropdownButton_OnPressed; ;
            _filterChooserDropdownButton.Transform.LocalPosition = new Vector2(0, TopBarHeight);
            AddChild(_filterChooserDropdownButton);
            _filterChooser = new DropdownUIElement(this, ResourceCreator, Width);
            _filterChooser.ButtonHeight = FILTER_CHOOSER_HEIGHT;
            foreach (var filterType in Enum.GetValues(typeof(ToolModel.ToolFilterTypeTitle)).Cast<ToolModel.ToolFilterTypeTitle>())
            {
                _filterChooser.AddOption(filterType.ToString(), FilterChooserItem_Clicked);
            }
            _filterChooser.Layout();

            _filterChooser.IsVisible = false;
            _filterChooser.Transform.LocalPosition = new Vector2(0, TopBarHeight + _filterChooserDropdownButton.Height);

            AddChild(_filterChooser);
        }


        public override void Dispose()
        {
            base.Dispose();
            _filterChooserDropdownButton.Tapped -= _dropdownButton_OnPressed;
        }

        /// <summary>
        /// When the dropdown button is pressed either show or hide the dropdown list filter chooser
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _dropdownButton_OnPressed(ButtonUIElement item, CanvasPointer pointer)
        {
            if (_filterChooser.IsVisible)
            {
                _filterChooser.IsVisible = false;
            }
            else
            {
                _filterChooser.IsVisible = true;
            }
        }

        /// <summary>
        /// The handler for when a filter is chosen
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void FilterChooserItem_Clicked(ButtonUIElement item, CanvasPointer pointer)
        {
            _filterChooser.IsVisible = false;
            _filterChooserDropdownButton.ButtonText = item.ButtonText;
        }

        /// <summary>
        /// Initializes the delete and refresh buttons
        /// </summary>
        private void SetUpButtons()
        {
            var deleteCircleShape = new EllipseUIElement(this, ResourceCreator)
            {
                Background = Colors.Red,
                Width = 50,
                Height = 50,
            };
            _deleteButton = new ButtonUIElement(this, ResourceCreator, deleteCircleShape);
            _deleteButton.Transform.LocalPosition = new Vector2(-(BUTTON_MARGIN + _deleteButton.Width / 2), _deleteButton.Height / 2 + BUTTON_MARGIN);
            AddChild(_deleteButton);

            var refreshCircleShape = new EllipseUIElement(this, ResourceCreator)
            {
                Background = Colors.Blue,
                Width = 50,
                Height = 50,
            };
            _refreshButton = new ButtonUIElement(this, ResourceCreator, refreshCircleShape);
            _refreshButton.Transform.LocalPosition = new Vector2(-(BUTTON_MARGIN + _deleteButton.Width / 2), _deleteButton.Transform.LocalY + _deleteButton.Height + BUTTON_MARGIN);
            AddChild(_refreshButton);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);
            
            //Make the width of the filter chooser and the button always fill the window
            if (_filterChooser.Width != Width)
            {
                _filterChooser.Width = Width;
                _filterChooser.Layout();
            }
            _filterChooserDropdownButton.Width = Width;

            //Set up draggable collection and stack elements local position.
            _draggableCollectionElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN), _draggableCollectionElement.Height / 2 + BUTTON_MARGIN);
            _draggableStackElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN), _draggableCollectionElement.Transform.LocalY + _draggableCollectionElement.Height + BUTTON_MARGIN);

            //Set up button bar at the bottom of tool
            ButtonBarRectangle.Transform.LocalY = this.Height - BUTTON_BAR_HEIGHT;
            ButtonBarRectangle.Width = Width;
        }
    }
}