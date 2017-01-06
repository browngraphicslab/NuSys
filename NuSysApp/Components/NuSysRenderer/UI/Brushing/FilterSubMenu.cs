using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
    public class FilterSubMenu : ResizeableWindowUIElement
    {
        /// <summary>
        /// List used to display library element models
        /// </summary>
        private ListViewUIElementContainer<string> _userIdListView;

        /// <summary>
        /// Column used to display creators with checkboxes to get checked
        /// </summary>
        private ListCheckBoxColumn<string> _creatorCheckboxColumn;

        /// <summary>
        /// List used to display element types
        /// </summary>
        private ListViewUIElementContainer<NusysConstants.ElementType> _elementTypeListView;

        /// <summary>
        /// column used to display element types as checkboxes to get checked
        /// </summary>
        private ListCheckBoxColumn<NusysConstants.ElementType> _typeCheckboxColumn;

        /// <summary>
        /// Date selector used to set the creation start date
        /// </summary>
        private DateSelector _creationStartDateSelector;
        
        /// <summary>
        /// Date selector used to set the creation end date
        /// </summary>
        private DateSelector _creationEndDateSelector;

        /// <summary>
        /// Date selector used to set the last edited start date
        /// </summary>
        private DateSelector _lastEditedStartDateSelector;

        /// <summary>
        /// date selector used to set the last edited end date
        /// </summary>
        private DateSelector _lastEditedEndDateSelector;

        /// <summary>
        /// Textbox used to display helpful text like what the DateSelectors are used for
        /// </summary>
        private TextboxUIElement _dateRangeStartTextbox;

        /// <summary>
        /// Textbox used to display helpful text like what the DateSelectors are used for
        /// </summary>
        private TextboxUIElement _dateRangeEndTextbox;

        private TextboxUIElement _creationDateHeader;

        private TextboxUIElement _lastEditedDateHeader;

        public BrushFilter CurrBrush { get; }

        public FilterSubMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

            TopBarColor = Colors.Azure;
            Height = 400;
            Width = 300;
            MinWidth = 200;
            MinHeight = 400;
            KeepAspectRatio = false;
            IsDraggable = false;

            // instantiate a new _libraryElementListview
            _userIdListView = new ListViewUIElementContainer<string>(this, ResourceCreator)
            {
                IsVisible = false,
                MultipleSelections = true
            };
            AddChild(_userIdListView);
            // give the list view a column to display creators
            _creatorCheckboxColumn = new ListCheckBoxColumn<string>()
            {
                RelativeWidth = 1,
                Title = FilterMenu.FilterCategoryToString(FilterMenu.FilterCategory.Creator),
                ColumnFunction = userid => SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(userid)
            };
            _userIdListView.AddColumn(_creatorCheckboxColumn);

            // instantiate a new _elementTypeListView
            _elementTypeListView = new ListViewUIElementContainer<NusysConstants.ElementType>(this, ResourceCreator)
            {
                IsVisible = false,
                MultipleSelections = true
            };
            AddChild(_elementTypeListView);
            // give the list view a column to display element types
            _typeCheckboxColumn = new ListCheckBoxColumn<NusysConstants.ElementType>()
            {
                RelativeWidth = 1,
                Title = "Type",
                ColumnFunction = elem => elem.ToString(),
            };
            _elementTypeListView.AddColumn(_typeCheckboxColumn);

            // add the headers for the creation date and the last edited date
            _creationDateHeader = new TextboxUIElement(this, ResourceCreator)
            {
                Text = "Creation Date Range",
                Height = 50,
                Background = Colors.LightGray,
                TextColor = Colors.Black,
                BorderWidth = 2,
                Bordercolor = Colors.Black,
                IsVisible = false
            };
            AddChild(_creationDateHeader);

            _lastEditedDateHeader = new TextboxUIElement(this, ResourceCreator)
            {
                Text = "Last Edited Date Range",
                Height = 50,
                Background = Colors.LightGray,
                TextColor = Colors.Black,
                BorderWidth = 2,
                Bordercolor = Colors.Black,
                IsVisible = false
            };
            AddChild(_lastEditedDateHeader);

            // add date selector labels
            _dateRangeStartTextbox = new TextboxUIElement(this, resourceCreator)
            {
                Text = "Range Start",
                Width = 100,
                Height = 20,
                TextVerticalAlignment = CanvasVerticalAlignment.Center,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                IsVisible = false
            };
            AddChild(_dateRangeStartTextbox);
            _dateRangeEndTextbox = new TextboxUIElement(this, resourceCreator)
            {
                Text = "Range End",
                Width = 100,
                Height = 20,
                TextVerticalAlignment = CanvasVerticalAlignment.Center,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                IsVisible = false
            };
            AddChild(_dateRangeEndTextbox);

            // add all the DateSelectors
            _creationEndDateSelector = new DateSelector(this, Canvas) // endDate added as child first for z-indexing reasons
            {
                IsVisible = false
            };
            AddChild(_creationEndDateSelector);
            _creationStartDateSelector = new DateSelector(this, Canvas)
            {
                IsVisible = false
            };
            AddChild(_creationStartDateSelector);

            _lastEditedEndDateSelector = new DateSelector(this, Canvas)
            {
                IsVisible = false
            };
            AddChild(_lastEditedEndDateSelector);
            _lastEditedStartDateSelector = new DateSelector(this, Canvas)
            {
                IsVisible = false
            };
            AddChild(_lastEditedStartDateSelector);

            CurrBrush = new BrushFilter();

            _userIdListView.RowTapped += OnUserIdSelected;
            _elementTypeListView.RowTapped += OnTypeSelected;
            _creationStartDateSelector.DateChanged += DateChanged;
            _creationEndDateSelector.DateChanged += DateChanged;
            _lastEditedStartDateSelector.DateChanged += DateChanged;
            _lastEditedEndDateSelector.DateChanged += DateChanged;

            // instantiate displaying the creator category
            DisplayViewFromCategory(FilterMenu.FilterCategory.Creator);
        }

        /// <summary>
        /// fired whenever a date is changed in a date selector, sets the filter value properly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="date"></param>
        private void DateChanged(DateSelector sender, DateTime? date)
        {
            if (sender == _creationStartDateSelector)
            {
                CurrBrush.CreationDateStart = date;
            } else if (sender == _creationEndDateSelector)
            {
                CurrBrush.CreationDateEnd = date;
            } else if (sender == _lastEditedStartDateSelector)
            {
                CurrBrush.LastEditedStart = date;
            } else if (sender == _lastEditedEndDateSelector)
            {
                CurrBrush.LastEditedEnd = date;
            }
            else
            {
                Debug.Fail("we should never hit this");
            }
        }

        /// <summary>
        /// fired whenever a type is selected, sets the filter properly
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        /// <param name="isSelected"></param>
        private void OnTypeSelected(NusysConstants.ElementType item, string columnName, CanvasPointer pointer, bool isSelected)
        {
            if (isSelected)
            {
                CurrBrush.Types.Add(item);
            }
            else
            {
                CurrBrush.Types.Remove(item);
            }
        }

        public override void Dispose()
        {
            _userIdListView.RowTapped -= OnUserIdSelected;
            _elementTypeListView.RowTapped -= OnTypeSelected;
            _creationStartDateSelector.DateChanged -= DateChanged;
            _creationEndDateSelector.DateChanged -= DateChanged;
            _lastEditedStartDateSelector.DateChanged -= DateChanged;
            _lastEditedEndDateSelector.DateChanged -= DateChanged;

            base.Dispose();
        }

        /// <summary>
        /// fired whenver a creator is selected, adds or removes the creator from the filter properly
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        /// <param name="isSelected"></param>
        private void OnUserIdSelected(string item, string columnName, CanvasPointer pointer, bool isSelected)
        {
            if (isSelected)
            {
                CurrBrush.Creators.Add(item);
            }
            else
            {
                CurrBrush.Creators.Remove(item);
            }
        }

        /// <summary>
        /// Called in an API like way to display certain views based on categories
        /// </summary>
        /// <param name="category"></param>
        public void DisplayViewFromCategory(FilterMenu.FilterCategory category)
        {
            HideAllViews();

            switch (category)
            {
                case FilterMenu.FilterCategory.Creator:
                    DisplayCreatorView();
                    break;
                case FilterMenu.FilterCategory.CreationDate:
                    DisplayCreationDateView();
                    break;
                case FilterMenu.FilterCategory.LastEditedDate:
                    DisplayLastEditedDateView();
                    break;
                case FilterMenu.FilterCategory.Type:
                    DisplayTypeView();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }

        }

        private void DisplayCreationDateView()
        {
            _creationStartDateSelector.IsVisible = true;
            _creationEndDateSelector.IsVisible = true;
            _dateRangeStartTextbox.IsVisible = true;
            _dateRangeEndTextbox.IsVisible = true;
            _creationDateHeader.IsVisible = true;
        }

        private void DisplayLastEditedDateView()
        {
            _lastEditedStartDateSelector.IsVisible = true;
            _lastEditedEndDateSelector.IsVisible = true;
            _dateRangeStartTextbox.IsVisible = true;
            _dateRangeEndTextbox.IsVisible = true;
            _lastEditedDateHeader.IsVisible = true;
        }

        private void DisplayTypeView()
        {
            _elementTypeListView.ClearItems();
            _elementTypeListView.AddItems(new List<NusysConstants.ElementType>
            {
                NusysConstants.ElementType.Audio,
                NusysConstants.ElementType.Collection,
                NusysConstants.ElementType.Image,
                NusysConstants.ElementType.PDF,
                NusysConstants.ElementType.Text,
                NusysConstants.ElementType.Video,            
            });
            foreach (var type in CurrBrush.Types)
            {
                _elementTypeListView.SelectItem(type);
            }
            _elementTypeListView.IsVisible = true;
        }

        /// <summary>
        /// Makes sure all the views are hidden, should be called before any display to make sure
        /// only one thing is displayed at a time
        /// </summary>
        private void HideAllViews()
        {
            _userIdListView.IsVisible = false;
            _elementTypeListView.IsVisible = false;
            _lastEditedStartDateSelector.IsVisible = false;
            _lastEditedEndDateSelector.IsVisible = false;
            _creationStartDateSelector.IsVisible = false;
            _creationEndDateSelector.IsVisible = false;
            _dateRangeStartTextbox.IsVisible = false;
            _dateRangeEndTextbox.IsVisible = false;
            _creationDateHeader.IsVisible = false;
            _lastEditedDateHeader.IsVisible = false;

        }

        /// <summary>
        /// Display the view associated with the creator category
        /// </summary>
        private void DisplayCreatorView()
        {
            _userIdListView.ClearItems();
            _userIdListView.AddItems(SessionController.Instance.ContentController.AllLibraryElementModels.Select(elem => elem.Creator).Distinct().ToList());
            foreach (var creator in CurrBrush.Creators)
            {
                _userIdListView.SelectItem(creator);
            }
            _userIdListView.IsVisible = true;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // make sure the list views fill the height and width of the cotnainer
            _userIdListView.Width = Width - 2*BorderWidth;
            _userIdListView.Height = Height - 2 * BorderWidth - TopBarHeight;
            _userIdListView.Transform.LocalPosition = new Vector2(BorderWidth, BorderWidth + TopBarHeight);
            _elementTypeListView.Width = Width - 2 * BorderWidth;
            _elementTypeListView.Height = Height - 2 * BorderWidth;
            _elementTypeListView.Transform.LocalPosition = new Vector2(BorderWidth, BorderWidth + TopBarHeight);

            // set the date views so they are stacked vertically and centered
            var dateSpacing = 5;
            _creationStartDateSelector.Transform.LocalPosition = new Vector2(Width/2 - _creationStartDateSelector.Width/2, Height/2 - _creationStartDateSelector.Height - dateSpacing);
            _creationEndDateSelector.Transform.LocalPosition = new Vector2(Width / 2 - _creationEndDateSelector.Width / 2, Height / 2 + _creationEndDateSelector.Height + dateSpacing);
            _lastEditedStartDateSelector.Transform.LocalPosition = _creationStartDateSelector.Transform.LocalPosition;
            _lastEditedEndDateSelector.Transform.LocalPosition = _creationEndDateSelector.Transform.LocalPosition;
            _dateRangeStartTextbox.Transform.LocalPosition = _creationStartDateSelector.Transform.LocalPosition +
                                                             new Vector2(
                                                                 _creationStartDateSelector.Width/2 -
                                                                 _dateRangeStartTextbox.Width/2,
                                                                 -_dateRangeStartTextbox.Height);
            _dateRangeEndTextbox.Transform.LocalPosition = _creationEndDateSelector.Transform.LocalPosition +
                                                 new Vector2(
                                                     _creationEndDateSelector.Width / 2 -
                                                     _dateRangeEndTextbox.Width / 2,
                                                     -_dateRangeEndTextbox.Height);

            // make the headers fill the width for creation date and last edited date
            _creationDateHeader.Width = Width - 2 * BorderWidth;
            _creationDateHeader.Transform.LocalPosition = new Vector2(BorderWidth, TopBarHeight);
            _lastEditedDateHeader.Width = Width - 2*BorderWidth;
            _lastEditedDateHeader.Transform.LocalPosition = new Vector2(BorderWidth, TopBarHeight);

            base.Update(parentLocalToScreenTransform);
        }

        public HashSet<ElementController> GetElementControllersForCurrentCollection()
        {
            return CurrBrush.GetElementControllersForCollection(
    SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.ViewModel.Controller
        .LibraryElementController as CollectionLibraryElementController);
        }

        public HashSet<LibraryElementController> GetLibraryElementControllers()
        {
            return CurrBrush.GetLibraryElementControllers();
        }
    }
}
