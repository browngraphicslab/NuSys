using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;
using NuSysApp.Util;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class AreaNodeViewModel : NodeContainerViewModel
    {

        public PointCollection Points { get; set; }

        public PathGeometry PathGeometry { get; set; }

        public AreaNodeViewModel(NodeContainerModel model):base(model)
        {

        
           // Points = new PointCollection();
           // model.Points.ForEach((p => Points.Add(p)));
            var points = model.Points.ToArray();

            Point2d[] cp1, cp2;
            BezierSpline.GetCurveControlPoints(points, out cp1, out cp2);

            // Draw curve by Bezier.
            PathSegmentCollection lines = new PathSegmentCollection();
            for (int i = 0; i < cp1.Length; ++i)
            {
                lines.Add(new BezierSegment()
                {
                    Point1 = cp1[i],
                    Point2 = cp2[i],
                    Point3 = points[i + 1]
                });
            }
            PathFigure f = new PathFigure {StartPoint = points[0], Segments = lines};
            f.IsClosed = true;
            
            var pfc = new PathFigureCollection();
            pfc.Add(f);
            PathGeometry = new PathGeometry() {Figures = pfc};
            RaisePropertyChanged("PathGeometry");
      

        }


        protected async override void Added(Sendable nodeModel)
        {
            var model = (AtomModel)nodeModel;
          //  model.X -= Model.X;
          //  model.Y -= Model.Y;
            
            
        }
    }
}
