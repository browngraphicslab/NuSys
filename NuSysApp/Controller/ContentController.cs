﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    public class ContentController
    {
        private ConcurrentDictionary<string, LibraryElementModel> _contents =
            new ConcurrentDictionary<string, LibraryElementModel>();

        private ConcurrentDictionary<string, LibraryElementController> _contentControllers =
            new ConcurrentDictionary<string, LibraryElementController>();

        //private Dictionary<string, ManualResetEvent> _waitingNodeCreations = new Dictionary<string, ManualResetEvent>();
        private ConcurrentDictionary<string, ContentDataModel> _contentDataModels =
            new ConcurrentDictionary<string, ContentDataModel>();

        private ConcurrentDictionary<string, ContentDataController> _contentDataControllers =
            new ConcurrentDictionary<string, ContentDataController>();

        /// <summary>
        /// the dictionary of content data mode id to analysis model
        /// </summary>
        private ConcurrentDictionary<string, AnalysisModelController> _analysisModels = new ConcurrentDictionary<string, AnalysisModelController>();

        /// <summary>
        /// delegate for the OnNewLibraryElement.
        /// It defines a single new library element being passed in
        /// </summary>
        /// <param name="libraryElement"></param>
        public delegate void NewLibraryElementEventHandler(LibraryElementModel libraryElement);

        /// <summary>
        /// Event fired whenever a new Library element is added to this session. 
        /// It will pass the new library element as a parameter
        /// </summary>
        public event NewLibraryElementEventHandler OnNewLibraryElement;

        /// <summary>
        /// the delegate for the OnNewContent event
        /// </summary>
        /// <param name="model"></param>
        public delegate void NewContentEventHandler(ContentDataModel model);

        /// <summary>
        /// event fired when a new contentDataModel is added.  
        /// It passes the new contentDataModel as an argument. 
        /// </summary>
        /// <param name="model"></param>
        public event NewContentEventHandler OnNewContent;


        public delegate void ElementDeletedEventHandler(LibraryElementModel element);

        public event ElementDeletedEventHandler OnLibraryElementDelete;

        public int Count
        {
            get { return _contents.Count; }
        }

        /// <summary>
        /// Returns all the library elment models for every item in the library
        /// </summary>
        public HashSet<LibraryElementModel> AllLibraryElementModels
        {
            get { return new HashSet<LibraryElementModel>(_contents.Values); }
        }

        /// <summary>
        /// Returns all the library element controllers for every item in the library
        /// </summary>
        public HashSet<LibraryElementController> AllLibraryElementControllers
        {
            get { return new HashSet<LibraryElementController>(_contentControllers.Values);}
        }

        public HashSet<string> IdList
        {
            get { return new HashSet<string>(_contents.Keys); }
        }

        /// <summary>
        /// Returns a library element model given the passed in library element model id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LibraryElementModel GetLibraryElementModel(string id)
        {
            Debug.Assert(id != null);
            return _contents.ContainsKey(id) ? _contents[id] : null;
        }

        /// <summary>
        /// Returns a library element controller associated with the passed in library element model id
        /// </summary>
        /// <param name="libraryElementModelId"></param>
        /// <returns></returns>
        public LibraryElementController GetLibraryElementController(string libraryElementModelId)
        {
            if (libraryElementModelId == null)
            {
                return null;
            }
            return _contentControllers.ContainsKey(libraryElementModelId) ? _contentControllers[libraryElementModelId] : null;
        }

        public ICollection<LibraryElementModel> ContentValues
        {
            get { return new List<LibraryElementModel>(_contents.Values); }
        }

        /// <summary>
        /// returns whether the queried content data model exists locally.   
        /// If not, that content is not yet loaded.  
        /// This should replace the "containsAndLoaded" method. 
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <returns></returns>
        public bool ContainsContentDataModel(string contentDataModelId)
        {
            Debug.Assert(contentDataModelId != null);
            return _contentDataModels.ContainsKey(contentDataModelId);
        }

        /// <summary>
        /// will create and add a LibraryElementModel based off a message.  
        /// This message will probably be from the server.  
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public LibraryElementModel CreateAndAddModelFromMessage(Message message)
        {
            Debug.Assert(
                SessionController.Instance.ContentController.GetLibraryElementModel(
                    message.GetString(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY)) == null);
            var model = LibraryElementModelFactory.CreateFromMessage(message);


            var controller = LibraryElementControllerFactory.CreateFromModel(model);
            Debug.Assert(controller != null);
            _contentControllers.TryAdd(model.LibraryElementId, controller);

            controller.UnPack(message);
            Add(model);

            return model;
        }

        public string Add(LibraryElementModel model)
        {
            if (!String.IsNullOrEmpty(model.LibraryElementId) && !_contents.ContainsKey(model.LibraryElementId))
            {
                _contents.TryAdd(model.LibraryElementId, model);

                var controller = LibraryElementControllerFactory.CreateFromModel(model);
                Debug.Assert(controller != null);
                _contentControllers.TryAdd(model.LibraryElementId, controller);

                AddModelToControllers(model);

                OnNewLibraryElement?.Invoke(model);
                return model.LibraryElementId;
            }
            Debug.WriteLine("content failed to add directly due to invalid id");
            return null;
        }

        /// <summary>
        /// Add a newly created library element model to any controllers. i.e. regionsController, linksController
        /// </summary>
        /// <param name="model"></param>
        private void AddModelToControllers(LibraryElementModel model)
        {
            SessionController.Instance.RegionsController.AddRegion(model);
            if (model.Type == NusysConstants.ElementType.Link)
            {
                var linkController =
                    SessionController.Instance.ContentController.GetLibraryElementController(model.LibraryElementId) as
                        LinkLibraryElementController;
                Debug.Assert(linkController != null);
                SessionController.Instance.LinksController.AddLinkLibraryElementController(linkController);
                SessionController.Instance.LinksController.CreateVisualLinks(linkController);
            }
        }

        public bool Remove(LibraryElementModel model)
        {
            if (!_contents.ContainsKey(model.LibraryElementId))
            {
                return false;
            }
            LibraryElementModel removedElement;
            LibraryElementController removedController;
            _contentControllers.TryRemove(model.LibraryElementId, out removedController);
            _contents.TryRemove(model.LibraryElementId, out removedElement);
            OnLibraryElementDelete?.Invoke(model);
            return true;
        }

        public string OverWrite(LibraryElementModel model)
        {
            if (!String.IsNullOrEmpty(model.LibraryElementId))
            {
                _contents[model.LibraryElementId] = model;
                _contentControllers[model.LibraryElementId] = LibraryElementControllerFactory.CreateFromModel(model);
                return model.LibraryElementId;
            }
            return null;
        }


        /// <summary>
        /// returns null if the content doesn't exist
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public ContentDataModel GetContentDataModel(string contentId)
        {
            Debug.Assert(contentId != null);
            return _contentDataModels.ContainsKey(contentId) ? _contentDataModels[contentId] : null;
        }

        /// <summary>
        /// Gets the content data controller class for a contentDataModel Id that you pass in 
        /// returns null if the content controller doesn't exist
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public ContentDataController GetContentDataController(string contentId)
        {
            Debug.Assert(contentId != null);
            return _contentDataControllers.ContainsKey(contentId) ? _contentDataControllers[contentId] : null;
        }

        //use this method to clear every contentData model from this controller
        public void ClearAllContentDataModels()
        {
            _contentDataModels.Clear();
        }

        //use this method to clear every contentData controller from this controller
        public void ClearAllContentDataControllers()
        {
            _contentDataControllers.Clear();
        }

        /// <summary>
        /// should be used to add (and therefore 'load') all content data models.  
        /// They should be recieved fully populated from the server. 
        /// </summary>
        /// <param name="contentDataModel"></param>
        /// <returns></returns>
        public bool AddContentDataModel(ContentDataModel contentDataModel)
        {
            if (_contentDataModels.ContainsKey(contentDataModel.ContentId))
            {
                return false;
            }

            // Here we create the controller for this content data model and add it to the ContentController's dictionary
            var contentDataController = ContentDataControllerFactory.CreateFromContentDataModel(contentDataModel);
            SessionController.Instance.ContentController.AddContentDataController(contentDataController);

            _contentDataModels.TryAdd(contentDataModel.ContentId, contentDataModel);

            OnNewContent?.Invoke(contentDataModel);

            return true;
        }

        /// <summary>
        /// should be used to add (and therefore 'load') all content data models.  
        /// They should be recieved fully populated from the server. 
        /// </summary>
        /// <param name="contentDataModel"></param>
        /// <returns></returns>
        public bool AddContentDataController(ContentDataController contentDataController)
        {
            if (_contentDataModels.ContainsKey(contentDataController.ContentDataModel.ContentId))
            {
                return false;
            }
            _contentDataControllers.TryAdd(contentDataController.ContentDataModel.ContentId, contentDataController);
            return true;
        }

        /// <summary>
        /// returns whether the analysis model for the given content data model is present locally.  
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <returns></returns>
        public bool HasAnalysisModel(string contentDataModelId)
        {
            Debug.Assert(!string.IsNullOrEmpty(contentDataModelId));
            return _analysisModels.ContainsKey(contentDataModelId);
        }

        /// <summary>
        /// method to add OR OVERWRITER  an analysis model locally.  
        /// You must also pass in a contentDataModel Id because THE MODELS YOU SAVE CAN BE NULL.  
        /// When it stores a null value, it means that it doesn't have one.  
        /// </summary>
        /// <param name="model"></param>
        public void AddAnalysisModel(AnalysisModel model, string contentDataModelId)
        {
            Debug.Assert(!string.IsNullOrEmpty(contentDataModelId));
            _analysisModels[contentDataModelId] = new AnalysisModelController(model);
        }

        /// <summary>
        /// method used to fetch the analysis model for a given content data model. 
        /// You must make sure it is present before using this method.  
        /// You can be sure it is already present by calling HasAnalysisModel(string contentDataModelId). 
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <returns></returns>
        public AnalysisModelController GetAnalysisModel(string contentDataModelId)
        {
            Debug.Assert(HasAnalysisModel(contentDataModelId));
            return _analysisModels[contentDataModelId];
        }

        /// <summary>
        /// method to remove an analysis model locally.  
        /// Returns a bool indicating whether it was present;
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <returns></returns>
        public bool RemoveAnalysisModel(string contentDataModelId)
        {
            AnalysisModelController outAnalysisModel;
            return _analysisModels.TryRemove(contentDataModelId, out outAnalysisModel);
        }
    }
}
