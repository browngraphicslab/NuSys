using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{

    public class WorkspaceModel : NodeContainerModel
    {   
        public double Zoom { get; set; }
        public double LocationX { get; set; }
        public double LocationY { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }

        public WorkspaceModel(string id ) : base(id)
        {
            NodeType = NodeType.Workspace;
            Zoom = 1;
            LocationX = -Constants.MaxCanvasSize;
            LocationY= -Constants.MaxCanvasSize;
            CenterX = -Constants.MaxCanvasSize;
            CenterY = -Constants.MaxCanvasSize;
        }

        public async override Task<Dictionary<string, string>> Pack()
        {
            var pack = await base.Pack();
            pack.Add("zoom", Zoom.ToString());
            pack.Add("locationX", LocationX.ToString());
            pack.Add("locationY", LocationY.ToString());
            pack.Add("centerX", CenterX.ToString());
            pack.Add("centerY", CenterY.ToString());
            return pack;
        }

        public async override Task UnPack(Message props)
        {
            Zoom =  props.GetDouble("zoom", 1);
            LocationX = props.GetDouble("locationX", -100000);
            LocationY = props.GetDouble("locationY", -100000);
            CenterX = props.GetDouble("centerX", -100000);
            CenterY = props.GetDouble("centerY", -100000);
            await base.UnPack(props);
        }

    }
}
