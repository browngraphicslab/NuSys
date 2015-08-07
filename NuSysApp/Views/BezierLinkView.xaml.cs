using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BezierLinkView : UserControl
    {
        public BezierLinkView(LinkViewModel vm)
        {
            this.InitializeComponent();
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