using System;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BezierLinkView : UserControl
    {
        public BezierLinkView(LinkViewModel vm)
        {
            this.InitializeComponent();
            this.ManipulationMode = ManipulationModes.All;
            this.DataContext = vm;
            //Universal apps does not support multiple databinding, so this is a workarround. 
            vm.Atom1.PropertyChanged += new PropertyChangedEventHandler(atom_PropertyChanged);
            vm.Atom2.PropertyChanged += new PropertyChangedEventHandler(atom_PropertyChanged);
            this.UpdateControlPoints();
            Canvas.SetZIndex(this, -2);//temporary fix to make sure events are propagated to nodes
        }

        /// <summary>
        /// Gets called every time either one of the atoms that this link binds to has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void atom_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateControlPoints();
        }

        /// <summary>
        /// Updates the location of the bezier controlpoints. 
        /// Do not call this method outside of this class.
        /// </summary>
        private void UpdateControlPoints()
        {
            var vm = (LinkViewModel) this.DataContext;
            var atom1 = vm.Atom1;
            var atom2 = vm.Atom2;
            var anchor1 = atom1.Anchor;
            var anchor2 = atom2.Anchor;
            var distanceX = anchor1.X - anchor2.X;

            curve.Point2 = new Point(anchor1.X - distanceX/2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX/2, anchor1.Y);
            this.UpdateEndPoints();
        }

        private void UpdateEndPoints()
        {
            var vm = (LinkViewModel)this.DataContext;
            var atom1 = vm.Atom1;
            var atom2 = vm.Atom2;

            if (atom1.AtomType == Constants.Node)
            {
                pathfigure.StartPoint = this.findIntersection(atom1, curve.Point3);
            }
            else //atom is a link - this link anchors to atom1's anchor point
            {
                pathfigure.StartPoint = atom1.Anchor;
            }
            if (atom2.AtomType == Constants.Node)
            {
                curve.Point3 = this.findIntersection(atom2, pathfigure.StartPoint);
            }
            else //atom2 is a link - this link anchors to atom2's anchor (midpoint)
            {
                curve.Point3 = atom2.Anchor;
            }


        }

        ///<summary>CalcY returns the y coord of the intersection between two lines 
        /// Use for finding the intersection between a line and the right/left edges of a square
        /// </summary>
        private double calcY(double xVal, double x0, double y0, double x1, double y1)
        {
            if (x0 == x1) //vertical line
            {
                return y0;
            }
            return y0 + (xVal - x0) * (y1 - y0) / (x1 - x0);
        }

        ///<summary> calcX returns the x coord of the intersection between two lines
        /// Use for finding the intersection between a line and the top/bottom edges of a square
        /// </summary
        private double calcX(double yVal, double x0, double y0, double x1, double y1)
        {
            return x0 + (yVal - y0) * (x1 - x0) / (y1 - y0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="atom"> Atom whose edge the link will bind to</param>
        /// <param name="endpoint">The other endpoint of the bezier curve</param>
        /// <returns></returns>
        private Point findIntersection(AtomViewModel atom, Point endpoint)
        {
            //Coords of rectangle
            double topY = atom.Anchor.Y + (.5 * atom.Height);
            double bottomY = atom.Anchor.Y - (.5 * atom.Height);
            double leftX = atom.Anchor.X - (.5 * atom.Width);
            double rightX = atom.Anchor.X + (.5 * atom.Width);

            //anchor coords of atom
            double x0 = atom.Anchor.X;
            double y0 = atom.Anchor.Y;

            //other endpoint
            double x1 = endpoint.X;
            double y1 = endpoint.Y;

            //intersection values of line with rectangle
            double topXIntersect = calcX(topY, x0, y0, x1, y1);
            double bottomXIntersect = calcX(bottomY, x0, y0, x1, y1);
            double leftYIntersect = calcY(leftX, x0, y0, x1, y1);
            double rightYIntersect = calcY(rightX, x0, y0, x1, y1);

            Point newEndPt = new Point();


            if (topXIntersect <= rightX && topXIntersect >= leftX && ((topY >= y1 && topY <= y0) || (topY <= y1 && topY >= y0) || (topY >= y1 && topY >= y0))) //intersects with top of square
            {
                newEndPt= new Point(topXIntersect, topY);
            }

            if (bottomXIntersect <= rightX && bottomXIntersect >= leftX && ((bottomY >= y1 && bottomY <= y0) || (bottomY <= y1 && bottomY >= y0) || (bottomY <= y1 && bottomY <= y0))) //intersects with bottom of square
            {
                if(this.calcDistance(newEndPt, endpoint) > this.calcDistance(new Point(bottomXIntersect, bottomY), endpoint))
                {
                    newEndPt = new Point(bottomXIntersect, bottomY);
                }
            }

            if (rightYIntersect <= topY && rightYIntersect >= bottomY && ((rightX >= x1 && rightX <= x0) || (rightX <= x1 && rightX >= x0) || (rightX >= x1 && rightX >= x0)))  //intersects with right of square
            {
                newEndPt = new Point(rightX, rightYIntersect);
            }

            if (leftYIntersect <= topY && leftYIntersect >= bottomY && ((leftX >= x1 && leftX <= x0) || (leftX <= x1 && leftX >= x0) || (leftX <= x1 && leftX <= x0))) //intersects with left of square
            {
                if (this.calcDistance(newEndPt, endpoint) > this.calcDistance(new Point(leftX, leftYIntersect), endpoint))
                {
                    newEndPt = new Point(leftX, leftYIntersect);
                }
            }

            return newEndPt;
        }

        private double calcDistance(Point pt1, Point pt2)
        {
            return Math.Sqrt(Math.Pow(pt1.X - pt2.X, 2) + Math.Pow(pt1.Y - pt2.Y, 2));
        }

        private void BezierLinkView_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = (LinkViewModel) this.DataContext;
            vm.ToggleSelection();
            e.Handled = true;
        }

        /// <summary>
        /// This handler makes sure that double tap events don't get interpreted as single tap events first.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BezierLinkView_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true; 
        }
    }
}