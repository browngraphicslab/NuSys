using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace NuSysApp
{
    public class ElementInstanceCollectionViewModel: ElementInstanceViewModel
    {
        public ObservableDictionary<string, FrameworkElement> Children { get; }
        
        public ObservableCollection<FrameworkElement> AtomViewList { get; set; } 

        protected INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
        public delegate Task ChildAddedHandler(object source, FrameworkElement node);
        public event ChildAddedHandler ChildAdded;
        public bool EnableChildMove { get; set; }
       
        public ElementInstanceCollectionViewModel(ElementInstanceController controller): base(controller)
        {
            Children = new ObservableDictionary<string, FrameworkElement>();
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));

            // TODO: refactor
            //model.ChildAdded += OnChildAdded;
            //model.ChildRemoved += OnChildRemoved;
            AtomViewList = new ObservableCollection<FrameworkElement>();
        }

        public async Task Init()
        {
     
        }

        public override void Dispose()
        {
            var model = (NodeContainerModel)Model;
            model.ChildAdded -= OnChildAdded;
            model.ChildRemoved -= OnChildRemoved;
            base.Dispose();
        }

        public void RemoveChild(string id)
        {
            var child = Children[id];
            AtomViewList.Remove(child);
            Children.Remove(id);
        }

        protected virtual async Task OnChildAdded(object source, Sendable nodeModel)
        {
            if (!Children.ContainsKey(nodeModel.Id))
            {
                var view = await _nodeViewFactory.CreateFromSendable(nodeModel, Children.Values.ToList());
            
                Children.Add(nodeModel.Id, view);
                AtomViewList.Add(view);

                if (view is AreaNodeView)
                {
                    Canvas.SetZIndex(view, -1);
                }

                var handler = ChildAdded;
                if (handler != null)
                {
                    var tasks = handler.GetInvocationList().Cast<ChildAddedHandler>().Select(s => s(this, (FrameworkElement)view));
                    await Task.WhenAll(tasks);
                }

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