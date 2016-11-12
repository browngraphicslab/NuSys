using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class BasicToolWindow: ToolWindow
    {
        

        //Margin for the buttons for changing the inner view of the tool (list, pie, bar)
        private const int VIEW_BUTTON_MARGIN = 10;

        private const int VIEW_BUTTON_HEIGHT = 40;

      

        /// <summary>
        /// The button for changing the inner view to the list view
        /// </summary>
        private ButtonUIElement _listToolViewButton;

        /// <summary>
        /// The button for changing the inner view to the pie chart view
        /// </summary>
        private ButtonUIElement _pieToolViewButton;

        /// <summary>
        /// The button for changing the inner view to the bar chart view.
        /// </summary>
        private ButtonUIElement _barToolViewButton;

        public BasicToolWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            SetUpBottomButtons();
        }

        private void SetUpBottomButtons()
        {
            //Set up list button
            var listButtonRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.CadetBlue,
                Height = VIEW_BUTTON_HEIGHT,
                Width = VIEW_BUTTON_HEIGHT,
            };
            _listToolViewButton = new ButtonUIElement(this, ResourceCreator, listButtonRectangle);
            _listToolViewButton.ButtonText = "List";
            _listToolViewButton.ButtonTextColor = Colors.Black;
            _listToolViewButton.Transform.LocalPosition = new Vector2(VIEW_BUTTON_MARGIN, ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN);
            AddChild(_listToolViewButton);

            //Set up pie button 
            var pieButtonRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.CadetBlue,
                Height = VIEW_BUTTON_HEIGHT,
                Width = VIEW_BUTTON_HEIGHT,
            };
            _pieToolViewButton = new ButtonUIElement(this, ResourceCreator, pieButtonRectangle);
            _pieToolViewButton.ButtonText = "Pie";
            _pieToolViewButton.ButtonTextColor = Colors.Black;
            _pieToolViewButton.Transform.LocalPosition = new Vector2(_listToolViewButton.Transform.LocalX + _listToolViewButton.Width + VIEW_BUTTON_MARGIN, ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN);
            AddChild(_pieToolViewButton);

            //Set up bar chart button 
            var barButtonRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.CadetBlue,
                Height = VIEW_BUTTON_HEIGHT,
                Width = VIEW_BUTTON_HEIGHT,
            };
            _barToolViewButton = new ButtonUIElement(this, ResourceCreator, barButtonRectangle);
            _barToolViewButton.ButtonText = "Bar";
            _barToolViewButton.ButtonTextColor = Colors.Black;
            _barToolViewButton.Transform.LocalPosition = new Vector2(_pieToolViewButton.Transform.LocalX + _pieToolViewButton.Width + VIEW_BUTTON_MARGIN, ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN);
            AddChild(_barToolViewButton);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            //Arrange the buttons at the bottom
            if (_listToolViewButton != null)
            {
                _listToolViewButton.Transform.LocalY = ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN;

            }

            if(_pieToolViewButton != null)
            {
                _pieToolViewButton.Transform.LocalY = ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN;

            }

            if (_barToolViewButton != null)
            {
                _barToolViewButton.Transform.LocalY = ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN;
            }

        }
    }
}