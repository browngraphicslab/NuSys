using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;

namespace NuSysApp
{
    public abstract class AtomModel : Sendable
    {
        public delegate void AlphaChangedEventHandler(object source);

        public delegate void DeleteEventHandler(object source);

        public delegate void LocationUpdateEventHandler(object source, PositionChangeEventArgs e);

        public delegate void MetadataChangeEventHandler(object source, string key);

        public delegate void NetworkUserChangedEventHandler(NetworkUser user);

        public delegate void ScaleChangedEventHandler(object source);

        public delegate void TitleChangedHandler(object source, string title);

        public delegate void WidthHeightUpdateEventHandler(object source, WidthHeightUpdateEventArgs e);

        public enum AtomType
        {
            Workspace,
            Node,
            Link
        }

        private double _alpha = 1;
        private SolidColorBrush _color;
        private double _height;
        private NetworkUser _lastNetworkUser;
        private double _scaleX = 1;
        private double _scaleY = 1;
        private string _title = string.Empty;
        private double _width;
        private double _x;
        private double _y;

        protected Dictionary<string, object> Metadata = new Dictionary<string, object>();

        protected AtomModel(string id) : base(id)
        {
            CanEdit = EditStatus.Maybe;

            Creator = null;

            SetMetaData("tags", new List<string>());
            SetMetaData("groups", new List<string>());
        }

        public AtomType Type { get; set; }

        // TODO: Move color to higher level type

        public SolidColorBrush Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public NetworkUser LastNetworkUser
        {
            get { return _lastNetworkUser; }
            set
            {
                if (value != null)
                {
                    _lastNetworkUser?.RemoveAtomInUse(this);
                    value.AddAtomInUse(this);
                    _lastNetworkUser = value;
                    UserChanged?.Invoke(value);
                }
                else
                {
                    _lastNetworkUser = null;
                    UserChanged?.Invoke(null);
                }
            }
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

        public event MetadataChangeEventHandler MetadataChange;
        public event DeleteEventHandler Deleted;
        public event LocationUpdateEventHandler PositionChanged;
        public event WidthHeightUpdateEventHandler SizeChanged;
        public event ScaleChangedEventHandler ScaleChanged;
        public event AlphaChangedEventHandler AlphaChanged;
        public event TitleChangedHandler TitleChanged;
        public event NetworkUserChangedEventHandler UserChanged;

        public object GetMetaData(string key)
        {
            if (Metadata.ContainsKey(key))
                return Metadata[key];
            return null;
        }

        public void SetMetaData(string key, object value)
        {
            Metadata[key] = value;
            MetadataChange?.Invoke(this, key);
        }

        public virtual void Delete()
        {
            Deleted?.Invoke(this);
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
            var md = props.GetDict<string, object>("metadata");
            if (md.ContainsKey("tags"))
                md["tags"] = JsonConvert.DeserializeObject<List<string>>(md["tags"].ToString());
            else
                md["tags"] = new List<string>();

            if (md.ContainsKey("groups"))
                md["groups"] = JsonConvert.DeserializeObject<List<string>>(md["groups"].ToString());
            else
                md["groups"] = new List<string>();

            foreach (var data in md)
            {
                Metadata[data.Key] = data.Value;
            }

            X = props.GetDouble("x", X);
            Y = props.GetDouble("y", Y);
            Width = props.GetDouble("width", Width);
            Height = props.GetDouble("height", Height);
            Alpha = props.GetDouble("alpha", Alpha);
            ScaleX = props.GetDouble("scaleX", ScaleX);
            ScaleY = props.GetDouble("scaleY", ScaleY);
            Creator = props.GetString("creator", Creator);
            Title = props.GetString("title", "");
            if (props.ContainsKey("system_sender_ip") &&
                SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(
                    props.GetString("system_sender_ip")))
            {
                LastNetworkUser =
                    SessionController.Instance.NuSysNetworkSession.NetworkMembers[props.GetString("system_sender_ip")];
            }
            await base.UnPack(props);
        }
    }
}