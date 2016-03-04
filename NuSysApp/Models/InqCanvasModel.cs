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
using Newtonsoft.Json;

namespace NuSysApp
{
    public class InqCanvasModel : Sendable
    {
 
        public event LineHandler LineFinalized;
        public event LineHandler LineFinalizedLocally;
        public event LineHandler LineRemoved;
        public event LineHandler LineAdded;
        public event PageChangeHandler PageChanged;
        public event DisposeInqHandler AppSuspended;
        public delegate void PageChangeHandler(int page);
        public delegate void LineHandler(InqLineModel lineModel);
        public delegate void DisposeInqHandler();
        
        private HashSet<InqLineModel> _lines = new HashSet<InqLineModel>();
        private Dictionary<string, HashSet<InqLineModel>> _partialLines;
        private int _page;

        public int Page {
            get { return _page; }
            set
            {
                _page = value;
                PageChanged?.Invoke(_page);
            } }

        public InqCanvasModel(string id) : base(id)
        {
            _partialLines = new Dictionary<string, HashSet<InqLineModel>>();
        }

        private void AddLine(InqLineModel line)
        {
            _lines.Add(line);
            LineAdded?.Invoke(line);
        }

        public void RemoveLine(InqLineModel line)
        {
            var lines = _lines.Where(l => l.Id == line.Id);
            if (!lines.Any()) return;
            var ln = lines.First();
            ln.Delete();
            _lines.Remove(ln);
        }

        public HashSet<InqLineModel> Lines {
            get { return _lines; }
         
        }

        public void FinalizeLineLocally(InqLineModel line)
        {
            line.OnDeleteInqLine += LineOnDeleteInqLine;
            LineFinalizedLocally?.Invoke(line);
        }

        public void FinalizeLine(InqLineModel line)
        {
            line.Page = Page;
            _lines.Add(line);
            LineFinalized?.Invoke( line );
        }

        public void DisposeInq()
        {
            AppSuspended?.Invoke();
        }

        private void LineOnDeleteInqLine(object source, InqLineModel inqLine)
        {
            _lines.Remove(inqLine);
            LineRemoved?.Invoke(inqLine);
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

        public override async Task<Dictionary<string, object>> Pack()
        {
            var dict =  await base.Pack();
            dict["page"] = Page;
            dict["lines"] = JsonConvert.SerializeObject(Lines.ToArray());
            return dict;
        }

        public override Task UnPack(Message props)
        {
            var lines = props.GetList<InqLineModel>("inqLines");
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    AddLine(line);
                }
            }
            if (props.ContainsKey("page"))
            {
                Page = props.GetInt("page", 0);
            }
            return base.UnPack(props);
        }
    }
}
