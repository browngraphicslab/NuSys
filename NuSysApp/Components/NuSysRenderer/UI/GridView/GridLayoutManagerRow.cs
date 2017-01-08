using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GridLayoutManagerRow
    {
        /// <summary>
        /// Fired whenever the relative height of this GridViewRow is changed
        /// </summary>
        public EventHandler<float> RelativeHeightChanged;

        /// <summary>
        /// Fired whenever the minimum height of this GridViewRow is changed
        /// </summary>
        public EventHandler<float> MinHeightChanged;

        /// <summary>
        /// private helper variable for public property RelativeHeight
        /// </summary>
        private float _relativeHeight { get; set; }

        /// <summary>
        /// The relative height of the row. Calculated by summing the relative heights of all the rows
        /// dividing the entire height of the grid by that sum. Then applying to each row the result
        /// of that division multiplied by this relative height
        /// </summary>
        public float RelativeHeight
        {
            get {return _relativeHeight;}
            set
            {
                _relativeHeight = value;
                RelativeHeightChanged?.Invoke(this, RelativeHeight);
            }
        }

        /// <summary>
        /// private helper variable for public property MinHeight
        /// </summary>
        private float _minHeight { get; set; }

        /// <summary>
        /// The minimum height defaults to zero. Set this if there is some minimum height needed to display
        /// content. The minimum height of the grid is the sum of the minimum heights of all the grid's columns
        /// </summary>
        public float MinHeight
        {
            get { return _minHeight; }
            set
            {
                _minHeight = value;
                MinHeightChanged?.Invoke(this, MinHeight);
            }
        }

        /// <summary>
        /// This is the Height of the row, but it should not be set by the user, it is set and used by the 
        /// GridView.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// The top y axis of the row. This should not be set by the user, it is set and used by the GridView
        /// </summary>
        public float Top { get; set; }

        /// <summary>
        /// The bottom y axis of the column. 
        /// </summary>
        public float Bottom => Top + Height;

        /// <summary>
        /// Creates a new GridViewRow with the specified relativeHeight and minHeight
        /// </summary>
        /// <param name="relativeWidth"></param>
        /// <param name="minWidth"></param>
        public GridLayoutManagerRow(float relativeHeight, float minHeight = 0)
        {
            RelativeHeight = relativeHeight;
            MinHeight = minHeight;
        }
    }
}
