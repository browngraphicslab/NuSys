using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ImageAnalysisUIElement : RectangleUIElement
    {


        public ImageAnalysisUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) : base(parent, resourceCreator)
        {
            Task.Run(async delegate
            {
                _analysisModel = await SessionController.Instance.NuSysNetworkSession.FetchAnalysisModelAsync(vm.LibraryElementController.LibraryElementModel.ContentDataModelId) as NusysImageAnalysisModel;
                UITask.Run(async delegate {
                    SetImageAnalysis();
                });
            });
        }


        /// <summary>
        /// Set information gained from cognitive image analysis.
        /// </summary>
        private void SetImageAnalysis()
        {
            if (_analysisModel != null)
            {
                //set description to caption with highest confidence
                var descriptionlist = _analysisModel.Description.Captions.ToList();
                var bestDescription = descriptionlist.OrderByDescending(x => x.Confidence).FirstOrDefault();
                xDescription.Text = bestDescription.Text;

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
                    xCategories.Text = string.Join(", ", categories.Select(category => string.Join(", ", category.Name)));
                }


                //get tag list and order them in order of confidence
                var taglist = _analysisModel.Tags?.ToList().OrderByDescending(x => x.Confidence);
                //add to items control of suggested tags
                foreach (var i in taglist)
                {
                    var tag = MakeSuggestedTag(i.Name);
                    xTags?.Items?.Add(tag);
                }
            }

        }

    }
}
