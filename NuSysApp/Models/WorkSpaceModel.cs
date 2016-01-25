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
            LocationX = -Constants.MaxCanvasSize/2.0;
            LocationY= -Constants.MaxCanvasSize/ 2.0;
            CenterX = -Constants.MaxCanvasSize/ 2.0;
            CenterY = -Constants.MaxCanvasSize/ 2.0;
        }

        public async override Task<Dictionary<string, object>> Pack()
        {
            var pack = await base.Pack();
            pack.Add("zoom", Zoom);
            pack.Add("locationX", LocationX);
            pack.Add("locationY", LocationY);
            pack.Add("centerX", CenterX);
            pack.Add("centerY", CenterY);
          //  pack["type"] = AtomType.Workspace;
            return pack;
        }

        public async override Task UnPack(Message props)
        {
            Zoom =  props.GetDouble("zoom", 1);
            LocationX = props.GetDouble("locationX", -Constants.MaxCanvasSize/2);
            LocationY = props.GetDouble("locationY", -Constants.MaxCanvasSize/2);
            CenterX = props.GetDouble("centerX", -Constants.MaxCanvasSize/2);
            CenterY = props.GetDouble("centerY", -Constants.MaxCanvasSize/2);
            await base.UnPack(props);
        }

    }
}
