using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageAnalysisUIElement : RectangleUIElement
    {
        /// <summary>
        /// The image analysis model we are going to use
        /// </summary>
        private NusysImageAnalysisModel _analysisModel;

        /// <summary>
        /// The header for the description test
        /// </summary>
        private TextboxUIElement _descriptionHeader;

        /// <summary>
        /// The actual description text
        /// </summary>
        private TextboxUIElement _descriptionText;

        /// <summary>
        /// The header for the categories text
        /// </summary>
        private TextboxUIElement _categoriesHeader;

        /// <summary>
        /// The actual categories text
        /// </summary>
        private TextboxUIElement _categoriesText;

        /// <summary>
        /// The header for the tags text
        /// </summary>
        private TextboxUIElement _tagsHeader;

        /// <summary>
        /// The actual tags text
        /// </summary>
        private TextboxUIElement _tagsText;

        /// <summary>
        /// the actual text layout manager
        /// </summary>
        private StackLayoutManager _textLayoutManager;

        public ImageAnalysisUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) : base(parent, resourceCreator)
        {
            //initialize the layout manager
            _textLayoutManager = new StackLayoutManager(StackAlignment.Vertical);

            // add the description header
            _descriptionHeader = new TextboxUIElement(this, resourceCreator);
            _descriptionHeader.Text = "Description: ";
            AddChild(_descriptionHeader);
            _textLayoutManager.AddElement(_descriptionHeader);
            SetHeaderUI(_descriptionHeader);

            // add the description text
            _descriptionText = new TextboxUIElement(this, resourceCreator);
            AddChild(_descriptionText);
            _textLayoutManager.AddElement(_descriptionText);
            SetTextUI(_descriptionText);

            // add the categories header
            _categoriesHeader = new TextboxUIElement(this, resourceCreator);
            _categoriesHeader.Text = "Categories: ";
            AddChild(_categoriesHeader);
            _textLayoutManager.AddElement(_categoriesHeader);
            SetHeaderUI(_categoriesHeader);

            // add the categories text
            _categoriesText = new TextboxUIElement(this, resourceCreator);
            AddChild(_categoriesText);
            _textLayoutManager.AddElement(_categoriesText);
            SetTextUI(_categoriesText);

            // add the tags header
            _tagsHeader = new TextboxUIElement(this, resourceCreator);
            _tagsHeader.Text = "Tags: ";
            AddChild(_tagsHeader);
            _textLayoutManager.AddElement(_tagsHeader);
            SetHeaderUI(_tagsHeader);

            // add the tags text
            _tagsText = new TextboxUIElement(this, resourceCreator);
            AddChild(_tagsText);
            _textLayoutManager.AddElement(_tagsText);
            SetTextUI(_tagsText);
            

            Task.Run(async delegate
            {
                // fetch the image analysis model
                _analysisModel = await SessionController.Instance.NuSysNetworkSession.FetchAnalysisModelAsync(controller.LibraryElementModel.ContentDataModelId) as NusysImageAnalysisModel;
                UITask.Run(async delegate {
                    // set the image analysis variables using the fetched image analysis model
                    SetImageAnalysis();
                });
            });
        }

        /// <summary>
        /// Sets the ui for headers
        /// </summary>
        /// <param name="header"></param>
        private void SetHeaderUI(TextboxUIElement header)
        {
            header.TextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            header.TextVerticalAlignment = CanvasVerticalAlignment.Center;
            header.Background = Colors.Azure;
            header.TextColor = Colors.DarkSlateGray;
        }

        /// <summary>
        /// Sets the ui for text elements
        /// </summary>
        /// <param name="text"></param>
        private void SetTextUI(TextboxUIElement text)
        {
            text.TextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            text.TextVerticalAlignment = CanvasVerticalAlignment.Center;
            text.Background = Colors.Azure;
            text.TextColor = Colors.DarkSlateGray;

        }


        /// <summary>
        /// Set information gained from cognitive image analysis.
        /// </summary>
        private void SetImageAnalysis()
        {
            Debug.Assert(_analysisModel != null,
                "This should never be called with a null analysis model, we await in the constructor follow that pattern");


            //set description to caption with highest confidence
            var descriptionlist = _analysisModel.Description.Captions.ToList();
            var bestDescription = descriptionlist.OrderByDescending(x => x.Confidence).FirstOrDefault();
            _descriptionText.Text = bestDescription.Text;

            if (_analysisModel.Categories != null && _analysisModel.Categories.Any())
            {
                //get categories and add the category if the score meets min confidence level
                var categorylist = _analysisModel.Categories.ToList();
                var categories =
                    categorylist.Where(x => x.Score > Constants.MinConfidence).OrderByDescending(x => x.Score);
                foreach (var i in categories)
                {
                    i.Name = i.Name.Replace("_", " ");
                    i.Name.Trim();
                }
                _categoriesText.Text = string.Join(", ", categories.Select(category => string.Join(", ", category.Name)));
            }


            //get tag list and order them in order of confidence
            var taglist = _analysisModel.Tags?.ToList().OrderByDescending(x => x.Confidence);
            //add to items control of suggested tags
            _tagsText.Text = string.Join(", ", taglist.Select(tag => string.Join(", ", tag.Name)));
        }

        /// <summary>
        /// Update the layout
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _textLayoutManager.SetSize(Width, Height);
            _textLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _textLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _textLayoutManager.Spacing = 5;
            _textLayoutManager.SetMargins(20);
            _textLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
