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
    public class AddRegionPublicPrivateUIElement : RectangleUIElement
    {
        /// <summary>
        /// The button to add a public region
        /// </summary>
        private ButtonUIElement _addPublicButton;

        /// <summary>
        /// The button to add a private region
        /// </summary>
        private ButtonUIElement _addPrivateButton;

        /// <summary>
        /// The layout manager for the buttons
        /// </summary>
        private StackLayoutManager _buttonLayoutManager;

        /// <summary>
        /// Delegate for when a region with acls is added, boolean of whether it is public or private
        /// </summary>
        /// <param name="isPublic"></param>
        public delegate void AddRegionWithAcls(bool isPublic);

        /// <summary>
        /// The event fired when a region is added using the ui element
        /// </summary>
        public event AddRegionWithAcls OnRegionAdded;

        public AddRegionPublicPrivateUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set default ui values
            Background = Colors.AliceBlue;
            Bordercolor = Colors.LightGray;

            // initialize the layout manager
            _buttonLayoutManager = new StackLayoutManager(StackAlignment.Vertical);


            // initialize the private button
            _addPrivateButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator))
            {
                ButtonText = "Add Private"
            };
            InitializeButtonValues(_addPrivateButton);
            AddChild(_addPrivateButton);
            _buttonLayoutManager.AddElement(_addPrivateButton);


            // initialize the public button
            _addPublicButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator))
            {
                ButtonText = "Add Public"
            };
            InitializeButtonValues(_addPublicButton);
            AddChild(_addPublicButton);
            _buttonLayoutManager.AddElement(_addPublicButton);

            // add event handlers
            _addPrivateButton.Tapped += AddButtonTapped;
            _addPublicButton.Tapped += AddButtonTapped;

        }

        /// <summary>
        /// Fired whenever one of the add buttons is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void AddButtonTapped(ButtonUIElement item, CanvasPointer pointer)
        {
            // invoke with public true if public button was tapped
            OnRegionAdded?.Invoke(item == _addPublicButton);
        }

        /// <summary>
        /// Initialize the ui values for the buttons
        /// </summary>
        /// <param name="button"></param>
        private void InitializeButtonValues(ButtonUIElement button)
        {
            button.Height = 25;
            button.Width = 100;
            button.ButtonTextColor = Colors.WhiteSmoke;
            button.Background = Colors.DarkSlateGray;
        }

        public override void Dispose()
        {
            _addPrivateButton.Tapped -= AddButtonTapped;
            _addPublicButton.Tapped -= AddButtonTapped;

            base.Dispose();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _buttonLayoutManager.SetSize(Width, Height);
            _buttonLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _buttonLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _buttonLayoutManager.Spacing = 20;
            _buttonLayoutManager.ArrangeItems();

            base.Update(parentLocalToScreenTransform);
        }
    }
}
