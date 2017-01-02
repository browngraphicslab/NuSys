using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// PieChartElement holds the item that represents the piece (e.g., a string "Republican") and its value (e.g., an int 49)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PieChartElement<T>
    {
        /// <summary>
        /// Angle where DrawPie starts drawing the arc that represents the piece of the pie.
        /// </summary>
        public float StartAngle
        {
            set;get;
        }
        /// <summary>
        /// Angle where the DrawPie stops drawing the arc that represents the piece of the pie
        /// </summary>
        public float EndAngle
        {
            set;
            get;
        }
        /// <summary>
        /// The EndAngle - StartAngle
        /// </summary>
        public float SweepAngle
        {
            set;get;

        }
        /// <summary>
        /// Item that represents this piece of the pie
        /// </summary>
        public T Item { set; get; }
        /// <summary>
        /// Value that this piece of the pie holds. NOT A PERCENTAGE
        /// </summary>
        public int Value { set; get; }

        public PieChartElement()
        {

        }
        /// <summary>
        /// IsHit checks if the angle passed in is between the Start and End angles.
        /// </summary>
        /// <param name="theta"></param>
        /// <returns></returns>
        public bool IsHit(double theta)
        {
            //theta is from 0 to 2pi
            if (theta <= EndAngle && theta >= StartAngle )
            {
                return true;
            }

            return false;
        }
    }
}
