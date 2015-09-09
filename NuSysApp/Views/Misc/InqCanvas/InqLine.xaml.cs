using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NuSysApp.EventArgs;
using System.Collections.Concurrent;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class InqLine : UserControl, Sendable
    {
        public delegate void DeleteInqLineEventHandler(object source, DeleteInqLineEventArgs e);
        public event DeleteInqLineEventHandler OnDeleteInqLine;

        private bool _isHighlighting = false;
        private bool _isSelected = false;

        public InqLine(string id)
        {
            this.InitializeComponent();
            ID = id;
            this.CanEdit = Atom.EditStatus.Maybe;
        }

        public InqLine(string id, List<Point> points, double thickness, Color stroke)
        {
            this.InitializeComponent();
            ID = id;
            this.CanEdit = Atom.EditStatus.Maybe;
            var pc = new PointCollection();
            foreach(var p in points)
            {
                pc.Add(p);
            }
            VisibleLine.Points = pc;
            VisibleLine.Stroke = new SolidColorBrush(stroke);
            VisibleLine.StrokeThickness = thickness;
        }


        public void Delete()
        {
            OnDeleteInqLine?.Invoke(this, new DeleteInqLineEventArgs(this));
            (this.Parent as InqCanvasView).Children.Remove(this);
        }

        public string ID { get; }

        public Atom.EditStatus CanEdit { set; get; }
        public void AddPoint(Point p)
        {
            VisibleLine.Points.Add(p);
        }

        public void SetPoints( PointCollection points)
        {
            VisibleLine.Points = points;
        }

        public void SetHighlighting(bool highlight)
        {
            if (highlight)
            {
                _isHighlighting = true;
                VisibleLine.Stroke = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                _isHighlighting = false;
                VisibleLine.Stroke = new SolidColorBrush(Colors.Black);
            }
        }


        public void ToggleSelection()
        {
            _isSelected = !_isSelected;
            if (_isSelected)
            {
                                                //so thicc
                this.BorderThickness = new Thickness(Double.MaxValue);
            }
            else
            {
                this.BorderThickness = new Thickness(0);
            }
        }

        public double StrokeThickness
        {
            get { return VisibleLine.StrokeThickness; }
            set { VisibleLine.StrokeThickness = value; }
        }

        public Brush Stroke
        {
            get { return VisibleLine.Stroke; }
            set { VisibleLine.Stroke = value; }

        }

        public bool IsHighlighting
        {
            get { return _isHighlighting; }
        }

        public List<Point> Points
        {
            get { return VisibleLine.Points.ToList(); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }

            set
            {
                if (value != _isSelected)
                {
                    ToggleSelection();
                }
            }
        }

        public string GetString()
        {
            string plines = "";
            if (VisibleLine.Points.Count > 0)
            {
                plines += "<polyline points='";
                foreach (Point point in VisibleLine.Points)
                {
                    plines += point.X + "," + point.Y + ";";
                }
                plines += "' thickness='" + VisibleLine.StrokeThickness + "'>";
            }
            return plines;
        }

        private void SetLine(string data, string id)
        {
            List<Point> points;
            double thickness;
            Color stroke;
            ParseToLineData(data, out points, out thickness, out stroke);
            VisibleLine = new InqLine(id) { StrokeThickness = thickness, Stroke = new SolidColorBrush(stroke)}.VisibleLine;
        }

        public static void ParseToLineData(string s, out List<Point> pc, out double thickness, out Color stroke)
        {
            pc = new List<Point>();
            thickness = 1;
            stroke = Colors.Black;

            string[] parts = s.Split(new string[] { "><" }, StringSplitOptions.RemoveEmptyEntries);
            string part = parts[0];
            string[] subparts = part.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
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
                        string sp = subpart.Substring(8, subpart.Length - 10);
                        stroke = Colors.Black;
                        //poly.Stroke = new SolidColorBrush(color.psp); TODO add in color
                    }
                }
            }
        }

        public string Stringify()
        {
            string s = "";
            s += "<polyline points='";
            foreach (Point point in this.Points)
            {
                s += Math.Floor(point.X) + "," + Math.Floor(point.Y) + ";";
            }
            s += "' thickness='" + this.StrokeThickness + "' stroke='" + this.Stroke + "'/>";
            return s;
        }



        public async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string,string> props = new Dictionary<string, string>();
            props.Add("id", ID);
            props.Add("canvasNodeID", ((InqCanvasViewModel) ((InqCanvasView) Parent).DataContext).Model.ID);
            props.Add("data", GetString());
            props.Add("type", "ink");
            props.Add("inkType", "full");
            return props;
        }

        public async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("data"))
            {
                SetLine(props["data"], props["id"]);
            }
            if (props.ContainsKey("delete") && props["delete"] == "true")
            {
                //TODO add in deletion
            }
        }

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {

            var inqCanvas = this.Parent as InqCanvasView;
            if (inqCanvas.Mode is EraseInqMode && inqCanvas.IsPressed)
            {
                NetworkConnector.Instance.RequestDeleteSendable(this.ID);
            }
        }

    }
}

