using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    public class CollectionLibraryElementController : LibraryElementController
    {
        public HashSet<string> InkLines;

        public delegate void InkEventHandler(string id);
        public event InkEventHandler OnInkAdded;
        public event InkEventHandler OnInkRemoved;

        public delegate void ChildAddedEventHandler(string id);
        public event ChildAddedEventHandler OnChildAdded;

        public delegate void ChildRemovedEventHandler(string id);
        public event ChildRemovedEventHandler OnChildRemoved;


        public CollectionLibraryElementModel CollectionModel
        {
            get
            {
                return base.LibraryElementModel as CollectionLibraryElementModel;
            }
        }

        public CollectionLibraryElementController(CollectionLibraryElementModel collectionLibraryElementModel) : base(collectionLibraryElementModel)
        {
            InkLines = new HashSet<string>();
        }
        public void AddInk(string id)
        {
            InkLines.Add(id);
            OnInkAdded?.Invoke(id);
        }

        public void RemoveInk(string id)
        {
            InkLines.Remove(id);
            OnInkAdded?.Invoke(id);
        }

        public bool AddChild(string id)
        {
            if (!CollectionModel.Children.Contains(id))
            {
                var elementController = SessionController.Instance.IdToControllers[id];
                elementController.Deleted += ElementControllerOnDeleted;

                CollectionModel.Children.Add(id);
                OnChildAdded?.Invoke(id);
                return true;
            }
            return false;
        }


        private void ElementControllerOnDeleted(object source)
        {
            var elementController = (ElementController)source;
            CollectionModel.Children.Remove(elementController.Model.Id);
        }


        public bool RemoveChild(string id)
        {
            if (CollectionModel.Children.Contains(id))
            {
                var elementController = SessionController.Instance.IdToControllers[id];
                elementController.Deleted -= ElementControllerOnDeleted;
                CollectionModel.Children.Remove(id);
                OnChildRemoved?.Invoke(id);
                return true;
            }
            return false;
        }

    }
}
