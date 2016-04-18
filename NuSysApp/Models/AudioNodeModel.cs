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
        public delegate void JumpEventHandler(TimeSpan time);
        public event JumpEventHandler OnJump;
        public AudioNodeModel(string id) : base(id)
        {
            ElementType = ElementType.Audio;
            // _linkedTimeBlocks = new ObservableCollection<LinkedTimeBlockViewModel>();
            _linkedTimeModels = new ObservableCollection<LinkedTimeBlockModel>();

        }

        public void Jump(TimeSpan time)
        {
            OnJump?.Invoke(time);
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
            //Dictionary<string, object> linkedTimeBlockDic = new Dictionary<string, object>();
            //for (int i = 0; i < _linkedTimeModels.Count; i++)
            //{
            //    Dictionary<string, TimeSpan> d = new Dictionary<string, TimeSpan>();
            //    d.Add("start", _linkedTimeModels[i].Start);
            //    d.Add("end", _linkedTimeModels[i].End);
            //    linkedTimeBlockDic.Add("timeblock" + i, d);
            //}
            //if (_linkedTimeModels.Count != 0)
            //{
            //    props["linkedTimeModels"] = linkedTimeBlockDic;
            //}

            return props;
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("fileName"))
            {
                FileName = props.GetString("fileName");
            }

            if (props.ContainsKey("linkedTimeModels"))
            {
                _linkedTimeModels = new ObservableCollection<LinkedTimeBlockModel>(props.GetList<LinkedTimeBlockModel>("linkedTimeModels"));
                //Dictionary<string, Dictionary<string, TimeSpan>> linkedTimeBlockDic = props.GetDict<string, Dictionary<string, TimeSpan>>("linkedTimeModels");
                //for (int i = 0; i < linkedTimeBlockDic.Count; i++)
                //{
                //    _linkedTimeModels.Add(new LinkedTimeBlockModel(linkedTimeBlockDic["timeblock" + i]["start"], linkedTimeBlockDic["timeblock" + i]["end"]));
                //}
            }

            base.UnPack(props);
        }
    }
}

