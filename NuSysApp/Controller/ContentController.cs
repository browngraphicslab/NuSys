﻿using System;
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

        private Dictionary<string, NodeContentModel> _contents = new Dictionary<string, NodeContentModel>();
        private Dictionary<string, ManualResetEvent> _waitingNodeCreations = new Dictionary<string, ManualResetEvent>(); 
        public int Count
        {
            get { return _contents.Count; }
        }
    
        public NodeContentModel Get(string id)
        {
            return _contents.ContainsKey(id) ? _contents[id] : null;
        }

        public string Add( string contentData, string presetID = null)
        {
            var id = presetID ?? SessionController.Instance.GenerateId();
            var n = new NodeContentModel(contentData, id);
            _contents.Add(id, n );
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
            }
            Debug.WriteLine("content added with ID: "+id);
            return id;
        }

        public void AddWaitingNodeCreation(string id, ManualResetEvent mre)
        {
            _waitingNodeCreations.Add(id, mre);
        }

        public async Task Load()
        {
            _contents.Clear();
            
            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "_contents.nusys");
            var lines = await FileIO.ReadLinesAsync(file);

            foreach (var line in lines)
            {
                var o = JsonConvert.DeserializeObject<NodeContentModel>(line);
                _contents.Add(o.Id, o);
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
