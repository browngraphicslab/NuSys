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
    public class ImageElementModel : ElementModel
    {
        public ImageElementModel(string id) : base(id)
        {
            ElementType = NusysConstants.ElementType.Image;
        }

        public string FilePath { get; set; }
       
        public override void UnPackFromDatabaseMessage(Message props)
        {
            base.UnPackFromDatabaseMessage(props);
            if (props.ContainsKey(NusysConstants.IMAGE_ELEMENT_FILE_PATH_KEY))
            {
                FilePath = props.GetString(NusysConstants.IMAGE_ELEMENT_FILE_PATH_KEY, FilePath);
            }
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            props.Add("filepath", FilePath);
          //  props.Add("data", Convert.ToBase64String(Content.Data));
            return props;
        }
    }
}