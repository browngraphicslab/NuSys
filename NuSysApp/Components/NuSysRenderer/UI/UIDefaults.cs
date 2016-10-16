using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public static class UIDefaults
    {
        public static  float Width = 100;
        public static float Height = 100;
        public static float Borderwidth = 5;
        public static Color Background = Colors.White;
        public static Color Bordercolor = Colors.Black;
        public static float TopBarHeight = 25;
        public static Color TopBarColor = Colors.Blue;
        public static float ErrorMargin = 15;
        public static string FontFamily = "/Assets/fonts/freightsans.ttf#FreightSans BookSC";

        public static CanvasHorizontalAlignment TextHorizontalAlignment = CanvasHorizontalAlignment.Left;

        public static CanvasVerticalAlignment TextVerticalAlignment  = CanvasVerticalAlignment.Center;

        public static Color TextColor = Colors.Black;

        public static FontStyle FontStyle = FontStyle.Normal;

        public static float FontSize = 14;
        public static CanvasWordWrapping Wrapping = CanvasWordWrapping.Wrap;

        public static CanvasTrimmingSign TrimmingSign = CanvasTrimmingSign.Ellipsis;

        public static CanvasTextTrimmingGranularity TrimmingGranularity = CanvasTextTrimmingGranularity.Character;
        public static float TabHeight = 25;

        public static Color TabColor = Colors.LightGray;
        public static float TabMaxWidth = 100;
        public static bool TabIsCloseable = true;
    }
}
