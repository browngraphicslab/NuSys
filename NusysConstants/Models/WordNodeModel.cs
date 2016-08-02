﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace NusysIntermediate
{
    public class WordNodeModel : ElementModel
    {
        public WordNodeModel(string id) : base(id)
        {
            ElementType = NusysConstants.ElementType.Word;
        }
        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            return props;
        }

        public override async Task UnPackFromDatabaseMessage(Message props)
        {
            await base.UnPackFromDatabaseMessage(props);
        }
    }
}
