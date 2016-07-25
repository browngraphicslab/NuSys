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

        public static Color color1 = Color.FromArgb(255, 199, 222, 222); // lightest blue
        public static Color color2 = Color.FromArgb(255, 107, 147, 151); // medium blue
        public static Color color3 = Color.FromArgb(255, 17, 61, 64); // darkest blue
        public static Color color4 = Color.FromArgb(255, 152, 26, 77); // red 
        public static Color color4light = Color.FromArgb(255, 219, 151, 179);//light red
        public static Color color5 = Color.FromArgb(255, 230, 230, 230); // dv bg blue
        public static Color color6 = Colors.White; // white
        public static Color foreground6 = Colors.White; // foreground white
        public static Color darktext = Colors.DarkSlateGray; // foreground dark slate gray
        public static Color color7 = Color.FromArgb(255, 230, 230, 230); // light gray
        public static Color color8 = Colors.DarkGray; // dark gray
        public static Color salmonColor = Colors.DarkSalmon; // dark salmon
        public static Color linkColor1 = Color.FromArgb(255, 255, 222, 222);
        public static Color linkColor2 = Color.FromArgb(255, 255, 163, 163);
        public static Color linkColor3 = Color.FromArgb(255, 220, 163, 197);
        public static Color linkColor4 = Color.FromArgb(255, 205, 163, 163);
        public static Color linkColor5 = Color.FromArgb(254, 254, 241, 223);
        public static Color linkColor6 = Color.FromArgb(255, 254, 226, 184);
        public static Color linkColor7 = Color.FromArgb(255, 254, 212, 184);
        public static Color linkColor8 = Color.FromArgb(255, 238, 186, 163);
        public static Color linkColor9 = Color.FromArgb(255, 237, 249, 175);
        public static Color linkColor10 = Color.FromArgb(255, 254, 249, 175);
        public static Color linkColor11 = Color.FromArgb(255, 237, 239, 175);
        public static Color linkColor12 = Color.FromArgb(255, 237, 219, 175);
        public static Color linkColor13 = Color.FromArgb(255, 237, 248, 226);
        public static Color linkColor14 = Color.FromArgb(255, 214, 234, 186);
        public static Color linkColor15 = Color.FromArgb(255, 184, 228, 190);
        public static Color linkColor16 = Color.FromArgb(255, 166, 195, 176);
        public static Color linkColor17 = Color.FromArgb(255, 220, 237, 245);
        public static Color linkColor18 = Color.FromArgb(255, 178, 225, 245);
        public static Color linkColor19 = Color.FromArgb(255, 169, 210, 255);
        public static Color linkColor20 = Color.FromArgb(255, 163, 187, 253);
        public static Color linkColor21 = Color.FromArgb(255, 237, 219, 251);
        public static Color linkColor22 = Color.FromArgb(255, 200, 179, 215);
        public static Color linkColor23 = Color.FromArgb(255, 180, 181, 216);
        public static Color linkColor24 = Color.FromArgb(255, 172, 163, 216);
        public static List<Color> linkColors = new List<Color>()
        {
            linkColor1, linkColor2, linkColor3, linkColor4, linkColor5, linkColor7,
            linkColor8, linkColor9, linkColor10, linkColor11, linkColor12, linkColor13, linkColor14, linkColor15, linkColor16,
            linkColor17, linkColor19, linkColor20, linkColor21, linkColor22, linkColor23, linkColor24
        };

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
        public static IEnumerable<string> WordFileTypes = new List<string> { ".docx" };
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

        #region InitialTransform
        public static readonly double InitialCenter = 50000;
        public static readonly double InitialTranslate = -50000;
        public static readonly double InitialScale = .85;
        #endregion InitialTransform

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

        public static long GetTimestampTicksOfLibraryElementModel(LibraryItemTemplate template)
        {
            if (template == null)
            {
                return 0;
            }
            return
                GetTimestampTicksOfLibraryElementModel(
                    SessionController.Instance.ContentController.GetContent(template.ContentID));
        }
        public static long GetTimestampTicksOfLibraryElementModel(LibraryElementModel model)
        {
            if (!String.IsNullOrEmpty(model.Timestamp))
            {
                try
                {
                    return DateTime.Parse(model.Timestamp).Ticks;
                }
                catch (Exception e)
                {
                    return 0;
                }
            }

            return 0;
        }

        public static bool IsRegionType(ElementType type)
        {
            return type == ElementType.AudioRegion || type == ElementType.ImageRegion || type == ElementType.VideoRegion ||
                   type == ElementType.PdfRegion;
        }
        #endregion StaticMethods
    }
}
