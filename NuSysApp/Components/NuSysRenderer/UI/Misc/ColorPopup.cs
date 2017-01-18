using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System.Numerics;
using Windows.UI;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// another pop up for editing collection settings (bounded/unbounded, shaped/unshaped)
    /// this chooses the color that a shape will be
    /// </summary>
    public class ColorPopup : PopupUIElement
    {
        /// <summary>
        /// title box of this pop up 
        /// </summary>
        private TextboxUIElement _titleBox;

        /// <summary>
        /// variable stores collection controller so we can act on the current element in the detail view
        /// </summary>
        private CollectionLibraryElementController _collectionController;

        /// <summary>
        /// these are all buttons for the colors :)
        /// </summary>
        private EllipseButtonUIElement _red;
        private EllipseButtonUIElement _orange;
        private EllipseButtonUIElement _yellow;
        private EllipseButtonUIElement _green;
        private EllipseButtonUIElement _blue;
        private EllipseButtonUIElement _purple;

        /// <summary>
        /// stored variable for size
        /// </summary>
        private float _size = 40;

        /// <summary>
        /// stack layout managers for the two rows of colors
        /// </summary>
        private StackLayoutManager _row1;
        private StackLayoutManager _row2;

        public ColorPopup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionLibraryElementController controller) : base(parent, resourceCreator)
        {
            _collectionController = controller;

            //set appearance
            Background = Colors.White;
            BorderWidth = 2;
            BorderColor = Constants.DARK_BLUE;
            Width = 600;
            Height = 500;

            //make title box
            _titleBox = new TextboxUIElement(this, resourceCreator)
            {
                Text = "Shape Color",
                FontSize = 20,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Center
            };
            AddChild(_titleBox);

            //color buttons
            _red = new EllipseButtonUIElement(this, resourceCreator)
            {
                Height = _size,
                Width = _size,
                Background = Constants.COLOR_RED
            };
            AddChild(_red);

            _orange = new EllipseButtonUIElement(this, resourceCreator)
            {
                Height = _size,
                Width = _size,
                Background = Constants.COLOR_ORANGE
            };
            AddChild(_orange);

            _yellow = new EllipseButtonUIElement(this, resourceCreator)
            {
                Height = _size,
                Width = _size,
                Background = Constants.COLOR_YELLOW
            };
            AddChild(_yellow);

            _green = new EllipseButtonUIElement(this, resourceCreator)
            {
                Height = _size,
                Width = _size,
                Background = Constants.COLOR_GREEN
            };
            AddChild(_green);

            _blue = new EllipseButtonUIElement(this, resourceCreator)
            {
                Height = _size,
                Width = _size,
                Background = Constants.COLOR_BLUE
            };
            AddChild(_blue);

            _purple = new EllipseButtonUIElement(this, resourceCreator)
            {
                Height = _size,
                Width = _size,
                Background = Constants.COLOR_PURPLE
            };
            AddChild(_purple);

            //first color row
            _row1 = new StackLayoutManager();
            _row1.StackAlignment = StackAlignment.Horizontal;
            _row1.Spacing = 10;
            _row1.ItemWidth = _size;
            _row1.ItemHeight = _size;
            _row1.AddElement(_red);
            _row1.AddElement(_orange);
            _row1.AddElement(_yellow);

            //second color row
            _row2 = new StackLayoutManager();
            _row2 = new StackLayoutManager();
            _row2.StackAlignment = StackAlignment.Horizontal;
            _row2.Spacing = 10;
            _row2.ItemWidth = _size;
            _row2.ItemHeight = _size;
            _row2.AddElement(_green);
            _row2.AddElement(_blue);
            _row2.AddElement(_purple);

            _red.Tapped += ColorButtonOnTapped;
            _orange.Tapped += ColorButtonOnTapped;
            _yellow.Tapped += ColorButtonOnTapped;
            _green.Tapped += ColorButtonOnTapped;
            _blue.Tapped += ColorButtonOnTapped;
            _purple.Tapped += ColorButtonOnTapped;
        }

        /// <summary>
        /// sets color of shape based on the color button that was pressed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ColorButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            
            if (_collectionController.CollectionContentDataController.CollectionModel.Shape == null ||
                _collectionController.CollectionContentDataController.CollectionModel.Shape.ImageUrl != null)
            {
                ///sets shape to the bounding square
                _collectionController.CollectionContentDataController.SetShapePoints(new List<PointModel>()
                {
                    new PointModel(50000,50000),
                    new PointModel(50000,51000),
                    new PointModel(50000,52000),
                    new PointModel(52000,52000),
                    new PointModel(52000,51000),
                    new PointModel(52000,50000),
                });
            }

            /// giant if else statement cause i can't figure out how to do this with a switch statement ((sorry))
            Color color;
            if (item.Equals(_red))
            {
                color = Constants.COLOR_RED;
            } else if (item.Equals(_orange))
            {
                color = Constants.COLOR_ORANGE;
            } else if (item.Equals(_yellow))
            {
                color = Constants.COLOR_YELLOW;
            } else if (item.Equals(_green))
            {
                color = Constants.COLOR_GREEN;
            } else if (item.Equals(_blue))
            {
                color = Constants.COLOR_BLUE;
            }
            else
            {
                color = Constants.COLOR_PURPLE;
            }
            
            _collectionController.CollectionContentDataController.SetShapeColor(color.ToColorModel()); 
        }


        public override void Update(Matrix3x2 parentToLocalTransform)
        {
            //set title box position and width
            _titleBox.Width = Width;
            _titleBox.Height = Height / 5;
            _titleBox.Transform.LocalPosition = new Vector2(Width / 2 - _titleBox.Width / 2, Height / 8);

            /// set position - doing math for centering these buddies
            _row1.TopMargin = Height * 2 / 5;
            _row2.TopMargin = Height - Height * 2 / 5 + 10;
            _row1.LeftMargin = (float)(Width / 2 - ((_size * 3) - 20)/1.5);
            _row2.LeftMargin = (float)(Width / 2 - ((_size * 3) - 20)/1.5);

            //arrange items
            _row1.ArrangeItems();
            _row2.ArrangeItems();
            base.Update(parentToLocalTransform);
        }

        public override void Dispose()
        {
            _red.Tapped -= ColorButtonOnTapped;
            _orange.Tapped -= ColorButtonOnTapped;
            _yellow.Tapped -= ColorButtonOnTapped;
            _green.Tapped -= ColorButtonOnTapped;
            _blue.Tapped -= ColorButtonOnTapped;
            _purple.Tapped -= ColorButtonOnTapped;
            base.Dispose();
        }
    }
}
