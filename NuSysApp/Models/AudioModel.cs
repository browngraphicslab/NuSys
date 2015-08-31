using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace NuSysApp
{
    public class AudioModel:Node
    {
        public AudioModel(string id) : base(id)
        {
            //FileName = "nusysAudioCapture" + DateTime.Now + ".mp3";
        }

        public StorageFile AudioFile { get; set; }

        public string FileName { get; set; }      
    }
}
