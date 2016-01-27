using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NuSysApp;
using System.Diagnostics;
using Windows.UI.Xaml.Input;
using Windows.Devices.Input;
using Windows.Foundation;

namespace NuSysApp
{
    public class TextBoxExtended : RichEditBox
    {

        //public static readonly DependencyProperty RtfProperty = DependencyProperty.RegisterAttached("Rtf", typeof(string), typeof(TextBoxExtended), new PropertyMetadata(null, RtfTextPropertyChanged));


        public TextBoxExtended()
        {

        }

        public double ComputeRtfHeight()
        {
            string str;
            Document.GetText(TextGetOptions.None, out str);
            Document.Selection.SetRange(0, str.Length);

            Rect rect;
            int hit;
            Document.Selection.GetRect(PointOptions.ClientCoordinates, out rect, out hit);
            return rect.Height;
        }

        public string GetRtfText()
        {
            string str;
            Document.GetText(TextGetOptions.FormatRtf, out str);
            return str;
        }

        public string GetNonRtfText()
        {
            string str;
            Document.GetText(TextGetOptions.None, out str);
            return str;
        }

        public void SetRtfText(string text)
        {
            Document.SetText(TextSetOptions.FormatRtf, text);
            Document.ApplyDisplayUpdates();
        }

       
    }     
}
