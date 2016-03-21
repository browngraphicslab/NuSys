using System;
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

        private Dictionary<string, LibraryElementModel> _contents = new Dictionary<string, LibraryElementModel>();
        //private Dictionary<string, ManualResetEvent> _waitingNodeCreations = new Dictionary<string, ManualResetEvent>(); 

        public delegate void NewContentEventHandler(LibraryElementModel element);
        public event NewContentEventHandler OnNewContent;

        public delegate void ElementDeletedEventHandler(LibraryElementModel element);
        public event ElementDeletedEventHandler OnElementDelete;
        public int Count
        {
            get { return _contents.Count; }
        }
    
        public LibraryElementModel Get(string id)
        {
            return _contents.ContainsKey(id) ? _contents[id] : null;
        }

        public ICollection<LibraryElementModel> Values
        {
            get { return new List<LibraryElementModel>(_contents.Values); }
        } 
        public bool ContainsAndLoaded(string id)
        {
            return _contents.ContainsKey(id) ? _contents[id].Loaded : false;
        }
        public string Add(LibraryElementModel model)
        {
            if (!String.IsNullOrEmpty(model.Id) && !_contents.ContainsKey(model.Id))
            {
                _contents.Add(model.Id, model);
                Debug.WriteLine("content directly added with ID: " + model.Id);
                OnNewContent?.Invoke(model);
                return model.Id;
            }
            Debug.WriteLine("content failed to add directly due to invalid id");
            return null;
        }
        public string Add( string contentData, ElementType elementType, string presetID = null)
        {
            var id = presetID ?? SessionController.Instance.GenerateId();
            var n = new LibraryElementModel(id, elementType);
            _contents.Add(id, n );
            OnNewContent?.Invoke(n);
            /*
            if (presetID != null)
            {
                foreach (var kvp in _waitingNodeCreations)
                {
                    if (kvp.Key == id)
                    {
                        kvp.Value.Set();
                        _waitingNodeCreations.Remove(kvp.Key);
                        break;
                    }
                }
            }*/
            Debug.WriteLine("content added with ID: "+id);
            return id;
        }
        /*
        public void AddWaitingNodeCreation(string id, ManualResetEvent mre)
        {
            _waitingNodeCreations.Add(id, mre);
        }*/

        public bool Remove(LibraryElementModel model)
        {
            if (!_contents.ContainsKey(model.Id))
            {
                return false;
            }
            _contents.Remove(model.Id);
            OnElementDelete?.Invoke(model);
            return true;
        }
        public string OverWrite(LibraryElementModel model)
        {
            if (!String.IsNullOrEmpty(model.Id))
            {
                _contents[model.Id] = model;
                return model.Id;
            }
            return null;
        }
        public async Task Load()
        {
            _contents.Clear();
            
            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "_contents.nusys");
            var lines = await FileIO.ReadLinesAsync(file);

            foreach (var line in lines)
            {
                var o = JsonConvert.DeserializeObject<LibraryElementModel>(line);

                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(o.Id, o.Data,o.Type));
                /*
                var request = new NewContentSystemRequest(o.Id,o.Data);//TODO not ideal
                await SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequestLocally(request);*/
            }
        }

        public async Task Save()
        {
            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "_contents.nusys");
            var lines = _contents.Values.Select(s => JsonConvert.SerializeObject(s) );
            await FileIO.WriteLinesAsync(file, lines);
        }
    }
}
