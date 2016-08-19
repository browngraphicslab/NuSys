using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class RelevanceLineView : UserControl
    {
        private ElementModel _node1;
        private ElementModel _node2;

        /// <summary>
        /// lines that visually show relevance from one node to its relevant nodes
        /// </summary>
        public RelevanceLineView(ElementModel node1, ElementModel node2, double relevance)
        {
            this.InitializeComponent();
            _node1 = node1;
            _node2 = node2;

            xLine.Opacity = CalculateOpacity(relevance);
            xLine.X1 = _node1.X;
            xLine.Y1 = _node1.Y;
            xLine.X2 = _node2.X;
            xLine.Y2 = _node2.Y;
        }

        /// <summary>
        /// if the relevance is in the form of a double - this will normalize it between 0 and 1 and set appropriate opacity
        /// </summary>
        /// <param name="relevance"></param>
        /// <returns></returns>
        private double CalculateOpacity(double relevance)
        {
            if (relevance > .25)
            {
                return 1;
            }
            return 0;
        }
    }
}
