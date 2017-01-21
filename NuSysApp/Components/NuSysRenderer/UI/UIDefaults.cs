using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.StartScreen;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public static class UIDefaults
    {

        #region size and color

        public static float Width = 100;
        public static float Height = 100;
        public static float Borderwidth = 0;
        public static Color Background = Constants.LIGHT_BLUE;
        public static Color Bordercolor = Colors.Transparent;
        public static Color SelectedBorderColor = Constants.DARK_BLUE;
        public static float TopBarHeight = 25;
        public static Color TopBarColor = Colors.Transparent;
        public static float ErrorMargin = 15;

        public static float Padding = 2;
        public static int PrimaryStyle = 0;
        public static int SecondaryStyle = 1;
        public static int AccentStyle = 2;
        public static int DraggableStyle = 3;
        public static int Bubble = 3;

        #endregion

        #region font and text

        public static string TitleFont = "ms-appx:///Assets/fonts/Rubik-Regular.ttf#Rubik"; /*"/Assets/fonts/freightsans.ttf#FreightSans BookSC"*/
        public static string TextFont = "Arial";
        public static CanvasHorizontalAlignment TextHorizontalAlignment = CanvasHorizontalAlignment.Left;
        public static CanvasVerticalAlignment TextVerticalAlignment = CanvasVerticalAlignment.Center;
        public static Color TextColor = Constants.ALMOST_BLACK;
        public static FontStyle FontStyle = FontStyle.Normal;
        public static FontWeight FontWeight = FontWeights.Normal;
        public static float FontSize = 14;
        public static CanvasWordWrapping Wrapping = CanvasWordWrapping.EmergencyBreak;
        public static CanvasTrimmingSign TrimmingSign = CanvasTrimmingSign.Ellipsis;
        public static CanvasTextTrimmingGranularity TrimmingGranularity = CanvasTextTrimmingGranularity.Character;
        public static int XTextPadding = 10;
        public static int YTextPadding = 5;

        #endregion

        #region tabs

        public static float TabHeight = 15;
        public static Color TabColor = Constants.LIGHT_BLUE;
        public static float TabMaxWidth = 100;
        public static bool TabIsCloseable = true;
        public static float TabSpacing = 0;
        public static float TabBarHeight = 50;
        public static Color TitleColor = Constants.DARK_BLUE;
        public static Color TabBarBackground = Constants.LIGHT_BLUE;
        public static float TabBarBorderWidth = 0;
        public static HorizontalAlignment TabHorizontalAlignment = HorizontalAlignment.Left;
        public static VerticalAlignment TabVerticalAlignment = VerticalAlignment.Center;
        public static CanvasHorizontalAlignment TabTextAlignment = CanvasHorizontalAlignment.Left;

        #endregion

        #region window

        public static bool WindowIsResizeable = true;
        public static bool WindowKeepsAspectRatio = true;
        public static bool WindowIsDraggable = true;
        public static bool WindowIsSnappable = false;
        public static float? WindowMaxWidth = 5000;
        public static float? WindowMaxHeight = 5000;
        public static float? WindowMinWidth = 25;
        public static float? WindowMinHeight = 25;
        public static float WindowBorderWidth = 3;
        public static Color ResizeHighlightColor = Constants.DARK_BLUE;
        public static float WindowDragBuffer = 25;
        public static float SnapBuffer = 30;
        public static Color SnapPreviewRectColor = Color.FromArgb(150, 208, 207, 184);


        #endregion

        #region button

        public static CanvasHorizontalAlignment ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center;
        public static CanvasVerticalAlignment ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
        public static float ButtonTextSize = 14;

        #endregion

        #region slider

        public static Color ThumbColor = Constants.DARK_BLUE;
        public static Color SliderHighlightColor = Constants.MED_BLUE;
        public static Color SliderBackground = Colors.White;
        public static float SliderPosition = 1;
        public static bool IsSliderTooltipEnabled = true;

        #endregion

        #region scrub bar

        public static Color ScrubBarHighlightColor = Constants.RED_TRANSLUCENT;
        public static Color ScrubBarBackgroundColor = Constants.MED_BLUE;
        public static float ScrubberPosition = 0;
        public static Color ScrubberBarColor = Constants.RED;

        #endregion

        #region media player

        public static float MediaPlayerScrubBarHeight = 30;
        public static float MediaPlayerButtonBarHeight = 30;
        public static Color ShadowColor = Color.FromArgb(50, 0, 0, 0);

        #endregion

        #region audio region

        public static Color AudioRegionColor = Color.FromArgb(125, 72, 182, 111);
        public static float AudioResizerHandleDiameter = 15;
        public static Color AudioResizerHandleColor = Colors.SlateGray;
        public static float AudioResizerConnectingLineWidth = 5;
        public static Color AudioRegionMaskColor = Color.FromArgb(160, 0, 0, 0);

        #endregion

        #region floating menu

        public static float floatingMenuHeight = 60;
        public static float floatingMenuWidth = 130;

        #endregion

        #region check box

        public static float CheckBoxUIElementHeight = 25;
        public static float CheckBoxUIElementWidth = 125;
        public static CheckBoxUIElement.CheckBoxLabelPosition CheckBoxLabelPosition = CheckBoxUIElement.CheckBoxLabelPosition.Right;
        public static CanvasHorizontalAlignment CheckBoxLabelTextHorizontalAlignmentAlignment = CanvasHorizontalAlignment.Left;
        public static float CheckBoxHeight = 15;
        public static float CheckBoxWidth = 15;


        #endregion

        public static int CornerRadius = 5;

        public static Color PlaceHolderTextColor = Constants.MED_BLUE;
        public static float MaxDropDownHeight = 250;
        public static Color HighlightColor = Constants.RED_TRANSLUCENT;
        public static float ScrollBarWidth = 15;
        public static Color ScrollButtonColor = Colors.LightGray;
        public static Color ScrollHandleBackground = Colors.LightGray;
        public static Color ScrollBarBackground = Colors.DarkGray;
        public static BorderType BorderType = BorderType.Inside;


        public static readonly float ScrollableTextboxBorderWidth = 7f;
        public static readonly Color ScrollableTextboxOverlayColor = new Color() {A = 100, R = Colors.LightGray.R, G = Colors.LightGray.G , B = Colors.LightGray.B };

        #region library
        public static float SearchBarHeight = 30f;
        public static float FilterButtonWidth = 50f;
        #endregion library
        #region list

        public static float ListHeaderHeight = 40;
        #endregion list


        #region scrollbar
        public static float MinSliderSize = 15f;

        #endregion scrollbar

        #region chat
        public static float MaxChatHeight = 125f;

        public static float MinChatHeight = 50f;
        #endregion chat

    }
}
