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
    class BingSearchPopup : PopupUIElement
    {
        private ScrollableTextboxUIElement _searchbar;
        private ButtonUIElement _addResults;
        private GridLayoutManager _layoutManager;
        private float _searchBarHeight = 25;

        public BingSearchPopup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            //set appearance
            Background = Colors.White;
            BorderWidth = 2;
            BorderColor = Constants.DARK_BLUE;
            Height = _searchBarHeight;

            _searchbar = new ScrollableTextboxUIElement(this, resourceCreator, false, false);
            _searchbar.Load();
            AddChild(_searchbar);

            _addResults = new RectangleButtonUIElement(this, ResourceCreator)
            {
                ButtonText = "Add",
                Height = _searchBarHeight,
                Width = _searchBarHeight
            };
            AddChild(_addResults);

            _layoutManager = new GridLayoutManager(this, ResourceCreator)
            {
                Height = Height,
                Width = Width,
            };
            _layoutManager.AddRows(new List<float>() { 1 });
            _layoutManager.AddColumns(new List<float>() { 5, 1 });
            _layoutManager.AddElement(_searchbar, 0, 0);
            _layoutManager.AddElement(_addResults, 0, 1);

            _addResults.Tapped += _addResults_Tapped;
        }

        private async void _addResults_Tapped(ButtonUIElement sender)
        {

            var searchRequest = new WebSearchRequest(new WebSearchRequestArgs() { SearchString = _searchbar.Text });
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(searchRequest);
            var s = searchRequest.WasSuccessful();
            DismissPopup();
            var notification = new CenteredPopup(SessionController.Instance.NuSessionView, Canvas, "Your search results are loading. Check your chat for a notification of when it finishes.");
            SessionController.Instance.NuSessionView.AddChild(notification);
        }
        public override void Update(Matrix3x2 parentToLocalTransform)
        {

            //arrange items
            _layoutManager.Height = Height;
            _layoutManager.Width = Width;
            base.Update(parentToLocalTransform);
        }

    }
}
