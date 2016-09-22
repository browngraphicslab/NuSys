using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// The nusys Intermediate model for saving colors.  
    /// Simply will hold the double R,G,B,A values for the color;
    /// </summary>
    public class ColorModel
    {
        /// <summary>
        /// the double Red value of the color.
        /// </summary>
        /// 
        public double R { get; set; }
        /// <summary>
        /// the double Green value of the color.
        /// </summary>
        public double G { get; set; }

        /// <summary>
        /// the double Blue value of the color.
        /// </summary>
        public double B { get; set; }

        /// <summary>
        /// the double Alpha value of the color.
        /// </summary>
        public double A { get; set; }
    }
}
