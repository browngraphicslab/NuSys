using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class CognitiveApiCategoryModel
    {
        /// <summary>
        /// One of 86 possible taxonomy based categories
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The Confidence level for the category, 0 is not confident, 1 is very confident
        /// </summary>
        public double? Score { get; set; }
    }
}
