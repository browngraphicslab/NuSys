using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp.Network;

namespace NuSysApp
{
    public abstract class Atom 
    {
        private DebouncingDictionary _debounceDict;
        private EditStatus _editStatus;
        public delegate void LinkedEventHandler(object source, LinkedEventArgs e);
        public event LinkedEventHandler OnLinked;

        public delegate void CanEditChangedEventHandler(object source, CanEditChangedEventArg e);
        public event CanEditChangedEventHandler OnCanEditChanged;
        public enum EditStatus
        {
            Yes,
            No,
            Maybe
        }
        public Atom(string id)
        {
            ID = id;
            _debounceDict = new DebouncingDictionary(this);
            CanEdit = EditStatus.Maybe;
        }

        public void AddToLink(Link link)
        {
            OnLinked?.Invoke(this, new LinkedEventArgs("Linked", link));
        }

        public SolidColorBrush Color { get; set; }
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
