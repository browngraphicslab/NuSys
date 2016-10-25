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
    class DetailViewPdfRegionContent : RectangleUIElement
    {
        /// <summary>
        /// The left page button for the pdf
        /// </summary>
        private ButtonUIElement _leftPageButton;

        /// <summary>
        /// The right page button for the pdf
        /// </summary>
        private ButtonUIElement _rightPageButton;

        /// <summary>
        /// The actual rectangle holding the image of the pdf itself
        /// </summary>
        private DetailViewPdfRegionRenderItem _pdfContent;

        public int CurrentPage => _pdfContent.CurrentPage;

        /// <summary>
        /// The pdf content data model associated with this pdf, used for changing pages
        /// </summary>
        private PdfContentDataModel _pdfContentDataModel;

        private StackLayoutManager _leftButtonLayoutManager;

        private StackLayoutManager _rightButtonLayoutManager;

        private StackLayoutManager _contentLayoutManager;

        public DetailViewPdfRegionContent(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, PdfLibraryElementController controller) : base(parent, resourceCreator)
        {
            // initialize the pdf content
            _pdfContent = new DetailViewPdfRegionRenderItem(this, resourceCreator, controller);
            _contentLayoutManager = new StackLayoutManager();
            _contentLayoutManager.AddElement(_pdfContent);
            AddChild(_pdfContent);

            // initailize the left page button
            _leftPageButton = new ButtonUIElement(this, resourceCreator, new EllipseUIElement(this, resourceCreator));
            _leftPageButton.Tapped += _leftPageButton_Tapped;
            _leftButtonLayoutManager = new StackLayoutManager();
            InitializeButtonUI(_leftPageButton);
            _leftButtonLayoutManager.AddElement(_leftPageButton);
            AddChild(_leftPageButton);

            // initialize the right page button
            _rightPageButton = new ButtonUIElement(this, resourceCreator, new EllipseUIElement(this, resourceCreator));
            _rightPageButton.Tapped += _rightPageButton_Tapped;
            InitializeButtonUI(_rightPageButton);
            _rightButtonLayoutManager = new StackLayoutManager();
            _rightButtonLayoutManager.AddElement(_rightPageButton);
            AddChild(_rightPageButton);
        }

        public void InitializeButtonUI(ButtonUIElement button)
        {
            button.Background = Colors.Black;
        }

        /// <summary>
        /// Called when the left page button is tapped, goes to the previous page
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _leftPageButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            GotoPage(_pdfContent.CurrentPage - 1);
        }

        /// <summary>
        /// Called when the right page button is tapped, goes to the next page
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _rightPageButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            GotoPage(_pdfContent.CurrentPage + 1);
        }

        /// <summary>
        /// Sets the page on the pdf
        /// </summary>
        /// <param name="page"></param>
        public async void GotoPage(int page)
        {
            _pdfContent.CurrentPage = page;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            var buttonWidth = 50;
            var buttonMargin = 5;

            _leftButtonLayoutManager.SetSize(buttonWidth + 2 * buttonMargin, Height);
            _leftButtonLayoutManager.SetMargins(buttonMargin, 0);
            _leftButtonLayoutManager.ItemHeight = buttonWidth;
            _leftButtonLayoutManager.VerticalAlignment = VerticalAlignment.Center;
            _leftButtonLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _leftButtonLayoutManager.ArrangeItems();

            _contentLayoutManager.SetSize(Width - 2 * _leftButtonLayoutManager.Width, Height);
            _contentLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _contentLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _contentLayoutManager.ArrangeItems(new Vector2(_leftButtonLayoutManager.Width, 0));

            _rightButtonLayoutManager.SetSize(_leftButtonLayoutManager.Width, Height);
            _rightButtonLayoutManager.SetMargins(buttonMargin, 0);
            _rightButtonLayoutManager.VerticalAlignment = VerticalAlignment.Center;
            _rightButtonLayoutManager.ItemHeight = buttonWidth;
            _rightButtonLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _rightButtonLayoutManager.ArrangeItems(new Vector2(_leftButtonLayoutManager.Width + _contentLayoutManager.Width, 0));


            base.Update(parentLocalToScreenTransform);
        }
    }
}
