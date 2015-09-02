using NuSysApp.MISC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Streams;

namespace NuSysApp
{
    public class AudioModel : Node
    {
        private readonly StorageFolder _rootFolder = NuSysStorages.Media;
        public AudioModel(byte[] byteArray, string id) : base(id)
        {
            Content = new Content(byteArray, id);
            ByteArray = byteArray;
            ID = id;
            MakeAudio(byteArray);
            //FileName = "nusysAudioCapture" + DateTime.Now + ".mp3";
        }

        public byte[] ByteArray
        {
            get { return Content.Data; }
            set
            {
                Content.Data = value;
                if (!NetworkConnector.Instance.ModelIntermediate.IsSendableLocked(ID))
                {
                    DebounceDict.Add("audio",Convert.ToBase64String(value));
                }
            }
        }

        public StorageFile AudioFile { get; set; }

        public string FileName { get; set; }

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            props.Add("audio", Convert.ToBase64String(ByteArray));
            props.Add("nodeType", NodeType.Audio.ToString());
            return props;
        }

        public override async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("audio"))
            {
                MakeAudio(Convert.FromBase64String(props["audio"]));
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

        public async Task<StorageFile> ConvertByteToAudio(byte[] byteArray)
        {
            StorageFile _recordStorageFile = await _rootFolder.CreateFileAsync(ID + ".mp3", CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteBytesAsync(_recordStorageFile, byteArray);
            return _recordStorageFile;
        }
    }
}
