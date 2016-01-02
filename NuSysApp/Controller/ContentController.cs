using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;

namespace NuSysApp
{
    
    public class ContentController
    {

        private Dictionary<string, NodeContentModel> _contents = new Dictionary<string, NodeContentModel>();

        public string Add( string contentData )
        {
            var id = Guid.NewGuid().ToString("N");
            var n = new NodeContentModel(contentData, id);
            _contents.Add(id, n );
            return id;
        }

        public async Task Load()
        {
            _contents.Clear();
            
            var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
            var lines = await FileIO.ReadLinesAsync(file);
;
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
            FileIO.WriteLinesAsync(file, lines);
        }
    }
}
