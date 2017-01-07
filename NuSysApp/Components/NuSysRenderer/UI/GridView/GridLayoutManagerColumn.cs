using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GridLayoutManagerColumn
    {
        /// <summary>
        /// Fired whenever the relative width of this GridViewColumn is changed
        /// </summary>
        public EventHandler<float> RelativeWidthChanged;

        /// <summary>
        /// Fired whenever the minimum width of this GridViewRow is changed
        /// </summary>
        public EventHandler<float> MinWidthChanged;

        /// <summary>
        /// private helper variable for public property RelativeWidth
        /// </summary>
        private float _relativeWidth { get; set; }

        /// <summary>
        /// The relative width of the column. Calculated by summing the relative widths of all the columns
        /// dividing the entire width of the grid by that sum. Then applying to each column the result
        /// of that division multiplied by this relative width
        /// </summary>
        public float RelativeWidth
        {
            get { return _relativeWidth; }
            set
            {
                _relativeWidth = value;
                RelativeWidthChanged?.Invoke(this, RelativeWidth);
            }
        }

        /// <summary>
        /// private helper variable for public property MinWidth
        /// </summary>
        private float _minWidth { get; set; }

        /// <summary>
        /// The minimum width defaults to zero. Set this if there is some minimum width needed to display
        /// content. The minimum width of the grid is the sum of the minimum widths of all the grid's columns
        /// </summary>
        public float MinWidth
        {
            get { return _minWidth; }
            set
            {
                _minWidth = value;
                MinWidthChanged?.Invoke(this, MinWidth);
            }
        }

        /// <summary>
        /// This is the Width of the column, but it should not be set by the user, it is set and used by the 
        /// GridView.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// The left x axis of the column. This shoudl not be set by the user, it is set and used by the GridView
        /// </summary>
        public float Left { get; set; }

        /// <summary>
        /// The right x axis of the column. 
        /// </summary>
        public float Right => Left + Width;

        /// <summary>
        /// Creates a new GridViewColumn with the specified relativeWidth and minWidth
        /// </summary>
        /// <param name="relativeWidth"></param>
        /// <param name="minWidth"></param>
        public GridLayoutManagerColumn(float relativeWidth, float minWidth = 0)
        {
            RelativeWidth = relativeWidth;
            MinWidth = minWidth;
        }
    }
}
