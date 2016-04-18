using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using NuSysApp.Controller;

namespace NuSysApp.Nodes.AudioNode
{
    public class LinkedTimeBlockModel
    {
        private TimeSpan _start;
        private TimeSpan _end;
        public delegate void selectHandler(LinkedTimeBlockModel playbackElement);
        public event selectHandler OnSelect;

        public delegate void deselectHandler(LinkedTimeBlockModel playbackElement);
        public event deselectHandler OnDeselect;
        private Boolean selected;
        //private ObservableCollection<IThumbnailable> _linkedNodes;

        public LinkedTimeBlockModel(TimeSpan start, TimeSpan end)
        {
            //_linkedNodes = new ObservableCollection<IThumbnailable>();
            _start = start;
            _end = end;
            //selected = false;
        }

        //public ObservableCollection<IThumbnailable> LinkedNodes
        //{
            //get { return _linkedNodes; }
            //set { _linkedNodes = value; }
        //}

        public TimeSpan Start
        {
            get { return _start; }
            set { _start = value; }
        }

        public void Select()
        {
             OnSelect?.Invoke(this);
        }

        public void Deselect()
        {
            OnDeselect?.Invoke(this);
        }

        public TimeSpan End
        {
            get { return _end; }
            set { _end = value; }
        }
    }
}