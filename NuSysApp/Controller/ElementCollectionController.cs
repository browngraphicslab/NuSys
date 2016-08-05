﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuSysApp.Tools;

namespace NuSysApp
{
    public class ElementCollectionController : ElementController
    {
        public delegate void CameraPositionChangedHandler(float x, float y);
        public event CameraPositionChangedHandler CameraPositionChanged;

        public delegate void CameraScaleChangedHandler(object source, float x);
        public event CameraScaleChangedHandler CameraScaleChanged;

        public event CameraPositionChangedHandler CameraCenterChanged;

        public delegate void ChildChangedHandler(object source, ElementController child);
        public event ChildChangedHandler ChildAdded;
        public event ChildChangedHandler ChildRemoved;

        public delegate void CollectionViewChangedHandler(object source, CollectionElementModel.CollectionViewType type);
        public event CollectionViewChangedHandler CollectionViewChanged;

        public ElementCollectionController(ElementModel model) : base(model)
        {
            var contentModel = SessionController.Instance.ContentController.GetContent(model.LibraryId);
            if (contentModel != null)
            {
                ((CollectionLibraryElementModel) contentModel).OnChildAdded += AddChildById;
                ((CollectionLibraryElementModel) contentModel).OnChildRemoved += RemoveChildById;
            }

            Disposed += OnDisposed;
        }

        private void OnDisposed(object source, object args)
        {
            var contentModel = SessionController.Instance.ContentController.GetContent(Model.LibraryId);
            if (contentModel != null)
            {
                ((CollectionLibraryElementModel)contentModel).OnChildAdded -= AddChildById;
                ((CollectionLibraryElementModel)contentModel).OnChildRemoved -= RemoveChildById;
            }

            Disposed -= OnDisposed;
        }

        private void AddChildById(string id)
        {
            if (SessionController.Instance.IdToControllers.ContainsKey(id))
            {
                AddChild(SessionController.Instance.IdToControllers[id]);
            }
        }

        private void RemoveChildById(string id)
        {
            if (SessionController.Instance.IdToControllers.ContainsKey(id))
            {
                RemoveChild(SessionController.Instance.IdToControllers[id]);
            }
        }
        public void AddChild( ElementController child )
        {
            ChildAdded?.Invoke(this, child);
        }

        public void RemoveChild(ElementController child)
        {
            ChildRemoved?.Invoke(this, child);
        }

        public void SetCameraPosition(float x, float y)
        {
            CameraPositionChanged?.Invoke(x,y);   
        }
        public void SetCameraCenter(float x, float y)
        {
            CameraCenterChanged?.Invoke(x, y);
        }

        public void SetCollectionViewType(CollectionElementModel.CollectionViewType type)
        {
            var colModel = (Model as CollectionElementModel);
            colModel.ActiveCollectionViewType = type;
            CollectionViewChanged?.Invoke(this, type);

            _debouncingDictionary.Add("collectionview", colModel.ActiveCollectionViewType.ToString());
        }
        
    }
}
