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
        public ObservableDictionary<string, UserControl> Children { get; }

        public ObservableCollection<UserControl> AtomViewList { get; set; } 

        protected INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
        public delegate Task ChildAddedHandler(object source, AnimatableUserControl node);
        public event ChildAddedHandler ChildAdded;
        public bool EnableChildMove { get; set; }
       
        public NodeContainerViewModel(NodeContainerModel model): base(model)
        {
            Children = new ObservableDictionary<string, UserControl>();
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            model.ChildAdded += OnChildAdded;
            model.ChildRemoved += OnChildRemoved;
            AtomViewList = new ObservableCollection<UserControl>();


        }

        public async Task Init()
        {
            var model = Model as NodeContainerModel;
            foreach (var sendable in SessionController.Instance.IdToSendables.Values.Where(s => (s as AtomModel).Creator == model.Id))
            {
                await model.AddChild(sendable);
            }
        }

        public override void Dispose()
        {
            var model = (NodeContainerModel)Model;
            model.ChildAdded += OnChildAdded;
            model.ChildRemoved += OnChildRemoved;
            base.Dispose();
        }

        public override void Translate(double dx, double dy)
        {
            base.Translate(dx, dy);

            if (!EnableChildMove)
                return;

            foreach (var sendable in Children.Values)
            {
                var nodeVm = (NodeViewModel)sendable.DataContext;
                nodeVm.Translate(dx, dy);
            }
        }

        protected virtual async Task OnChildAdded(object source, Sendable nodeModel)
        {
            var view = await _nodeViewFactory.CreateFromSendable(nodeModel, Children.Values.ToList());
            Children.Add(nodeModel.Id, view);
            AtomViewList.Add(view);

            var handler = ChildAdded;
            if (handler != null)
            {
                var tasks = handler.GetInvocationList().Cast<ChildAddedHandler>().Select(s => s(this, (AnimatableUserControl)view));
                await Task.WhenAll(tasks);
            }
        }

        protected virtual async Task OnChildRemoved(object source, Sendable sendable)
        {
            if (sendable is InqLineModel)
            {
                var inqLineModel = (InqLineModel) sendable;
                inqLineModel.Delete();
                return;
            }

            //var view = AtomViewList.Values.Where((a => { var vm = (AtomViewModel)a.DataContext; return vm.Model == sendable; }));
           // if (view.Count() > 0)
           //     AtomViewList.Remove(view.First());
        }
    }
}