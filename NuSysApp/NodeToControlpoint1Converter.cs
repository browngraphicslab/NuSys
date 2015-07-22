using System;
using Windows.Foundation;
using Windows.UI.Xaml.Data;


namespace NuSysApp
{
    public class NodeToControlpoint1Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var nodeVm = (NodeViewModel) value;
            var linkvm = (LinkViewModel) parameter;
            var node1 = linkvm.Node1;
            var node2 = linkvm.Node2;
            var anchor1 = node1.Anchor;
            var anchor2 = node2.Anchor;
            var distanceX = anchor1.X - anchor2.X;
            return new Point(anchor1.X - distanceX / 2, anchor2.Y);         
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
