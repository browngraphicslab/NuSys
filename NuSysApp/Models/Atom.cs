using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class Atom :Sendable
    {
        private DebouncingDictionary _debounceDict;
        private EditStatus _editStatus;
        public delegate void LinkedEventHandler(object source, LinkedEventArgs e);
        public delegate void CreateGroupEventHandler(object source, CreateGroupEventArgs e);
        public event LinkedEventHandler OnLinked;
        public event CreateGroupEventHandler OnCreatedGroup;
        public delegate void CanEditChangedEventHandler(object source, CanEditChangedEventArg e);
        public event CanEditChangedEventHandler OnCanEditChanged;
        public enum EditStatus
        {
            Yes,
            No,
            Maybe
        }

        private SolidColorBrush _color;

        protected Atom(string id)
        {
            ID = id;
            _debounceDict = new DebouncingDictionary(this);
            CanEdit = EditStatus.Maybe;
        }

        public abstract void Delete();

        public void AddToLink(Link link)
        {
            OnLinked?.Invoke(this, new LinkedEventArgs("Linked", link));
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
                    if (NetworkConnector.Instance.ModelIntermediate.IsSendableLocked(ID))
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

        public DebouncingDictionary DebounceDict
        {
            get { return _debounceDict; }
        }
        public EditStatus CanEdit {
            get
            {
                return _editStatus;
            }
            set
            {
                if (_editStatus == value)
                {
                    return;
                }
                _editStatus = value;
                OnCanEditChanged?.Invoke(this, new CanEditChangedEventArg("Can edit changed", CanEdit));
            }
        } //Network locks
        public string ID { get; set; }

        public virtual async Task UnPack(Dictionary<string, string> props)
        { 
            if (props.ContainsKey("color"))
            {
                //TODO add in color
            }
        }

        public virtual async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string,string> dict = new Dictionary<string, string>();
            //dict.Add("color") //TODO add in color
            dict.Add("id", ID);
            return dict;
        } 
    } 
}
