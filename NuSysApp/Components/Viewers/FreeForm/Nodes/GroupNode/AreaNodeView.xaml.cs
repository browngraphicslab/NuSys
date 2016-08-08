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
using NusysIntermediate;
// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AreaNodeView : AnimatableUserControl
    {
        private SelectMode _selectMode;
        private NodeManipulationMode _nodeManipulationMode;
        private CreateGroupMode _createGroupMode;
        private PanZoomMode _panZoomMode;
        private NusysConstants.ElementType _elementType;
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

            var collElementModel = ((vm.Controller as ElementCollectionController).Model as CollectionElementModel);
            var finite = (SessionController.Instance.ContentController.GetLibraryElementModel(collElementModel.LibraryId) as CollectionLibraryElementModel).IsFinite;
            if (finite)
            {
                _nodeManipulationMode.Deactivate();
                //_panZoomMode.Deactivate(); //this should be commented out when panning and zooming isn't buggy anymore
            }
            var shapePoints = (SessionController.Instance.ContentController.GetLibraryElementModel(collElementModel.LibraryId) as CollectionLibraryElementModel).ShapePoints;


            var adornment = new AdornmentView(new List<Point>(shapePoints.Select(p => new Point(p.X, p.Y))));
            vm.AtomViewList.Add(adornment);
            adornment.SetFill(new SolidColorBrush(Colors.Green));
            Canvas.SetZIndex(adornment,-1);

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
