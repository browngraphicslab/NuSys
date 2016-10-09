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
    public class TabButtonUIElement<T, T1> : ButtonUIElement where T : ITabType<T1> where T1 : IEqualityComparer<T1>
    {

        /// <summary>
        /// The type of the tab
        /// </summary>
        public T TabType { get; private set; }

        public delegate void TabEventHandler(T tabType);

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

        public TabButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            BaseInteractiveUIElement shapeElement, T tabType) : base(parent, resourceCreator, shapeElement)
        {
            // create a close button
            _closeButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas))
            {
                Background = Colors.Red,
                BorderWidth = 0,
                GetParentBounds = ReturnBounds,
                GetParentScreenToLocalMatrix = ReturnScreenToLocalMatrix,
                Height = Height,
                Width = Width/5,
                InitialOffset = new Vector2((Width/5)*4, 0),
            };

            // set the text of the button
            ButtonText = tabType.Title;

            // add the close button as a child of the _shape of the tab butotn
            AddChild(_closeButton);

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
            OnSelected?.Invoke(TabType);
        }

        /// <summary>
        /// Fired when the close button is tapped should be a close!
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _closeButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnClosed?.Invoke(TabType);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;

            // Delegate drawing to the shape.
            Shape.Draw(ds);

            //todo add a child that will draw the text
            base.Draw(ds);
        }
    }
}
