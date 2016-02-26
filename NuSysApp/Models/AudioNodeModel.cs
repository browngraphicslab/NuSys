using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;

namespace NuSysApp
{
    public class AudioNodeModel : ElementModel
    {
        private readonly StorageFolder _rootFolder = NuSysStorages.Media;
        private StorageFile _audioFile;
        public AudioNodeModel(string id) : base(id)
        {
            ElementType = ElementType.Audio;
        }

        public string FileName { get; set; }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            props["fileName"] = FileName;
            return props;
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("fileName"))
            {
                FileName = props.GetString("fileName");
            }
            base.UnPack(props);
        } 
    }
}
