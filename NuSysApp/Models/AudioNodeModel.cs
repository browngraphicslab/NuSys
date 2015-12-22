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
    public class AudioNodeModel : NodeModel
    {
        private readonly StorageFolder _rootFolder = NuSysStorages.Media;
        private StorageFile _audioFile;
        public AudioNodeModel(byte[] byteArray, string id) : base(id)
        {
            NodeType = NodeType.Audio;
            Content = new NodeContentModel(byteArray, id);
            ByteArray = byteArray;
            MakeAudio(byteArray);
            //FileName = "nusysAudioCapture" + DateTime.Now + ".mp3";
        }

        public byte[] ByteArray
        {
            get {return Content.Data;}
            set {Content.Data = value;}
        }

        public StorageFile AudioFile {
            get
            {
                return _audioFile;
            }
            set
            {
                if (_audioFile == value) return;
                _audioFile = value;
            }
        }
        public string FileName { get; set; }

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            if (ByteArray != null)
            {
                props.Add("audio", Convert.ToBase64String(ByteArray));
            }
            props.Add("nodeType", NodeType.Audio.ToString());
            return props;
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("audio"))
            {
                ByteArray = Convert.FromBase64String(props["audio"]);
                MakeAudio(ByteArray);
            }
            base.UnPack(props);
        } 
        public async Task<byte[]> ConvertAudioToByte(StorageFile file)
        {
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            return fileBytes;
        }

        public async Task MakeAudio(byte[] byteArray)
        {
            if (byteArray == null) return;
            AudioFile = await this.ConvertByteToAudio(byteArray);
        }
        public async Task SendNetworkUpdate()
        {
            byte[] bytes = await ConvertAudioToByte(AudioFile);
            if (!NetworkConnector.Instance.IsSendableBeingUpdated(Id))
            {
                Debug.WriteLine("add to debounce dict called");
                DebounceDict.MakeNextMessageTCP();
                DebounceDict.Add("audio", Convert.ToBase64String(bytes));
                DebounceDict.MakeNextMessageTCP();
            }
        }
        public async Task<StorageFile> ConvertByteToAudio(byte[] byteArray)
        {
            var recordStorageFile = await _rootFolder.CreateFileAsync(Id + ".mp3", CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteBytesAsync(recordStorageFile, byteArray);
            return recordStorageFile;
        }
    }
}
