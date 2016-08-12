using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using NusysIntermediate;

namespace NusysServer
{

    /// <summary>
    /// Static class to process images using microsoft cognitive services
    /// </summary>
    public static class ImageProcessor
    {

        /// <summary>
        /// Returns the AnalysisResult from Microsoft Cognitive Services Analysing an Image URL
        /// 
        /// Supported input methods: image URL.
        /// Supported image formats: JPEG, PNG, GIF, BMP.
        /// Image file size: Less than 4MB.
        /// Image dimension: Greater than 50 x 50 pixels
        /// </summary>
        /// <param name="ImgURL">The url of the image to be analyzed, cannot be localhost:...</param>
        /// <returns></returns>
        private static async Task<AnalysisResult> GetAnalysisResultsAsync(string ImgURL)
        {
            // Create Project Oxford Computer Vision API Service client
            VisionServiceClient VisionServiceClient = new VisionServiceClient(CognitiveApiConstants.COMPUTER_VISION);

            // Analyze the image for all visual features
            VisualFeature[] visualFeatures = { VisualFeature.Adult, VisualFeature.Categories, VisualFeature.Color, VisualFeature.Description, VisualFeature.Faces, VisualFeature.Tags};
            AnalysisResult analysisResult = await VisionServiceClient.AnalyzeImageAsync(ImgURL, visualFeatures);
            return analysisResult;
        }

        /// <summary>
        /// Returns the Object Character Recognition Results from Microsoft Cognitive Services applying OCR to an Image URL
        /// 
        /// Supported input methods: image URL.
        /// Supported image formats: JPEG, PNG, GIF, BMP.
        /// Image file size: Less than 4MB.
        /// Image dimension: Greater than 50 x 50 pixels
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        private static async Task<OcrResults> GetOcrResults(string imageUrl)
        {
            // Create Project Oxford Vision API Service client
            VisionServiceClient VisionServiceClient = new VisionServiceClient(CognitiveApiConstants.COMPUTER_VISION);

            // Perform OCR on the given url
            OcrResults ocrResult = await VisionServiceClient.RecognizeTextAsync(imageUrl);
            return ocrResult;
        }

        /// <summary>
        /// Performs CongitiveAnalysis on an ImgUrl, and returns a NusysImageAnalysisModel
        /// 
        /// Supported input methods: image URL.
        /// Supported image formats: JPEG, PNG, GIF, BMP.
        /// Image file size: Less than 4MB.
        /// Image dimension: Greater than 50 x 50 pixels
        /// </summary>
        /// <param name="ImgURL"></param>
        /// <returns></returns>
        public static async Task<NusysImageAnalysisModel> GetNusysImageAnalysisModelFromUrlAsync(string ImgURL)
        {
            var analysisResult = await GetAnalysisResultsAsync(ImgURL);

            // crazy object initializer syntax that converts an analysis result to a NuSysImageAnalysisModel. : )
            return new NusysImageAnalysisModel
            {
                // simply set the property of the nusysImageAnalysisModel using different paths in the analysisResult
                AccentColor = analysisResult.Color?.AccentColor,
                AdultScore = analysisResult.Adult?.AdultScore,
                DominantColorBackground = analysisResult.Color?.DominantColorBackground,
                DominantColorForeground = analysisResult.Color?.DominantColorForeground,
                FileType = analysisResult.Metadata?.Format,
                Height = analysisResult.Metadata?.Height,
                Width = analysisResult.Metadata?.Width,
                IsAdultContent = analysisResult.Adult?.IsAdultContent,
                IsRacyContent = analysisResult.Adult?.IsRacyContent,
                RacyScore = analysisResult.Adult?.RacyScore,

                // These are slightly more complex because we are setting the value for each item in an array
                Categories = analysisResult.Categories?.Select(category => new CognitiveApiCategoryModel { Name = category?.Name, Score = category?.Score }).ToArray(),
                Tags = analysisResult.Tags?.Select(tag => new CognitiveApiTagModel { Confidence  = tag?.Confidence, Hint = tag?.Hint, Name = tag?.Name}).ToArray(),
                Faces = analysisResult.Faces?.Select(face => new CognitiveApiFaceModel {Age = face?.Age, Gender = face?.Gender, FaceRectangle = new CognitiveApiFaceRectangleModel {Height = face?.FaceRectangle.Height / analysisResult.Metadata.Height, Width = face?.FaceRectangle.Width / analysisResult.Metadata.Width, Left = face?.FaceRectangle.Left / analysisResult.Metadata.Width, Top = face?.FaceRectangle.Top / analysisResult.Metadata.Height}}).ToArray(),
                Description = new CognitiveApiDescriptionModel { Tags = analysisResult.Description?.Tags, Captions = analysisResult.Description?.Captions.Select(caption => new CognitiveApiCaptionModel { Confidence = caption.Confidence, Text = caption.Text }).ToArray() }
                
            };
        }
    }
}