using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using SharpDX.Direct2D1;

namespace NuSysApp.Components.Viewers.Detail.Views
{

    /// <summary>
    /// Wraps a metadata entry, which is defined by a key, value, 
    /// and mutability. If you can edit the key or value, then the entr is mutable. 
    /// </summary>
    public class MetadataEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public Visibility TextBlockVisiblility => Mutability ? Visibility.Collapsed : Visibility.Visible;
        public Visibility TextBoxVisiblility => Mutability ? Visibility.Visible : Visibility.Collapsed;
        public bool Mutability { get; set; }
        public Windows.UI.Xaml.Media.SolidColorBrush Brush { get; set; }

        /// <summary>
        /// Creates a meta data entry with a key, value, mutability, and brush (based on mutability)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="mutability"></param>
        public MetadataEntry(string key, string value, bool mutability)
        {
            Key = key;
            Value = value;
            Mutability = mutability;
            Brush = new Windows.UI.Xaml.Media.SolidColorBrush(Colors.DarkGray);
            if (!mutability)
            {
                // On Wednesdays, we code in pink
                Brush.Color = Colors.DarkMagenta;
                Brush.Opacity = .15;
            }
        }
    }

}
