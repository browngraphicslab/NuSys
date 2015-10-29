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
        public delegate void FinalizedLine(InqLineModel lineModel);

        private HashSet<InqLineModel> _lines;
        private Dictionary<string, HashSet<InqLineModel>> _partialLines;


        public InqCanvasModel(string id)
        {
            ID = id;
            _lines = new HashSet<InqLineModel>();
            _partialLines = new Dictionary<string, HashSet<InqLineModel>>();
            OnFinalizedLine += delegate(InqLineModel model)
            {
                int i = 0;
            };
            /*
            _partialLines = new ObservableDictionary<string, ObservableCollection<InqLineView>>();
            _partialLines.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs args)
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (ObservableCollection<InqLineView> n in _partialLines.Values)
                    {
                        n.CollectionChanged += delegate (object o, NotifyCollectionChangedEventArgs eventArgs)
                        {
                            InqLineView l = ((InqLineView)((object[])eventArgs.NewItems.SyncRoot)[0]);
                            OnPartialLineAddition?.Invoke(this, new AddPartialLineEventArgs("Added Partial Lines", l));
                        };
                    }
                }
            };*/

        }
        public HashSet<InqLineModel> Lines {
            get { return _lines; } 
            set { _lines = value; }
        }
        public string ID { get; }
        public void Delete()
        {
            
        }

        public string StringLines
        {
            get { return InqlinesToString(); }
        }

        private string InqlinesToString()
        {
            string plines = "";
            foreach (InqLineModel pl in _lines)
            {
                if (pl.Points.Count > 0)
                {
                    plines += pl.GetString();
                }
            }
            return plines;
        }

        public void AddTemporaryPoint(Point p)
        {
            
        }

        public void FinalizeLine(InqLineModel line)
        {
            this._lines.Add(line);
            line.OnDeleteInqLine += LineOnDeleteInqLine;
            //OnPartialLineAddition?.Invoke(this, new AddPartialLineEventArgs("Added Lines", line));
            OnFinalizedLine?.Invoke( line );
        }

        private void LineOnDeleteInqLine(object source, DeleteInqLineEventArgs deleteInqLineEventArgs)
        {
            this._lines.Remove(deleteInqLineEventArgs.LineModelToDelete);
        }
        public Dictionary<string, HashSet<InqLineModel>> PartialLines
        {
            get { return _partialLines; }
        }

        public void AddTemporaryInqline(InqLineModel lineModel, string temporaryID)
        {
            if (!_partialLines.ContainsKey(temporaryID))
            {
                _partialLines.Add(temporaryID, new HashSet<InqLineModel>());
            }
            _partialLines[temporaryID].Add(lineModel);
            OnPartialLineAddition?.Invoke(this, new AddPartialLineEventArgs("Added Partial Lines", lineModel));
        }

        public void RemovePartialLines(string oldID)
        {
            if (_partialLines.ContainsKey(oldID))
            {
                foreach (InqLineModel l in _partialLines[oldID])
                {
                    l.Delete();
                    _lines.Remove(l);
                }
                _partialLines.Remove(oldID);
            }
        }
        public AtomModel.EditStatus CanEdit { get; set; }
    }
}
