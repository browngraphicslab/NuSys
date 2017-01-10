using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserHelper
{
    public class SiteScore
    {
        /// <summary>
        /// This is the ratio between the number of text tags and image tags in a website
        /// The rationale is that in wikipedia there are a lot of text tags as opposed to image tags
        /// A bad website might have more images(ads,etc) or may fall into a valley
        /// </summary>
        public double TextImageRatio;

        /// <summary>
        /// This is the ratio between the number of header tags and the text tags in a website
        /// If there's a lot of text compared to the number of headers then there could be some correlation
        /// </summary>
        public double HeaderTextRatio;

        /// <summary>
        /// If the average image size(area) is really small then we can assume that they are mostly ads 
        /// </summary>
        public double AverageImageSize;

        /// <summary>
        /// This is the average character count of each text block that is in the website, if they are really
        ///  large then we can assume that there is a lot of information 
        /// </summary>
        public double AverageTextBlockSize;

        /// <summary>
        /// This is the final goodness or badness score of a website, 0 represents the optimally bad website 
        /// and 1 represents the greatest website to ever exist(well not really but as the number increases
        /// they get better)
        /// </summary>
        public double Score;

        /// <summary>
        /// This gets fed into the classifier as the input and then the classifier will train itself or predict
        /// the final good or bad score of the website based on these stats
        /// </summary>
        /// <returns></returns>
        public List<double> GetScores()
        {
            return new List<double>() {TextImageRatio,HeaderTextRatio,AverageImageSize,AverageTextBlockSize};
        }

        public string ToString()
        {
            return "" + TextImageRatio + "," + HeaderTextRatio + "," + AverageTextBlockSize + "," + AverageImageSize +
                   "," + Score;
        }

        /// <summary>
        /// Use Object initializer syntax to create these
        /// </summary>
        public SiteScore() {}

    }
}
