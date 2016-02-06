using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LibraryView : UserControl
    {
        private Dictionary<string, LibraryElement> _elements = new Dictionary<string, LibraryElement>();
        public LibraryView()
        {
            this.InitializeComponent();
        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Container.Visibility = Visibility.Collapsed;
        }

        public async void ToggleVisiblity()
        {
            Visibility = Visibility == Visibility.Visible ? Visibility.Collapsed: Visibility.Visible;
            if (Visibility == Visibility.Visible)
            {
                await Reload();
            }
        }

        private async Task Reload()
        {
            Task.Run(async delegate
            {
                var dictionaries = await SessionController.Instance.NuSysNetworkSession.GetAllLibraryElements();
                foreach (var kvp in dictionaries)
                {
                    //id, data, type, title
                    var dict = kvp.Value;
                    var id = dict["id"];
                    var element = new LibraryElement(id);
                    if (dict.ContainsKey("title"))
                    {
                        element.Title = dict["title"];
                    }
                    try
                    {
                        if (dict.ContainsKey("type"))
                        {
                            element.NodeType = (NodeType)Enum.Parse(typeof(NodeType), dict["type"]);
                        }
                    }
                    catch (Exception e)
                    {


                    }
                    if (!_elements.ContainsKey(id))
                    {
                        _elements.Add(id, element);
                    }
                }
            });
        }
    }
}
