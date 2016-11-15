using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public abstract class BaseToolInnerView : RectangleUIElement
    {
        private RectangleUIElement _dragFilterItem;
        protected BasicToolViewModel Vm;
        public BaseToolInnerView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BasicToolViewModel viewModel) : base(parent, resourceCreator)
        {
            Debug.Assert(viewModel != null);
            Vm = viewModel;
            SetUpDragFilterItem();

        }

        private void SetUpDragFilterItem()
        {
            _dragFilterItem = new RectangleUIElement(this, ResourceCreator)
            {
                Height = 50,
                Width = 50,
                Background = Colors.Red
            };
            AddChild(_dragFilterItem);
            _dragFilterItem.IsVisible = false;
        }

        public abstract void SetProperties(List<string> propertiesList);

        public override void Dispose()
        {
            base.Dispose();
        }

        public abstract void SetVisualSelection(HashSet<string> itemsToSelect);
        /// <summary>
        /// When an item (e.g list view item, pie slice, bar chart) is tapped, change the selection accordingly
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="type"></param>
        public void Item_OnTapped(string selection, CanvasPointer pointer)
        {
            var type = pointer.DeviceType;
            if (Vm.Selection != null && Vm.Controller.Model.Selected && Vm.Selection.Contains(selection))
            {
                if (type == PointerDeviceType.Pen) //|| CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down
                {
                    Vm.Selection.Remove(selection);
                    Vm.Selection = Vm.Selection;
                }
                else
                {
                    Vm.Controller.UnSelect();
                }
            }
            else
            {
                if (type == PointerDeviceType.Pen) // || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down
                {
                    if (Vm.Selection != null)
                    {
                        Vm.Selection.Add(selection);
                        Vm.Selection = Vm.Selection;
                    }
                    else
                    {
                        Vm.Selection = new HashSet<string> { selection };
                    }
                }
                else
                {
                    Vm.Selection = new HashSet<string> { selection };
                }
            }
        }

        /// <summary>
        /// When an item (e.g list view item, pie slice, bar chart) is double tapped, try to open the detail view
        /// </summary>
        public void Item_OnDoubleTapped(string selection)
        {
            if (!Vm.Selection.Contains(selection) && Vm.Selection.Count == 0 || Vm.Controller.Model.Selected == false)
            {
                Vm.Selection = new HashSet<string> { selection };
            }
            if (Vm.Selection.Count == 1 &&
                Vm.Selection.First().Equals(selection))
            {
                Vm.OpenDetailView();
            }
        }

        ///// <summary>
        /////Set up drag item
        ///// </summary>
        //public void Item_ManipulationStarted(object sender)
        //{
        //    if (getCanvas().Children.Contains(_dragFilterItem))
        //        getCanvas().Children.Remove(_dragFilterItem);
        //    _currentDragMode = DragMode.Filter;
        //    getCanvas().Children.Add(_dragFilterItem);
        //    _dragFilterItem.RenderTransform = new CompositeTransform();
        //    var t = (CompositeTransform)_dragFilterItem.RenderTransform;
        //    t.TranslateX = _x;
        //    t.TranslateY = _y;
        //    _dragFilterItem.Visibility = Visibility.Collapsed;
        //    _draggedOutside = false;
        //    //not a great way of doing this. find out if there is a way to STOP all manipulation events once a new manipulation event has started.
        //    currentManipultaionSender = sender;

        //}

        /// <summary>
        ///Either scroll or drag depending on the location of the point and the origin of the event
        /// </summary>
        public void Item_Dragging(CanvasPointer pointer)
        {
            _dragFilterItem.Transform.LocalPosition = Vector2.Transform(pointer.CurrentPoint, this.Transform.ScreenToLocalMatrix) ;
            if (_dragFilterItem.Transform.LocalX > 0 && _dragFilterItem.Transform.LocalX < Width &&
                _dragFilterItem.Transform.LocalY < Height && _dragFilterItem.Transform.LocalY > 0)
            {
                _dragFilterItem.IsVisible = false;
            }
            else
            {
                _dragFilterItem.IsVisible = true;
            }
        }

        /// <summary>
        ///If the point is located outside the tool, logically set the selection based on selection type (Multi/Single) and either create new tool or add to existing tool
        /// </summary>
        public void Item_DragCompleted(string selection, CanvasPointer pointer)
        {
            if (_dragFilterItem.IsVisible)
            {
                _dragFilterItem.IsVisible = false;
                if (Vm.Selection.Contains(selection) || pointer.DeviceType == PointerDeviceType.Pen) // || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down
                {
                    Vm.Selection.Add(selection);
                    Vm.Selection = Vm.Selection;
                }
                else
                {
                    Vm.Selection = new HashSet<string>() { selection };
                }

                //IDK WHAT TO DO W THIS SHIT
                //var wvm = SessionController.Instance.ActiveFreeFormViewer;
                //var el = (FrameworkElement)sender;
                //var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);
                //var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(sp.X, sp.Y, 300, 300));
                //var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(sp, null);

                //Vm.FilterIconDropped(hitsStart, wvm, r.X, r.Y);
            }
            

        }

    }
}