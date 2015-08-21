using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class InkModel : Node
    {
        private List<Polyline> _polylines; 
        public InkModel(string id) : base(id)
        {

            _polylines = new List<Polyline>();
        }

        public List<Polyline> PolyLines
        {
            get { return _polylines; }
            set
            {
                _polylines = value;
                DebounceDict.Add("polylines",PolylinesToString());
            }
        }

        private string PolylinesToString()
        {
            string plines = "";
            foreach (Polyline pl in PolyLines)
            {
                if (pl.Points.Count > 0)
                {
                    plines += "<polyline points='";
                    foreach (Point point in pl.Points)
                    {
                        plines += Math.Floor(point.X) + "," + Math.Floor(point.Y) + ";";
                    }
                    plines += "' thickness='" + pl.StrokeThickness + "' stroke='" + pl.Stroke + "'/>";
                }
            }
            return plines;
        }
        private List<Polyline> ParseToPolyline(string s)
        {
            List<Polyline> polys = new List<Polyline>();
            string[] parts = s.Split("><".ToCharArray());
            foreach (string part in parts)
            {
                Polyline poly = new Polyline();
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
                                    poly.Points.Add(new Point(Int32.Parse(coords[0]), Int32.Parse(coords[1])));
                                }
                            }
                        }
                        else if (subpart.Substring(0, 9) == "thickness")
                        {
                            string sp = subpart.Substring(11, subpart.Length - 12);
                            poly.StrokeThickness = double.Parse(sp);
                        }
                        else if (subpart.Substring(0, 6) == "stroke")
                        {
                            string sp = subpart.Substring(8, subpart.Length - 10);
                            poly.Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(0,0,0,250));
                            //poly.Stroke = new SolidColorBrush(color.psp); TODO add in color
                        }
                    }
                }
                if (poly.Points.Count > 0)
                {
                    polys.Add(poly);
                }
            }
            return polys;
        }
        public override async Task<Dictionary<string,string>>  Pack()
        {
            Dictionary<string, string> props = await base.Pack();
            props.Add("polylines", PolylinesToString());
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
