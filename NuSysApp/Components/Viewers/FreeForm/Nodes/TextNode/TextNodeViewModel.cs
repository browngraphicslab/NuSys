using System.Collections.Generic;
using System.IO;
using System;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class TextNodeViewModel : ElementInstanceViewModel
    {
        #region Private Members
        private string _rtf = string.Empty;

        #endregion Private Members
        public TextNodeViewModel(ElementInstanceController controller) : base(controller)
        {           
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 164, 220, 255));
        }
 
        #region Public Properties     

        public string RtfText
        {
            get
            {
                return _rtf;
            }

            set
            {
                _rtf = value;
                RaisePropertyChanged("RtfText");
            }

        }

        #endregion Public Properties
    }
}