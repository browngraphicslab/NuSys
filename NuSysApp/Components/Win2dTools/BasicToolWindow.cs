using System;
using System.Collections.Generic;
using System.Numerics;
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



        public BasicToolWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BasicToolViewModel vm) : base(parent, resourceCreator, vm)
        {
            //Vm = vm;
            SetUpBottomButtons();
            _toolView = new BasicToolListInnerView(this, ResourceCreator, vm);
            AddChild(_toolView);

            //vm.Controller.SetLocation(x, y);


            vm.ReloadPropertiesToDisplay();
            _toolView.SetProperties((Vm as BasicToolViewModel).PropertiesToDisplay);
            _currentViewMode = ViewMode.List;
            //SetSize(250, 450);
            (vm.Controller as BasicToolController).SelectionChanged += OnSelectionChanged;
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
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
            if (vm.Selection != null && (Vm.Controller as BasicToolController).Model.Selected)
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
            _toolView.Height = Height - FILTER_CHOOSER_HEIGHT - UIDefaults.TopBarHeight - BUTTON_BAR_HEIGHT;
            _toolView.Width = Width;
            _toolView.Transform.LocalPosition = new Vector2(0, UIDefaults.TopBarHeight+FILTER_CHOOSER_HEIGHT);
            
            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Sets up the buttons at the bottom of the tool ()
        /// </summary>
        private void SetUpBottomButtons()
        {
            //Set up list button
            var listButtonRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Transparent,
                Height = VIEW_BUTTON_HEIGHT,
                Width = VIEW_BUTTON_HEIGHT,
            };
            _listToolViewButton = new ButtonUIElement(this, ResourceCreator, listButtonRectangle);
            
            _listToolViewButton.Transform.LocalPosition = new Vector2(VIEW_BUTTON_MARGIN,
                ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN);
            AddChild(_listToolViewButton);

            //Set up pie button 
            var pieButtonRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Transparent,
                Height = VIEW_BUTTON_HEIGHT,
                Width = VIEW_BUTTON_HEIGHT,
            };
            _pieToolViewButton = new ButtonUIElement(this, ResourceCreator, pieButtonRectangle);
            _pieToolViewButton.ButtonTextColor = Constants.color3;
            _pieToolViewButton.Transform.LocalPosition =
                new Vector2(_listToolViewButton.Transform.LocalX + _listToolViewButton.Width + VIEW_BUTTON_MARGIN,
                    ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN);
            AddChild(_pieToolViewButton);

            //Set up bar chart button 
            var barButtonRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Transparent,
                Height = VIEW_BUTTON_HEIGHT,
                Width = VIEW_BUTTON_HEIGHT,
                BorderWidth = 1,
                Bordercolor = Constants.color2
            };
            _barToolViewButton = new ButtonUIElement(this, ResourceCreator, barButtonRectangle);
            _barToolViewButton.ButtonTextColor = Colors.Black;
            _barToolViewButton.Transform.LocalPosition =
                new Vector2(_pieToolViewButton.Transform.LocalX + _pieToolViewButton.Width + VIEW_BUTTON_MARGIN,
                    ButtonBarRectangle.Transform.LocalY + VIEW_BUTTON_MARGIN);
            AddChild(_barToolViewButton);

            UITask.Run(async delegate
            {
                _listToolViewButton.Image =
                    await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/listview bluegreen.png"));
                _listToolViewButton.ImageWidth = 30;
                _listToolViewButton.ImageHeight = 30;

                _pieToolViewButton.Image =
                    await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/piegraph bluegreen.png"));
                _pieToolViewButton.ImageWidth = 30;
                _pieToolViewButton.ImageHeight = 30;

                _barToolViewButton.Image =
                    await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/bar chart icon.png"));
                _barToolViewButton.ImageWidth = 30;
                _barToolViewButton.ImageHeight = 30;
            });
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