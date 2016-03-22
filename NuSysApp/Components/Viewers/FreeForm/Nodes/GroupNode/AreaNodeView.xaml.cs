using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AreaNodeView : AnimatableUserControl
    {
        private SelectMode _selectMode;
        private NodeManipulationMode _nodeManipulationMode;
        private CreateGroupMode _createGroupMode;
        private PanZoomMode _panZoomMode;
        private ElementType _elementType;
        private Image _dragItem;
        private DragOutMode _dragOutMode;

        public AreaNodeView(AreaNodeViewModel vm)
        {
            DataContext = vm;
            this.InitializeComponent();

            _selectMode = new SelectMode(this);
            _selectMode.Activate();

            _nodeManipulationMode = new NodeManipulationMode(this);
            _nodeManipulationMode.Activate();

            _panZoomMode = new PanZoomMode(this);
            _panZoomMode.Activate();

            _dragOutMode = new DragOutMode(this);
            _dragOutMode.Activate();
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
            };
            
            
        }

        //private async void BtnAddNodeOnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs args)
        //{
        //    _elementType = sender == btnAddNode ? ElementType.Text : ElementType.Image;

        //    args.Container = xWrapper;
        //    var bmp = new RenderTargetBitmap();
        //    await bmp.RenderAsync((UIElement)sender);
        //    var img = new Image();
        //    img.Opacity = 0;
        //    var t = new CompositeTransform();

        //    img.RenderTransform = new CompositeTransform();
        //    img.Source = bmp; 
        //    _dragItem = img;

        //    xWrapper.Children.Add(_dragItem);
        //}
        public void Update()
        {
            // TODO: refactor
            /*
            var _vm = (AreaNodeViewModel) DataContext;
            foreach (var atom in _vm.)
            {
                var vm = (AtomViewModel)atom.DataContext; //access viewmodel
                vm.X= vm.Model.X;
                vm.X= vm.Model.X;
                vm.CanEdit = Sendable.EditStatus.No;
                vm.Height = vm.Model.Height;
                vm.Width = vm.Model.Width;
                
            }
            */
        }

        public Canvas OuterCanvas => xCanvas;
    }
}
