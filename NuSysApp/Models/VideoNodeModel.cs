using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace NuSysApp
{
    public class VideoNodeModel : ElementModel
    {
        private byte[] _byteArray;
        private InMemoryRandomAccessStream _recording;
        private int _resX, _resY;
        public VideoNodeModel(string id) : base(id)
        {
            //ByteArray = byteArray;
            Recording = new InMemoryRandomAccessStream();
            ElementType = ElementType.Video;
            _resX = 1;
            _resY = 1;
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
        public int ResolutionX
        {
            get {return _resX;}
            set {_resX = value;}
        }
        public int ResolutionY
        {
            get {return _resY;}
            set {_resY = value;}
        }
        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            if (ByteArray != null)
            {
                props.Add("video", Convert.ToBase64String(ByteArray));
            }
            if (ResolutionX != null)
            {
                props.Add("resolutionX", ResolutionX);
            }
            if (ResolutionY != null)
            {
                props.Add("resolutionY", ResolutionY);
            }

            return props;
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("video"))
            {
                ByteArray = Convert.FromBase64String(props.GetString("video"));
            }
            if (props.ContainsKey("resolutionX"))
            {
                ResolutionX = props.GetInt("resolutionX");
            }
            if (props.ContainsKey("resolutionY"))
            {
                ResolutionY = props.GetInt("resolutionY");
            }
            base.UnPack(props);
        } 
    }
}
