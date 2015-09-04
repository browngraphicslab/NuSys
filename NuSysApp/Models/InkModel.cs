using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class InkModel : Node
    {
        private List<InqLine> _inqlines; 
        public InkModel(string id) : base(id)
        {

            _inqlines = new List<InqLine>();
        }

        public InkModel(string id, List<InqLine> lines) : base(id)
        {
            _inqlines = lines;
        }

        public List<InqLine> PolyLines
        {
            get { return _inqlines; }
            set
            {
                _inqlines = value;
                DebounceDict.Add("polylines",InqlinesToString());
            }
        }

        private string InqlinesToString()
        {
            string plines = "";
            foreach (InqLine pl in _inqlines)
            {
                if (pl.Points.Count > 0)
                {
                    plines += pl.Stringify();
                }
            }
            return plines;
        }
        public override async Task<Dictionary<string,string>>  Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            props.Add("polylines", InqlinesToString());
            return props;
        }

        public override async Task UnPack(Dictionary<string, string> props)
        {
            base.UnPack(props);
        }
    }
}
