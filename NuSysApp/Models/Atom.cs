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
    public abstract class Atom : BaseINPC
    {
        private DebouncingDictionary _debounceDict;
        public Atom(string id)
        {
            ID = id;
            _debounceDict = new DebouncingDictionary(this);
            CanEdit = true;
        }
        public SolidColorBrush Color { get; set; }
        public DebouncingDictionary DebounceDict
        {
            get { return _debounceDict; }
        }
        public bool CanEdit { get; set; } //Network locks
        public string ID { get; set; }

        public virtual void UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("color"))
            {
                //TODO add in color
            }
        }

        public virtual Dictionary<string, string> Pack()
        {
            Dictionary<string,string> dict = new Dictionary<string, string>();
            //dict.Add("color") //TODO add in color
            return dict;
        } 
    } 
}
