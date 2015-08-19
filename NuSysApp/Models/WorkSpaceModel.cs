using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class WorkSpaceModel
    {

        //Node _selectedNode;
        private Dictionary<string, Atom> _idDict;
        private WorkspaceViewModel _workspaceViewModel;
        private int _currentId;
        private bool _isNetwork = false;
        //private Factory _factory;
        public WorkSpaceModel(WorkspaceViewModel vm)
        {
            _idDict = new Dictionary<string, Atom>();
            _workspaceViewModel = vm;
            AtomDict = new Dictionary<string, AtomViewModel>();
            _currentId = 0;
            NetworkConnector.Instance.WorkSpaceModel = this;
            // _factory = new Factory(this);
        }
        public Dictionary<string, AtomViewModel> AtomDict
        { set; get; }

        public void CreateNewTextNode(string data)
        {
            //_nodeDict.Add(CurrentID, _factory.createNewTextNode(data));
            //CurrentID++;
        }
        public int CurrentId
        {
            get { return _currentId; }
            set { if(value >= _currentId)//decreasing the current ID doesn't make sense
                {
                    _currentId = value;
                }
            }
        }

        public bool Locked
        {
            get { return _isNetwork; }
        }
        public async void HandleMessage(string s)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                _isNetwork = true;
                Dictionary<string, string> props = ParseOutProperties(s);
                string id = props["id"];//since we called parse properties, it MUST have an id
                if (_idDict.ContainsKey(id))
                {
                    Atom n = _idDict[id];
                    n.UnPack(props);
                }
                else
                {
                    if (props.ContainsKey("type") && props["type"] == "node")
                    {
                        NodeType type = NodeType.Text;
                        double x = 0;
                        double y = 0;
                        object data = null;
                        if (props.ContainsKey("nodeType"))
                        {
                            string t = props["nodeType"];
                            type = (NodeType) Enum.Parse(typeof (NodeType), t);
                        }
                        if (props.ContainsKey("x"))
                        {
                            double.TryParse(props["x"], out x);
                        }
                        if (props.ContainsKey("y"))
                        {
                            double.TryParse(props["y"], out y);
                        }
                        if (props.ContainsKey("data"))
                        {
                            string d = props["data"];
                            if (d.Substring(0, 10).Contains("polyline"))
                            {
                                data = ParseToPolyline(d);
                            }
                        }
                        NodeViewModel vm = await _workspaceViewModel.CreateNewNode(props["id"], type, x, y, data);
                        Node node = (Node) vm.Model;
                        if (node == null)
                        {
                            _isNetwork = false;
                            return;
                        }
                        _idDict.Add(id, node);
                    }
                    else if (props.ContainsKey("type") && (props["type"] == "link" || props["type"] == "linq"))
                    {
                        string id1 = "null";
                        string id2 = "null";
                        if (props.ContainsKey("id1"))
                        {
                            id1 = props["id1"];
                        }
                        if (props.ContainsKey("id2"))
                        {
                            id1 = props["id2"];
                        }
                        AtomViewModel avm1;
                        AtomViewModel avm2;
                        if (_idDict.ContainsKey(id1))
                        {
                            //avm1 = _idDict[id1]
                        }

                        //LinkViewModel vm = await _workspaceViewModel.CreateNewLink(id)
                    }
                }
                _isNetwork = false;
            });
        }

        private Polyline[] ParseToPolyline(string s)
        {
            List<Polyline> polys = new List<Polyline>();
            string[] parts = s.Split("><".ToCharArray());
            foreach (string part in parts)
            {
                Polyline poly = new Polyline();
                string[] subparts = part.Split(" ".ToCharArray());
                foreach (string subpart in subparts)
                {
                    if (subpart.Length > 0 && subpart!="polyline")
                    {
                        if (subpart.Substring(0, 6) == "points")
                        {
                            string innerPoints = subpart.Substring(8, subpart.Length - 9);
                            string[] points = innerPoints.Split(";".ToCharArray());
                            foreach (string p in points)
                            {
                                if (p.Length > 0)
                                {
                                    string[] coords = p.Split(",".ToCharArray());
                                    //Point point = new Point(double.Parse(coords[0]), double.Parse(coords[1]));
                                    poly.Points.Add(new Point(Int32.Parse(coords[0]), Int32.Parse(coords[1])));
                                }
                            }
                        }
                        else if (subpart.Substring(0, 9) == "thickness")
                        {
                            string sp = subpart.Substring(11, subpart.Length - 12);
                            poly.StrokeThickness = double.Parse(sp);
                        }
                        else if (subpart.Substring(0, 6) == "stroke")
                        {
                            string sp = subpart.Substring(8, subpart.Length - 10);
                            poly.Stroke = new SolidColorBrush(Color.FromArgb(255,0,0,1));
                            //poly.Stroke = new SolidColorBrush(color.psp); TODO add in color
                        }
                    }
                }
                if (poly.Points.Count > 0)
                {
                    polys.Add(poly);
                }
            }
            return polys.ToArray();
        }
        public bool HasAtom(string id)
        {
            return _idDict.ContainsKey(id);
        }
        private Dictionary<string, string> ParseOutProperties(string message)
        {
            message = message.Substring(1, message.Length - 2);
            string[] parts = message.Split(Constants.CommaReplacement.ToCharArray());
            Dictionary<string, string> props = new Dictionary<string, string>();
            foreach (string part in parts)
            {
                if (part.Length > 0)
                {
                    string[] subParts = part.Split("=".ToCharArray(),2);
                    if (subParts.Length != 2)
                    {
                        Debug.WriteLine("Error, property formatted wrong in message: " + message);
                        continue;
                    }
                    props.Add(subParts[0], subParts[1]);
                }
            }
            return props;
        }
        public void RemoveNode(string id)
        {
            if (_idDict.ContainsKey(id))
            {
                //TODO Remove node visually
                _idDict.Remove(id);
            }
        }

        public string GetFullWorkspace()
        {
            if (_idDict.Count > 0)
            {
                string ret = "";
                foreach (KeyValuePair<string, Atom> kvp in _idDict)
                {
                    ret += '<';
                    Atom atom = kvp.Value;
                    Dictionary<string, string> parts = atom.Pack();
                    foreach (KeyValuePair<string,string> tup in parts)
                    {
                        ret += tup.Key + '=' + tup.Value + Constants.CommaReplacement;
                    }
                    ret += "id=" + atom.ID + ">&&";
                }
                ret = ret.Substring(0, ret.Length - 2);
                return ret;
            }
            return "";
        }
    }
}
