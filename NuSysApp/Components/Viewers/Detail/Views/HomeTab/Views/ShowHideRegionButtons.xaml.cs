using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        public ShowHideRegionButtons()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Event handler for when ShowHideAllRegions button is clicked. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xShowHideAllRegionsButton_Click(object sender, RoutedEventArgs e)
        {

            //Changes visibility of regions, update button text of show/hide button and current visibility state.
            switch (_currentRegionsVisibility)
            {
                case RegionsVisibility.ShowAll:
                    xShowHideAllRegionsButtonText.Text = "Show Regions";
                    Wrapper.HideAllRegions();
                    _currentRegionsVisibility = RegionsVisibility.HideAll;
                    break;
                case RegionsVisibility.ShowOnlyChildren:
                    xShowHideAllRegionsButtonText.Text = "Show Regions";
                    Wrapper.HideAllRegions();
                    _currentRegionsVisibility = RegionsVisibility.HideOnlyChildren;  
                    break;
                case RegionsVisibility.HideAll:
                    xShowHideAllRegionsButtonText.Text = "Hide Regions";
                    Wrapper.ShowAllRegions();
                    _currentRegionsVisibility = RegionsVisibility.ShowAll;
                    break;
                case RegionsVisibility.HideOnlyChildren:
                    xShowHideAllRegionsButtonText.Text = "Hide Regions";
                    Wrapper.ShowOnlyChildrenRegions();
                    _currentRegionsVisibility = RegionsVisibility.ShowOnlyChildren;
                    break;
            }

        }

        /// <summary>
        /// Event handler for when Show/Hide only children button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xShowOnlyChildrenButton_Click(object sender, RoutedEventArgs e)
        {

            //Based on updated visibility of regions, update showonlychildren button text.
            switch (_currentRegionsVisibility)
            {
                case RegionsVisibility.ShowAll:
                    xShowOnlyChildrenButtonText.Text = "Show All Regions";
                    Wrapper.ShowOnlyChildrenRegions();
                    _currentRegionsVisibility = RegionsVisibility.ShowOnlyChildren;
                    break;
                case RegionsVisibility.ShowOnlyChildren:
                    xShowOnlyChildrenButtonText.Text = "Show Only Children Regions";
                    Wrapper.ShowAllRegions();
                    _currentRegionsVisibility = RegionsVisibility.ShowAll;
                    break;
                case RegionsVisibility.HideAll:
                    xShowOnlyChildrenButtonText.Text = "Show All Regions";
                    _currentRegionsVisibility = RegionsVisibility.HideOnlyChildren;
                    break;
                case RegionsVisibility.HideOnlyChildren:
                    xShowOnlyChildrenButtonText.Text = "Show Only Children Regions";
                    _currentRegionsVisibility = RegionsVisibility.HideAll;
                    break;
            }
        }
    }
}
