using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using NuSysApp.Components.NuSysRenderer.UI;

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

        private StackLayoutManager _dropDownManager;

        public DateSelector(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // instantiate the three dropdowns
            _monthDropDownUIElement = new DropdownUIElement(this, ResourceCreator);
            AddChild(_monthDropDownUIElement);
            _dayDropDownUIElement = new DropdownUIElement(this, ResourceCreator);
            AddChild(_dayDropDownUIElement);
            _yearDropDownUIElement = new DropdownUIElement(this, ResourceCreator);
            AddChild(_yearDropDownUIElement);

            // add all the options to the dropdowns, these bounds have nothing to do with AVD birthday. literally arbitrary
            _yearDropDownUIElement.AddOptionRange(Enumerable.Range(1938, DateTime.Now.Year).Select(numYear => numYear.ToString()));
            _yearDropDownUIElement.AddOption(string.Empty);

            _monthDropDownUIElement.AddOptionRange(Enumerable.Range(1, 12).Select(numMonth => numMonth.ToString()));
            _monthDropDownUIElement.AddOption(string.Empty);

            _dayDropDownUIElement.AddOptionRange(Enumerable.Range(1, 31).Select(numDay => numDay.ToString()));
            _dayDropDownUIElement.AddOption(string.Empty);

            Height = 50;
            Width = 100;

            _dropDownManager = new StackLayoutManager()
            {
                TopMargin = 10,
                BottomMargin = 10,
                LeftMargin = 7.5f,
                RightMargin = 7.5f,
                ItemHeight = 30,
                ItemWidth = 25,
                Spacing = 5,
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
        }

        public override void Dispose()
        {
            _monthDropDownUIElement.Selected -= OnMenuSelected;
            base.Dispose();
        }

        private void AssertValidDate()
        {
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
                _dayDropDownUIElement.RemoveOptionRange(Enumerable.Range(numDays + 1, maxDay).Select(day => day.ToString()));
            }
            // otherwise try to add the non existing days
            else if (maxDay < numDays)
            {
                _dayDropDownUIElement.AddOptionRange(Enumerable.Range(maxDay + 1, numDays).Select(day =>day.ToString()));
            }
        }

        private void OnMenuSelected(DropdownUIElement sender, string item)
        {
            AssertValidDate();
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
