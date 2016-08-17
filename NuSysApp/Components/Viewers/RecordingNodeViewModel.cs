using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NusysIntermediate;
using NuSysApp.Controller;
using NuSysApp.Util;

namespace NuSysApp
{
    public class RecordingNodeViewModel : BaseINPC
    {
        // private variables

        // the position of the recording node on the workspace
        private double _x;
        private double _y;

        // the size of the recording node
        private double _width;
        private double _height;

        // public variables

        /// <summary>
        /// The x coordinate of the recording node on the workspace
        /// </summary>
        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                RaisePropertyChanged("X");
            }
        }

        /// <summary>
        /// The y coordinate of the recording node on the workspace
        /// </summary>
        public double Y
        {
            get { return _y; }
            set
            {
                _y = value;
                RaisePropertyChanged("Y");
            }
        }

        /// <summary>
        /// The width of the recording node
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        /// <summary>
        /// The height of the recording node
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        /// <summary>
        /// Takes in two parameters, the x and y coordinate of the recording node view on the workspace
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public RecordingNodeViewModel(double x, double y)
        {
            // set the width height and position correctly
            X = x;
            Y = y;
            Width = Constants.DefaultVideoNodeSize;
            Height = Constants.DefaultVideoNodeSize;
        }

        
    }
}