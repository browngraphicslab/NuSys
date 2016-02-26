﻿using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class AudioNodeViewModel: ElementViewModel
    {
        private IRandomAccessStream _stream;
        public AudioNodeViewModel(ElementController model) : base(model)
        {
            Width = 200;
            Height = 80;
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
        }

        public IRandomAccessStream AudioSource
        {
            get { return _stream; }
        }
        public override async Task Init()
        {
            var byteArray = Convert.FromBase64String(SessionController.Instance.ContentController.Get(ContentId).Data);
            MemoryStream s = new MemoryStream(byteArray);
            _stream = s.AsRandomAccessStream();
        }
        public string FileName
        {
            get { return ((AudioNodeModel)Model).FileName; }
            set { ((AudioNodeModel)Model).FileName = value; }
        }
       
    }
}
