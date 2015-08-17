using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace NuSysApp
{
    public class WorkSpaceModel
    {

        //Node _selectedNode;
        Dictionary<int, Node> _nodeDict;
        private Dictionary<string, Atom> _idDict;
        private WorkspaceViewModel _workspaceViewModel;
        private int _currentId;
        private bool _isNetwork = false;
        //private Factory _factory;
        public WorkSpaceModel(WorkspaceViewModel vm)
        {
            _nodeDict = new Dictionary<int, Node>();
            _idDict = new Dictionary<string, Atom>();
            _workspaceViewModel = vm;
            _currentId = 0;
            Globals.Network.WorkSpaceModel = this;
            // _factory = new Factory(this);
        }
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
                        NodeViewModel vm = await _workspaceViewModel.CreateNewNode(props["id"], type, x, y);
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

        public bool HasAtom(string id)
        {
            return _idDict.ContainsKey(id);
        }
        private Dictionary<string, string> ParseOutProperties(string message)
        {
            message = message.Substring(1, message.Length - 2);
            string[] parts = message.Split(",".ToCharArray());
            Dictionary<string, string> props = new Dictionary<string, string>();
            foreach (string part in parts)
            {
                string[] subParts = part.Split('=');
                if (subParts.Length != 2)
                {
                    Debug.WriteLine("Error, property formatted wrong in message: " + message);
                    continue;
                }
                props.Add(subParts[0], subParts[1]);
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
                        ret += tup.Key + '=' + tup.Value + ',';
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
