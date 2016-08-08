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
    public static class ImageProcessor
    {

        /// <summary>
        /// Returns the AnalysisResult from Microsoft Cognitive Services Analysing an Image URL
        /// </summary>
        /// <param name="ImgURL"></param>
        /// <returns></returns>
        public static async Task<AnalysisResult> GetAnalysisResults(string ImgURL)
        {
            // Create Project Oxford Computer Vision API Service client
            VisionServiceClient VisionServiceClient = new VisionServiceClient(CognitiveServicesConstants.COMPUTER_VISION);

            // Analyze the image for all visual features
            VisualFeature[] visualFeatures = new VisualFeature[] { VisualFeature.Categories, VisualFeature.Description, VisualFeature.Faces, VisualFeature.Tags };
            AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(ImgURL, visualFeatures);
            return analysisResult;
        }

        /// <summary>
        /// Returns the Object Character Recognition Results from Microsoft Cognitive Services applying OCR to an Image URL
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public static async Task<OcrResults> GetOcrResults(string imageUrl)
        {
            // Create Project Oxford Vision API Service client
            VisionServiceClient VisionServiceClient = new VisionServiceClient(CognitiveServicesConstants.COMPUTER_VISION);

            // Perform OCR on the given url
            OcrResults ocrResult = await VisionServiceClient.RecognizeTextAsync(imageUrl);
            return ocrResult;
        }
    }
}