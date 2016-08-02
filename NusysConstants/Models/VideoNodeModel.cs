using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class VideoNodeModel : ElementModel
    {
        private byte[] _byteArray;
        private InMemoryRandomAccessStream _recording;
        private int _resX, _resY;
        public MediaElement Test { get; }
        public delegate void JumpEventHandler(TimeSpan time);
        public event JumpEventHandler OnJump;

        public VideoNodeModel(string id) : base(id)
        {
            Recording = new InMemoryRandomAccessStream();
            ElementType = NusysConstants.ElementType.Video;
        }

        public void Jump(TimeSpan time)
        {
            OnJump?.Invoke(time);
        }
        private void Test_CurrentStateChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _resX = Test.AspectRatioWidth;
            _resY = Test.AspectRatioHeight;
        }

        public InMemoryRandomAccessStream Recording
        {
            get { return _recording; }
            set { _recording = value; }
        }
        public byte[] ByteArray
        {
            get { return _byteArray; }
            set { _byteArray = value; }
        }
        public int ResolutionX
        {
            get { return _resX; }
            set { _resX = value; }
        }
        public int ResolutionY
        {
            get { return _resY; }
            set { _resY = value; }
        }
        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            if (ByteArray != null)
            {
                props.Add("video", Convert.ToBase64String(ByteArray));
            }
 
            props.Add("resolutionX", ResolutionX);
            props.Add("resolutionY", ResolutionY);
            

            return props;
        }

        public override async Task UnPackFromDatabaseMessage(Message props)
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
            await base.UnPackFromDatabaseMessage(props);
        }
    }
}
