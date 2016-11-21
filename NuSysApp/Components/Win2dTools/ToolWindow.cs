﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    public abstract class ToolWindow : ResizeableWindowUIElement
    {
        /// <summary>
        /// The delete to delete the tool
        /// </summary>
        private ButtonUIElement _deleteButton;

        /// <summary>
        /// The button to refresh the tool
        /// </summary>
        private ButtonUIElement _refreshButton;

        /// <summary>
        /// This functions as a simple list that is hidden or shown depending on whether 
        /// _filterChooserDropdownButton is clicked effectively creating a dropdown list
        /// </summary>
        private DropdownUIElement _filterChooser;

        /// <summary>
        /// The button for showing the dropdown list for choosing filter type
        /// </summary>
        private ButtonUIElement _filterChooserDropdownButton;

        /// <summary>
        /// The ButtonUIElement that can be dragged to create a new collection
        /// </summary>
        private ButtonUIElement _draggableCollectionElement;

        private RectangleUIElement _collectionDragIcon;

        /// <summary>
        /// The ButtonUIElement that can be dragged to create a stack
        /// </summary>
        private ButtonUIElement _draggableStackElement;

        /// <summary>
        /// This is what shows up under the cursor when you start dragging the stack button
        /// </summary>
        private RectangleUIElement _stackDragIcon;

        /// <summary>
        /// This is the rectangle on top of the tool that allows you to change the parent operator (AND, OR)
        /// </summary>
        private ButtonUIElement _parentOperatorButton;


        /// <summary>
        /// The height of the rows and button for the filter chooser dropdown menu
        /// </summary>
        protected const int FILTER_CHOOSER_HEIGHT = 60;

        /// <summary>
        ///Height of the button bar at the bottom of the tool
        /// </summary>
        protected const int BUTTON_BAR_HEIGHT = 60;

        private const int PARENT_OPERATOR_BUTTON_HEIGHT = 50;

        private const int PARENT_OPERATOR_BUTTON_WIDTH = 150;



        /// <summary>
        /// The rectangle at the bottom of the tool window
        /// </summary>
        protected RectangleUIElement ButtonBarRectangle;

        public ToolViewModel Vm { get; private set; }

        private const int BUTTON_MARGIN = 10;
        public ToolWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, ToolViewModel vm) : base(parent, resourceCreator)
        {
            Vm = vm;
            SetUpButtons();
            SetUpFilterDropDown();
            SetUpDraggableIcons();
            SetUpBottomButtonBar();
            vm.Controller.NumberOfParentsChanged += Controller_NumberOfParentsChanged;
            Height = (float)Vm.Height;
            Width = (float)Vm.Width;
            Transform.LocalX = (float)Vm.X;
            Transform.LocalY = (float)Vm.Y;
        }

        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            return base.HitTest(screenPoint);
        }


        public override void Dispose()
        {
            //TODO: Remove the tool window as a child
            _filterChooserDropdownButton.Tapped -= _dropdownButton_OnPressed;
            Vm.Controller.NumberOfParentsChanged -= Controller_NumberOfParentsChanged;
            _parentOperatorButton.Tapped -= _parentOperatorButton_Tapped;
            Vm.Dispose();
            base.Dispose();
        }

        /// <summary>
        ///If the number of parents is greater than 1, this sets the visibility of the parent operator (AND/OR) grid 
        /// </summary>
        private void Controller_NumberOfParentsChanged(int numOfParents)
        {
            if (numOfParents > 1)
            {
                _parentOperatorButton.IsVisible = true;
                _parentOperatorButton.ButtonText = Vm.Controller.Model.ParentOperator.ToString();
            }
            else
            {
                _parentOperatorButton.IsVisible = false;
            }
        }

        /// <summary>
        /// Sets up the bar in the bottom behind all the buttons
        /// </summary>
        private void SetUpBottomButtonBar()
        {
            ButtonBarRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Azure,
                Height = BUTTON_BAR_HEIGHT,
                Width = Width
            };
            ButtonBarRectangle.Transform.LocalPosition = new Vector2(0, Height - BUTTON_BAR_HEIGHT);
            AddChild(ButtonBarRectangle);
        }

        /// <summary>
        /// This sets up the collection and stack draggable elements 
        /// </summary>
        private void SetUpDraggableIcons()
        {
            //Sets up button to be dragged
            var collectionRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Green,
                Height = 50,
                Width = 50,
            };
            _draggableCollectionElement = new ButtonUIElement(this, ResourceCreator, collectionRectangle);
            _draggableCollectionElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN + _draggableCollectionElement.Width / 2), BUTTON_MARGIN);
            _draggableCollectionElement.Dragging += CollectionOrStack_Dragging;
            _draggableCollectionElement.DragCompleted += CollectionOrStack_DragCompleted;
            AddChild(_draggableCollectionElement);

            //sets up icon to show under pointer while dragging
            _collectionDragIcon = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Green,
                Height = 50, 
                Width = 50
            };
            _collectionDragIcon.IsVisible = false;
            AddChild(_collectionDragIcon);


            //Set up button to be dragged
            var stackRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Purple,
                Height = 50,
                Width = 50,
            };
            _draggableStackElement = new ButtonUIElement(this, ResourceCreator, stackRectangle);
            _draggableStackElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN + _draggableStackElement.Width / 2), _draggableCollectionElement.Height  + BUTTON_MARGIN);
            _draggableStackElement.Dragging += CollectionOrStack_Dragging;
            _draggableStackElement.DragCompleted += CollectionOrStack_DragCompleted;
            AddChild(_draggableStackElement);

            //sets up icon to show under pointer while dragging
            _stackDragIcon = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Purple,
                Height = 50,
                Width = 50
            };
            _stackDragIcon.IsVisible = false;
            AddChild(_stackDragIcon);

        }

        /// <summary>
        /// When a dragging of the collection or stack elements has completed, this will be called which creates a collection or stack at the point of release
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void CollectionOrStack_DragCompleted(ButtonUIElement item, CanvasPointer pointer)
        {
            var canvasCoordinate = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(new Vector2(pointer.CurrentPoint.X, pointer.CurrentPoint.Y), SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection);
            if (item == _draggableCollectionElement)
            {
                _collectionDragIcon.IsVisible = false;
                Vm.CreateCollection(canvasCoordinate.X, canvasCoordinate.Y);

            }
            else
            {
                Debug.Assert(item == _draggableStackElement);
                _stackDragIcon.IsVisible = false;
                Vm.CreateStack(canvasCoordinate.X, canvasCoordinate.Y);

            }
        }

        /// <summary>
        /// When the stack or collection item is being dragged, move the drag icon to be under the mouse.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void CollectionOrStack_Dragging(ButtonUIElement item, CanvasPointer pointer)
        {
            RectangleUIElement icon;
            if (item == _draggableCollectionElement)
            {
                icon = _collectionDragIcon;
            }
            else
            {
                Debug.Assert(item == _draggableStackElement);
                icon = _stackDragIcon;
            }
            icon.IsVisible = true;
            icon.Transform.LocalPosition = Vector2.Transform(pointer.CurrentPoint, this.Transform.ScreenToLocalMatrix);

        }

        /// <summary>
        /// Sets up the button and the basic list so that they work together like a dropdown list
        /// </summary>
        private void SetUpFilterDropDown()
        {
            _filterChooserDropdownButton = new ButtonUIElement(this, ResourceCreator, new RectangleUIElement(this, ResourceCreator) {Height = 100, Width = 500});
            _filterChooserDropdownButton.ButtonText = Vm.Filter.ToString();
            _filterChooserDropdownButton.Width = Width;
            _filterChooserDropdownButton.Height = FILTER_CHOOSER_HEIGHT;
            _filterChooserDropdownButton.ButtonTextColor = Colors.Black;
            _filterChooserDropdownButton.BorderWidth = 2;
            _filterChooserDropdownButton.Bordercolor = Colors.Black;
            _filterChooserDropdownButton.ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            _filterChooserDropdownButton.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            _filterChooserDropdownButton.Tapped += _dropdownButton_OnPressed; ;
            _filterChooserDropdownButton.Transform.LocalPosition = new Vector2(0, TopBarHeight);
            AddChild(_filterChooserDropdownButton);

            _filterChooser = new DropdownUIElement(this, ResourceCreator, Width);
            _filterChooser.ButtonHeight = FILTER_CHOOSER_HEIGHT;
            foreach (var filterType in Enum.GetValues(typeof(ToolModel.ToolFilterTypeTitle)).Cast<ToolModel.ToolFilterTypeTitle>())
            {
                _filterChooser.AddOption(filterType.ToString(), FilterChooserItem_Clicked);
            }
            _filterChooser.Layout();

            _filterChooser.IsVisible = false;
            _filterChooser.Transform.LocalPosition = new Vector2(0, TopBarHeight + _filterChooserDropdownButton.Height);
            AddChild(_filterChooser);
        }



        /// <summary>
        /// When the dropdown button is pressed either show or hide the dropdown list filter chooser
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _dropdownButton_OnPressed(ButtonUIElement item, CanvasPointer pointer)
        {
            if (_children.Last() != _filterChooser)
            {
                _children.Remove(_filterChooser);
                _children.Add(_filterChooser);
            }
            if (_filterChooser.IsVisible)
            {
                _filterChooser.IsVisible = false;
            }
            else
            {
                _filterChooser.IsVisible = true;
            }
        }

        /// <summary>
        /// The handler for when a filter is chosen
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void FilterChooserItem_Clicked(ButtonUIElement item, CanvasPointer pointer)
        {
            _filterChooser.IsVisible = false;
            //var vm = Vm as BasicToolViewModel;
            var filter = (ToolModel.ToolFilterTypeTitle)Enum.Parse(typeof(ToolModel.ToolFilterTypeTitle), item.ButtonText);
            var canvasCoordinate = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(new Vector2((float)Transform.Position.X, (float)Transform.Position.Y), SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection);
            if (filter == ToolModel.ToolFilterTypeTitle.AllMetadata) 
            {
                //TODO: I think dispose is getting called before switching to all metadata tool
                await Vm.SwitchToAllMetadataTool((float)canvasCoordinate.X, (float)canvasCoordinate.Y);
                this.Dispose();
            }
            else if (Vm.Filter == ToolModel.ToolFilterTypeTitle.AllMetadata)
            {
                await Vm.SwitchToBasicTool(filter, (float)canvasCoordinate.X, (float)canvasCoordinate.Y);
                this.Dispose();
            }
            else
            {
                Vm.Filter = filter;
                _filterChooserDropdownButton.ButtonText = item.ButtonText;

            }
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
        }

        /// <summary>
        /// Initializes the parent operator, delete and refresh buttons
        /// </summary>
        private void SetUpButtons()
        {
            var parentOperatorRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Height = PARENT_OPERATOR_BUTTON_HEIGHT,
                Width = PARENT_OPERATOR_BUTTON_WIDTH,
                Background = Colors.Red
            };
            _parentOperatorButton = new ButtonUIElement(this, ResourceCreator, parentOperatorRectangle);
            _parentOperatorButton.Transform.LocalY = -PARENT_OPERATOR_BUTTON_HEIGHT;
            _parentOperatorButton.ButtonText = "AND";
            _parentOperatorButton.ButtonTextColor = Colors.Black;
            _parentOperatorButton.IsVisible = false;
            _parentOperatorButton.Tapped += _parentOperatorButton_Tapped;
            AddChild(_parentOperatorButton);


            var deleteCircleShape = new EllipseUIElement(this, ResourceCreator)
            {
                Background = Colors.Red,
                Width = 50,
                Height = 50,
            };
            _deleteButton = new ButtonUIElement(this, ResourceCreator, deleteCircleShape);
            _deleteButton.Transform.LocalPosition = new Vector2(-(BUTTON_MARGIN + _deleteButton.Width), _deleteButton.Height / 2 + BUTTON_MARGIN);
            _deleteButton.Tapped += _deleteButton_Tapped;
            AddChild(_deleteButton);

            var refreshCircleShape = new EllipseUIElement(this, ResourceCreator)
            {
                Background = Colors.Blue,
                Width = 50,
                Height = 50,
            };
            _refreshButton = new ButtonUIElement(this, ResourceCreator, refreshCircleShape);
            _refreshButton.Tapped += _refreshButton_Tapped;
            _refreshButton.Transform.LocalPosition = new Vector2(-(BUTTON_MARGIN + _deleteButton.Width), _deleteButton.Transform.LocalY + _deleteButton.Height + BUTTON_MARGIN);
            AddChild(_refreshButton);
        }
        /// <summary>
        /// When the delete button is tapped, call the dispose method
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _deleteButton_Tapped(ButtonUIElement item, CanvasPointer pointer)
        {
            Dispose();
        }

        /// <summary>
        /// Refresh the filter chain when the refresh button is clicked
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _refreshButton_Tapped(ButtonUIElement item, CanvasPointer pointer)
        {
            Task.Run(delegate {
                Vm.Controller?.RefreshFromTopOfChain();
            });
        }

        /// <summary>
        /// When the parent operator button is tapped, change the parent operator appropriately (AND/OR)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _parentOperatorButton_Tapped(ButtonUIElement item, CanvasPointer pointer)
        {
            if (Vm.Controller.Model.ParentOperator == ToolModel.ParentOperatorType.And)
            {
                Vm.Controller.SetParentOperator(ToolModel.ParentOperatorType.Or);
                _parentOperatorButton.ButtonText = "OR";
            }
            else if (Vm.Controller.Model.ParentOperator == ToolModel.ParentOperatorType.Or)
            {
                Vm.Controller.SetParentOperator(ToolModel.ParentOperatorType.And);
                _parentOperatorButton.ButtonText = "AND";
            }
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {            
            //Make the width of the filter chooser and the button always fill the window
            if (_filterChooser.Width != Width)
            {
                _filterChooser.Width = Width;
                _filterChooser.Layout();
            }
            _filterChooserDropdownButton.Width = Width;

            _parentOperatorButton.Transform.LocalX = Width / 2 - _parentOperatorButton.Width/2;

            //Set up draggable collection and stack elements local position.
            _draggableCollectionElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN), _draggableCollectionElement.Height / 2 + BUTTON_MARGIN);
            _draggableStackElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN), _draggableCollectionElement.Transform.LocalY + _draggableCollectionElement.Height + BUTTON_MARGIN);

            //Set up button bar at the bottom of tool
            ButtonBarRectangle.Transform.LocalY = this.Height - BUTTON_BAR_HEIGHT;
            ButtonBarRectangle.Width = Width;

            base.Update(parentLocalToScreenTransform);
        }
    }
}