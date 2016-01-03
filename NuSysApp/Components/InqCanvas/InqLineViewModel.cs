using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class InqLineViewModel : BaseINPC
    {
        public PointCollection Points { get; set; }

        public InqLineModel Model { get; }

        private Size _canvasSize;

        public InqLineViewModel(InqLineModel model, Size canvasSize)
        {
            _canvasSize = canvasSize;
            Model = model;
            Model.Points.CollectionChanged += PointsOnCollectionChanged;

            Points = new PointCollection();
            var unNormalizedPoints = model.Points.Select(p => new Point(p.X * _canvasSize.Width, p.Y * _canvasSize.Height));
            foreach (var p in unNormalizedPoints)
            {
                Points.Add(p);
            }
        }

        private void PointsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null)
                return;

            var p = (Point2d) notifyCollectionChangedEventArgs.NewItems[0];
            Points.Add(new Point(p.X * _canvasSize.Width, p.Y * _canvasSize.Height));
            RaisePropertyChanged("Points");
        }
        
    }
}
