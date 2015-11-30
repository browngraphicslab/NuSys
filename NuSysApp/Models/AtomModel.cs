﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json.Converters;

namespace NuSysApp
{
    public abstract class AtomModel : Sendable
    {

        public delegate void MetadataChangeEventHandler(object source, string key);
        public event MetadataChangeEventHandler MetadataChanged;

        public delegate void LinkedEventHandler(object source, LinkedEventArgs e);
        protected Dictionary<string, string> Metadata = new Dictionary<string, string>();

        private readonly DebouncingDictionary _debounceDict;
        private SolidColorBrush _color;
        

        protected AtomModel(string id) : base(id)
        {
            ID = id;
            _debounceDict = new DebouncingDictionary(this);
            CanEdit = EditStatus.Maybe;
        }

        //takes in string converts to SolidColorBrush
        private SolidColorBrush StringToColor(string colorString)
        {
            string aVal = colorString.Substring(0, 2);
            string rVal = colorString.Substring(3, 5);
            string gVal = colorString.Substring(6, 8);
            string bVal = colorString.Substring(9, 11);

            Color color = Windows.UI.Color.FromArgb(Byte.Parse(aVal),Byte.Parse(rVal),Byte.Parse(gVal),Byte.Parse(bVal));

            SolidColorBrush colorBrush = new SolidColorBrush(color);

            return colorBrush;
        }

        //takes in SolidColorBrush converts to string
        private string ColorToString(SolidColorBrush brush)
        {
            Color color = brush.Color;
            var aVal = color.A;
            var rVal = color.R;
            var gVal = color.G;
            var bVal = color.B;
            string colorString = aVal.ToString() + rVal.ToString() + gVal.ToString() + bVal.ToString();
            return colorString;
        }

        public SolidColorBrush Color {
            get { return _color; }
            set
            {
                if (value != null && _color != value)
                {
                    _color = value;
                    if (NetworkConnector.Instance.IsSendableBeingUpdated(ID))
                    {
                        //TODO raise property changed
                    }
                    else
                    {
                        this.DebounceDict.Add("color", ColorToString(value));
                    }
                }
            }
        }

        public string GetMetaData(string key)
        {
            if (Metadata.ContainsKey(key))
                return Metadata[key];
            return "";
        }

        public void SetMetaData(string key, string value)
        {
            Metadata[key] = value;
            var metadatastring = Newtonsoft.Json.JsonConvert.SerializeObject(Metadata).Replace("\\\\\"", "'");
            if (NetworkConnector.Instance.IsSendableBeingUpdated(ID))
            {
                Debug.WriteLine("Senable is currently being updated");
                
                DebounceDict.Add("metadata", Newtonsoft.Json.JsonConvert.SerializeObject(Metadata).Replace("\"", "'").Replace("{", "<").Replace("}", ">"));
            }
            else
            {
                DebounceDict.Add("metadata", Newtonsoft.Json.JsonConvert.SerializeObject(Metadata).Replace("\"", "'").Replace("{", "<").Replace("}", ">"));
            }

            MetadataChanged?.Invoke(this, key);
        }

        public DebouncingDictionary DebounceDict
        {
            get { return _debounceDict; }
        }
        public string ID { get; set; }

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> dict = await base.Pack();
            var metadatastring = Newtonsoft.Json.JsonConvert.SerializeObject(Metadata).Replace("\"", "'").Replace("{", "<").Replace("}", ">");

            //var s = Newtonsoft.Json.JsonConvert.SerializeObject(Metadata);
            dict.Add("metadata", metadatastring);
            return dict;
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("metadata"))
            {
                Metadata = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,string>>( props["metadata"].Replace("'", "\"").Replace("<", "{").Replace(">", "}"));
            }
            await base.UnPack(props);
        }
    } 
}
