using System;
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
        public const string FolderSave = "Save";
        public const string FolderThumbs = "Thumbs";
        public const string FolderChromeTransferName = "ChromeTransfer";
        public const string FolderWordTransferName = "WordTransfer";
        public const string FolderPowerpointTransferName = "PowerPointTransfer";
        public const string FolderMediaName = "Media";
        public const string FileChromeTransferName = "selections.nusys";
        public const string FolderOfficeToPdf = "OfficeToPdf";
        public const string FolderOpenDocParams = "OpenDocParams";
        public const string NuSysFALFiles = "NuSysFALFiles.txt";
        public const string NuSysFALFolders = "NuSysFALFolders.txt";
        public const string FirstTimeWord = "FirstTimeWord.txt";
        public const string FirstTimePowerpoint = "FirstTimePowerpoint.txt";
        #endregion Folders and files

        #region Node Dimensions
        public const double DefaultNodeSize = 200;
        public const double DefaultAnnotationSize = 100;
        public const double ExtraPaddingSpace = 50;
        public const double MinNodeSize = 40;
        public const double MinNodeSizeX = 40;
        public const double MinNodeSizeY = 40;

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
        /*
        public static Color color1 = Color.FromArgb(255, 230, 237, 236);   // lightest blue
        public static Color color2 = Color.FromArgb(255, 156, 197, 194);   // medium blue
        public static Color color3 = Color.FromArgb(255, 131, 166, 163);   // darkest blue
        public static Color color4 = Color.FromArgb(255, 197, 158, 156);   // red
        public static Color color5 = Color.FromArgb(255, 143, 152, 161);   // dark gray blue
        public static Color color6 = Colors.White;                         // white
        public static Color foreground6 = Colors.Black;                    // foreground white
        public static Color color7 = Colors.LightGray;                     // light gray
        public static Color color8 = Colors.DarkGray;                      // dark gray
        public static Color salmonColor = Colors.DarkSalmon;                    // dark salmon
        */

        public static Color color1 = Color.FromArgb(255, 199, 222, 222);   // lightest blue
        public static Color color2 = Color.FromArgb(255, 107, 147, 151);   // medium blue
        public static Color color3 = Color.FromArgb(255, 17, 61, 64);      // darkest blue
        public static Color color4 = Color.FromArgb(255, 152, 26, 77);     // red
        public static Color color5 = Color.FromArgb(255, 230, 230, 230);   // dv bg blue
        public static Color color6 = Colors.White;                         // white
        public static Color foreground6 = Colors.White;                    // foreground white
        public static Color darktext = Colors.DarkSlateGray;               // foreground dark slate gray
        public static Color color7 = Color.FromArgb(255, 230, 230, 230);   // light gray
        public static Color color8 = Colors.DarkGray;                      // dark gray
        public static Color salmonColor = Colors.DarkSalmon;               // dark salmon

        #endregion Color

        #region AtomType
        public const string Node = "Node";
        public const string Link = "Link";
        #endregion AtomType

        #region Network

        public const string CommaReplacement = "$*;;$&$";
        public const string AndReplacement = "$m&^@gfdsgs$";
        #endregion Network

        #region Timeline
        public const int TLNodeWidth = 174;
        public const int ExpandOn = 400;
        #endregion Timeline

        public const int InitialPenSize = 4;

        public const int MaxCanvasSize = 100000;

        public const double ButtonActivatedOpacity = 1.0;
        public const double ButtonDeactivatedOpacity = 0.5;

        public static IEnumerable<string> ImageFileTypes = new List<string> { ".bmp", ".png", ".jpeg", ".jpg", ".tif", ".tiff" };
        public static IEnumerable<string> WordFileTypes = new List<string> { };
        public static IEnumerable<string> PowerpointFileTypes = new List<string> { };

        public static IEnumerable<string> PdfFileTypes   = new List<string> { ".pdf" };
        public static IEnumerable<string> VideoFileTypes   = new List<string> { ".mp4"};//TODO add more types
        public static IEnumerable<string> AudioFileTypes = new List<string> { ".mp3" };
        public static IEnumerable<string> AllFileTypes   = VideoFileTypes.Concat(ImageFileTypes.Concat(PdfFileTypes).Concat(AudioFileTypes).Concat(WordFileTypes).Concat(PowerpointFileTypes));

        #region Cortana
        //public static IEnumerable<string> SpeechCommands = new List<string> { "open document", "create text", "create ink" };
        #endregion

        #region Other
        public const string NuSysWorkspaceToken = "NuSysWorkspaceToken";
        #endregion Other

        #region StaticMethods

        public static bool IsNode(ElementType type)
        {
            if (type == ElementType.Tag || type == ElementType.Area ||
                type == ElementType.Link)
            {
                return false;
            }
            return true;
        }
        public static Uri GetServerURI(string url)
        {
            return new Uri("http://" + WaitingRoomView.ServerName + "/" + url);
        }
        #endregion StaticMethods
    }
}
