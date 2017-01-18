using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp.Components.NuSysRenderer.UI
{
    class AutoSuggestTextBox<T> : ScrollableTextboxUIElement
    {
        /// <summary>
        /// the list of suggestions to display
        /// </summary>
        private ListViewUIElementContainer<T> suggestion_list;

        /// <summary>
        /// The single column used in the suggestions list
        /// </summary>
        private ListTextColumn<T> suggestion_list_column;

        public delegate void SuggestionChosenHandler(AutoSuggestTextBox<T> sender, T item);

        /// <summary>
        /// Event fired whenever a suggestion is chosen in the auto suggest box
        /// </summary>
        public event SuggestionChosenHandler SuggestionChosen;


        /// <summary>
        /// Always check if HasSelection is true before using this selection
        /// </summary>
        public T CurrentSelection { get; private set; }

        /// <summary>
        /// True if the auto suggest text box currently has a selection
        /// </summary>
        public bool HasSelection { get; private set; }

        /// <summary>
        /// helper variable for the column function
        /// </summary>
        private Func<T, string> _columnFunction { get; set; }

        public float MaxDropDownHeight { get; set; } = UIDefaults.MaxDropDownHeight;

        /// <summary>
        /// This function takes in a generic item, and returns the string to be displayed in the suggestions list
        /// </summary>
        public Func<T, string> ColumnFunction {
            private get
            {
                return _columnFunction; 
            }
            set
            {
                _columnFunction = value;
                suggestion_list_column.ColumnFunction = value;
            }
        }

        /// <summary>
        /// This function takes in a string and returns the generic item to be displayed in the suggestions list
        /// </summary>
        public Func<string, List<T>> FilterFunction { private get; set; } 

        public AutoSuggestTextBox(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator, false, false)
        {

            suggestion_list = new ListViewUIElementContainer<T>(this, resourceCreator)
            {
                ShowHeader = false,
                IsVisible = false
            };
            AddChild(suggestion_list);

            suggestion_list_column = new ListTextColumn<T>()
            {
                RelativeWidth = 1,
            };

            suggestion_list.AddColumn(suggestion_list_column);

            TextChanged += OnQueryTextChanged;
            suggestion_list.RowTapped += Suggestion_list_RowTapped;
        }

        private void Suggestion_list_RowTapped(T item, string columnName, CanvasPointer pointer, bool isSelected)
        {
            if (isSelected)
            {
                SuggestionChosen?.Invoke(this, item);
                Text = ColumnFunction(item);
                suggestion_list.IsVisible = false;
                CurrentSelection = item;
                HasSelection = true;
            }
        }

        public override void Dispose()
        {
            TextChanged -= OnQueryTextChanged;
            suggestion_list.RowTapped -= Suggestion_list_RowTapped;


            base.Dispose();
        }

        /// <summary>
        /// Called whenever the text is changed in the auto suggest box
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        private void OnQueryTextChanged(InteractiveBaseRenderItem item, string text)
        {
            suggestion_list.ClearItems();
            HasSelection = false;
            if (!string.IsNullOrEmpty(text))
            {
                suggestion_list.AddItems(FilterFunction(text));
                suggestion_list.IsVisible = true;
            }
            else
            {
                suggestion_list.IsVisible = false;
            }

        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            suggestion_list.Width = Width;
            if (suggestion_list.GetItems().Any())
            {
                suggestion_list.Height = Math.Min(MaxDropDownHeight, suggestion_list.HeightOfAllRows);
            } else
            {
                suggestion_list.Height = MaxDropDownHeight;
            }
            suggestion_list.Transform.LocalPosition = new Vector2(0, Height);

            base.Update(parentLocalToScreenTransform);
        }
    }
}
