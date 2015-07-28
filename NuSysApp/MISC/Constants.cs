using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace NuSysApp
{
    /// <summary>
    /// This static class holds all important constants used in my application. This allows me to change all 
    /// these constants by modifiying only one reference in code.
    /// </summary>
    public class Constants
    {
        #region Folders and files
        public static string FolderNusysTemp = "NuSys";
        public static string FolderChromeTransferName = "ChromeTransfer";
        public static string FileChromeTransferName = "selections.nusys";
        #endregion Folders and files

        #region Node Dimensions
        public const double DefaultNodeSize = 200;
        public const double DefaultAnnotationSize = 100;
        public const double ExtraPaddingSpace = 50;
        public const double MinNodeSize = 40;

        public const double DefaultVideoNodeSize = 300;

        #endregion Node Dimensions

        #region Font 
        public const double DefaultFontSize = 20;
        public const string DefaultFont = "Verdana";
        #endregion

        #region  Ink Node
        public const double DefaultInkWidth = 0.5;
        public const double MaxZIndex = 10000;
        #endregion Ink Node

        #region Color
        public static Color DefaultColor = Color.FromArgb(100, 82, 171, 255);
        public static Color SelectedColor = Color.FromArgb(200, DefaultColor.R, DefaultColor.G, DefaultColor.B);
        #endregion Color

        //global ink

        public const int InitialPenSize = 4;

        public const int MaxCanvasSize = 100000;

        public static IEnumerable<string> ImageFileTypes = new List<string> { ".bmp", ".png", ".jpeg", ".jpg" };
        public static IEnumerable<string> PdfFileTypes   = new List<string> { ".pdf", ".pptx", ".docx" };
        public static IEnumerable<string> AllFileTypes   = ImageFileTypes.Concat(PdfFileTypes);
    }
}
