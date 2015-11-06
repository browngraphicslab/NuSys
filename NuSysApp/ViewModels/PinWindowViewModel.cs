using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class PinWindowViewModel
    {
        public ObservableCollection<PinModel> Pins { get; set; }

        public PinWindowViewModel()
        {
            Pins = new ObservableCollection<PinModel>();

            SessionController.Instance.WorkspaceChanged += delegate(object source, WorkspaceViewModel workspace)
            {

                workspace.AtomViewList.CollectionChanged +=
                    delegate(object sender, NotifyCollectionChangedEventArgs args)
                    {

                        foreach (var newItem in args.NewItems.OfType<PinView>())
                        {
                            Pins.Add((PinModel)((PinViewModel)newItem.DataContext).Model);
                        }
                    };

              
            };
        }
 
    }
}
