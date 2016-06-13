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

namespace NuSysApp
{
    public class ContentController
    {

        private ConcurrentDictionary<string, LibraryElementModel> _contents = new ConcurrentDictionary<string, LibraryElementModel>();
        private ConcurrentDictionary<string, LibraryElementController> _contentControllers = new ConcurrentDictionary<string, LibraryElementController>();
        //private Dictionary<string, ManualResetEvent> _waitingNodeCreations = new Dictionary<string, ManualResetEvent>(); 

        public delegate void NewContentEventHandler(LibraryElementModel element);
        public event NewContentEventHandler OnNewContent;

        public delegate void ElementDeletedEventHandler(LibraryElementModel element);
        public event ElementDeletedEventHandler OnElementDelete;
        public int Count
        {
            get { return _contents.Count; }
        }
    
        public LibraryElementModel GetContent(string id)
        {
            return _contents.ContainsKey(id) ? _contents[id] : null;
        }
        public LibraryElementController GetLibraryElementController(string id)
        {
            return _contentControllers.ContainsKey(id) ? _contentControllers[id] : null;
        }
        public ICollection<LibraryElementModel> ContentValues
        {
            get { return new List<LibraryElementModel>(_contents.Values); }
        } 
        public bool ContainsAndLoaded(string id)
        {
            return _contentControllers.ContainsKey(id) ? _contentControllers[id].IsLoaded : false;
        }
        public string Add(LibraryElementModel model)
        {
            if (!String.IsNullOrEmpty(model.LibraryElementId) && !_contents.ContainsKey(model.LibraryElementId))
            {
                _contents.TryAdd(model.LibraryElementId, model);
                var controller = new LibraryElementController(model);
                _contentControllers.TryAdd(model.LibraryElementId, controller);
                Debug.WriteLine("content directly added with ID: " + model.LibraryElementId);
                OnNewContent?.Invoke(model);
                return model.LibraryElementId;
            }
            Debug.WriteLine("content failed to add directly due to invalid id");
            return null;
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
                _contentControllers[model.LibraryElementId] = new LibraryElementController(model);
                return model.LibraryElementId;
            }
            return null;
        }
    }
}
