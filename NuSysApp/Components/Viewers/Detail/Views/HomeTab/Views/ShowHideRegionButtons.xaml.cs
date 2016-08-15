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
    public sealed partial class ShowHideRegionButtons : UserControl
    {

        private RegionsVisibility _currentRegionsVisibility;


        public IRegionHideable Wrapper { set; get; }
        public enum RegionsVisibility { ShowAll, HideAll, ShowOnlyChildren, HideOnlyChildren }

        public ShowHideRegionButtons()
        {
            this.InitializeComponent();
        }

        private void xShowHideAllRegionsButton_Click(object sender, RoutedEventArgs e)
        {

            //Based on updated visibility of regions, update button text of show/hide button and current visibility state.
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
