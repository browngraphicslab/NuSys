using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class TabButtonUIElement<T> : RectangleUIElement where T : IComparable<T>
    {

        /// <summary>
        /// The tab the event was fired on
        /// </summary>
        /// <param name="tab"></param>
        public delegate void TabEventHandler(T tab);

        /// <summary>
        /// Fired when the tab is selected
        /// </summary>
        public event TabEventHandler OnSelected;

        /// <summary>
        /// Fired when the tab is closed
        /// </summary>
        public event TabEventHandler OnClosed;

        /// <summary>
        /// Private variable to store the close button
        /// </summary>
        private ButtonUIElement _closeButton;

        /// <summary>
        /// Private variable that stores the background button the TabButton is made up of
        /// </summary>
        private ButtonUIElement _backgroundButton;

        /// <summary>
        /// The Title of the tab
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The color of the title of the tab
        /// </summary>
        public Color TitleColor { get; set; }

        public CanvasHorizontalAlignment TextAlignment { get; set; }

        /// <summary>
        /// True if the tab is closeable false otherwise
        /// </summary>
        public bool IsCloseable;

        /// <summary>
        /// The current tab associated with the TabButton
        /// </summary>
        public T Tab { get; private set; }

        /// <summary>
        /// getter and setter for whether the tab button is underlined
        /// </summary>
        public bool Underlined { get; set; }

        public TabButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, T tab) : base(parent, resourceCreator)
        {
            // create a close button
            _closeButton = new TransparentButtonUIElement(this, Canvas)
            {
                Height = 20,
                Background = Constants.RED,
                ImageBounds = new Rect(.25,.25,.5,.5)
            };

            // create a background button
            _backgroundButton = new RectangleButtonUIElement(this, Canvas);

            // set the current tab
            Tab = tab;

            BorderWidth = 0;
            IsCloseable = UIDefaults.TabIsCloseable;

            // add the background button as a child
            AddChild(_backgroundButton);
            // add the close button as a child
            AddChild(_closeButton);

            _closeButton.Tapped += _closeButton_Tapped;
            _backgroundButton.Tapped += TabButtonUIElement_Tapped;
        }

        public override async Task Load()
        {
            _closeButton.Image = _closeButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/x.png"));
        }

        /// <summary>
        /// The dispose method, remove event handlers here
        /// </summary>
        public override void Dispose()
        {
            _closeButton.Tapped -= _closeButton_Tapped;
            _backgroundButton.Tapped -= TabButtonUIElement_Tapped;
            base.Dispose();
        }

        /// <summary>
        /// Fired when the tab is tapped, should be a selection!
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void TabButtonUIElement_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnSelected?.Invoke(Tab);
        }

        /// <summary>
        /// Fired when the close button is tapped should be a close!
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _closeButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            Dispose();
            OnClosed?.Invoke(Tab);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // set the parameters for the close button
            SetBackgroundAndCloseButtonParams();

            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Sets the parameters on the close button and background button
        /// </summary>
        public void SetBackgroundAndCloseButtonParams()
        {
            if (IsCloseable)
            {
                _closeButton.IsVisible = true;
                _closeButton.Background = Colors.Transparent;
                _closeButton.SelectedBackground = Constants.RED_TRANSLUCENT;
                _closeButton.BorderWidth = 0;
                _closeButton.Height = 20;
                _closeButton.Width = _closeButton.Height;
                _closeButton.Transform.LocalPosition = new Vector2(Width - _closeButton.Width, Height/4);
            }
            else
            {
                _closeButton.IsVisible = false;
                _closeButton.Height = 0;
                _closeButton.Width = 0;
            }

            

            _backgroundButton.ButtonText = Title;
            _backgroundButton.ButtonTextColor = TitleColor;
            _backgroundButton.ButtonTextHorizontalAlignment = TextAlignment;
            _backgroundButton.Background = Background;
            _backgroundButton.BorderWidth = 0;
            _backgroundButton.Width = Math.Max(Width - _closeButton.Width,0);
            _backgroundButton.Height = Height;

            if (Underlined)
            {
                _backgroundButton.RichTextButton = true;
            }
        }
    }
}
