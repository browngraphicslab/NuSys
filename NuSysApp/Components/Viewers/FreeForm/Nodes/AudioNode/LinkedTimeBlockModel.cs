using System;
using System.Collections.ObjectModel;

namespace NuSysApp.Nodes.AudioNode
{
    public class LinkedTimeBlockModel
    {
        private TimeSpan _start;
        private TimeSpan _end;
        private ObservableCollection<IThumbnailable> _linkedNodes;

        public LinkedTimeBlockModel(TimeSpan start, TimeSpan end)
        {
            _linkedNodes = new ObservableCollection<IThumbnailable>();
            _start = start;
            _end = end;
        }

        public ObservableCollection<IThumbnailable> LinkedNodes
        {
            get { return _linkedNodes; }
            set { _linkedNodes = value; }
        }

        public TimeSpan Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public TimeSpan End
        {
            get { return _end; }
            set { _end = value; }
        }
    }
}