using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using NuSysApp.Network;

namespace NuSysApp
{
    public abstract class Atom
    {
        private DebouncingDictionary _debounceDict;
        public Atom(string id)
        {
            ID = id;
            _debounceDict = new DebouncingDictionary(id);
        }
        public SolidColorBrush Color { get; set; }

        public DebouncingDictionary DebounceDict
        {
            get { return _debounceDict; }
        }

        public string ID { get; set; }

        public virtual void Update(Dictionary<string, string> props)
        {
            if (props.ContainsKey("color"))
            {
                //TODO add in color
            }
        }
    } 
}
