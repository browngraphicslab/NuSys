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
    public class DetailViewTextContent : RectangleUIElement, ILockable
    {
        /// <summary>
        /// The main textbox we write in
        /// </summary>
        private ScrollableTextboxUIElement _mainTextBox;

        /// <summary>
        /// The layout manager for the main textbox
        /// </summary>
        private StackLayoutManager _mainTextboxLayoutManager;

        /// <summary>
        /// The library element controller for the text associated with this detail view page
        /// </summary>
        private LibraryElementController _controller;

        /// <summary>
        ///This is for the IIlockable interface.  This will be the ID of the content Data model.
        /// </summary>
        public string Id
        {
            get { return _controller?.ContentDataController?.ContentDataModel?.ContentId; }
        }

        public DetailViewTextContent(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            LibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;
            this.Register(false);

            // create the main textbox
            _mainTextBox = new ScrollableTextboxUIElement(this, resourceCreator, true, true)
            {
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray,
                PlaceHolderText = "Enter text here...",
                Text = _controller.ContentDataController.ContentDataModel.Data
            };
            AddChild(_mainTextBox);


            // add the main textbox to a new layout manager
            _mainTextboxLayoutManager = new StackLayoutManager()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            _mainTextboxLayoutManager.AddElement(_mainTextBox);

            // event for when the controllers text changes
            _controller.ContentDataController.ContentDataUpdated += LibraryElementControllerOnContentChanged;
            _mainTextBox.TextChanged += _mainTextBox_TextChanged;

            _mainTextBox.OnFocusGained += MainTextBoxOnOnFocusGained;
            _mainTextBox.OnFocusLost += MainTextBoxOnOnFocusLost;
        }

        private void MainTextBoxOnOnFocusLost(BaseRenderItem item)
        {
            if (this.HasLock())
            {
                this.ReturnLock();
            }
        }

        private void MainTextBoxOnOnFocusGained(BaseRenderItem item)
        {
            this.GetLock();
        }

        private void _mainTextBox_TextChanged(InteractiveBaseRenderItem item, string text)
        {
            _controller.ContentDataController.ContentDataUpdated -= LibraryElementControllerOnContentChanged;
            _controller.ContentDataController.SetData(text);
            _controller.ContentDataController.ContentDataUpdated += LibraryElementControllerOnContentChanged;

        }

        private void LibraryElementControllerOnContentChanged(object sender, string e)
        {
            _mainTextBox.TextChanged -= _mainTextBox_TextChanged;
            _mainTextBox.Text = e;
            _mainTextBox.TextChanged += _mainTextBox_TextChanged;

        }

        public override void Dispose()
        {
            _controller.ContentDataController.ContentDataUpdated -= LibraryElementControllerOnContentChanged;
            _mainTextBox.TextChanged -= _mainTextBox_TextChanged;
            _mainTextBox.OnFocusGained -= MainTextBoxOnOnFocusGained;
            _mainTextBox.OnFocusLost -= MainTextBoxOnOnFocusLost;

            this.UnRegister();
            base.Dispose();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _mainTextboxLayoutManager.SetSize(Width, Height);
            _mainTextboxLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }

        public void LockChanged(object sender, NetworkUser currentUser)
        {
            
        }
    }
}
