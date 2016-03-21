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
using NuSysApp.Controller;
using NuSysApp.Nodes.AudioNode;

namespace NuSysApp
{
    public class AudioNodeModel : ElementModel
    {
        //private ObservableCollection<LinkedTimeBlockViewModel> _linkedTimeBlocks;

        private ObservableCollection<LinkedTimeBlockModel> _linkedTimeModels;
        private MediaController _controller;

        private readonly StorageFolder _rootFolder = NuSysStorages.Media;
        private StorageFile _audioFile;
        public AudioNodeModel(string id) : base(id)
        {
            ElementType = ElementType.Audio;
            // _linkedTimeBlocks = new ObservableCollection<LinkedTimeBlockViewModel>();
            _linkedTimeModels = new ObservableCollection<LinkedTimeBlockModel>();

        }

        public string FileName { get; set; }

        public ObservableCollection<LinkedTimeBlockModel> LinkedTimeModels
        {
            get { return _linkedTimeModels; }
        }

        public MediaController Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }

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

