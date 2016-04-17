using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input.Inking;

namespace NuSysApp
{
    public class InkWrapper
    {
        public  InkWrapper(InkStroke stroke, string type, Color color = new Color())
        {
            Stroke = stroke;
            Type = type;
            Color = color;
        }
        public InkStroke Stroke { get; set; }
        public string Type { get; set; }
        public Color Color { get; set; }

        public bool Equals(InkWrapper wrapper)
        {
            return wrapper.Color == Color && wrapper.Stroke == Stroke && wrapper.Type == Type;
        }
    }
}
