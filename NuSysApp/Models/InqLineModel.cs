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
using NusysIntermediate;


namespace NuSysApp
{
    public class InqLineModel : Sendable
    {
        //We might change this to hashset for erasing but we'll cross that bridge later
        private ObservableCollection<Point2d> _points;

        public double StrokeThickness { get; set; }
        public string InqCanvasId { get; set; }
        public int Page { get; set; }
        public SolidColorBrush Stroke { get; set; }
        public bool IsGesture { get; set; }
        public bool IsDeleted { get; set; }

        public delegate void DeleteInqLineEventHandler(object source, InqLineModel inqLine);
        public event DeleteInqLineEventHandler OnDeleteInqLine;

        public InqLineModel(string id) : base(id)
        {
            _points = new ObservableCollection<Point2d>();
            Stroke = new SolidColorBrush(Colors.Black);
            StrokeThickness = 3;
        }

        public void AddPoint(Point2d p)
        {
            Points.Add(p);
        }

        public void Delete()
        {
            IsDeleted = true;
            OnDeleteInqLine?.Invoke(this, this);
            ElementController removed;
            SessionController.Instance.IdToControllers.TryRemove(Id, out removed);
        }

        public override Task UnPack(Message props)
        {
            InqCanvasId = props.GetString("canvasNodeID", null);
            Points = new ObservableCollection<Point2d>(props.GetList<Point2d>("points"));
            Page = props.GetInt("page", 0);
            IsGesture = props.GetBool("isGesture", false);
            return base.UnPack(props);
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            var props = new Dictionary<string, object>();
            props.Add("id", Id);
            props.Add("canvasNodeID", InqCanvasId);
            props.Add("points", Points);
            props.Add("type", "ink");
            props.Add("inkType", "full");
            props.Add("page", Page);
            props.Add("isGesture", IsGesture);
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

        public IList<LineSegment> ToLineSegments()
        {
            var result = new List<LineSegment>();
            for (var i = 0; i < Points.Count - 1; ++i)
            {
                Point2d start = Points[i];
                Point2d end = Points[i + 1];
                result.Add(new LineSegment(start.ToVector2(), end.ToVector2()));
            }
            return result;
        }

        public InqLineModel GetScaled(double scale)
        {
            var nm = new InqLineModel("");
            foreach (var point in Points)
            {
                nm.AddPoint(new Point2d(point.X * scale, point.Y * scale));
            }
            return nm;
        }
    }
}
