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

namespace NuSysApp
{
    public class TextBoxExtended : RichEditBox
    {
        public static readonly DependencyProperty RtfProperty = DependencyProperty.RegisterAttached("Rtf", typeof(string),
            typeof(TextBoxExtended), new PropertyMetadata(null, RtfTextPropertyChanged));

        private bool _changingText;

        public TextBoxExtended()
        {
            //TextChanged += RichEditBoxExtended_TextChanged;
        }

        public string Rtf
        {
            get { return (string)GetValue(RtfProperty); }
            set {
                SetValue(RtfProperty, value);
            }
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

        private static void RtfTextPropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var rtb = dependencyObject as TextBoxExtended;
            rtb.Document.SetText(TextSetOptions.FormatRtf, rtb.Rtf);
            rtb._changingText = false;
        }
    }     
}
