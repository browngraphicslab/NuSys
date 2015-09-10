using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class InqLineViewModel : BaseINPC
    {
        public InqLineModel Model { get; }

        public PointCollection Points
        {
            get { return Model.Points; }
        } 
        public InqLineViewModel(InqLineModel model)
        {
            Model = model;
            Model.OnPointAdded += Model_OnPointAdded;
            Model.OnDeleteInqLine += Model_OnDeleteInqLine;
        }

        public void SetParentID(string id)
        {
            Model.ParentID = id;
        }

        private void Model_OnDeleteInqLine(object source, EventArgs.DeleteInqLineEventArgs e)
        {
            RaisePropertyChanged("ToDelete");
        }

        private void Model_OnPointAdded(object source, AddPointEventArgs e)
        {
            RaisePropertyChanged("Points");
        }
    }
}
