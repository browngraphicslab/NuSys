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

            var model = elementController.Model;
            if (model.ContentId != null )
            {
                if (!SessionController.Instance.ContentController.ContainsAndLoaded(model.ContentId))
                {

                    if (SessionController.Instance.LoadingDictionary.ContainsKey(model.ContentId))
                    {
                        SessionController.Instance.LoadingDictionary[model.ContentId]?.Add(elementController);
                    }
                    else
                    {
                        SessionController.Instance.LoadingDictionary[model.ContentId] =
                            new List<ElementController>() {elementController};
                    }
                }
                else
                {
                    elementController.FireContentLoaded(
                        SessionController.Instance.ContentController.Get(model.ContentId));
                }
            }
        }

        private void OnChildDeleted(object source)
        {
            var c = (ElementCollectionController) Controller;
            c.RemoveChild((ElementController)source);
        }

        private void OnChildRemoved(object source, ElementController elementController)
        {
            AtomViewList.Remove(AtomViewList.Where(a => ((ElementViewModel)a.DataContext).Id == elementController.Model.Id).First());
        }
    }
}