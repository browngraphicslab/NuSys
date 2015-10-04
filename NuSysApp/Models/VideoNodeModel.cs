using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class VideoNodeModel : NodeModel
    {
        private byte[] _byteArray;
        public VideoNodeModel(byte[] byteArray, string id) : base(id)
        {
            ByteArray = byteArray;
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
