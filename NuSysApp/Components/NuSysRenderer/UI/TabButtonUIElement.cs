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

        /// <summary>
        /// True if the tab is closeable false otherwise
        /// </summary>
        public bool IsCloseable;

        /// <summary>
        /// The current tab associated with the TabButton
        /// </summary>
        public T Tab { get; private set; }

        public TabButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, T tab) : base(parent, resourceCreator)
        {
            // create a close button
            _closeButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas));

            // create a background button
            _backgroundButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(parent, resourceCreator));

            // set the current tab
            Tab = tab;

            BorderWidth = 0;
            IsCloseable = UIDefaults.TabIsCloseable;

            // add the background button as a child
            base.AddChild(_backgroundButton);
            // add the close button as a child
            base.AddChild(_closeButton);

            _closeButton.OnPressed += _closeButton_Tapped;
            _backgroundButton.OnPressed += TabButtonUIElement_Tapped;

        }

        /// <summary>
        /// The dispose method, remove event handlers here
        /// </summary>
        public override void Dispose()
        {
            _closeButton.OnPressed -= _closeButton_Tapped;
            _backgroundButton.OnPressed -= TabButtonUIElement_Tapped;
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

        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;

            // set the parameters for the close button
            SetBackgroundAndCloseButtonParams();

            base.Draw(ds);
        }

        /// <summary>
        /// Sets the parameters on the close button and background button
        /// </summary>
        public void SetBackgroundAndCloseButtonParams()
        {
            if (IsCloseable)
            {
                _closeButton.IsVisible = true;
                _closeButton.Background = Colors.Red;
                _closeButton.BorderWidth = 0;
                _closeButton.Height = Height/3;
                _closeButton.Width = Height/3;
                _closeButton.Transform.LocalPosition = new Vector2(Width - 2*(Height/3), Height/3);
            }
            else
            {
                _closeButton.IsVisible = false;
                _closeButton.Height = 0;
                _closeButton.Width = 0;
            }


            _backgroundButton.ButtonText = Title;
            _backgroundButton.ButtonTextColor = TitleColor;
            _backgroundButton.Background = Background;
            _backgroundButton.BorderWidth = 0;
            _backgroundButton.Width = Width - _closeButton.Width * 3;
            _backgroundButton.Height = Height;
        }



    }
}
