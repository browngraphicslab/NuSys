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
        private List<InqLine> ParseToPolyline(string s)
        {
            List<InqLine> polys = new List<InqLine>();
            string[] parts = s.Split("><".ToCharArray());
            foreach (string part in parts)
            {
                InqLine line = new InqLine();
                string[] subparts = part.Split(" ".ToCharArray());
                foreach (string subpart in subparts)
                {
                    if (subpart.Length > 0 && subpart != "polyline")
                    {
                        if (subpart.Substring(0, 6) == "points")
                        {
                            string innerPoints = subpart.Substring(8, subpart.Length - 9);
                            string[] points = innerPoints.Split(";".ToCharArray());
                            foreach (string p in points)
                            {
                                if (p.Length > 0)
                                {
                                    string[] coords = p.Split(",".ToCharArray());
                                    //Point point = new Point(double.Parse(coords[0]), double.Parse(coords[1]));
                                    Point parsedPoint = new Point(Int32.Parse(coords[0]), Int32.Parse(coords[1]));
                                    line.AddPoint(parsedPoint);
                                }
                            }
                        }
                        else if (subpart.Substring(0, 9) == "thickness")
                        {
                            string sp = subpart.Substring(11, subpart.Length - 13);
                            line.StrokeThickness = double.Parse(sp);
                        }
                        else if (subpart.Substring(0, 6) == "stroke")
                        {
                            string sp = subpart.Substring(8, subpart.Length - 10);
                            line.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 250));
                            //poly.Stroke = new SolidColorBrush(color.psp); TODO add in color
                        }
                    }
                }
                if (line.Points.Count > 0)
                {
                    polys.Add(line);
                }
            }
            return polys;
        }
        public override async Task<Dictionary<string,string>>  Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            props.Add("polylines", InqlinesToString());
            return props;
        }

        public override async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("polylines"))
            {
                PolyLines = ParseToPolyline(props["polylines"]);
            }
        }
    }
}
