using System;
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

        public delegate void NewContentEventHandler(LibraryElementModel element);

        public event NewContentEventHandler OnNewContent;

        public delegate void ElementDeletedEventHandler(LibraryElementModel element);

        public event ElementDeletedEventHandler OnElementDelete;

        public int Count
        {
            get { return _contents.Count; }
        }

        public HashSet<string> IdList
        {
            get { return new HashSet<string>(_contents.Keys); }
        }

        public LibraryElementModel GetLibraryElementModel(string id)
        {
            Debug.Assert(id != null);
            return _contents.ContainsKey(id) ? _contents[id] : null;
        }

        public LibraryElementController GetLibraryElementController(string id)
        {
            if (id == null)
            {
                return null;
            }
            return _contentControllers.ContainsKey(id) ? _contentControllers[id] : null;
        }

        public ICollection<LibraryElementModel> ContentValues
        {
            get { return new List<LibraryElementModel>(_contents.Values); }
        }

        /// <summary>
        /// returns whether the queried content data model exists lcoally.   
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

                OnNewContent?.Invoke(model);
                return model.LibraryElementId;
            }
            Debug.WriteLine("content failed to add directly due to invalid id");
            return null;
        }

        /// <summary>
        /// Add a newly added library element model to any controllers. i.e. regionsController, linksController
        /// </summary>
        /// <param name="model"></param>
        private void AddModelToControllers(LibraryElementModel model)
        {

            if (NusysConstants.IsRegionType(model.Type))
            {
                Debug.Assert(model is Region);
                SessionController.Instance.RegionsController.AddRegion(model as Region);
            }
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
            OnElementDelete?.Invoke(model);
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

        //use this method to clear every contentData model from this controller
        public void ClearAllContentDataModels()
        {
            _contentDataModels.Clear();
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
            _contentDataModels.TryAdd(contentDataModel.ContentId, contentDataModel);
            return true;
        }
    }
}
