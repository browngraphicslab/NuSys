﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Components.Misc;

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
        private DuplicateNodeMode _duplicateNodeMode;

        public AreaNodeView(AreaNodeViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;
            

            _selectMode = new SelectMode(this);
            _selectMode.Activate();

            _nodeManipulationMode = new NodeManipulationMode(this, true);
            _nodeManipulationMode.Activate();

            //_panZoomMode = new PanZoomMode(this);
            //_panZoomMode.Activate();

            _dragOutMode = new DragOutMode(this);
            _dragOutMode.Activate();

           // vm.AtomViewList.Add(new Windows.UI.Xaml.Shapes.Ellipse() {Width = 700, Height = 500, Fill = new SolidColorBrush(Colors.Red
            //    ) });
            vm.AtomViewList.Add(new AdornmentView());
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

        public Canvas AtomContainer { get { return xAtomContainer; } }


    }
}
