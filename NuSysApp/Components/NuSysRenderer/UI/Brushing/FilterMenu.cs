using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    public class FilterMenu : ResizeableWindowUIElement
    {
        /// <summary>
        /// Dictionary that transforms filter categories into label strings for the buttons
        /// </summary>
        private static BiDictionary<FilterCategory, string> _filterToStringDict = new BiDictionary<FilterCategory, string>
        {
            {FilterCategory.Creator, "Creator"},
            {FilterCategory.CreationDate, "Creation Date"},
            {FilterCategory.LastEditedDate, "Last Edited Date"},
            {FilterCategory.Type, "Type"},

        };

        /// <summary>
        /// dictionary to convert buttons to filter categories
        /// </summary>
        private Dictionary<ButtonUIElement, FilterCategory> _buttonToFilterCategories;

        /// <summary>
        /// enum of filter categories used throughout this filtering sub menu as a type switch
        /// </summary>
        public enum FilterCategory
        {
            Creator,
            CreationDate,
            LastEditedDate,
            Type
        }

        /// <summary>
        /// Filter by creator button used to open the filter by creator menu
        /// </summary>
        private ButtonUIElement _filterByCreatorButton;

        /// <summary>
        /// filter by creation date button used to open the filter by creation date menu
        /// </summary>
        private ButtonUIElement _filterByCreationDateButton;

        /// <summary>
        /// Filter by last edited date button used to open the filter by last edited date menu
        /// </summary>
        private ButtonUIElement _filterByLastEditedDateButton;

        /// <summary>
        /// filter by type button used to open the filter by type menu
        /// </summary>
        private ButtonUIElement _filterByTypeButton;

        /// <summary>
        /// button used to apply the filter
        /// </summary>
        private ButtonUIElement _applyFilterbutton;

        /// <summary>
        /// helper list of all the buttons used to apply similar functions to all the buttons
        /// </summary>
        private List<ButtonUIElement> _filterMenuButtons;


        private FilterSubMenu _filterSubMenu;

        private StackLayoutManager _buttonLayoutManager;

        private float buttonHeight = 50;
        private float leftMargin = 5;
        private float topMargin;
        private float rightMargin = 5;
        private float spacing = 5;


        private ButtonUIElement _removeFilterButton;

        public FilterMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            IsDraggable = false;
            topMargin = TopBarHeight;
            KeepAspectRatio = false;
            TopBarColor = Colors.Azure;

            // initialize the button layout manager so buttons are stretched horizontally and stay at the top 
            // of the window
            _buttonLayoutManager = new StackLayoutManager(StackAlignment.Vertical)
            {
                TopMargin = topMargin,
                LeftMargin = leftMargin,
                RightMargin = rightMargin,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Spacing = spacing,
                ItemHeight = buttonHeight = 50
            };

            // initialize the list to hold all the buttons
            _filterMenuButtons = new List<ButtonUIElement>();

            // initialize the button to filter category dictionary
            _buttonToFilterCategories = new Dictionary<ButtonUIElement, FilterCategory>();

            // create a button for each filter category
            foreach (var category in Enum.GetValues(typeof(FilterCategory)).Cast<FilterCategory>())
            {
                var button = new RectangleButtonUIElement(this, ResourceCreator, UIDefaults.SecondaryStyle);
                InitializeFilterButton(category, button);
                _buttonLayoutManager.AddElement(button);
                _filterMenuButtons.Add(button);
                AddChild(button);
            }

            _applyFilterbutton = new RectangleButtonUIElement(this, ResourceCreator,
                UIDefaults.SecondaryStyle, "Apply Filter");
            AddChild(_applyFilterbutton);
            _buttonLayoutManager.AddElement(_applyFilterbutton);
            _filterMenuButtons.Add(_applyFilterbutton);
            _applyFilterbutton.Tapped += OnApplyFilterButtonTapped;

            _removeFilterButton = new RectangleButtonUIElement(this, ResourceCreator, UIDefaults.SecondaryStyle,
                "Remove Filter");
            AddChild(_removeFilterButton);
            _buttonLayoutManager.AddElement(_removeFilterButton);
            _filterMenuButtons.Add(_removeFilterButton);
            _removeFilterButton.Tapped += OnRemoveFilterButtonTapped;

            _filterSubMenu = new FilterSubMenu(this, ResourceCreator);
            AddChild(_filterSubMenu);


            // set the MinHeight based on the number of buttons we passed in
            MinHeight = _filterMenuButtons.Count*buttonHeight + (_filterMenuButtons.Count - 1)*spacing + topMargin + BorderWidth;
            MinWidth = 100;

        }

        private void OnApplyFilterButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            BrushManager.ApplyBrush(_filterSubMenu.CurrBrush.GetLibraryElementControllers());
        }

        private void OnRemoveFilterButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            BrushManager.RemoveBrush();
        }

        /// <summary>
        /// Initializes a filter button based on the passed in category
        /// </summary>
        /// <param name="category"></param>
        private void InitializeFilterButton(FilterCategory category, ButtonUIElement button)
        {
            switch (category)
            {
                case FilterCategory.Creator:
                    _filterByCreatorButton = button;
                    button.ButtonText = FilterCategoryToString(FilterCategory.Creator);
                    _buttonToFilterCategories.Add(button, FilterCategory.Creator);
                    button.Tapped += OnCategoryButtonTapped;
                    break;
                case FilterCategory.CreationDate:
                    _filterByCreationDateButton = button;
                    button.ButtonText = FilterCategoryToString(FilterCategory.CreationDate);
                    _buttonToFilterCategories.Add(button, FilterCategory.CreationDate);
                    button.Tapped += OnCategoryButtonTapped;
                    break;
                case FilterCategory.LastEditedDate:
                    _filterByLastEditedDateButton = button;
                    button.ButtonText = FilterCategoryToString(FilterCategory.LastEditedDate);
                    _buttonToFilterCategories.Add(button, FilterCategory.LastEditedDate);
                    button.Tapped += OnCategoryButtonTapped;
                    break;
                case FilterCategory.Type:
                    _filterByTypeButton = button;
                    button.ButtonText = FilterCategoryToString(FilterCategory.Type);
                    _buttonToFilterCategories.Add(button, FilterCategory.Type);
                    button.Tapped += OnCategoryButtonTapped;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }

        public override void Dispose()
        {
            foreach (var button in _filterMenuButtons)
            {
                if (button != _applyFilterbutton && button != _removeFilterButton)
                    button.Tapped -= OnCategoryButtonTapped;
            }
            _applyFilterbutton.Tapped -= OnApplyFilterButtonTapped;
            _removeFilterButton.Tapped -= OnRemoveFilterButtonTapped;
            base.Dispose();
        }

        /// <summary>
        /// Called whenever a category button is tapped. displays the subfilter menu for that category
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnCategoryButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var button = item as ButtonUIElement;
            Debug.Assert(button != null);
            Debug.Assert(_buttonToFilterCategories.ContainsKey(button));
            var category = _buttonToFilterCategories[button];
            _filterSubMenu.DisplayViewFromCategory(category);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // make the library fill the resizeable window leaving room for the search bar and filter button
            _buttonLayoutManager.SetSize(Width, Height);
            _buttonLayoutManager.ArrangeItems();

            _filterSubMenu.Transform.LocalPosition = new Vector2(Width, 0);

            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Converts a filter category to a human readable string
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static string FilterCategoryToString(FilterCategory category)
        {
            switch (category)
            {
                case FilterCategory.Creator:
                    return _filterToStringDict[FilterCategory.Creator];
                case FilterCategory.CreationDate:
                    return _filterToStringDict[FilterCategory.CreationDate];
                case FilterCategory.LastEditedDate:
                    return _filterToStringDict[FilterCategory.LastEditedDate];
                case FilterCategory.Type:
                    return _filterToStringDict[FilterCategory.Type];
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }
    }
}
