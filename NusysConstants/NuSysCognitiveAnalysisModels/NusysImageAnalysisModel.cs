using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class NusysImageAnalysisModel : AnalysisModel
    {
        /// <summary>
        /// constructor requires that a content Data model Id be set.  
        /// Pass in the Id of the content data model that this analysis model analyzes.
        /// </summary>
        /// <param name="contentDataModelId"></param>
        public NusysImageAnalysisModel(string contentDataModelId) : base(contentDataModelId, NusysConstants.ContentType.Image){ }

        /// <summary>
        /// The total height of the image in pixels
        /// </summary>
        public int? Height { get; set; }
        /// <summary>
        /// The total width of the image in pixels
        /// </summary>
        public int? Width { get; set; }
        /// <summary>
        /// The filetype of the image, either  JPEG, PNG, GIF, BMP.
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// Hexadecimal string representation of the accent color in the image
        /// </summary>
        public string AccentColor { get; set; }
        /// <summary>
        /// black, blue, brown, grey, green, orange, pink, purple, red, white, yellow, and teal
        /// 
        /// Represents the dominant foregroundColor of the image
        /// </summary>
        public string DominantColorForeground { get; set; }
        /// <summary>
        /// black, blue, brown, grey, green, orange, pink, purple, red, white, yellow, and teal
        /// 
        /// Represents the dominant backgroundColor of the image
        /// </summary>
        public string DominantColorBackground { get; set; }
        /// <summary>
        /// True if Microsoft thinks this is adult content
        /// </summary>
        public bool? IsAdultContent { get; set; }
        /// <summary>
        /// True if Microsoft thinks this is racy content
        /// </summary>
        public bool? IsRacyContent { get; set; }
        /// <summary>
        /// A value from 0 to 1 where 0 is not adult, and 1 is adult
        /// </summary>
        public double? AdultScore { get; set; }
        /// <summary>
        /// A value from 0 to 1 where 0 is not racy, and 1 is racy
        /// </summary>
        public double? RacyScore { get; set; }
        /// <summary>
        /// Taxonomy Based Categories that classify the image
        /// </summary>
        public CognitiveApiCategoryModel[] Categories { get; set; }
        /// <summary>
        /// A content based summary of what is in the image. Specifically not hierarchical as opposed to categories.
        /// </summary>
        public CognitiveApiTagModel[] Tags { get; set; }
        /// <summary>
        /// Contains list of human readable sentences and confidence values, describing what is in the image
        /// Also contains a list of tags which are the basis of the human readable sentences
        /// </summary>
        public CognitiveApiDescriptionModel Description { get; set; }
        /// <summary>
        /// An array of faces found in the image
        /// </summary>
        public CognitiveApiFaceModel[] Faces { get; set; }
    }
}
