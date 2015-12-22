using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace NuSysApp
{
    public class NodeContainerViewModel: NodeViewModel
    {
        public ObservableCollection<UserControl> AtomViewList { get; }

        protected INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
        public delegate Task ChildAddedHandler(object source, AnimatableNodeView node);
        public event ChildAddedHandler ChildAdded;
        public bool EnableChildMove { get; set; }
        

        public NodeContainerViewModel(NodeContainerModel model): base(model)
        {
            AtomViewList = new ObservableCollection<UserControl>();
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            model.ChildAdded += OnChildAdded;
            model.ChildRemoved += OnChildRemoved;
        }

        public override void Translate(double dx, double dy)
        {
            base.Translate(dx, dy);

            if (!EnableChildMove)
                return;

            foreach (var sendable in AtomViewList)
            {
                var nodeVm = (NodeViewModel)sendable.DataContext;
                nodeVm.Translate(dx, dy);
            }
        }

        protected virtual async Task OnChildAdded(object source, Sendable nodeModel)
        {
            var view = await _nodeViewFactory.CreateFromSendable(nodeModel, AtomViewList.ToList());
            AtomViewList.Add(view);
            ChildAdded?.Invoke(this, (AnimatableNodeView)view);
        }

        protected  virtual async Task OnChildRemoved(object source, Sendable sendable)
        {
            if (sendable is InqLineModel)
            {
                var inqLineModel = (InqLineModel) sendable;
                inqLineModel.Delete();
                return;
            }

            var view = AtomViewList.Where((a => { var vm = (AtomViewModel)a.DataContext; return vm.Model == sendable; }));
            if (view.Count() > 0)
                AtomViewList.Remove(view.First());
        }
    }
}