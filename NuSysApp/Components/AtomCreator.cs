﻿using System;
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
            if (props.ContainsKey("type") && props["type"] == "ink")
            {
                await HandleCreateNewInk(id, props);
            }
            else if (props.ContainsKey("type") && props["type"] == "group")
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
                    x = double.Parse(props["x"]);
                    y = double.Parse(props["y"]);
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
                id1 = props["id1"];
            }
            else
            {
                Debug.WriteLine("Could not create link");
                return;
            }
            if (props.ContainsKey("id2"))
            {
                id2 = props["id2"];
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
                string t = props["nodeType"];
                type = (NodeType)Enum.Parse(typeof(NodeType), t);
            }
            if (props.ContainsKey("x"))
            {
                double.TryParse(props["x"], out x);
            }
            if (props.ContainsKey("y"))
            {
                double.TryParse(props["y"], out y);
            }
            
            await UITask.Run(async () => { await SessionController.Instance.CreateNewNode(props["id"], type, x, y); });

        }



        private async Task HandleCreateNewGroup(string id, Message props)
        {
            NodeModel node1 = null;
            NodeModel node2 = null;
            double x = 0;
            double y = 0;
            if (props.ContainsKey("id1") && props.ContainsKey("id2") && SessionController.Instance.IdToSendables.ContainsKey(props["id1"]) && SessionController.Instance.IdToSendables.ContainsKey(props["id2"]))
            {
                node1 = (NodeModel)SessionController.Instance.IdToSendables[props["id1"]];
                node2 = (NodeModel)SessionController.Instance.IdToSendables[props["id2"]];
            }
            if (props.ContainsKey("x"))
            {
                double.TryParse(props["x"], out x);
            }
            if (props.ContainsKey("y"))
            {
                double.TryParse(props["y"], out y);
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
                double.TryParse(props["x"], out x);
            }
            if (props.ContainsKey("y"))
            {
                double.TryParse(props["y"], out y);
            }
            if (props.ContainsKey("width"))
            {
                double.TryParse(props["width"], out w);
            }
            if (props.ContainsKey("height"))
            {
                double.TryParse(props["height"], out h);
            }
            if (props.ContainsKey("title"))
            {
                title = props["title"];
            }
            await UITask.Run(async () => { await SessionController.Instance.CreateGroupTag(id, x, y, w, h, title); });
        }

        private async Task HandleCreateNewInk(string id, Message props)
        {

            if (props.ContainsKey("canvasNodeID") && props["canvasNodeID"] == SessionController.Instance.ActiveWorkspace.Id)
            {
                InqCanvasModel canvas = null;
                if (props["canvasNodeID"] != SessionController.Instance.ActiveWorkspace.Id)
                {
                    await UITask.Run(async delegate { canvas = ((NodeModel)SessionController.Instance.IdToSendables[props["canvasNodeID"]]).InqCanvas; });
                }
                else
                {
                    canvas = (SessionController.Instance.ActiveWorkspace.Model as WorkspaceModel).InqCanvas;
                }
                if (props.ContainsKey("inkType") && props["inkType"] == "partial")
                {
                    Point2d one;
                    Point2d two;
                    ParseToLineSegment(props, out one, out two);

                    await UITask.Run(() =>
                    {
                        var lineModel = new InqLineModel(props["canvasNodeID"]);
                      //  var line = new InqLineView(new InqLineViewModel(lineModel), 2, new SolidColorBrush(Colors.Black));
                        var pc = new ObservableCollection<Point2d>();
                        pc.Add(one);
                        pc.Add(two);
                        lineModel.Points = pc;
                        lineModel.Stroke = new SolidColorBrush(Colors.Black);
                        if (props.ContainsKey("stroke") && props["stroke"] != "black")
                        {
                            lineModel.Stroke = new SolidColorBrush(Colors.Yellow);
                        }
                        canvas.AddTemporaryInqline(lineModel, id);
                    });
                }
                else if (props.ContainsKey("inkType") && props["inkType"] == "full")
                {
                    await UITask.Run(async delegate {

                        ObservableCollection<Point2d> points;
                        double thickness;
                        SolidColorBrush stroke;

                        if (props.ContainsKey("data"))
                        {
                            InqLineModel.ParseToLineData(props["data"], out points, out thickness, out stroke);
                            thickness = 2;
                            if (props.ContainsKey("previousID") && (SessionController.Instance.ActiveWorkspace.Model as WorkspaceModel).InqCanvas.PartialLines.ContainsKey(props["previousID"]))
                            {
                                canvas.LineFinalized += async delegate
                                {
                                    await UITask.Run(() => { canvas.RemovePartialLines(props["previousID"]); });
                                };
                            }

                            var lineModel = new InqLineModel(id);
                            if (props.ContainsKey("canvasNodeID"))
                            {
                                lineModel.InqCanvasId = props["canvasNodeID"];
                            }
                          //  var line = new InqLineView(new InqLineViewModel(lineModel), thickness, stroke);
                            lineModel.Points = points;
                            lineModel.Stroke = stroke;
                            canvas.FinalizeLine(lineModel);
                            try
                            {
                                if (!SessionController.Instance.IdToSendables.ContainsKey(id))
                                {
                                    SessionController.Instance.IdToSendables.Add(id, lineModel);
                                }
                                else
                                {
                                    SessionController.Instance.IdToSendables.Remove(id);
                                    SessionController.Instance.IdToSendables.Add(id, lineModel);
                                }

                            }
                            catch (System.ArgumentException argument)
                            {
                                //Debug.Write(argument.StackTrace);
                            }

                        }
                    });
                }
            }
            else
            {
                Debug.WriteLine("Ink creation failed because no canvas ID was given or the ID wasn't valid");
            }
        }

        private byte[] ParseToByteArray(string s)
        {
            return Convert.FromBase64String(s);
        }

        private void ParseToLineSegment(Message props, out Point2d one, out Point2d two)
        {
            one = new Point2d(Double.Parse(props["x1"]), Double.Parse(props["y1"]));
            two = new Point2d(Double.Parse(props["x2"]), Double.Parse(props["y2"]));
        }

    }
}
