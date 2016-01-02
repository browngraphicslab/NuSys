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
    public class InqCanvasModel : Sendable
    {
 
        public event AddPartialLineEventHandler PartialLineAdded;
        public event LineHandler LineFinalized;
        public event LineHandler LineRemoved;
        public delegate void LineHandler(InqLineModel lineModel);
        public delegate void AddPartialLineEventHandler(object source, AddLineEventArgs e);
        
        private HashSet<InqLineModel> _lines = new HashSet<InqLineModel>();
        private Dictionary<string, HashSet<InqLineModel>> _partialLines;


        public InqCanvasModel(string id) : base(id)
        {
            _partialLines = new Dictionary<string, HashSet<InqLineModel>>();
        }

        public HashSet<InqLineModel> Lines {
            get { return _lines; }
            set
            {
                _lines = value;
            }
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

        public void FinalizeLine(InqLineModel line)
        {
            _lines.Add(line);
            line.OnDeleteInqLine += LineOnDeleteInqLine;
            LineFinalized?.Invoke( line );
        }

        private void LineOnDeleteInqLine(object source, DeleteInqLineEventArgs deleteInqLineEventArgs)
        {
            _lines.Remove(deleteInqLineEventArgs.LineModelToDelete);
            LineRemoved?.Invoke(deleteInqLineEventArgs.LineModelToDelete);
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
            PartialLineAdded?.Invoke(this, new AddLineEventArgs(lineModel));
        }

        public void RemovePartialLines(string oldID)
        {
            if (_partialLines.ContainsKey(oldID))
            {
                foreach (InqLineModel l in _partialLines[oldID])
                {
//                    l.Delete();
                    _lines.Remove(l);
                }
                _partialLines.Remove(oldID);
            }
        }
    }
}
