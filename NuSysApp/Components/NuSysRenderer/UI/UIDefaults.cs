﻿using System;
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
        public static float Borderwidth = 0;
        public static Color Background = Colors.White;
        public static Color Bordercolor = Colors.Black;
        public static float TopBarHeight = 25;
        public static Color TopBarColor = Colors.Blue;
        public static float ErrorMargin = 15;
        public static string FontFamily = "Arial";//"/Assets/fonts/freightsans.ttf#FreightSans BookSC";

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
        public static bool WindowIsResizeable = true;
        public static bool WindowKeepsAspectRatio = true;
        public static bool WindowIsDraggable = true;
        public static float? WindowMaxWidth = 5000;
        public static float? WindowMaxHeight = 5000;
        public static float? WindowMinWidth = 25;
        public static float? WindowMinHeight = 25;
        public static float WindowBorderWidth = 3;
        public static CanvasHorizontalAlignment ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center;
        public static CanvasVerticalAlignment ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
        public static Color ThumbColor = Colors.LightGray;
        public static Color SliderHighlightColor = Colors.DodgerBlue;
        public static Color SliderBackground = Colors.LightGray;
        public static float SliderPosition = 1;
        public static bool IsSliderTooltipEnabled = true;
        public static Color ScrubBarHighlightColor = Colors.IndianRed;
        public static Color ScrubBarBackgroundColor = Colors.LightGray;
        public static float ScrubberPosition = 0;
        public static Color ScrubberBarColor = Colors.Yellow;
        public static float MediaPlayerScrubBarHeight = 50;
        public static float MediaPlayerButtonBarHeight = 30;
        public static Color ShadowColor = Color.FromArgb(50, 0, 0, 0);
        public static int XTextPadding = 10;
        public static int YTextPadding = 5;
        public static Color AudioRegionColor = Color.FromArgb(125, 72, 182, 111);
        public static float AudioResizerHandleDiameter = 15;
        public static Color AudioResizerHandleColor = Colors.SlateGray;
        public static float AudioResizerConnectingLineWidth = 5;
        public static Color AudioRegionMaskColor = Color.FromArgb(160, 0, 0, 0);
        public static float ButtonTextSize = 14;
        public static float floatingMenuHeight = 60;
        public static float floatingMenuWidth = 130;

        public static float CheckBoxUIElementHeight = 25;
        public static float CheckBoxUIElementWidth = 125;
        public static CheckBoxUIElement.CheckBoxLabelPosition CheckBoxLabelPosition = CheckBoxUIElement.CheckBoxLabelPosition.Right;
        public static CanvasHorizontalAlignment CheckBoxLabelTextHorizontalAlignmentAlignment = CanvasHorizontalAlignment.Left;
        public static float CheckBoxHeight = 15;
        public static float CheckBoxWidth = 15;
        public static float TabSpacing = 0;
        public static float TabBarHeight = 50;
        public static Color TabBarBackground = Colors.White;
        public static float TabBarBorderWidth = 0;
        public static HorizontalAlignment TabHorizontalAlignment = HorizontalAlignment.Left;
        public static VerticalAlignment TabVerticalAlignment = VerticalAlignment.Center;
        public static CanvasHorizontalAlignment TabTextAlignment = CanvasHorizontalAlignment.Left;
    }
}
