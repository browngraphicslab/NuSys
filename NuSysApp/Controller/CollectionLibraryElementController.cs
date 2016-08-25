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

        protected override void OnSessionControllerEnterNewCollectionStarting(object sender, string newCollectionLibraryId)
        {
            (LibraryElementModel as CollectionLibraryElementModel)?.Children?.Clear();
            base.OnSessionControllerEnterNewCollectionStarting(sender, newCollectionLibraryId);
        }

        /// <summary>
        /// this method is used to set the CollectionLibraryElementModel's ShapePoints property.
        /// This method will set the model, send a server call with the update, and also will eventually fire an event.
        /// </summary>
        /// <param name="newPoints"></param>
        public void SetCollectionPoints(List<PointModel> newPoints)
        {
            //tODO add in the event firing
            Debug.Assert(newPoints != null);
            Debug.Assert(CollectionModel != null);//check the state of values being used

            CollectionModel.ShapePoints = newPoints;
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPED_POINTS_LIST_KEY, newPoints);
            }
        }

        /// <summary>
        /// this method is used to update the collection library element model's IsFinite boolean.
        /// This will set the model's property, update the server, and eventually will fire an event for this change
        /// </summary>
        /// <param name="isFiniteValue"></param>
        public void SetFiniteBoolean(bool isFiniteValue)
        {
            //tODO add in the event firing
            Debug.Assert(CollectionModel != null);
            CollectionModel.IsFinite = isFiniteValue;
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_FINITE_BOOLEAN_KEY, isFiniteValue);
            }
        }
    }
}
