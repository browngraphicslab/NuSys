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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class InqLine : UserControl, ISelectable, Sendable
    {

        private bool _isHighlighting = false;
        private bool _isSelected = false;
        public InqLine()
        {
            this.InitializeComponent();
            this.CanEdit = Atom.EditStatus.Maybe;
            ID = DateTime.UtcNow.Ticks.ToString();

        }

        public InqLine(string id,string data)
        {
            this.InitializeComponent();
            SetLine(data);
            ID = id;
            this.CanEdit = Atom.EditStatus.Maybe;
        }

        public string ID { get; }

        public Atom.EditStatus CanEdit { set; get; }
        public void AddPoint(Point p)
        {
            Line.Points.Add(p);
            SelectedBorder.Points.Add(p);
        }

        public void SetHighlighting(bool highlight)
        {
            if (highlight)
            {
                _isHighlighting = true;
                Line.Stroke = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                _isHighlighting = false;
                Line.Stroke = new SolidColorBrush(Colors.Black);
            }
        }


        public void ToggleSelection()
        {
            _isSelected = !_isSelected;
            if (_isSelected)
            {
                SelectedBorder.Opacity = .8;
            }
            else
            {
                SelectedBorder.Opacity = 0;
            }
        }

        public double StrokeThickness
        {
            get { return Line.StrokeThickness; }
            set { Line.StrokeThickness = value; }
        }

        public Brush Stroke
        {
            get { return Line.Stroke; }
            set { Line.Stroke = value; }

        }

        public bool IsHighlighting
        {
            get { return _isHighlighting; }
        }

        public List<Point> Points
        {
            get { return Line.Points.ToList(); }
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
            if (Line.Points.Count > 0)
            {
                plines += "<polyline points='";
                foreach (Point point in Line.Points)
                {
                    plines += Math.Floor(point.X) + "," + Math.Floor(point.Y) + ";";
                }
                plines += "' thickness='" + Line.StrokeThickness + "'>";
            }
            return plines;
        }

        public void SetLine(string data)
        {
            Polyline poly = new Polyline();
            string[] subparts = data.Split(" ".ToCharArray());
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
                        poly.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 1));
                        //poly.Stroke = new SolidColorBrush(color.psp); TODO add in color
                    }
                }
            }
            Line = poly;
        }
        public async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string,string> props = new Dictionary<string, string>();
            props.Add("line", GetString());
            return props;
        }

        public async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("line"))
            {
                SetLine(props["line"]);
            }
            if (props.ContainsKey("delete") && props["delete"] == "true")
            {
                //TODO add in deletion
            }
        }
    }
}

