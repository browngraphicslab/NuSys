using System;
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
        protected Dictionary<string, object> Metadata = new Dictionary<string, object>();

        private readonly DebouncingDictionary _debounceDict;
        private SolidColorBrush _color;
        

        protected AtomModel(string id) : base(id)
        {
            _debounceDict = new DebouncingDictionary(this);
            CanEdit = EditStatus.Maybe;
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

        // TODO: Move color to higher level type

        public SolidColorBrush Color {
            get { return _color; }
            set
            {
                _color = value;
            }
        }

        public object GetMetaData(string key)
        {
            if (Metadata.ContainsKey(key))
                return Metadata[key];
            return "";
        }

        public void SetMetaData(string key, string value)
        {
            Metadata[key] = value;
            //DebounceDict.Add("metadata", Newtonsoft.Json.JsonConvert.SerializeObject(Metadata).Replace("\"", "'").Replace("{", "<").Replace("}", ">"));
            MetadataChanged?.Invoke(this, key);
        }

        public DebouncingDictionary DebounceDict
        {
            get { return _debounceDict; }
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> dict = await base.Pack();
            var metadatastring = Newtonsoft.Json.JsonConvert.SerializeObject(Metadata).Replace("\"", "'").Replace("{", "<").Replace("}", ">");
            dict.Add("metadata", metadatastring);
            return dict;
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("metadata"))
            {
                Metadata = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string,object>>( props["metadata"].Replace("'", "\"").Replace("<", "{").Replace(">", "}"));
            }
            await base.UnPack(props);
        }
    } 
}
