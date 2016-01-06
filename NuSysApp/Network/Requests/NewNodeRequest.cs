using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewNodeRequest : Request
    {
        //public async Task CreateNewNode(string Id, NodeType type, double XCoordinate, double YCoordinate, object Data = null)
        public string Id;
        public NodeType Nodetype;
        public double XCoordinate = Double.PositiveInfinity;
        public double YCoordinate = Double.PositiveInfinity;
        public object Data;
        public Dictionary<string, string> ExtraProperties;
        /*
        public NewNodeRequest() : base(RequestType.NewNodeRequest)
        {
            Init();
        }

        public NewNodeRequest(NodeType type) : base(RequestType.NewNodeRequest)
        {
            Nodetype = type;
            Init();
        }*/

        public NewNodeRequest(double x, double y, NodeType type, string id = null, object data = null,
            Dictionary<string, string> extraProperties = null) : base(Request.RequestType.NewNodeRequest)
        {
            XCoordinate = x;
            YCoordinate = y;
            Nodetype = type;
            Id = id;
            Data = data;
            ExtraProperties = extraProperties;

            if (Id == null)
            {
                Id = SessionController.Instance.GenerateId();
            }
            _message["x"] = XCoordinate.ToString();
            _message["y"] = YCoordinate.ToString();
            _message["Nodetype"] = Nodetype.ToString();
            _message["type"] = "node";
            _message["Id"] = Id;

            if (Data != null && Data != "null" && Data != "")
            {
                _message["Data"] = Data.ToString();
            }
            if (ExtraProperties != null && ExtraProperties.Count > 0)
            {
                _message["extra_props"] = Newtonsoft.Json.JsonConvert.SerializeObject(ExtraProperties);
            }
        }


        public NewNodeRequest(Message message) : base(message)
        {
            if (message.ContainsKey("x"))
            {
                XCoordinate = message.GetDouble("x");
            }
            if (message.ContainsKey("y"))
            {
                YCoordinate = message.GetDouble("y");
            }
            if (message.ContainsKey("Nodetype"))
            {
                Nodetype = (NodeType) Enum.Parse(typeof (NodeType), message.GetString("Nodetype"));
            }
            if (message.ContainsKey("Id"))
            {
                Id = message.GetString("Id");
            }
            if (message.ContainsKey("extra_props"))
            {
                ExtraProperties = _message.GetDict<string, string>("extra_props");
            }
            if (message.ContainsKey("Data") && message.ContainsKey("Nodetype"))
            {
                switch (Nodetype)
                {
                    case NodeType.Text:
                        Data = message.GetString("Data");
                        break;
                    case NodeType.Image: case NodeType.PDF: case NodeType.Audio: case NodeType.Video:
                        try
                        {
                            Data = message.GetByteArray("Data");
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Node Creation ERROR: Data could not be parsed into a byte array");
                        }
                        break;
                }
            }
        }

        public override async Task CheckRequest()
        {
            if (Double.IsPositiveInfinity(_message.GetDouble("x")) || Double.IsPositiveInfinity(_message.GetDouble("y")))
            {
                throw new NewNodeRequestException("X or Y coordinate of new node request was invalid (positive infinity)");
            }
        }

        public override async Task ExecuteRequestFunction()
        {
            NodeModel node = await SessionController.Instance.CreateNewNode(Id, Nodetype);
            Message m = new Message(ExtraProperties);

            SessionController.Instance.IdToSendables[Id] = node;
            await node.UnPack(m);
            var creator = (node as AtomModel).Creator;
            if (creator != null)
                await (SessionController.Instance.IdToSendables[creator] as NodeContainerModel).AddChild(node);
            //else
                //await SessionController.Instance.ActiveWorkspace.Model.AddChild(node);

        }
        private byte[] ParseToByteArray(string s)
        {
            return Convert.FromBase64String(s);
        }
    }
    public class NewNodeRequestException : Exception
    {
        public NewNodeRequestException(string message) : base(message) { }
        public NewNodeRequestException() : base("There was an error in the NewNodeRequest") { }
    }
}
