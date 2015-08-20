using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp.ViewModels
{
    interface ILinkable
    {
        void ToggleSelection();
       
        void AddLink();
        void Remove();
        void UpdateAnchor();
        ObservableCollection<LinkViewModel> LinkList { get; set; }
        WorkspaceViewModel WorkSpaceViewModel { get; }
        bool IsSelected { get; set; }
        UserControl View {get; set; }
        MatrixTransform Transform { get; set; }
        GroupViewModel ParentGroup { get; set; }
        Point Anchor { get; set; }//TODO get rid of this
        int AnchorX { get; set; }
        int AnchorY { get; set; }
        string AtomType { get; set; }

    }
}
