﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp2
{
    public class WordNodeModel : ElementModel
    {
        public WordNodeModel(string id) : base(id)
        {
            ElementType = ElementType.Word;
        }
        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            return props;
        }

        public override async Task UnPack(Message props)
        {
            await base.UnPack(props);
        }
    }
}