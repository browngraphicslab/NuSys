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
    public class ElementCollectionInstanceViewModel: ElementInstanceViewModel
    {
        
        public ObservableCollection<FrameworkElement> AtomViewList { get; set; } 
        protected INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
       
        public ElementCollectionInstanceViewModel(ElementCollectionInstanceController controller): base(controller)
        {
            controller.ChildAdded += OnChildAdded;
            controller.ChildRemoved += OnChildRemoved;
            
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            AtomViewList = new ObservableCollection<FrameworkElement>();

            
        }

        public override void Dispose()
        {
            var controller = (ElementCollectionInstanceController) Controller;
            controller.ChildAdded -= OnChildAdded;
            controller.ChildRemoved -= OnChildRemoved;
            base.Dispose();
        }

        public void RemoveChild(string id)
        {
            // TODO: refactor remove this method
        }

        private async void OnChildAdded(object source, ElementInstanceController elementInstanceController)
        {
            var view = await _nodeViewFactory.CreateFromSendable(elementInstanceController, AtomViewList.ToList());   
            AtomViewList.Add(view);

            var model = elementInstanceController.Model;
            if (model.ContentId != null )
            {
                if (SessionController.Instance.ContentController.Get(model.ContentId) == null)
                {

                    if (SessionController.Instance.LoadingDictionary.ContainsKey(model.ContentId))
                    {
                        SessionController.Instance.LoadingDictionary[model.ContentId]?.Add(elementInstanceController);
                    }
                    else
                    {
                        SessionController.Instance.LoadingDictionary[model.ContentId] =
                            new List<ElementInstanceController>() {elementInstanceController};
                    }
                }
                else
                {
                    elementInstanceController.FireContentLoaded(
                        SessionController.Instance.ContentController.Get(model.ContentId));
                }
            }
        }

        private void OnChildRemoved(object source, ElementInstanceController elementInstanceController)
        {
            AtomViewList.Remove(AtomViewList.Where(a => ((ElementInstanceViewModel)a.DataContext).Id == elementInstanceController.Model.Id).First());
        }
    }
}