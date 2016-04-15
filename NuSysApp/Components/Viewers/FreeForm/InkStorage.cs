using Microsoft.Graphics.Canvas.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input.Inking;

namespace NuSysApp
{
    public class InkStorage
    {

        public static BiDictionary<string, Tuple<string, InkStroke>> _inkStrokes = new BiDictionary<string, Tuple<string, InkStroke>>();

        public static AddInkRequest CreateAddInkRequest(string id, InkStroke stroke, string type, Color color)
        {
            var data = new Dictionary<string, string>();
            data["inkpoints"] = JsonConvert.SerializeObject(stroke.GetInkPoints());
            data["type"] = type;
            data["color"] = JsonConvert.SerializeObject(new int[3] { color.R, color.G, color.B });

            var msg = new Message();
            msg["id"] = id;
            msg["data"] = data;
            return new AddInkRequest(msg);
        }

        public static RemoveInkRequest CreateRemoveInkRequest(Tuple<string, InkStroke> stroke)
        {
            if (!InkStorage._inkStrokes.ContainsValue(stroke))
                return null;

            var id = InkStorage._inkStrokes.GetKeyByValue(stroke);
            var msg = new Message();
            msg[id] = id;
            return new RemoveInkRequest(msg);
        }

        
    }
}
