using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class InqLineViewModel : BaseINPC
    {
        private Point _lastAddedPoint;
        public InqLineViewModel(InqLineModel model)
        {
            Model = model;
        }
        public InqLineModel Model { get; }

        public Point LastAddedPoint
        {
            set
            {
                if (_lastAddedPoint == value) return;
                _lastAddedPoint = value;
                RaisePropertyChanged("LastAddedPoint");
            }
            get { return _lastAddedPoint; }
        }
    }
}
