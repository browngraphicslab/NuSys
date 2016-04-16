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
            data["inkpoints"] = JsonConvert.SerializeObject(stroke.GetInkPoints().ToArray());
            data["type"] = type;
            data["id"] = id;
            data["color"] = JsonConvert.SerializeObject(new int[3] { color.R, color.G, color.B });

            var msg = new Message();
            msg["id"] = id;
            msg["data"] = data;
            return new AddInkRequest(msg);
        }

        public static Tuple<RemoveInkRequest,string> CreateRemoveInkRequest(Tuple<string, InkStroke> stroke)
        {
            var foundId = string.Empty;
            foreach (var inkStroke in InkStorage._inkStrokes)
            {
                if (inkStroke.Value.Item2 == stroke.Item2)
                {
                    foundId = inkStroke.Key;
                }
            }

            if (foundId == string.Empty)
                return null;

            var msg = new Message();
            msg["id"] = foundId;
            return new Tuple<RemoveInkRequest, string>(new RemoveInkRequest(msg),foundId);
        }

        
    }
}
