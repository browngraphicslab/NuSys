using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using NuSysApp.Components.Nodes.GroupNode;

namespace NuSysApp
{
    public class GroupNodeTimelineViewModel : ElementInstanceCollectionViewModel
    {
        //private List<Tuple<FrameworkElement, DateTime>> _atomList;
        //private AtomModel _nodeModel;

        public GroupNodeTimelineViewModel(ElementInstanceController model) : base(model)
        {
            
        }
    }
}