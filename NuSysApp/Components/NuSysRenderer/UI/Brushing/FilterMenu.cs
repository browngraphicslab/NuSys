using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class FilterMenu : ResizeableWindowUIElement
    {
        private ListViewUIElementContainer<FilterCategory> _filterList;

        private static BiDictionary<FilterCategory, string> _filterToStringDict = new BiDictionary<FilterCategory, string>
        {
            {FilterCategory.Creator, "Creator"},
            {FilterCategory.CreationDate, "Creation Date"},
            {FilterCategory.LastEditedDate, "Last Edited Date"},
            {FilterCategory.Type, "Type"},

        };

        public enum FilterCategory
        {
            Creator,
            CreationDate,
            LastEditedDate,
            Type
        }


        public FilterMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

            IsDraggable = false;

            InitializeFilterList();
            AddChild(_filterList);
        }

        /// <summary>
        /// Initialize the UI for the library list 
        /// </summary>
        public void InitializeFilterList()
        {
            _filterList = new ListViewUIElementContainer<FilterCategory>(this, Canvas)
            {
                DisableSelectionByClick = true
            };

            var listColumn = new ListTextColumn<FilterCategory>
            {
                Title = "Filter By",
                RelativeWidth = 1,
                ColumnFunction = FilterCategoryToString
            };

            _filterList.AddColumns(new List<ListColumn<FilterCategory>> { listColumn});


            _filterList.AddItems( new List<FilterCategory>
            {
                FilterCategory.Creator,
                FilterCategory.CreationDate,
                FilterCategory.LastEditedDate,
                FilterCategory.Type
            });

            BorderWidth = 5;
            Bordercolor = Colors.Black;
            TopBarColor = Colors.Azure;
            Height = 400;
            Width = 400;
            MinWidth = 400;
            MinHeight = 400;


        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // make the library fill the resizeable window leaving room for the search bar and filter button
            _filterList.Width = Width - 2 * BorderWidth;
            _filterList.Height = Height - TopBarHeight - BorderWidth;
            _filterList.Transform.LocalPosition = new Vector2(BorderWidth, TopBarHeight);

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
