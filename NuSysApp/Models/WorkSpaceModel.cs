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
        private Dictionary<string, Node> _idDict;
        private WorkspaceViewModel _workspaceViewModel;
        private int _currentId;
        private bool _isNetwork = false;
        //private Factory _factory;
        public WorkSpaceModel(WorkspaceViewModel vm)
        {
            _nodeDict = new Dictionary<int, Node>();
            _idDict = new Dictionary<string, Node>();
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
                    Node n = _idDict[id];
                    await n.Update(props);
                }
                else
                {
                    NodeType type = NodeType.Text;
                    double x = 0;
                    double y = 0;
                    if (props.ContainsKey("nodeType"))
                    {
                        string t = props["nodeType"];
                        type = (NodeType)Enum.Parse(typeof(NodeType),t);
                    }
                    if (props.ContainsKey("x"))
                    {
                        double.TryParse(props["x"], out x);
                    }
                    if (props.ContainsKey("y"))
                    {
                        double.TryParse(props["y"], out y);
                    }
                    Node a = await _workspaceViewModel.CreateNewNode(props["id"],type, x, y);
                    if (a == null)
                    {
                        _isNetwork = false;
                        return;
                    }
                    _idDict.Add(id, a);
                }
                _isNetwork = false;
            });
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

        public async Task UpdateNetwork(string message)
        {
            if (!_isNetwork)
            {
                await Globals.Network.SendMassUDPMessage(message);
            }
        }

        public async Task SendMessageToHost(string message)
        {
            if (!_isNetwork)
            {
                await Globals.Network.SendMessageToHost(message);
            }
        }

        public string GetFullWorkspace()
        {
            string ret = "";
            foreach (KeyValuePair<string, Node> kvp in _idDict)
            {
                ret += '<';
                Node node = kvp.Value;
                List<Tuple<string,double>> parts = new List<Tuple<string, double>>();
                parts.Add(new Tuple<string,double>("x",node.X));
                parts.Add(new Tuple<string, double>("y", node.Y));
                parts.Add(new Tuple<string, double>("width", node.Width));
                parts.Add(new Tuple<string, double>("height", node.Height));
                foreach (Tuple<string, double> tup in parts)
                {
                    ret += tup.Item1 + '=' + tup.Item2 + ',';
                }
                ret += "id="+node.ID + ">&&";
            }
            ret = ret.Substring(0, ret.Length - 2);
            return ret;
        }
    }
}
