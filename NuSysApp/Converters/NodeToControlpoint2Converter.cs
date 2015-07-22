using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

using Windows.Foundation;

namespace NuSysApp
{
    public class NodeToControlpoint2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var linkvm = (LinkViewModel)parameter;
            var node1 = linkvm.Node1;
            var node2 = linkvm.Node2;
            var anchor1 = node1.Anchor;
            var anchor2 = node2.Anchor;
            var distanceX = anchor1.X - anchor2.X;
            return new Point(anchor2.X + distanceX / 2, anchor1.Y);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
