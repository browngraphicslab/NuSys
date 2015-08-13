using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace NuSysApp
{
    class RichTextBlockBinder : DependencyObject
    {
        public static string GetText(DependencyObject obj)
        {
            return (string) obj.GetValue((TextProperty));
        }

        public static void SetText(DependencyObject obj, string value)
        {
            obj.SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof (string), typeof (RichTextBlockBinder),
                new PropertyMetadata(null));

        private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as RichTextBlock;
            if (control != null)
            {
                control.Blocks.Clear();
                string value = e.NewValue.ToString();

                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run {Text = value});
                control.Blocks.Add(paragraph);
            }
        }
    }
}
