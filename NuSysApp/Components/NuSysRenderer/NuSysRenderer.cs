﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Numerics;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyToolkit.Mathematics;
using SharpDX.Direct2D1;
using Matrix3x2 = System.Numerics.Matrix3x2;
using System.Numerics;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;
using Point = Windows.Foundation.Point;



namespace NuSysApp
{
    public class NuSysRenderer
    { 
        private static volatile NuSysRenderer instance;
        private static object syncRoot = new Object();

        private CanvasAnimatedControl _canvas;

        private MinimapRenderItem _minimap;
        private SelectMode _selectMode;
        private List<uint> _activePointers = new List<uint>();
        private ElementSelectionRenderItem _elementSelectionRenderItem;
        private CollectionRenderItem _currentCollection;
        private Dictionary<CollectionRenderItem, CollectionInteractionManager> _interactionManagers = new Dictionary<CollectionRenderItem, CollectionInteractionManager>();
        public CanvasAnimatedControl Canvas
        {
            get { return _canvas; }
        }

        public Size Size { get; set; }
        public CollectionRenderItem InitialCollection { get; private set; }
        public ObservableCollection<ElementRenderItem> Selections { get; set; } = new ObservableCollection<ElementRenderItem>();

        private NuSysRenderer()
        {
        }

        private void CanvasOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
        }

        public async Task Init(CanvasAnimatedControl canvas)
        {
            Size = new Size(canvas.Width, canvas.Height);
            _canvas = canvas;
            _canvas.Draw += CanvasOnDraw;
            _canvas.Update += CanvasOnUpdate;
            _canvas.CreateResources += CanvasOnCreateResources;
            _canvas.SizeChanged += CanvasOnSizeChanged;

            _canvas.PointerPressed += XRenderCanvasOnPointerPressed;
            _canvas.PointerReleased += XRenderCanvasOnPointerReleased;

            var vm = (FreeFormViewerViewModel) canvas.DataContext;
            InitialCollection = new CollectionRenderItem(vm, null, canvas, true);
            _currentCollection = InitialCollection;

            _interactionManagers[_currentCollection] = new CollectionInteractionManager(InitialCollection);
            _interactionManagers[_currentCollection].ItemTapped += OnItemTapped;
            _interactionManagers[_currentCollection].ItemLongTapped += OnItemLongTapped;
            _interactionManagers[_currentCollection].InkStarted += OnInkStarted;
            _interactionManagers[_currentCollection].InkDrawing += OnInkDrawing;
            _interactionManagers[_currentCollection].InkStopped += OnInkStopped;
            

            vm.X = 0;
            vm.Y = 0;
            vm.Width = Size.Width;
            vm.Height = Size.Height;
            _elementSelectionRenderItem = new ElementSelectionRenderItem(vm, InitialCollection, _canvas);
     
            _minimap = new MinimapRenderItem(vm, InitialCollection, canvas);
        }

        private void OnInkStopped(PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("ink stopped");
            _currentCollection.InkRenderItem.StopInkByEvent(e);
        }

        private void OnInkDrawing(PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("ink drawing");
            _currentCollection.InkRenderItem.UpdateInkByEvent(e);
        }

        private void OnInkStarted(PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("ink started");
            _currentCollection.InkRenderItem.StartInkByEvent(e);
        }

