using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using static NuSysApp.ButtonUIElement;
using Microsoft.Graphics.Canvas.Text;
using Windows.UI;
using RTools_NTS.Util;

namespace NuSysApp
{
    public class DropdownUIElement : ButtonUIElement
    {
        /// <summary>
        /// helper variable for the current selection property
        /// </summary>
        private string _currentSelection;

        /// <summary>
        /// The current selection, the empty string if nothing has been selected yet
        /// </summary>
        public string CurrentSelection
        {
            get { return _currentSelection; }
            set
            {
                Debug.Assert(_dropDownList.GetItems().Contains(value), "make sure the dropdown list contains the value we are setting the current selection to");
                Debug.Assert(value != null, "make sure the value is not null, the dropdown list can have an empty string but not a null string");
                
                // support prompts
                if (value == string.Empty && Prompt != null)
                {
                    _currentSelection = value;
                    ButtonText = Prompt;
                    _dropDownList.SelectItem(value);
                    Selected?.Invoke(this, value);
                }


                if (_currentSelection != value)
                {
                    _currentSelection = value;
                    ButtonText = value;
                    _dropDownList.SelectItem(value);
                    Selected?.Invoke(this, value);
                }
            }
        }

        /// <summary>
        /// The height of the rows in the drop down list
        /// </summary>
        public float RowHeight
        {
            get { return _dropDownList.RowHeight;}
            set
            {
                Debug.Assert(value >= 0);
                _dropDownList.RowHeight = value;
            }
        }

        public Color ListBackground
        {
            get { return _dropDownList.Background; }
            set { _dropDownList.Background = value; }
        }

        public float ListBorder
        {
            get { return _dropDownList.BorderWidth; }
            set { _dropDownList.BorderWidth = value; }
        }

        

        /// <summary>
        /// The height of the dropdown list
        /// </summary>
        public float ListHeight
        {
            get { return _dropDownList.Height; }
            set
            {
                Debug.Assert(value >= 0);
                _dropDownList.Height = value;
            }
        }

        /// <summary>
        /// Returns a list of all the items in the drop down list
        /// </summary>
        public List<string> Items => _dropDownList.GetItems();

        /// <summary>
        /// list that is hidden but can dropdown if the user presses the display button
        /// </summary>
        private ListViewUIElementContainer<string> _dropDownList;

        /// <summary>
        /// Column for the drop down list
        /// </summary>
        private ListTextColumn<string> _dropDownItems;

        public delegate void SelectedHandler(DropdownUIElement sender, string item);

        /// <summary>
        /// Event fired whenever a selection is made in the dropdownui element
        /// does not fire if the same selection is made twice
        /// </summary>
        public event SelectedHandler Selected;

        public delegate void OpenStateChangedHandler(DropdownUIElement sender, bool isOpen);

        /// <summary>
        /// Event fired whenever the dropdown is opened or closed, has an isOpen bool which is true if the dropdown
        /// was just opened, or false if it was just closed
        /// </summary>
        public event OpenStateChangedHandler OpenOrClosed;

        /// <summary>
        /// boolean used to determine if the dropdown is being displayed or not, true if the dropdown list is visible
        /// false if only the header button is visible
        /// </summary>
        public bool IsOpen => _dropDownList.IsVisible;

        /// <summary>
        /// the prompt to display to the user, if you want to display a prompt add an empty string to the dropdown values,
        /// When the empty string is selected, the prompt will be displayed instead, the value of CurrentSelection
        /// will only be the empty string, but the value displayed to the user will be the prompt, if you would like to remove
        /// prompt support, set Prompt to null, and do not include an empty string in the values, if you would like an empty
        /// string in your values but no prompt support, just set prompt to null. In order to display the prompt to the user
        /// initially, you could simply set the CurrentSelection to the empty string, after adding the empty string to the
        /// list of possible values that the dropdown can display
        /// </summary>
        public string Prompt { get; set; }


        public DropdownUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator, new RectangleUIElement(parent, resourceCreator))
        {
            // set default ui
            Background = Colors.Transparent;
            ButtonTextColor = Colors.Black;
            ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            BorderWidth = 1;
            Bordercolor = Colors.Black;
            SelectedBorder = Colors.Black;

            // instantiate a list and column for the dropdown
            _dropDownList = new ListViewUIElementContainer<string>(this, ResourceCreator)
            {
                IsVisible = false,
                ShowHeader = false
            };
            AddChild(_dropDownList);

            // set default height and width for the dropdown list
            RowHeight = 30;
            ListHeight = 300;

            _dropDownItems = new ListTextColumn<string>()
            {
                ColumnFunction = text => text,
                RelativeWidth = 1,
            };
            _dropDownList.AddColumn(_dropDownItems);          

            // add manipulation events, display the list when the display button is tapped
            // and select an element when the row is selected
            Tapped += OnDisplayButtonTapped;
            _dropDownList.RowTapped += OnRowSelected;
        }

        public override void Dispose()
        {
            Tapped -= OnDisplayButtonTapped;
            _dropDownList.RowTapped -= OnRowSelected;
            base.Dispose();
        }

        /// <summary>
        /// Called whenever a row is selected, changes the current selection which fires the Selected event
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        /// <param name="isSelected"></param>
        private void OnRowSelected(string item, string columnName, CanvasPointer pointer, bool isSelected)
        {
            CurrentSelection = item;
        }

        /// <summary>
        /// called whenever the main header button is tapped, changes the display status of the dropdown
        /// if it is visible then it is no longer visible, if it was invisible it is visible
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnDisplayButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _dropDownList.IsVisible = !_dropDownList.IsVisible;
            OpenOrClosed?.Invoke(this, _dropDownList.IsVisible);
        }

        /// <summary>
        /// Hides the dropdown list programmatically
        /// </summary>
        public void HideDropDown()
        {
            _dropDownList.IsVisible = false;
            OpenOrClosed?.Invoke(this, _dropDownList.IsVisible);
        }

        /// <summary>
        /// Adds a new option to the dropdown, never adds the same option twice
        /// </summary>
        /// <param name="item"></param>
        public void AddOption(string item)
        {
            if (_dropDownList.GetItems().Count(s => s.Equals(item)) == 0)
            {
                _dropDownList.AddItems(new List<string> { item });
            }
        }

        /// <summary>
        /// Adds a range of options to the dropdown, does not add the same option twice
        /// </summary>
        /// <param name="items"></param>
        public void AddOptionRange(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                AddOption(item);
            }
        }

        /// <summary>
        /// Removes the option from the dropdown list, does nothing if the option does not exist
        /// </summary>
        /// <param name="item"></param>
        public void RemoveOption(string item)
        {
            if (_dropDownList.GetItems().Count(s => s.Equals(item)) != 0)
            {
                // if we remove the selected option
                if (CurrentSelection == item)
                {
                    // set the selected option to the first item in the dropdown
                    CurrentSelection = _dropDownList.GetItems().First();
                }

                _dropDownList.RemoveItems(new List<string> { item });
            }
        }

        /// <summary>
        /// Removes a range of options from the dropdown list, does nothing fi the option does not exist
        /// </summary>
        /// <param name="items"></param>
        public void RemoveOptionRange(IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                RemoveOption(item);
            }
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _dropDownList.Transform.LocalPosition = new Vector2(0, Height);
            _dropDownList.ShowHeader = false;
            _dropDownList.Width = Width;

            base.Update(parentLocalToScreenTransform);
        }

       
    }
}
