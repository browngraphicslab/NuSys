using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace NuSysApp
{
    public class VideoNodeModel : NodeModel
    {
        private byte[] _byteArray;
        private InMemoryRandomAccessStream _recording;
        public VideoNodeModel(byte[] byteArray, string id) : base(id)
        {
            ByteArray = byteArray;
            Recording = new InMemoryRandomAccessStream();
        }
        public InMemoryRandomAccessStream Recording
        {
            get {return _recording;}
            set {_recording = value;}
        }
        public byte[] ByteArray
        {
            get {return _byteArray;}
            set {_byteArray = value;}
        }
        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            if (ByteArray != null)
            {
                props.Add("video", Convert.ToBase64String(ByteArray));
            }
            props.Add("nodeType", NodeType.Video.ToString());
            return props;
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("video"))
            {
                ByteArray = Convert.FromBase64String(props["video"]);
                //make video out of Array
            }
            base.UnPack(props);
        } 
    }
}
