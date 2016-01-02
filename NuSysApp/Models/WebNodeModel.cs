﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class WebNodeModel : NodeModel
    {
        private string _url = string.Empty;
        
        public delegate void UrlChangedHandler(object source, string url);
        public event UrlChangedHandler UrlChanged;

        public WebNodeModel(string id) : base(id)
        {
            NodeType = NodeType.Web;
        }
        
        public override async Task UnPack(Message props)
        {
            Url = props.GetString("url", "");
            await base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = await base.Pack();
            props.Add("url", Url);
            return props;
        }

        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                UrlChanged?.Invoke(this, _url);
            }
        }
    }
}
