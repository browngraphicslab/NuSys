using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using NuSysApp.Nodes.AudioNode;

namespace NuSysApp
{
    public class VideoNodeModel : ElementModel
    {
        private byte[] _byteArray;
        private InMemoryRandomAccessStream _recording;
        private int _resX, _resY;
        public MediaElement Test { get; }
        private ObservableCollection<LinkedTimeBlockModel> _linkedTimeModels;


        public VideoNodeModel(string id) : base(id)
        {
            //ByteArray = byteArray;
            Recording = new InMemoryRandomAccessStream();
            //Test = new MediaElement();
            //var byteArray = Convert.FromBase64String(SessionController.Instance.ContentController.Get((vm.Model as VideoNodeModel).ContentId).Data);
            //Recording.AsStreamForWrite().Write(byteArray, 0, byteArray.Length);
            //Recording.Seek(0);
            //Debug.WriteLine(memoryStream);
            //Test.SetSource(_recording, "video/mp4");
            //Test.CurrentStateChanged += Test_CurrentStateChanged;

            //Test.AutoPlay = true;
            ElementType = ElementType.Video;
            _linkedTimeModels = new ObservableCollection<LinkedTimeBlockModel>();

            //_resX = 1;
            //_resY = 1;
        }

        public ObservableCollection<LinkedTimeBlockModel> LinkedTimeModels
        {
            get { return _linkedTimeModels; }
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
