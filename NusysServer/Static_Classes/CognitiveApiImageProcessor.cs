using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace NusysServer
{

    /// <summary>
    /// Static class to process images using microsoft cognitive services
    /// </summary>
    public static class CognitiveApiImageProcessor
    {

        /// <summary>
        /// Returns the AnalysisResult from Microsoft Cognitive Services Analysing an Image URL
        /// 
        /// use this to find
        ///     categories - taxonomical description of what is in the image
        ///     tags - textual representation of what might be in the iomage
        ///     faces - rectangles containing faces which are found in the image
        ///     description - a single sentence description of the image  
        /// </summary>
        /// <param name="ImgURL">The url of the image to be analyzed, cannot be localhost:...</param>
        /// <returns></returns>
        public static async Task<AnalysisResult> GetAnalysisResults(string ImgURL)
        {
            // Create Project Oxford Computer Vision API Service client
            VisionServiceClient VisionServiceClient = new VisionServiceClient(CognitiveApiConstants.COMPUTER_VISION);

            // Analyze the image for all visual features
            VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Categories, VisualFeature.Description, VisualFeature.Faces, VisualFeature.Tags };
            AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(ImgURL, visualFeatures);
            //Todo build a model for these analysis results
            return analysisResult;
        }

        /// <summary>
        /// Returns the Object Character Recognition Results from Microsoft Cognitive Services applying OCR to an Image URL
        /// 
        /// use this to find
        ///     text that is in an image, i.e. a picture of a whiteboard. works best with type font text not handwriting
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public static async Task<OcrResults> GetOcrResults(string imageUrl)
        {
            // Create Project Oxford Vision API Service client
            VisionServiceClient VisionServiceClient = new VisionServiceClient(CognitiveApiConstants.COMPUTER_VISION);

            // Perform OCR on the given url
            OcrResults ocrResult = await VisionServiceClient.RecognizeTextAsync(imageUrl);
            //Todo build a model for these analysis results
            return ocrResult;
        }
    }
}