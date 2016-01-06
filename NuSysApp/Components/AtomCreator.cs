using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp.Components
{
    class AtomCreator
    {

        public async Task HandleCreateNewSendable(string id, Message props)
        {

            if (props.ContainsKey("type") && props["type"] == "group")
            {
                await HandleCreateNewGroup(id, props);
            }
            else if (props.ContainsKey("type") && props["type"] == "grouptag")
            {
                await HandleCreateNewGroupTag(id, props);
            }
            else if (props.ContainsKey("type") && props["type"] == "node")
            {
                await HandleCreateNewNode(id, props);
            }
            else if (props.ContainsKey("type") && (props["type"] == "link" || props["type"] == "linq"))
            {
                await HandleCreateNewLink(id, props);
            }
            else if (props.ContainsKey("type") && (props["type"] == "pin"))
            {
                await HandleCreateNewPin(id, props);
            }
        }

        private async Task HandleCreateNewPin(string id, Message props)
        {
            double x = 0;
            double y = 0;
            if (props.ContainsKey("x") && props.ContainsKey("y"))
            {
                try
                {
                    x = double.Parse(props.GetString("x"));
                    y = double.Parse(props.GetString("y"));
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Pin creation failed because coordinates could not be parsed to doubles");
                }
                await UITask.Run(async () =>
                {
                    await SessionController.Instance.CreateNewPin(id, x, y);
                });
            }
        }
        private async Task HandleCreateNewLink(string id, Message props)
        {
            string id1 = "null";
            string id2 = "null";
            if (props.ContainsKey("id1"))
            {
                id1 = props.GetString("id1");
            }
            else
            {
                Debug.WriteLine("Could not create link");
                return;
            }
            if (props.ContainsKey("id2"))
            {
                id2 = props.GetString("id2");
            }
            else
            {
                Debug.WriteLine("Could not create link");
                return;
            }

            if (SessionController.Instance.IdToSendables.ContainsKey(id1) && (SessionController.Instance.IdToSendables.ContainsKey(id2)))
            {
                await UITask.Run(async () => { SessionController.Instance.CreateLink((AtomModel)SessionController.Instance.IdToSendables[id1], (AtomModel)SessionController.Instance.IdToSendables[id2], id); });

            }
        }
        private async Task HandleCreateNewNode(string id, Message props)
        {
            NodeType type = NodeType.Text;
            double x = 0;
            double y = 0;
            object data = null;
            if (props.ContainsKey("nodeType"))
            {
                string t = props.GetString("nodeType");
                type = (NodeType)Enum.Parse(typeof(NodeType), t);
            }
            if (props.ContainsKey("x"))
            {
                double.TryParse(props.GetString("x"), out x);
            }
            if (props.ContainsKey("y"))
            {
                double.TryParse(props.GetString("y"), out y);
            }
            
            await UITask.Run(async () => { await SessionController.Instance.CreateNewNode(props.GetString("id"), type); });

        }



        private async Task HandleCreateNewGroup(string id, Message props)
        {
            NodeModel node1 = null;
            NodeModel node2 = null;
            double x = 0;
            double y = 0;
            if (props.ContainsKey("id1") && props.ContainsKey("id2") && SessionController.Instance.IdToSendables.ContainsKey(props.GetString("id1")) && SessionController.Instance.IdToSendables.ContainsKey(props.GetString("id2")))
            {
                node1 = (NodeModel)SessionController.Instance.IdToSendables[props.GetString("id1")];
                node2 = (NodeModel)SessionController.Instance.IdToSendables[props.GetString("id2")];
            }
            if (props.ContainsKey("x"))
            {
                double.TryParse(props.GetString("x"), out x);
            }
            if (props.ContainsKey("y"))
            {
                double.TryParse(props.GetString("y"), out y);
            }
            await UITask.Run(async () => { await SessionController.Instance.CreateGroup(id, node1, node2, x, y); });
        }

        private async Task HandleCreateNewGroupTag(string id, Message props)
        {
            double x = 0;
            double y = 0;
            double w = 0;
            double h = 0;
            string title = string.Empty;
            if (props.ContainsKey("x"))
            {
                double.TryParse(props.GetString("x"), out x);
            }
            if (props.ContainsKey("y"))
            {
                double.TryParse(props.GetString("y"), out y);
            }
            if (props.ContainsKey("width"))
            {
                double.TryParse(props.GetString("width"), out w);
            }
            if (props.ContainsKey("height"))
            {
                double.TryParse(props.GetString("height"), out h);
            }
            if (props.ContainsKey("title"))
            {
                title = props.GetString("title");
            }
            await UITask.Run(async () => { await SessionController.Instance.CreateGroupTag(id, x, y, w, h, title); });
        }
    }
}
