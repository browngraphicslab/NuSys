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

namespace NuSysApp.Components.NuSysRenderer.UI
{
    class DropdownUIElement : ButtonUIElement
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
                Debug.Assert(_dropDownList.GetItems().Contains(value));
                if (_currentSelection == value)
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


        public DropdownUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator, new RectangleUIElement(parent, resourceCreator))
        {
            // set default ui
            Background = Colors.Transparent;
            ButtonTextColor = Colors.Black;
            ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;

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
                RelativeWidth = 1
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

        private void OnRowSelected(string item, string columnName, CanvasPointer pointer, bool isSelected)
        {
            if (isSelected)
            {
                CurrentSelection = item;
            }
        }

        private void OnDisplayButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _dropDownList.IsVisible = !_dropDownList.IsVisible;
        }

        /// <summary>
        /// Adds a new option to the dropdown, never adds the same option twice
        /// </summary>
        /// <param name="item"></param>
        public void AddOption(string item)
        {
            if (_dropDownList.GetItems().Count(s => s.Equals(item)) != 0)
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
            _dropDownList.Transform.LocalPosition = new Vector2(Width, Height);

            base.Update(parentLocalToScreenTransform);
        }

       
    }
}
