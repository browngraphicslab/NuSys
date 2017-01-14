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
    class BingSearchPopup : PopupUIElement
    {
        private ScrollableTextboxUIElement _searchbar;
        private ButtonUIElement _addResults;
        private GridLayoutManager _layoutManager;

        public BingSearchPopup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            //set appearance
            Background = Colors.White;
            BorderWidth = 2;
            BorderColor = Constants.DARK_BLUE;
            Height = 50;

            _searchbar = new ScrollableTextboxUIElement(this, resourceCreator, false, false);
            AddChild(_searchbar);

            _addResults = new RectangleButtonUIElement(this, ResourceCreator)
            {
                ButtonText = "Add",
                Height = 50,
                Width = 50
            };
            AddChild(_addResults);

            _layoutManager = new GridLayoutManager(this,ResourceCreator)
            {
             Height   = Height,
             Width = Width,
            };
            _layoutManager.AddRows(new List<float>() {1});
            _layoutManager.AddColumns(new List<float>() {5,1});
            _layoutManager.AddElement(_searchbar,0,1);
            _layoutManager.AddElement(_addResults,0,1);
            
            _addResults.Tapped += _addResults_Tapped;
        }

        private void _addResults_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            //TODO MAKE REQUEST
            DismissPopup();
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
