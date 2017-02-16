using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class BasicToolWindow: ToolWindow
    {
        

        //Margin for the buttons for changing the inner view of the tool (list, pie, bar)
        private const int VIEW_BUTTON_MARGIN = 10;

        private const int VIEW_BUTTON_HEIGHT = 40;

        private BasicToolInnerView _toolView;

        private enum ViewMode { PieChart, List, BarChart }
        private ViewMode _currentViewMode;

        /// <summary>
        /// The button for changing the inner view to the list view
        /// </summary>
        private ButtonUIElement _listToolViewButton;

        /// <summary>
        /// The button for changing the inner view to the pie chart view
        /// </summary>
        private ButtonUIElement _pieToolViewButton;

        /// <summary>
        /// The button for changing the inner view to the bar chart view.
        /// </summary>
        private ButtonUIElement _barToolViewButton;

        private BasicToolViewModel _vm;


        private BasicToolListInnerView _listInnerView;
        private PieToolInnerView _pieInnerView;
        private BarToolInnerView _barInnerView;

        public BasicToolWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BasicToolViewModel vm) : base(parent, resourceCreator, vm)
        {
            _vm = vm;
            SetUpBottomButtons();
            SetUpInnerViews();

            _toolView = _listInnerView;
            vm.ReloadPropertiesToDisplay();
            _toolView.SetProperties((Vm as BasicToolViewModel).PropertiesToDisplay);
            _currentViewMode = ViewMode.List;
            (vm.Controller as BasicToolController).SelectionChanged += OnSelectionChanged;
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
        }

        private void SetUpInnerViews()
        {
            _listInnerView = new BasicToolListInnerView(this, ResourceCreator, _vm);
            _pieInnerView = new PieToolInnerView(this, ResourceCreator, _vm);
            _barInnerView = new BarToolInnerView(this, ResourceCreator, _vm);

            _listInnerView.IsVisible = true;
            _pieInnerView.IsVisible = false;
            _barInnerView.IsVisible = false;

            AddChild(_listInnerView);
            AddChild(_pieInnerView);
            AddChild(_barInnerView);

            _currentViewMode = ViewMode.List;
        }

        public override void Dispose()
        {
            (Vm.Controller as BasicToolController).SelectionChanged -= OnSelectionChanged;
            Vm.PropertiesToDisplayChanged -= Vm_PropertiesToDisplayChanged;
            _toolView.Dispose();
            base.Dispose();
        }

        /// <summary>
        ///Passes new properties to display to the toolview
        /// </summary>
        private void Vm_PropertiesToDisplayChanged()
        {
            _toolView.SetProperties((Vm as BasicToolViewModel).PropertiesToDisplay);
        }

        /// <summary>
        ///Passes new selection to the inner tool view
        /// </summary>
        private void OnSelectionChanged(object sender)
        {
            var vm = Vm as BasicToolViewModel;
            if (vm.Selection != null && (Vm.Controller as BasicToolController).ToolModel.Selected)
            {
                _toolView.SetVisualSelection(vm.Selection);
            }
            else
            {
                _toolView.SetVisualSelection(new HashSet<string>());
            }
        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _toolView.Height = Height - FILTER_CHOOSER_HEIGHT - BUTTON_BAR_HEIGHT;
            _toolView.Width = Width;
            _toolView.Transform.LocalPosition = new Vector2(0, FILTER_CHOOSER_HEIGHT);

            base.Update(parentLocalToScreenTransform);
        }


        /// <summary>
        /// Sets up the buttons at the bottom of the tool ()
        /// </summary>
        private void SetUpBottomButtons()
        {
            //Set up list button
            _listToolViewButton = new TransparentButtonUIElement(this, ResourceCreator, UIDefaults.PrimaryStyle)
            {
                Height = VIEW_BUTTON_HEIGHT,
                Width = VIEW_BUTTON_HEIGHT
            };
            _listToolViewButton.Tapped += ListToolViewButton_Tapped;
            _listToolViewButton.ButtonTextColor = Constants.ALMOST_BLACK;
            _listToolViewButton.Transform.LocalPosition = new Vector2(VIEW_BUTTON_MARGIN,
                ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN);
            AddChild(_listToolViewButton);

            _pieToolViewButton = new TransparentButtonUIElement(this, ResourceCreator, UIDefaults.PrimaryStyle)
            {
                Height = VIEW_BUTTON_HEIGHT,
                Width = VIEW_BUTTON_HEIGHT
            };
            _pieToolViewButton.Tapped += PieToolViewButton_Tapped;
            _pieToolViewButton.ButtonTextColor = Constants.ALMOST_BLACK;
            _pieToolViewButton.Transform.LocalPosition =
                new Vector2(_listToolViewButton.Transform.LocalX + _listToolViewButton.Width + VIEW_BUTTON_MARGIN,
                    ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN);
            AddChild(_pieToolViewButton);

            //Set up bar chart button 
            _barToolViewButton = new TransparentButtonUIElement(this, ResourceCreator, UIDefaults.PrimaryStyle)
            {
                Height = VIEW_BUTTON_HEIGHT,
                Width = VIEW_BUTTON_HEIGHT
            };
            _barToolViewButton.Tapped += BarToolViewButton_Tapped;
            _barToolViewButton.ButtonTextColor = Constants.ALMOST_BLACK;
            _barToolViewButton.Transform.LocalPosition =
                new Vector2(_pieToolViewButton.Transform.LocalX + _pieToolViewButton.Width + VIEW_BUTTON_MARGIN,
                    ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN);
            AddChild(_barToolViewButton);


            UITask.Run(async delegate
            {
                _listToolViewButton.Image =
                    await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/listview.png"));
                _listToolViewButton.ImageBounds = new Rect(.25,.25,.5,.5);


                _pieToolViewButton.Image =
                    await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/pie chart.png"));
                _pieToolViewButton.ImageBounds = new Rect(.25, .25, .5, .5);


                _barToolViewButton.Image =
                    await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/bar chart.png"));
                _barToolViewButton.ImageBounds = new Rect(.25, .25, .5, .5);

            });
        }


        private void ListToolViewButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_currentViewMode == ViewMode.List)
            {
                return;
            }
            //RemoveChild(_toolView);
            _listInnerView.IsVisible = true;
            _pieInnerView.IsVisible = false;
            _barInnerView.IsVisible = false;

            _toolView = _listInnerView;
            //AddChild(_toolView);

            _vm.ReloadPropertiesToDisplay();
            _toolView.SetProperties(_vm.PropertiesToDisplay);
            _currentViewMode = ViewMode.List;

        }

        private void PieToolViewButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_currentViewMode == ViewMode.PieChart)
            {
                return;
            }
            _listInnerView.IsVisible = false;
            _pieInnerView.IsVisible = true;
            _barInnerView.IsVisible = false;

            _toolView = _pieInnerView;

            _vm.ReloadPropertiesToDisplay();
            _toolView.SetProperties(_vm.PropertiesToDisplay);
            _currentViewMode = ViewMode.PieChart;

        }

        private void BarToolViewButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_currentViewMode == ViewMode.BarChart)
            {
                return;
            }

            _listInnerView.IsVisible = false;
            _pieInnerView.IsVisible = false;
            _barInnerView.IsVisible = true;

            _toolView = _barInnerView;

            _vm.ReloadPropertiesToDisplay();
            _toolView.SetProperties(_vm.PropertiesToDisplay);
            _currentViewMode = ViewMode.BarChart;

        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);


            //Arrange the buttons at the bottom
            if (_listToolViewButton != null)
            {
                _listToolViewButton.Transform.LocalY = ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN;

            }

            if(_pieToolViewButton != null)
            {
                _pieToolViewButton.Transform.LocalY = ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN;

            }

            if (_barToolViewButton != null)
            {
                _barToolViewButton.Transform.LocalY = ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN;
            }


        }
    }
}