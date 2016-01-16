﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class InqCanvasViewModel : BaseINPC
    {
        public InqCanvasModel Model { get; }

        private List<InqLineView> _lines = new List<InqLineView>();

        public ObservableCollection<InqLineView> Lines { get; set; }

        private Size _canvasSize;

        private InqCanvasImageSource _source;

        private int _page;

       public int Page
        {
            get { return _page; }
            set
            {
                _page = value;
                Lines.Clear();
                
                foreach (var line in _lines.ToList())
                {
                  if ((line.DataContext as InqLineViewModel).Page == value)
                    Lines.Add(line);
                }
            }
        }

        public InqCanvasViewModel(InqCanvasModel model, Size canvasSize)
        {
            _canvasSize = canvasSize;
            Model = model;
            Model.LineFinalized += OnLineFinalized;
            Model.LineRemoved += OnLineRemoved;
            Model.LineAdded += OnLineAdded;
            Model.PageChanged +=OnPageChanged;
            _page = model.Page;

            _source = new InqCanvasImageSource((int)canvasSize.Width, (int)canvasSize.Height, true);
            _source.BeginDraw();
            _source.Clear(Windows.UI.Colors.White);
            _source.EndDraw();

            Lines = new ObservableCollection<InqLineView>();

            if (model.Lines == null)
                return;

            foreach (var inqLineModel in model.Lines)
            {
                var lineView = new InqLineView(new InqLineViewModel(inqLineModel, _canvasSize));;
                AddLine(lineView);
            }
        }

        private void OnPageChanged(int page)
        {
            Page = page;
        }

        private void OnLineAdded(InqLineModel lineModel)
        {
            AddLine(new InqLineView(new InqLineViewModel(lineModel, _canvasSize)));
        }

        private void OnLineRemoved(InqLineModel lineModel)
        {
            var ls = _lines.Where(l => (l.DataContext as InqLineViewModel).Model == lineModel);
            if (!ls.Any())
                return;
            RemoveLine(ls.First());
        }
        
        private async void OnLineFinalized(InqLineModel lineModel)
        {
            var lineView = new InqLineView(new InqLineViewModel(lineModel, _canvasSize));
            AddLine(lineView);

            List<Point> allP = new List<Point>();
            List<InqLineModel> ll = Model.Lines.ToList();
            
            foreach(InqLineModel ilm in ll)
            {
                foreach(Point2d p in ilm.Points) {
                    allP.Add(new Point(p.X * Constants.MaxCanvasSize, p.Y * Constants.MaxCanvasSize));
                }
            }


            //foreach(InqLineView ilv in _lines)
            //{
            //    foreach(Point p in ilv.Points)
            //    {
            //        allP.Add(p);
            //    }
            //}
            _source.BeginDraw();

            // Clear background
            _source.Clear(Windows.UI.Colors.White);
            _source.DrawLine(Windows.UI.Colors.Black, allP.ToArray());
            _source.EndDraw();
            RaisePropertyChanged("FinalLineAdded");
        }

        public void RemoveLine(InqLineView line)
        {
            _lines.Remove(line);
            Lines.Remove(line);
        }

        public void AddLine(InqLineView line)
        {
            _lines.Add(line);
            if ((line.DataContext as InqLineViewModel).Page == Page)
                Lines.Add(line);
        }

        public Windows.UI.Xaml.Media.ImageSource CanvasSource
        {
            get { return _source; }
        }

        public Size CanvasSize {
            get { return _canvasSize; }
            set
            {
                _canvasSize = value;

                foreach (var inqLineView in _lines)
                {
                    (inqLineView.DataContext as InqLineViewModel).CanvasSize = _canvasSize;
                }

                RaisePropertyChanged("CanvasSize.Height");
                RaisePropertyChanged("CanvasSize.Height");
                RaisePropertyChanged("CanvasSize");

            }
        }
    }
}
