﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;
using System.Numerics;

namespace NuSysApp
{
    /// <summary>
    /// a flyout list, similar to a menuflyout. 
    /// essentially has a list of options like menu drop downs.
    /// these options are buttons that trigger actions.
    /// </summary>
    public class FlyoutPopup : PopupUIElement
    {
        /// <summary>
        /// list of all the items in the flyout
        /// </summary>
        private List<ButtonUIElement> _flyoutItems;

        /// <summary>
        /// flyout item height
        /// </summary>
        private float _flyoutItemHeight;

        /// <summary>
        /// constructor for flyout list
        /// 
        /// all appearances are set here, but YOU WILL HAVE TO SET THE TRANSFORM.LOCALPOSITION TO WHEREEVER YOU WANT IT TO BE.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public FlyoutPopup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _flyoutItems = new List<ButtonUIElement>();
            _flyoutItemHeight = 35;
            Width = 150;
            Height = 100;
            Background = Colors.White;
            BorderWidth = 1;
            Bordercolor = Constants.MED_BLUE;
        }

        /// <summary>
        /// Make a new flyout item and attaches appropriate handler to button
        /// </summary>
        /// <param name="text"></param>
        public void AddFlyoutItem(string text, PointerHandler handler, ICanvasResourceCreatorWithDpi resourceCreator)
        {
            var flyoutItem = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator));
            flyoutItem.Height = _flyoutItemHeight;
            flyoutItem.Width = Width;
            flyoutItem.Background = Colors.White;
            flyoutItem.Bordercolor = Constants.MED_BLUE;
            flyoutItem.ButtonTextColor = Constants.ALMOST_BLACK;
            flyoutItem.BorderWidth = 1;
            flyoutItem.ButtonText = text;
            flyoutItem.ButtonTextSize = 12;
            flyoutItem.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            flyoutItem.SelectedBackground = Constants.LIGHT_BLUE;
            flyoutItem.Transform.LocalPosition = new Vector2(0, _flyoutItems.Count * _flyoutItemHeight);
            _flyoutItems.Add(flyoutItem);
            AddChild(flyoutItem);
        }

        /// <summary>
        /// overrides draw in order to calculate the correct height of the flyout based on
        /// the list of its flyout items
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            Height = _flyoutItems.Count * _flyoutItemHeight;
            base.Draw(ds);
        }
    }
}