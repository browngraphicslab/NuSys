using System;
using System.Collections.Generic;
using System.Drawing;
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
        private static async Task<OcrResults> GetOcrResultsAsync(string imageUrl)
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
        /// <param name="contentDataModelId"> The string id of the contentDataModel that this image analysis model is analyzing</param>
        /// <returns></returns>
        public static async Task<NusysImageAnalysisModel> GetNusysImageAnalysisModelFromUrlAsync(string ImgURL, string contentDataModelId)
        {
            var analysisResult = await GetAnalysisResultsAsync(ImgURL);

            // crazy object initializer syntax that converts an analysis result to a NuSysImageAnalysisModel. : )
            var a =  new NusysImageAnalysisModel(contentDataModelId)
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
                Faces = analysisResult.Faces?.Select(face => new CognitiveApiFaceModel {Age = face?.Age, Gender = face?.Gender, FaceRectangle = new CognitiveApiFaceRectangleModel {Height = ((double)(face?.FaceRectangle.Height) / (double)(analysisResult.Metadata.Height)), Width = ((double)(face?.FaceRectangle.Width)) / ((double)(analysisResult.Metadata.Width)), Left = ((double)(face?.FaceRectangle.Left)) / ((double)(analysisResult.Metadata.Width)), Top = ((double)(face?.FaceRectangle.Top)) / ((double)(analysisResult.Metadata.Height))}}).ToArray(),
                Description = new CognitiveApiDescriptionModel { Tags = analysisResult.Description?.Tags, Captions = analysisResult.Description?.Captions.Select(caption => new CognitiveApiCaptionModel { Confidence = caption.Confidence, Text = caption.Text }).ToArray() }
                
            };
            return a;
        }

        /// <summary>
        /// Performs Object Character Recognition on an ImgUrl and returns a NuSysOcrAnalysisModel
        /// 
        /// Supported image formats: JPEG, PNG, GIF, BMP.
        /// Image file size must be less than 4MB.
        /// Image dimensions must be between 40 x 40 and 3200 x 3200 pixels, and the image cannot be larger than 100 megapixels.
        /// </summary>
        /// <param name="ImgURL">The image url of the image to be analyzed</param>
        /// <param name="contentDataModelId">The string id of the contentDataModel that this image analysis model is analyzing</param>
        /// <param name="imageWidth">The width of the image to be analyzed</param>
        /// <param name="imageHeight">The height of theimage to be analyzed</param>
        /// <returns></returns>
        public static async Task<NuSysOcrAnalysisModel> GetNusysOcrAnalysisModelFromUrlAsync(string ImgURL, double imageWidth, double imageHeight)
        {
            var ocrResult = await GetOcrResultsAsync(ImgURL);

            // crazy object initializer syntax that converts an ocr result to a NuSysOcrAnalysisModel. : )
            var a = new NuSysOcrAnalysisModel()
            {
                // simply set the property of the nusysImageAnalysisModel using different paths in the analysisResult
                Language = ocrResult.Language,
                Orientation = ocrResult.Orientation,
                TextAngle = ocrResult.TextAngle,

                // These are slightly more complex because we are setting the value for each item in an array
                Regions = ocrResult.Regions?.Select(region => new CognitiveApiRegionModel
                {
                    Rectangle = new CognitiveApiRectangleModel
                    {
                        Height = region.Rectangle.Height / imageHeight,
                        Left = region.Rectangle.Left / imageWidth,
                        Top = region.Rectangle.Top / imageHeight,
                        Width = region.Rectangle.Width / imageWidth
                    },
                    Lines = region.Lines.Select(line => new CognitiveApiLineModel
                    {
                        Rectangle = new CognitiveApiRectangleModel
                        {
                            Height = region.Rectangle.Height / imageHeight,
                            Left = region.Rectangle.Left / imageWidth,
                            Top = region.Rectangle.Top / imageHeight,
                            Width = region.Rectangle.Width / imageWidth
                        },
                        Words = line.Words.Select(word => new CognitiveApiWordModel
                        {
                            Rectangle = new CognitiveApiRectangleModel
                            {
                                Height = region.Rectangle.Height / imageHeight,
                                Left = region.Rectangle.Left / imageWidth,
                                Top = region.Rectangle.Top / imageHeight,
                                Width = region.Rectangle.Width / imageWidth
                            },
                            Text = word.Text
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return a;
        }
    }
}