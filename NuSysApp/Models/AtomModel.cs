using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class AtomModel : Sendable
    {
        private DebouncingDictionary _debounceDict;
        public delegate void LinkedEventHandler(object source, LinkedEventArgs e);
        public delegate void CreateGroupEventHandler(object source, CreateGroupEventArgs e);
        public event LinkedEventHandler OnLinked;
        public event CreateGroupEventHandler OnCreatedGroup;


        private SolidColorBrush _color;

        protected AtomModel(string id) : base(id)
        {
            ID = id;
            _debounceDict = new DebouncingDictionary(this);
            CanEdit = EditStatus.Maybe;
        }

        public void AddToLink(LinkModel link)
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

        public DebouncingDictionary DebounceDict
        {
            get { return _debounceDict; }
        }
        public string ID { get; set; }

        public override async Task UnPack(Message props)
        {
            await base.UnPack(props);
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            return await base.Pack();
        } 
    } 
}
