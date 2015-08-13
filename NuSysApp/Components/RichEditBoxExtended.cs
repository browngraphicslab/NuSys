using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NuSysApp;

namespace NuSysApp
{
    public class RichEditBoxExtended : RichEditBox
    {
        public static readonly DependencyProperty RtfTextProperty = DependencyProperty.RegisterAttached("RtfText", typeof(string),
            typeof(RichEditBoxExtended), new PropertyMetadata(null, RtfTextPropertyChanged));

        private bool _changingText;

        public RichEditBoxExtended()
        {
            TextChanged += RichEditBoxExtended_TextChanged;
        }

        public string RtfText
        {
            get { return (string)GetValue(RtfTextProperty); }
            set { SetValue(RtfTextProperty, value); }
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
                    RtfText = "";
                }
                else
                {
                    Document.GetText(TextGetOptions.FormatRtf, out text);
                    RtfText = text;
                }
                _changingText = false;
            }
        }

        private static void RtfTextPropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var rtb = dependencyObject as RichEditBoxExtended;
            if (rtb == null) return;
            if (!rtb._changingText)
            {
                rtb._changingText = true;
                rtb.Document.SetText(TextSetOptions.FormatRtf, rtb.RtfText);
                rtb._changingText = false;
            }
        }

    } 

    
}
