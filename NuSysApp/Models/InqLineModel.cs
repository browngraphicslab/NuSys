using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<Point2d> _points; 

        public delegate void DeleteInqLineEventHandler(object source, DeleteInqLineEventArgs e);
        public event DeleteInqLineEventHandler OnDeleteInqLine;

        public InqLineModel(string id) : base(id)
        {
            _points = new ObservableCollection<Point2d>();
        }
        
        public void AddPoint(Point2d p)
        {
            Points.Add(p);
        }

        public void Delete()
        {
            OnDeleteInqLine?.Invoke(this, new DeleteInqLineEventArgs(this));
            SessionController.Instance.IdToSendables.Remove(Id);
        }

        public async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("data"))
            {
                //TODO: Re-add
                //SetLine(props["data"], props["id"]);
            }
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = new Dictionary<string, object>();
            props.Add("id", Id);
            props.Add("canvasNodeID", InqCanvasId);
            props.Add("data", GetString());
            props.Add("type", "ink");
            props.Add("inkType", "full");
            return props;
        }

        public ObservableCollection<Point2d> Points
        {
            get { return _points; }
            set
            {
                _points = value;
            }
        }

        private void SetLine(string data, string id)
        {
            ObservableCollection<Point2d> points;
            double thickness;
            SolidColorBrush stroke;
            ParseToLineData(data, out points, out thickness, out stroke);
            //Points = points;
            StrokeThickness = thickness;
            Stroke = stroke;
            var view = new InqLineView(new InqLineViewModel(this), StrokeThickness, Stroke);
        }

        public double StrokeThickness { get; set; }
        public string InqCanvasId { get; set; }
        public SolidColorBrush Stroke { get; set; }

        public static void ParseToLineData(string s, out ObservableCollection<Point2d> pc, out double thickness, out SolidColorBrush stroke)
        {
            pc = new ObservableCollection<Point2d>();
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
                                var parsedPoint = new Point2d(double.Parse(coords[0]), double.Parse(coords[1]));
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

        public PointCollection ToPointCollection()
        {
            var pc = new PointCollection();
            foreach (var p in _points)
            {
                pc.Add(new Point(p.X,p.Y));   
            }
            return pc;
        }


    }
}
