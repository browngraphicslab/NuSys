using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using NuSysApp.EventArgs;

namespace NuSysApp
{
    public class InqLineModel : Sendable
    {
        private PointCollection _points;

        public delegate void DeleteInqLineEventHandler(object source, DeleteInqLineEventArgs e);
        public event DeleteInqLineEventHandler OnDeleteInqLine;

        public PointCollection Points
        {
            get { return _points; }
            set
            {
                _points = value;
                RaisePropertyChanged("Model.Points");
            }
        }

        public double StrokeThickness { get; set; }
        public string ParentID { get; set; }
        public SolidColorBrush Stroke { get; set; }
        public InqLineModel(string id) : base(id)
        {
            _points = new PointCollection();
        }

        public InqLineModel(string id, PointCollection points) : base(id)
        {
            _points = points;
        }

        public void AddPoint(Point p)
        {
            Points.Add(p);

            RaisePropertyChanged("Model.Points");
        }

        public void Delete()
        {
            OnDeleteInqLine?.Invoke(this, new DeleteInqLineEventArgs(this));
        }

        public async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("data"))
            {
                SetLine(props["data"], props["id"]);
            }
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> props = new Dictionary<string, string>();
            props.Add("id", Id);
            props.Add("canvasNodeID", ParentID);
            props.Add("data", GetString());
            props.Add("type", "ink");
            props.Add("inkType", "full");
            return props;
        }

        private void SetLine(string data, string id)
        {
            PointCollection points;
            double thickness;
            SolidColorBrush stroke;
            ParseToLineData(data, out points, out thickness, out stroke);
            Points = points;
            StrokeThickness = thickness;
            Stroke = stroke;
            var view = new InqLineView(new InqLineViewModel(this), StrokeThickness, Stroke);
        }

        public static void ParseToLineData(string s, out PointCollection pc, out double thickness, out SolidColorBrush stroke)
        {
            pc = new PointCollection();
            thickness = 1;
            stroke = new SolidColorBrush(Colors.Black);
            string[] parts = s.Split(new string[] { "><" }, StringSplitOptions.RemoveEmptyEntries);
            string part = parts[0];
            string[] subparts = part.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            SolidColorBrush color;
            foreach (string subpart in subparts)
            {
                if (subpart.Length > 0 && subpart != "polyline")
                {
                    if (subpart.Substring(0, 6) == "points")
                    {
                        string innerPoints = subpart.Substring(8, subpart.Length - 9);
                        string[] points = innerPoints.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string p in points)
                        {
                            if (p.Length > 0)
                            {
                                string[] coords = p.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                                Point parsedPoint = new Point(double.Parse(coords[0]), double.Parse(coords[1]));
                                pc.Add(parsedPoint);
                            }
                        }
                    }
                    else if (subpart.Substring(0, 9) == "thickness")
                    {
                        string sp = subpart.Substring(subpart.IndexOf("'") + 1);
                        sp = sp.Substring(0, sp.IndexOf("'"));
                        thickness = double.Parse(sp);
                    }
                    else if (subpart.Substring(0, 6) == "stroke")
                    {
                        string sp = subpart.Substring(subpart.IndexOf("'") + 1);
                        sp = sp.Substring(0, sp.IndexOf("'"));
                        if (sp.Equals("#FF000000"))
                        {
                            stroke.Color = Colors.Black;
                        } else if (sp.Equals("#FFFFFF00"))
                        {
                            stroke.Color = Colors.Yellow;
                        }
                    }
                }
            }
        }

        public string GetString()
        {
            string plines = "";
            if (Points.Count > 0)
            {
                plines += "<polyline points='";
                foreach (Point point in Points)
                {
                    plines += point.X + "," + point.Y + ";";
                }
                plines += "' thickness='" + StrokeThickness + "';";
                plines += "' stroke='" + Stroke.Color.ToString() + "'>";
            }
            return plines;
        }


    }
}
