using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class FullScreenViewerView : AnimatableUserControl
    {
        public FullScreenViewerView()
        {
            InitializeComponent();

            this.Opacity = 0;


          DataContextChanged += delegate(FrameworkElement sender, DataContextChangedEventArgs args)
          {
              if (!(DataContext is FullScreenViewerViewModel))
                  return;
              
              var vm = (FullScreenViewerViewModel)DataContext;
              vm.PropertyChanged += OnPropertyChanged;
          };

            IsHitTestVisible = false;
            PointerReleased += OnPointerReleased;

        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            Anim.To(this, "Alpha", 0, 400);
            IsHitTestVisible = false;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "View")
            {
                Anim.To(this, "Alpha", 1, 400, null, (a,i) => { IsHitTestVisible = true; });
            }
        }

        private void OnShown(object o)
        {
            
        }
    }
}
