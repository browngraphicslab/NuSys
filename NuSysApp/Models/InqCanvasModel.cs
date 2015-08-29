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
    public class InqCanvasModel : Sendable
    {
        public delegate void AddPartialLineEventHandler(object source, AddPartialLineEventArgs e);
        public event AddPartialLineEventHandler OnPartialLineAddition;

        private List<InqLine> _lines;
        private ObservableDictionary<string, ObservableCollection<InqLine>> _partialLines;

        public InqCanvasModel()
        {
            _lines = new List<InqLine>();
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

        public async Task<Dictionary<string, string>> Pack()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("id", ID);
            dict.Add("polylines", InqlinesToString());
            return dict;
        }

        private List<InqLine> ParseToPolyline(string s)
        {
            return InqLine.ParseToPolyline(s);
        }

        public void AddTemporaryPoint(Point p)
        {
            
        }

        public void FinalizeLine(InqLine line)
        {
            this._lines.Add(line);
            OnPartialLineAddition?.Invoke(this, new AddPartialLineEventArgs("Added Lines", line));
        }

        public async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("polylines"))
            {
                _lines = ParseToPolyline(props["polylines"]);
            }
        }

        public string ID { get; }
        public ObservableDictionary<string, ObservableCollection<InqLine>> PartialLines
        {
            get { return _partialLines; }
        }
        public Atom.EditStatus CanEdit { get; set; }
    }
}
