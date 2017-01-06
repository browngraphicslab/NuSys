using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DateSelector : RectangleUIElement
    {

        private DropdownUIElement _monthDropDownUIElement;

        private DropdownUIElement _dayDropDownUIElement;

        private DropdownUIElement _yearDropDownUIElement;

        /// <summary>
        /// Tries to get the year from the DateSelector, returns null if the year has not been selected yet
        /// </summary>
        public int? Year => TryParseInt(_yearDropDownUIElement.CurrentSelection);

        /// <summary>
        /// Tries to get the month from the DateSelector, returns null if the month has not been selected yet
        /// </summary>
        public int? Month => TryParseInt(_monthDropDownUIElement.CurrentSelection);

        /// <summary>
        /// Tries to get the day from the DateSelector, returns null if the day has not been selected yet
        /// </summary>
        public int? Day => TryParseInt(_dayDropDownUIElement.CurrentSelection);

        /// <summary>
        /// Tries to get the full Date from the DateSelector, returns null if the date has not been selected yet
        /// </summary>
        public DateTime? Date
        {
            get
            {
                if (Year != null && Month != null && Day != null)
                {
                    return new DateTime(Year.Value, Month.Value, Day.Value);
                }
                return null;
            }
        }

        public delegate void OnDateChangedHandler(DateSelector sender, DateTime? date);

        /// <summary>
        /// Event fired whenever a new valid date has been created. or if a valid date existed and now no longer exists
        /// In that case date would be null
        /// </summary>
        public event OnDateChangedHandler DateChanged;

        private StackLayoutManager _dropDownManager;

        private bool _hasValidDate;

        public DateSelector(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // instantiate the three dropdowns
            _monthDropDownUIElement = new DropdownUIElement(this, ResourceCreator)
            {
                Prompt = "MM"
            };
            AddChild(_monthDropDownUIElement);
            _dayDropDownUIElement = new DropdownUIElement(this, ResourceCreator)
            {
                Prompt = "DD"
            };
            AddChild(_dayDropDownUIElement);
            _yearDropDownUIElement = new DropdownUIElement(this, ResourceCreator)
            {
                Prompt = "YYYY"
            };
            AddChild(_yearDropDownUIElement);

            // add all the options to the dropdowns, these bounds have nothing to do with AVD birthday. literally arbitrary
            _yearDropDownUIElement.AddOption(string.Empty);
            _yearDropDownUIElement.AddOptionRange(Enumerable.Range(1938, DateTime.Now.Year - 1938 + 1).Reverse().Select(numYear => numYear.ToString()));
            // cause the year to display the prompt
            _yearDropDownUIElement.CurrentSelection = String.Empty;

            _monthDropDownUIElement.AddOption(string.Empty);
            _monthDropDownUIElement.AddOptionRange(Enumerable.Range(1, 12).Select(numMonth => numMonth.ToString()));
            _monthDropDownUIElement.CurrentSelection = String.Empty;

            _dayDropDownUIElement.AddOption(string.Empty);
            _dayDropDownUIElement.AddOptionRange(Enumerable.Range(1, 31).Select(numDay => numDay.ToString()));
            _dayDropDownUIElement.CurrentSelection = String.Empty;

            Height = 40;
            Width = 150;
            BorderWidth = 3;
            Bordercolor = Colors.Black;
            

            _dropDownManager = new StackLayoutManager()
            {
                TopMargin = 5,
                BottomMargin = 5,
                LeftMargin = 5,
                RightMargin = 5,
                ItemHeight = 30,
                ItemWidth = 40,
                Spacing = 15,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Width = Width,
                Height = Height
            };
            _dropDownManager.AddElement(_yearDropDownUIElement);
            _dropDownManager.AddElement(_monthDropDownUIElement);
            _dropDownManager.AddElement(_dayDropDownUIElement);


            // add events for selections
            _monthDropDownUIElement.Selected += OnMenuSelected;
            _dayDropDownUIElement.Selected += OnMenuSelected;
            _yearDropDownUIElement.Selected += OnMenuSelected;
            _monthDropDownUIElement.OpenOrClosed += OnMenuOpenOrClosed;
            _yearDropDownUIElement.OpenOrClosed += OnMenuOpenOrClosed;
            _dayDropDownUIElement.OpenOrClosed += OnMenuOpenOrClosed;
        }


        /// <summary>
        /// Called whenever any menu is opened or closed, hides the other menus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="isOpen"></param>
        private void OnMenuOpenOrClosed(DropdownUIElement sender, bool isOpen)
        {
            // if we open a minute hide all the other ones
            if (isOpen)
            {
                if (sender != _dayDropDownUIElement)
                {
                    _dayDropDownUIElement.HideDropDown();
                }
                if (sender != _monthDropDownUIElement)
                {
                    _monthDropDownUIElement.HideDropDown();
                }
                if (sender != _yearDropDownUIElement)
                {
                    _yearDropDownUIElement.HideDropDown();
                }
            }
        }

        public override void Dispose()
        {
            _monthDropDownUIElement.Selected -= OnMenuSelected;
            _dayDropDownUIElement.Selected -= OnMenuSelected;
            _yearDropDownUIElement.Selected -= OnMenuSelected;
            _monthDropDownUIElement.OpenOrClosed -= OnMenuOpenOrClosed;
            _yearDropDownUIElement.OpenOrClosed -= OnMenuOpenOrClosed;
            _dayDropDownUIElement.OpenOrClosed -= OnMenuOpenOrClosed;
            base.Dispose();
        }

        private void AssertValidDate()
        {
            // if we don't have a null date, then tell the user that we have a valid date by firing datechanged
            if (Date != null)
            {
                _hasValidDate = true;
                DateChanged?.Invoke(this, Date.Value);
            }

            // otherwise if we have a null date and we had a valid date, tell the user that we no longer have a valid date by firing datechanged
            if (Date == null && _hasValidDate)
            {
                _hasValidDate = false;
                DateChanged?.Invoke(this, Date);
            }

            // if either the year or month is null we cannot assert that a valid day was chosen so return
            if (Year == null || Month == null)
            {
                return;
            }
            // get the number of days for the chosen month and year
            var numDays = DateTime.DaysInMonth(Year.Value, Month.Value);
            // get the maximum day we currently have in the dropdown
            var maxDay =
                _dayDropDownUIElement.Items.Where(item => TryParseInt(item) != null)
                    .Select(item => (int) TryParseInt(item)).Max();
            // if the current max is too long, remove the extra days
            if (maxDay > numDays)
            {
                _dayDropDownUIElement.RemoveOptionRange(Enumerable.Range(numDays + 1, maxDay - numDays).Select(day => day.ToString()));
            }
            // otherwise try to add the non existing days
            else if (maxDay < numDays)
            {
                _dayDropDownUIElement.AddOptionRange(Enumerable.Range(maxDay + 1, numDays - maxDay).Select(day => day.ToString()));
            }
        }

        private void OnMenuSelected(DropdownUIElement sender, string item)
        {
            AssertValidDate();
            _dayDropDownUIElement.HideDropDown();
            _yearDropDownUIElement.HideDropDown();
            _monthDropDownUIElement.HideDropDown();
        }

        /// <summary>
        /// Tries to parse an integer, returns null if it fails
        /// </summary>
        /// <param name="stringToParse"></param>
        /// <returns></returns>
        private int? TryParseInt(string stringToParse)
        {
            int output;
            if (int.TryParse(stringToParse, out output))
            {
                return output;
            }
            return null;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _dropDownManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
