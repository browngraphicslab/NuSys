﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp.Views.Workspace
{
    public class FloatingMenuMode : AbstractWorkspaceViewMode
    {
        public FloatingMenuMode(WorkspaceView view) : base(view) { }

        public override void Activate()
        {
            _view.IsDoubleTapEnabled = true;
            _view.DoubleTapped += OnDoubleTapped;
        }

        public override void Deactivate()
        {
            _view.IsDoubleTapEnabled = false;
            _view.DoubleTapped -= OnDoubleTapped;
        }

        protected void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if (!(dc is WorkspaceViewModel))
            {
                e.Handled = true;
                return;
            }

            var vm = (WorkspaceViewModel)_view.DataContext;
            var floatingMenuTransform = new CompositeTransform();

            var p = e.GetPosition(_view);
            floatingMenuTransform.TranslateX = p.X;
            floatingMenuTransform.TranslateY = p.Y;
            vm.FMTransform = floatingMenuTransform;

            _view.FloatingMenu.Visibility = _view.FloatingMenu.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
