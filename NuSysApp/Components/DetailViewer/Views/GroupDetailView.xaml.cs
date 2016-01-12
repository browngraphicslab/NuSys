using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class GroupDetailView : UserControl
    {

        private ObservableCollection<FrameworkElement> _views;
        private FreeFormNodeViewFactory _factory;
        //private GroupItemThumbFactory _factory;
        private int _count = 0;

        public GroupDetailView(NodeContainerViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;

            NodeContainerModel model = (NodeContainerModel)vm.Model;

            _views = new ObservableCollection<FrameworkElement>();

            _factory = new FreeFormNodeViewFactory();
            //_factory = new GroupItemThumbFactory();
            this.AddChildren();
        }

        public async Task AddChildren()
        {
            var vm = (NodeContainerViewModel) DataContext;
            var allNodes = SessionController.Instance.IdToSendables.Values;
            var modelList = new ObservableCollection<AtomModel>();
            foreach (var sendable in allNodes)
            {
                var node = (AtomModel) sendable;
                var groups = (List<string>) node.GetMetaData("groups");
                if (groups.Contains(vm.Id))
                {
                    modelList.Add(node);
                }
            }

            foreach (var model in modelList)
            {
                //var foundViews =
                //    SessionController.Instance.ActiveWorkspace.AtomViewList.Where(
                //        s => { (s.DataContext as AtomModel).Id == model.Id);

                //var view = (IThumbnailable)foundViews.First();
                //var thumb = await view.ToThumbnail(300, 300);
                //var img = new Image();
                //img.Source = thumb;
                Sendable nodeModel = SessionController.Instance.IdToSendables[model.Id];
                var view = await _factory.CreateFromSendable(nodeModel, null);
                var viewVm = (AtomViewModel)view.DataContext;
                viewVm.X = 0;
                viewVm.Y = 0;
                _views.Add(view);
            }

            var numCols = 2;
            var numRows = 4;

            

            foreach (FrameworkElement view in _views)
            {
                Border wrapping = new Border();
                wrapping.Padding = new Thickness(10);
                wrapping.Child = view;
                Grid.SetRow(wrapping, _count / numRows);
                Grid.SetColumn(wrapping, _count % numCols);
                xGrid.Children.Add(wrapping);
                _count++;
            }
        }

    }
}
