using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using NuSysApp.EventArgs;

namespace NuSysApp
{
    public class InqCanvasModel
    {
        public delegate void AddPartialLineEventHandler(object source, AddPartialLineEventArgs e);
        public event AddPartialLineEventHandler OnPartialLineAddition;

        public event FinalizedLine OnFinalizedLine;
        public delegate void FinalizedLine();

        private HashSet<InqLine> _lines;
        private ObservableDictionary<string, ObservableCollection<InqLine>> _partialLines;


        public InqCanvasModel(string id)
        {
            ID = id;
            _lines = new HashSet<InqLine>();
            _partialLines = new ObservableDictionary<string, ObservableCollection<InqLine>>();
            _partialLines.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs args)
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (ObservableCollection<InqLine> n in _partialLines.Values)
                    {
                        n.CollectionChanged += delegate (object o, NotifyCollectionChangedEventArgs eventArgs)
                        {
                            InqLine l = ((InqLine)((object[])eventArgs.NewItems.SyncRoot)[0]);
                            OnPartialLineAddition?.Invoke(this, new AddPartialLineEventArgs("Added Partial Lines", l));
                        };
                    }
                }
            };

        }
        public HashSet<InqLine> Lines {
            get { return _lines; } 
            set { _lines = value; }
        }
        public string ID { get; }
        public void Delete()
        {
            
        }

        private string InqlinesToString()
        {
            string plines = "";
            foreach (InqLine pl in _lines)
            {
                if (pl.Points.Count > 0)
                {
                    plines += pl.Stringify();
                }
            }
            return plines;
        }

        public void AddTemporaryPoint(Point p)
        {
            
        }

        public void FinalizeLine(InqLine line)
        {
            this._lines.Add(line);
            line.OnDeleteInqLine += LineOnDeleteInqLine;
            OnPartialLineAddition?.Invoke(this, new AddPartialLineEventArgs("Added Lines", line));
            OnFinalizedLine?.Invoke();
        }

        private void LineOnDeleteInqLine(object source, DeleteInqLineEventArgs deleteInqLineEventArgs)
        {
            this._lines.Remove(deleteInqLineEventArgs.LineToDelete);
        }
        public ObservableDictionary<string, ObservableCollection<InqLine>> PartialLines
        {
            get { return _partialLines; }
        }

        public void AddTemporaryInqline(InqLine line, string temporaryID)
        {
            Debug.WriteLine("temp line added with temp ID: "+temporaryID);
            if (!_partialLines.ContainsKey(temporaryID))
            {
                _partialLines.Add(temporaryID, new ObservableCollection<InqLine>());
            }
            _partialLines[temporaryID].Add(line);
        }

        public void RemovePartialLines(string oldID)
        {
            Debug.WriteLine("old id to be removed for partial line destruction: " + oldID);
            if (_partialLines.ContainsKey(oldID))
            {
                foreach (InqLine l in _partialLines[oldID])
                {
                    l.Delete();
                }
                _partialLines.Remove(oldID);
            }
        }
        public Atom.EditStatus CanEdit { get; set; }
    }
}
