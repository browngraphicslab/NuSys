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

        public static readonly DependencyProperty RtfProperty = DependencyProperty.RegisterAttached("Rtf", typeof(string), typeof(TextBoxExtended), new PropertyMetadata(null, RtfTextPropertyChanged));

        private bool _changingText;

        public TextBoxExtended()
        {
            TextChanged += delegate
            {
                var s = ComputeContentSize();
            };
        }

        public Size ComputeContentSize()
        {
            string str;
            Document.GetText(TextGetOptions.FormatRtf, out str);
            Document.Selection.SetRange(0, str.Length);

            Rect rect;
            int hit;
            Document.Selection.GetRect(PointOptions.None, out rect, out hit);
            return new Size(rect.Width, rect.Height);
        }

        public string Rtf
        {
            get { return (string)GetValue(RtfProperty); }
            set {  SetValue(RtfProperty, value); }
        }

        private void RichEditBoxExtended_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!_changingText)
            {
                _changingText = true;
                string text;
                Document.GetText(TextGetOptions.None, out text);
                if (string.IsNullOrWhiteSpace(text))
                {
                    Rtf = "";
                }
                else
                {
                    Document.GetText(TextGetOptions.FormatRtf, out text);
                    Rtf = text;
                }
                _changingText = false;
            }
        }

        private static void RtfTextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var rtb = dependencyObject as TextBoxExtended;
            rtb.Document.SetText(TextSetOptions.FormatRtf, rtb.Rtf);
            rtb.Document.ApplyDisplayUpdates();
            rtb._changingText = false;
        }
    }     
}
