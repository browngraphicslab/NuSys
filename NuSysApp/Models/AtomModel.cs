using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace NuSysApp
{
    public abstract class AtomModel : Sendable
    {
        private double _x;
        private double _y;
        private double _alpha = 1;
        private double _scaleX = 1;
        private double _scaleY = 1;
        private double _width;
        private double _height;
        private string _title;

        private readonly DebouncingDictionary _debounceDict;
        private SolidColorBrush _color;

        protected Dictionary<string, object> Metadata = new Dictionary<string, object>();

        public delegate void MetadataChangeEventHandler(object source, string key);
        public event MetadataChangeEventHandler MetadataChange;
        public delegate void LinkedEventHandler(object source, LinkedEventArgs e);
        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);
        public event DeleteEventHandler Deleted;
        public delegate void LocationUpdateEventHandler(object source, PositionChangeEventArgs e);
        public event LocationUpdateEventHandler PositionChanged;
        public delegate void WidthHeightUpdateEventHandler(object source, WidthHeightUpdateEventArgs e);
        public event WidthHeightUpdateEventHandler SizeChanged;
        public delegate void ScaleChangedEventHandler(object source);
        public delegate void AlphaChangedEventHandler(object source);
        public event ScaleChangedEventHandler ScaleChanged;
        public event AlphaChangedEventHandler AlphaChanged;
        public delegate void TitleChangedHandler(object source, string title);
        public event TitleChangedHandler TitleChanged;
        

        protected AtomModel(string id) : base(id)
        {
            _debounceDict = new DebouncingDictionary(this);
            CanEdit = EditStatus.Maybe;

            SetMetaData("tags", new List<string> {"none"});
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

        public void SetMetaData(string key, object value)
        {
            Metadata[key] = value;
            MetadataChange?.Invoke(this, key);
        }

        public DebouncingDictionary DebounceDict
        {
            get { return _debounceDict; }
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict = await base.Pack();
            dict.Add("metadata", Metadata);
            dict.Add("creator", Creator);
            dict.Add("x", X);
            dict.Add("y", Y);
            dict.Add("width", Width);
            dict.Add("height", Height);
            dict.Add("alpha", Alpha);
            dict.Add("scaleX", ScaleX);
            dict.Add("scaleY", ScaleY);
            dict.Add("title", Title);
            return dict;
        }

        public override async Task UnPack(Message props)
        {     
            Metadata = props.GetDict<string, object>("metadata");
            if (Metadata.ContainsKey("tags"))
                Metadata["tags"] = JsonConvert.DeserializeObject<List<string>>(Metadata["tags"].ToString());
            else
                Metadata["tags"] = new List<string>();

            X = props.GetDouble("x", X);
            Y = props.GetDouble("y", Y);
            Width = props.GetDouble("width", Width);
            Height = props.GetDouble("height", Height);
            Alpha = props.GetDouble("alpha", Alpha);
            ScaleX = props.GetDouble("scaleX", ScaleX);
            ScaleY = props.GetDouble("scaleY", ScaleY);
            Creator = props.GetString("creator", "WORKSPACE_ID");
            Title = props.GetString("title", "");
            await base.UnPack(props);
        }

        public string Creator { get; set; }
        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                PositionChanged?.Invoke(this, new PositionChangeEventArgs(X, Y));
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                PositionChanged?.Invoke(this, new PositionChangeEventArgs(X, Y));
            }
        }

        public virtual double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                SizeChanged?.Invoke(this, new WidthHeightUpdateEventArgs(Width, Height));
            }
        }

        public virtual double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                SizeChanged?.Invoke(this, new WidthHeightUpdateEventArgs(Width, Height));
            }
        }

        public virtual double ScaleX
        {
            get { return _scaleX; }
            set
            {
                _scaleX = value;
                ScaleChanged?.Invoke(this);
            }
        }

        public virtual double ScaleY
        {
            get { return _scaleY; }
            set
            {
                _scaleY = value;
                ScaleChanged?.Invoke(this);
            }
        }

        public virtual double Alpha
        {
            get { return _alpha; }
            set
            {
                _alpha = value;
                AlphaChanged?.Invoke(this);
            }
        }

        public virtual string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                TitleChanged?.Invoke(this, _title);
            }
        }
    } 
}
