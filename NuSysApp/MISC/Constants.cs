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
        public const string FolderNusysTemp = "NuSys";
        public const string FolderChromeTransferName = "ChromeTransfer";
        public const string FolderWordTransferName = "WordTransfer";
        public const string FolderPowerpointTransferName = "PowerPointTransfer";
        public const string FolderMediaName = "Media";
        public const string FileChromeTransferName = "selections.nusys";
        public const string FolderOfficeToPdf = "OfficeToPdf";
        #endregion Folders and files

        #region Node Dimensions
        public const double DefaultNodeSize = 200;
        public const double DefaultAnnotationSize = 100;
        public const double ExtraPaddingSpace = 50;
        public const double MinNodeSize = 40;
        public const double MinNodeSizeX = 150;
        public const double MinNodeSizeY = 110;

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
        public static Color DefaultColor = Color.FromArgb(20, 0, 76, 153);
        public static Color SelectedColor = Color.FromArgb(200, DefaultColor.R, DefaultColor.G, DefaultColor.B);
        #endregion Color

        #region AtomType
        public const string Node = "Node";
        public const string Link = "Link";
        #endregion AtomType

        #region Node Type
        public enum NodeType
        {
            text,
            image,
            ink,
            pdf,
            richText,
            group
        }
        #endregion Node Type

        public const int InitialPenSize = 4;

        public const int MaxCanvasSize = 100000;

        public const double ButtonActivatedOpacity = 1.0;
        public const double ButtonDeactivatedOpacity = 0.5;

        public static IEnumerable<string> ImageFileTypes = new List<string> { ".bmp", ".png", ".jpeg", ".jpg" };
        public static IEnumerable<string> PdfFileTypes   = new List<string> { ".pdf", ".pptx", ".docx" };
        public static IEnumerable<string> AllFileTypes   = ImageFileTypes.Concat(PdfFileTypes);

        #region Cortana
        //public static IEnumerable<string> SpeechCommands = new List<string> { "open document", "create text", "create ink" };
        #endregion
    }
}
