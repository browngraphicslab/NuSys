using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace NuSysApp
{
    public class ElementCollectionViewModel: ElementViewModel
    {
        
        public ObservableCollection<FrameworkElement> AtomViewList { get; set; } 
        protected INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
       
        public ElementCollectionViewModel(ElementCollectionController controller): base(controller)
        {
            controller.ChildAdded += OnChildAdded;
            controller.ChildRemoved += OnChildRemoved;
            
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));

            AtomViewList = new ObservableCollection<FrameworkElement>();
        }

        public override void Dispose()
        {
            var controller = (ElementCollectionController) Controller;
            controller.ChildAdded -= OnChildAdded;
            controller.ChildRemoved -= OnChildRemoved;
            base.Dispose();
        }

        private async void OnChildAdded(object source, ElementController elementController)
        {
            var view = await _nodeViewFactory.CreateFromSendable(elementController);   
            AtomViewList.Add(view);

            elementController.Deleted += OnChildDeleted;
        }

        private void OnChildDeleted(object source)
        {
            var c = (ElementCollectionController) Controller;
            c.RemoveChild((ElementController)source);
        }

        private void OnChildRemoved(object source, ElementController elementController)
        {
            var soughtChildren = AtomViewList.Where(a => ((ElementViewModel) a.DataContext).Id == elementController.Model.Id);
            if (soughtChildren.Any())
            {
                AtomViewList.Remove( soughtChildren.First());
            }
        }
    }
}