        private void XRenderCanvasOnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                _activePointers.Remove(args.Pointer.PointerId);
        }

        private void XRenderCanvasOnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                _activePointers.Add(args.Pointer.PointerId);
        }

        private void OnItemLongTapped(BaseRenderItem element, PointerDeviceType device)
        {
            var collection = element as CollectionRenderItem;
            SwitchCollection(collection);
        }

        private void SwitchCollection(CollectionRenderItem collection)
        {
            if (collection != _currentCollection && collection != null)
            {
                _interactionManagers[_currentCollection].ItemTapped -= OnItemTapped;
                _interactionManagers[_currentCollection].ItemLongTapped -= OnItemLongTapped;
                _interactionManagers[_currentCollection].InkStarted -= OnInkStarted;
                _interactionManagers[_currentCollection].InkDrawing -= OnInkDrawing;
                _interactionManagers[_currentCollection].InkStopped -= OnInkStopped;
                _interactionManagers[_currentCollection].Dispose();
                _interactionManagers.Remove(_currentCollection);

                _currentCollection = collection;

                _interactionManagers[collection] = new CollectionInteractionManager(collection);
                _interactionManagers[collection].ItemTapped += OnItemTapped;
                _interactionManagers[collection].ItemLongTapped += OnItemLongTapped;
                _interactionManagers[_currentCollection].InkStarted += OnInkStarted;
                _interactionManagers[_currentCollection].InkDrawing += OnInkDrawing;
                _interactionManagers[_currentCollection].InkStopped += OnInkStopped;


            }
        }

        private void OnItemTapped(BaseRenderItem element, PointerDeviceType device)
        {
            var elementRenderItem = element as ElementRenderItem;
            
            var vm = InitialCollection.ViewModel;
            if (elementRenderItem == InitialCollection || elementRenderItem == _currentCollection || elementRenderItem == null)
            {
                Selections.Clear();
                if (element == null)
                    SwitchCollection(InitialCollection);
            }
            else
            {
                if (device == PointerDeviceType.Mouse)
                {
                    var keyState = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Control);
                  
                    if (!keyState.HasFlag(CoreVirtualKeyStates.Down))
                        Selections.Clear();
                    Selections.Add(elementRenderItem);
                }

                if (device == PointerDeviceType.Touch)
                {
                    if (_activePointers.Count == 0)
                        Selections.Clear();
                    Selections.Add(elementRenderItem);
                }
            }
        }


        private void CanvasOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Size = sizeChangedEventArgs.NewSize;
            _minimap.CreateResources();
            _minimap.IsDirty = true;
        }

        public BaseRenderItem GetRenderItemAt(Vector2 sp, CollectionRenderItem collection = null, int maxLevel = int.MaxValue)
        {
            collection = collection ?? InitialCollection;
            var mat = GetTransformUntil(collection);
            return _GetRenderItemAt(collection, sp, mat, 0, maxLevel);
        }

        public BaseRenderItem GetRenderItemAt(Point sp, CollectionRenderItem collection = null, int maxLevel = int.MaxValue)
        {
            var result = GetRenderItemAt(new Vector2((float)sp.X, (float)sp.Y), collection, maxLevel);
            return result;
        }

        public Matrix3x2 GetCollectionTransform(CollectionRenderItem collection)
        {
            var transforms = new List<CollectionRenderItem> {collection};

            var parent = collection.Parent;
            while (parent != null)
            {
                transforms.Add(parent);
                parent = parent.Parent;
            }

            transforms.Reverse();
            return transforms.Aggregate(Matrix3x2.Identity, (current, t) => Win2dUtil.Invert(t.Camera.C)*t.Camera.S*t.Camera.C*t.Camera.T*Win2dUtil.Invert(t.C)*t.S*t.C*t.T*current);
        }

        public Matrix3x2 GetTransformUntil(BaseRenderItem collection)
        {
            var transforms = new List<BaseRenderItem>();
            var parent = collection.Parent;
            while (parent != null)
            {
                transforms.Add(parent);
                parent = parent.Parent;
            }

            transforms.Reverse();

            return transforms.Select(t1 => t1 as CollectionRenderItem).Aggregate(Matrix3x2.Identity, (current, t) => Win2dUtil.Invert(t.Camera.C)*t.Camera.S*t.Camera.C*t.Camera.T*Win2dUtil.Invert(t.C)*t.S*t.C*t.T*current);
        }

        private BaseRenderItem _GetRenderItemAt(CollectionRenderItem collection, Vector2 sp, Matrix3x2 transform, int currentLevel, int maxLevel)
        {


            if (currentLevel < maxLevel)
            {
                var poo = Win2dUtil.Invert(collection.Camera.C) * collection.Camera.S * collection.Camera.C * collection.Camera.T *
          Win2dUtil.Invert(collection.C) * collection.S * collection.C * collection.T * transform;
                var childTransform = Win2dUtil.Invert(poo);

                foreach (var renderItem in collection.GetRenderItems())
                {
                    var innerCollection = renderItem as CollectionRenderItem;
                    if (innerCollection != null)
                    {

                        if (currentLevel + 1 < maxLevel)
                        {
                            var result = _GetRenderItemAt(innerCollection, sp, poo, currentLevel + 1, maxLevel);
                            if (result != collection)
                                return result;
                        }
                        else
                        {
                            if (innerCollection.HitTest(Vector2.Transform(sp, childTransform)))
                            {
                                return innerCollection;
                            }
                        }
                    }


                    if (renderItem.HitTest(Vector2.Transform(sp, childTransform)))
                    {
                        return renderItem;
                    }
                }
            }

            var poo2 = transform;
            var collectionTranform = Win2dUtil.Invert(poo2);

            if (collection.HitTest(Vector2.Transform(sp, collectionTranform)))
                return collection;

            return null;
        }

        private void CanvasOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            InitialCollection.Update();
            _minimap.IsDirty = true;
            _minimap.Update();
            _elementSelectionRenderItem.Update();
        }

        private void CanvasOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            using(var ds = args.DrawingSession) {
                ds.Clear(Colors.LightGoldenrodYellow);
                ds.Transform = Matrix3x2.Identity;
                InitialCollection.Draw(ds);
                ds.Transform = Matrix3x2.Identity;
                _minimap.Draw(ds);
                _elementSelectionRenderItem.Draw(ds);
            }
        }

        public static NuSysRenderer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new NuSysRenderer();
                    }
                }

                return instance;
            }
        }
    }
}
