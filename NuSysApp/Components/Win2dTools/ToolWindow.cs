﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyToolkit.Converters;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    public abstract class ToolWindow : ElementRenderItem
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
        /// This is the resizer triangle that allows you to resize a tool
        /// </summary>
        private ToolResizerRenderItem _resizer;


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

        private const int MIN_HEIGHT = 300;

        private const int MIN_WIDTH = 200;


        /// <summary>
        /// The rectangle at the bottom of the tool window
        /// </summary>
        protected RectangleUIElement ButtonBarRectangle;

        public ToolViewModel Vm { get; private set; }

        private const int BUTTON_MARGIN = 10;
        public ToolWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, ToolViewModel vm) : base(vm,parent as CollectionRenderItem, resourceCreator)
        {
            //Set up draggable and resizable attributes
            //TopBarColor = Constants.color3;
            //KeepAspectRatio = false;

            Vm = vm;
            SetUpButtons();
            SetUpDraggableIcons();
            SetUpBottomButtonBar();
            SetUpResizer();
            Vm.Controller.NumberOfParentsChanged += Controller_NumberOfParentsChanged;
            this.BorderWidth = 0;
            Height = (float)Vm.Height;
            Width = (float)Vm.Width;
            Transform.LocalX = (float)Vm.X;
            Transform.LocalY = (float)Vm.Y;
        }

        private void SetUpResizer()
        {
            _resizer = new ToolResizerRenderItem(this, ResourceCreator)
            {
                Width = 60,
                Height = 60,
            };
            //_resizer.
            _resizer.Dragged += _resizer_Dragged;
            AddChild(_resizer);

        }

        private void _resizer_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var transform = SessionController.Instance.SessionView.FreeFormViewer.Transform;
            var collection = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection;
            var delta = pointer.DeltaSinceLastUpdate;
            var newX = Vm.Width + delta.X / (transform.M11 * collection.Camera.S.M11);
            var newY = Vm.Height + delta.Y / (transform.M22 * collection.Camera.S.M22);
            if (newX > MIN_WIDTH && newY > MIN_HEIGHT)
            {
                this.Vm.Controller.SetSize(newX, newY);

            }
            else if (newX > MIN_WIDTH)
            {
                this.Vm.Controller.SetSize(newX, Vm.Height);
            }
            else if (newY > MIN_HEIGHT)
            {
                this.Vm.Controller.SetSize(Vm.Width, newY);
            }
            else
            {
                return;
            }
            SessionController.Instance.SessionView.FreeFormViewer.InvalidateMinimap();

        }

        public override Task Load()
        {
            SetUpFilterDropDown();
            return base.Load();
        }


        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            return base.HitTest(screenPoint);
        }


        public override bool IsInteractable()
        {
            return true;
        }

        public override void Dispose()
        {
            //TODO: Remove the tool window as a child
            _filterChooser.Dragged -= FilterChooserDropdownButtonOnDragged;
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
                _parentOperatorButton.ButtonText = Vm.Controller.ToolModel.ParentOperator.ToString();
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
                Background = Constants.color1,
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
            //Sets collection button to be dragged
            var collectionRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Transparent,
                Height = 50,
                Width = 50,
            };
            _draggableCollectionElement = new ButtonUIElement(this, ResourceCreator, collectionRectangle)
            {
                ButtonText = "collection",
                ButtonTextSize = 10,
                ButtonTextVerticalAlignment = CanvasVerticalAlignment.Bottom,
                ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                ButtonTextColor = Constants.color3
            };
            _draggableCollectionElement.ImageBounds = new Rect(_draggableCollectionElement.Width/4,
                _draggableCollectionElement.Height/4, _draggableCollectionElement.Width/2,
                _draggableCollectionElement.Height/2);
            _draggableCollectionElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN + _draggableCollectionElement.Width / 2), BUTTON_MARGIN);
            _draggableCollectionElement.Dragged += CollectionOrStack_Dragging; //TODO DISPOSE OF THESE
            _draggableCollectionElement.DragCompleted += CollectionOrStack_DragCompleted; //TODO DISPOSE OF THESE
            AddChild(_draggableCollectionElement);

            //sets up icon to show under pointer while dragging
            _collectionDragIcon = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Transparent,
                Height = 30, 
                Width = 30
            };
            _collectionDragIcon.IsVisible = false;
            AddChild(_collectionDragIcon);


            //Set up stack button to be dragged
            var stackRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Transparent,
                Height = 50,
                Width = 50,
            };
            _draggableStackElement = new ButtonUIElement(this, ResourceCreator, stackRectangle)
            {
                ButtonText = "stack",
                ButtonTextSize = 10,
                ButtonTextVerticalAlignment = CanvasVerticalAlignment.Bottom,
                ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                ButtonTextColor = Constants.color3
            };
            _draggableStackElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN + _draggableStackElement.Width / 2), _draggableCollectionElement.Height  + BUTTON_MARGIN);
            _draggableStackElement.ImageBounds = new Rect(_draggableStackElement.Width/4, _draggableStackElement.Height/4, _draggableStackElement.Width/2, _draggableStackElement.Height/2);
            _draggableStackElement.Dragged += CollectionOrStack_Dragging; //TODO DISPOSE OF THESE
            _draggableStackElement.DragCompleted += CollectionOrStack_DragCompleted; //TODO DISPOSE OF THESE
            AddChild(_draggableStackElement);

            //sets up icon to show under pointer while dragging
            _stackDragIcon = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Colors.Transparent,
                Height = 30,
                Width = 30
            };
            _stackDragIcon.IsVisible = false;
            AddChild(_stackDragIcon);

            UITask.Run(async delegate
            {
                _draggableCollectionElement.Image =
                    await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/collection icon bluegreen.png"));

                _collectionDragIcon.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/collection icon bluegreen.png"));

                _draggableStackElement.Image =
                    await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/collection icon bluegreen.png"));



                _stackDragIcon.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/collection icon bluegreen.png"));

            });

        }

        /// <summary>
        /// When a dragging of the collection or stack elements has completed, this will be called which creates a collection or stack at the point of release
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void CollectionOrStack_DragCompleted(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            var item = interactiveBaseRenderItem as ButtonUIElement;
            Debug.Assert(item != null);

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
        private void CollectionOrStack_Dragging(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            var item = interactiveBaseRenderItem as ButtonUIElement;
            Debug.Assert(item != null);

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

            // create a new dropdown ui element and set the ui for it
            _filterChooser = new DropdownUIElement(this, ResourceCreator)
            {
                Width = Width,
                Height = FILTER_CHOOSER_HEIGHT,
                RowHeight = FILTER_CHOOSER_HEIGHT,
                ButtonTextColor = Constants.color3,
                BorderWidth = 2,
                Bordercolor = Constants.color3,
                ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Left,
                ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center,
                Background = Constants.color1,
                ListBackground = Constants.color2,
                ListBorder = 1
                
            };
            foreach (var filterType in Enum.GetValues(typeof(ToolModel.ToolFilterTypeTitle)).Cast<ToolModel.ToolFilterTypeTitle>())
            {
                _filterChooser.AddOption(filterType.ToString());
            }
            _filterChooser.CurrentSelection = Vm.Filter.ToString();

            // add a method for when an item is selected in the dropdown
            _filterChooser.Selected += FilterChooserItem_Clicked; //TODO DISPOSE OF THIS

            // add all the filter options to the dropdown
            _filterChooser.Dragged += FilterChooserDropdownButtonOnDragged;

            AddChild(_filterChooser);
        }

        private void FilterChooserDropdownButtonOnDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var transform = SessionController.Instance.SessionView.FreeFormViewer.Transform;
            var collection = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection;
            var delta = pointer.DeltaSinceLastUpdate;
            var newX = Vm.X + delta.X / (transform.M11 * collection.Camera.S.M11);
            var newY = Vm.Y + delta.Y / (transform.M22 * collection.Camera.S.M22);
            this.Vm.Controller.SetPosition(newX,newY);
            SessionController.Instance.SessionView.FreeFormViewer.InvalidateMinimap();
        }

        /// <summary>
        /// The handler for when a filter is chosen
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void FilterChooserItem_Clicked(DropdownUIElement sender, string item)
        {
            var filter = (ToolModel.ToolFilterTypeTitle)Enum.Parse(typeof(ToolModel.ToolFilterTypeTitle), item);

            if (filter == ToolModel.ToolFilterTypeTitle.AllMetadata) 
            {
                await Vm.SwitchToAllMetadataTool((float)Transform.LocalX, (float)Transform.LocalY);
                Delete();
            }
            else if (Vm.Filter == ToolModel.ToolFilterTypeTitle.AllMetadata)
            {
                await Vm.SwitchToBasicTool(filter, (float)Transform.LocalX, (float)Transform.LocalY);
                Delete();
            }
            else
            {
                Vm.Filter = filter;
            }
        }

        /// <summary>
        /// Initializes the parent operator, delete and refresh buttons
        /// </summary>
        private async Task SetUpButtons()
        {
            var parentOperatorRectangle = new RectangleUIElement(this, ResourceCreator)
            {
                Height = PARENT_OPERATOR_BUTTON_HEIGHT,
                Width = PARENT_OPERATOR_BUTTON_WIDTH,
                Background = Constants.color1
            };
            _parentOperatorButton = new ButtonUIElement(this, ResourceCreator, parentOperatorRectangle)
            {
                ButtonText = "AND",
                ButtonTextSize = 32,
                ButtonTextColor = Constants.color3,
                ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center,
                IsVisible = false,
                Transform = {LocalY = -PARENT_OPERATOR_BUTTON_HEIGHT}
            };
            _parentOperatorButton.Tapped += _parentOperatorButton_Tapped;
            AddChild(_parentOperatorButton);


            var deleteCircleShape = new EllipseUIElement(this, ResourceCreator)
            {
                Background = Constants.color1,
                Width = 50,
                Height = 50,
            };
            _deleteButton = new ButtonUIElement(this, ResourceCreator, deleteCircleShape);
            _deleteButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/node icons/delete.png"));
            _deleteButton.ImageBounds = new Rect(_deleteButton.Width/4, _deleteButton.Height/4, _deleteButton.Width/2, _deleteButton.Height/2);
            _deleteButton.Transform.LocalPosition = new Vector2(-(BUTTON_MARGIN + _deleteButton.Width), _deleteButton.Height / 2 + BUTTON_MARGIN);
            _deleteButton.Tapped += _deleteButton_Tapped;
            AddChild(_deleteButton);

            var refreshCircleShape = new EllipseUIElement(this, ResourceCreator)
            {
                Background = Constants.color1,
                Width = 50,
                Height = 50,
            };
            _refreshButton = new ButtonUIElement(this, ResourceCreator, refreshCircleShape);
            _refreshButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/refresh icon.png"));
            _refreshButton.ImageBounds = new Rect(_refreshButton.Width / 4, _refreshButton.Height / 4, _refreshButton.Width / 2, _refreshButton.Height / 2);
            _refreshButton.Tapped += _refreshButton_Tapped;
            _refreshButton.Transform.LocalPosition = new Vector2(-(BUTTON_MARGIN + _deleteButton.Width), _deleteButton.Transform.LocalY + _deleteButton.Height + BUTTON_MARGIN);
            AddChild(_refreshButton);
        }
        /// <summary>
        /// When the delete button is tapped, call the dispose method
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void _deleteButton_Tapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            Delete();
        }

        public async void Delete()
        {
            await UITask.Run(() =>
            {
                Vm.Controller.Delete(this);
            });
            Dispose();
        }

        /// <summary>
        /// Refresh the filter chain when the refresh button is clicked
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _refreshButton_Tapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
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
        private void _parentOperatorButton_Tapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            if (Vm.Controller.ToolModel.ParentOperator == ToolModel.ParentOperatorType.And)
            {
                Vm.Controller.SetParentOperator(ToolModel.ParentOperatorType.Or);
                _parentOperatorButton.ButtonText = "OR";
            }
            else if (Vm.Controller.ToolModel.ParentOperator == ToolModel.ParentOperatorType.Or)
            {
                Vm.Controller.SetParentOperator(ToolModel.ParentOperatorType.And);
                _parentOperatorButton.ButtonText = "AND";
            }
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _resizer.Transform.LocalPosition = new Vector2((float)(Width - _resizer.Width), (float)(Height - _resizer.Height));
            Height = (float) Vm.Height;
            Width = (float) Vm.Width;
            //Make the width of the filter chooser and the button always fill the window
            if (_filterChooser != null)
            {
                _filterChooser.Width = Width;
            }

            

            if (_parentOperatorButton != null)
            {
                _parentOperatorButton.Transform.LocalX = Width / 2 - _parentOperatorButton.Width / 2;
            }

            if (_draggableCollectionElement != null)
            {
                //Set up draggable collection and stack elements local position.
                _draggableCollectionElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN), _draggableCollectionElement.Height / 2 + BUTTON_MARGIN);
                _draggableStackElement.Transform.LocalPosition = new Vector2(Width + (BUTTON_MARGIN), _draggableCollectionElement.Transform.LocalY + _draggableCollectionElement.Height + BUTTON_MARGIN);
            }

            if (ButtonBarRectangle != null)
            {
                //Set up button bar at the bottom of tool
                ButtonBarRectangle.Transform.LocalY = this.Height - BUTTON_BAR_HEIGHT;
                ButtonBarRectangle.Width = Width;
            }

            base.Update(parentLocalToScreenTransform);
        }
    }
}