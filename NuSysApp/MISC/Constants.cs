
using Windows.Storage;
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
        public static string FOLDER_NUSYS_TEMP = "NuSys";
        public static string FOLDER_CHROME_TRANSFER_NAME = "ChromeTransfer";
        public static string FILE_CHROME_TRANSFER_NAME = "selections.nusys";
        #endregion Folders and files

        #region Node Dimensions
        public const double DEFAULT_NODE_SIZE = 200;
        public const double EXTRA_PADDING_SPACE = 50;
        public const double MIN_NODE_SIZE = 40;

        public const double DEFAULT_VIDEONODE_SIZE = 300;

        #endregion Node Dimensions

        #region Font 
        public const double DEFAULT_FONT_SIZE = 20;
        public const string DEFAULT_FONT = "Verdana";
        #endregion

        #region  Ink Node
        public const double DEFAULT_INK_WIDTH = 0.5;
        public const double MAX_Z_INDEX = 10000;
        #endregion Ink Node

        #region Color
        public static Color DEFAULT_COLOR = Color.FromArgb(100, 82, 171, 255);
        public static Color SELECTED_COLOR = Color.FromArgb(200, DEFAULT_COLOR.R, DEFAULT_COLOR.G, DEFAULT_COLOR.B);
        #endregion Color

        //global ink

        public const int INITIAL_PEN_SIZE = 4;
        
    }
}
