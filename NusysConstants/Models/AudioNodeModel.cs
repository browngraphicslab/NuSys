using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.Storage.Streams;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class AudioNodeModel : ElementModel
    { 
        private StorageFile _audioFile;
        public delegate void JumpEventHandler(TimeSpan time);
        public event JumpEventHandler OnJump;
        public AudioNodeModel(string id) : base(id)
        {
            ElementType = NusysConstants.ElementType.Audio;
        }

        public void Jump(TimeSpan time)
        {
            OnJump?.Invoke(time);
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
            await base.UnPack(props);
        }
    }
}

