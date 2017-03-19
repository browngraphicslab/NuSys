﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;
using Windows.System;

namespace NuSysApp.Components.NuSysRenderer.UI
{
    class EditTagsUIElement : RectangleUIElement
    {
        private static string SEARCH_BAR_TEXT = "Search tags";
        private static string ADD_TAG__BUTTON_TEXT = "Add";

        private AutoSuggestTextBox<Keyword> _autoSuggest;
        private ButtonUIElement _addButton;
        private EditTagsUIElement _editTagsElement;
        private ImmutableHashSet<Keyword> _tags;
        private string _currentText = null;

        public EditTagsUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
            : base(parent, resourceCreator)
        {
            var models = new List<LibraryElementModel>(SessionController.Instance.ContentController.ContentValues);
            models.RemoveAll(model => model == null || model.Keywords == null);
            var keywords = new List<Keyword>(models.SelectMany(model => model.Keywords));
            keywords.RemoveAll(keyword => keyword == null);
            var keywordSet = (new HashSet<Keyword>(keywords));
            _tags = keywordSet.ToImmutableHashSet();

            var backgroundRect = new RectangleUIElement(parent, resourceCreator);
            backgroundRect.Width = UIDefaults.backgroundRectWidth;
            backgroundRect.Height = UIDefaults.backgroundRectHeight;
          
            Color backgroundRectColor = Constants.LIGHT_BLUE;
            backgroundRect.Background = backgroundRectColor;
            backgroundRect.Transform.LocalPosition = new Vector2(-UIDefaults.margin, -UIDefaults.margin);

            AddChild(backgroundRect);

            _autoSuggest = new AutoSuggestTextBox<Keyword>(this, Canvas)
            {
                PlaceHolderText = SEARCH_BAR_TEXT,
                Background = Colors.WhiteSmoke,
                BorderWidth = 1.5f,
                BorderColor = Colors.SlateGray,
                ColumnFunction = keyword => keyword.Text,
                Width = UIDefaults.autoSuggestWidth,
                Height = UIDefaults.autoSuggestHeight,
                FilterFunction = delegate (string s)
                {
                    //return new List<Keyword>(_tags.Where(keyword => keyword.Text.ToLower().StartsWith(s)));
                    return new List<Keyword>(_tags.Where(keyword => keyword.Text.Contains(s)));
                },
            };
            _autoSuggest.Transform.LocalPosition = new Vector2(0, 0);
            _autoSuggest.KeyPressed += AutoSuggest_KeyPressed;

            AddChild(_autoSuggest);

            

            _addButton = new RectangleButtonUIElement(parent, resourceCreator);
            _addButton.Tapped += OnAddButtonTapped;
            _addButton.Height = UIDefaults.autoSuggestHeight;
            _addButton.Width = UIDefaults.buttonWidth;
            _addButton.ButtonText = ADD_TAG__BUTTON_TEXT;
            _addButton.Transform.LocalPosition = new Vector2(_autoSuggest.Width, 0.0f);
            AddChild(_addButton);
            
            Width = 270.0f;
            Height = 0.0f;
            Background = Colors.Transparent;
        }

        private void AutoSuggest_KeyPressed(KeyArgs args)
        {
            if (args.Key == VirtualKey.Enter)
            {
                AddKeyword();
            }
        }

        public override void Dispose()
        {
            _addButton.Tapped -= OnAddButtonTapped;
        }

        private void SearchBarTextChanged(InteractiveBaseRenderItem item, string text)
        {
            _currentText = text;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {

            base.Update(parentLocalToScreenTransform);
        }

        public override void Draw(CanvasDrawingSession ds)
        {

            base.Draw(ds);
        }

        public void OnAddButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            AddKeyword();
        }

        private void AddKeyword()
        {
            if (string.IsNullOrEmpty(_autoSuggest.Text) || string.IsNullOrWhiteSpace(_autoSuggest.Text))
            {
                //Don't allow creation of empty tag
                return;
            }
            var selections = SessionController.Instance.SessionView.FreeFormViewer.Selections;
            foreach (ElementRenderItem element in selections)
            {
                element.ViewModel.Controller.LibraryElementController.AddKeyword(new Keyword(_autoSuggest.Text));
            }
            _autoSuggest.ClearText();
        }
        public void UpdatePositionWithSize(Size size)
        {
            Transform.LocalPosition = new Vector2((float)(size.Width - Width) / 2, (float)(size.Height - Height) / 2);
        }
    }
}
