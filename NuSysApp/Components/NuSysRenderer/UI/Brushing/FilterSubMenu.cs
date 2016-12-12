using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;
using NuSysApp.Components.NuSysRenderer.UI;

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



        private BrushFilter _currFilter;

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
                ColumnFunction = elem => elem.ToString()
            };
            _elementTypeListView.AddColumn(_typeCheckboxColumn);

            _currFilter = new BrushFilter();

            _userIdListView.RowTapped += OnUserIdSelected;
            _elementTypeListView.RowTapped += OnTypeSelected;
        }


        private void OnTypeSelected(NusysConstants.ElementType item, string columnName, CanvasPointer pointer, bool isSelected)
        {
            if (isSelected)
            {
                _currFilter.Types.Add(item);
            }
            else
            {
                _currFilter.Types.Remove(item);
            }
        }

        public override void Dispose()
        {
            _userIdListView.RowTapped -= OnUserIdSelected;
            _elementTypeListView.RowTapped -= OnTypeSelected;

            base.Dispose();
        }

        private void OnUserIdSelected(string item, string columnName, CanvasPointer pointer, bool isSelected)
        {
            if (isSelected)
            {
                _currFilter.Creators.Add(item);
            }
            else
            {
                _currFilter.Creators.Remove(item);
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
                    break;
                case FilterMenu.FilterCategory.LastEditedDate:
                    break;
                case FilterMenu.FilterCategory.Type:
                    DisplayTypeView();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }

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
        }

        /// <summary>
        /// Display the view associated with the creator category
        /// </summary>
        private void DisplayCreatorView()
        {
            _userIdListView.ClearItems();
            _userIdListView.AddItems(SessionController.Instance.ContentController.AllLibraryElementModels.Select(elem => elem.Creator).Distinct().ToList());
            foreach (var creator in _currFilter.Creators)
            {
                _userIdListView.SelectItem(creator);
            }
            _userIdListView.IsVisible = true;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _userIdListView.Width = Width - 2*BorderWidth;
            _userIdListView.Height = Height - 2 * BorderWidth - TopBarHeight;
            _userIdListView.Transform.LocalPosition = new Vector2(BorderWidth, BorderWidth + TopBarHeight);
            _elementTypeListView.Width = Width - 2 * BorderWidth;
            _elementTypeListView.Height = Height - 2 * BorderWidth;
            _elementTypeListView.Transform.LocalPosition = new Vector2(BorderWidth, BorderWidth + TopBarHeight);
            base.Update(parentLocalToScreenTransform);
        }
    }
}
