using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using NusysIntermediate;

namespace NuSysApp
{
    public static class ColorExtensions
    { 
        public static ColorModel ToColorModel(this Color color)
        {
            return new ColorModel {A = color.A, B = color.B, G = color.G, R = color.R};
        }

        public static Color ToColor(this ColorModel colorModel)
        {
            return Color.FromArgb((byte)colorModel.A, (byte)colorModel.R, (byte)colorModel.G, (byte)colorModel.B);
        }

    }
}
