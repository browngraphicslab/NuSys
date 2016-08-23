using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    /// <summary>
    /// These are two buttons that are inside the detail views of Images, PDFs, Audios, Videos.
    /// 
    /// They control which (if any) regions are visible in the detail view.
    /// </summary>
    public sealed partial class ShowHideRegionButtons : UserControl
    {
        public RegionsVisibility CurrentRegionsVisibility => _currentRegionsVisibility;
        private RegionsVisibility _currentRegionsVisibility;

        //RectangleWrapper or AudioWrapper
        public IRegionHideable Wrapper { set; get; }

        /// <summary>
        /// ShowAll describes when all regions are visible.
        /// HideAll describes when all regions are invisible. There is no visual difference between this and HideOnlyChildren
        /// ShowOnlyChildren describes when only direct descendant regions are visible.
        /// HideOnlyChildren describes when all regions are invisible. Once again, no difference between this and Hide All.
        /// 
        /// The reason we have both HideAll and HideOnlyChildren is to make sure the correct regions are visible
        /// when the Show/Hide regions button is clicked.
        /// </summary>
        public enum RegionsVisibility { ShowAll, HideAll, ShowOnlyChildren, HideOnlyChildren }

        /// <summary>
        /// set width of the ui element from another class
        /// </summary>
        public static readonly DependencyProperty SetWidthProperty = DependencyProperty.RegisterAttached("SetWidth",
            typeof(double), typeof(ShowHideRegionButtons), null);

        public ShowHideRegionButtons()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Show all regions when this is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowRegionsOnTapped(object sender, TappedRoutedEventArgs e)
        {
            Wrapper.ShowAllRegions();
            _currentRegionsVisibility = RegionsVisibility.ShowAll;
        }

        /// <summary>
        /// Hide all regions when this is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HideRegionsOnTapped(object sender, TappedRoutedEventArgs e)
        {
            Wrapper.HideAllRegions();
            _currentRegionsVisibility = RegionsVisibility.HideAll;
        }

        /// <summary>
        /// Show only children regions when this is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowChildrenOnTapped(object sender, TappedRoutedEventArgs e)
        {
            Wrapper.ShowOnlyChildrenRegions();
            _currentRegionsVisibility = RegionsVisibility.ShowOnlyChildren;
        }

        public double SetWidth
        {
            set { RegionsOptionsBox.Width = value; }
        }
    }
}
