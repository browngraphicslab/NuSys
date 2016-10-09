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
    public class TabButtonUIElement<T> : ButtonUIElement where T : IComparable<T>
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
        /// The current tab associated with the TabButton
        /// </summary>
        public T Tab { get; private set; }

        public TabButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            BaseInteractiveUIElement shapeElement, T tab) : base(parent, resourceCreator, shapeElement)
        {
            // create a close button
            _closeButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas));

            // set the current tab
            Tab = tab;

            // add the close button as a child of the _shape of the tab butotn
            base.AddChild(_closeButton);

        }

        /// <summary>
        /// The initializer method put event handlers here
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            _closeButton.Tapped += _closeButton_Tapped;
            Tapped += TabButtonUIElement_Tapped;
            return base.Load();
        }

        /// <summary>
        /// The dispose method, remove event handlers here
        /// </summary>
        public override void Dispose()
        {
            _closeButton.Tapped -= _closeButton_Tapped;
            Tapped -= TabButtonUIElement_Tapped;
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
            OnClosed?.Invoke(Tab);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;

            // set the parameters for the close button
            SetCloseButtonParams();

            base.Draw(ds);
        }

        /// <summary>
        /// Sets the parameters on the close button
        /// </summary>
        public void SetCloseButtonParams()
        {

            _closeButton.Background = Colors.Red;
            _closeButton.BorderWidth = 0;
            _closeButton.GetParentBounds = ReturnBounds;
            _closeButton.GetParentScreenToLocalMatrix = ReturnScreenToLocalMatrix;
            _closeButton.Height = base.Height;
            _closeButton.Width = base.Width/5;
            _closeButton.Transform.LocalPosition = new Vector2((base.Width/5)*4, 0);
        }
    }
}
