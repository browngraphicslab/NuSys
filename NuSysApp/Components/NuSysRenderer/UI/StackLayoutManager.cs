using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class StackLayoutManager : LayoutManager
    {
        private List<BaseInteractiveUIElement> _elements;

        /// <summary>
        /// The spacing between the elements in the stack.
        /// </summary>
        public float Spacing { get; set; }

        /// <summary>
        /// The margin in pixels between the left side of the stack and the left side of the parent
        /// </summary>
        public float LeftMargin { get; set; }

        /// <summary>
        /// The margin in pixels between the top side of the stack and the top side of the parent
        /// </summary>
        public float TopMargin { get; set; }

        /// <summary>
        /// The margin in pixels between the right side of the stack and the right side of the parent
        /// </summary>
        public float RightMargin { get; set; }

        /// <summary>
        /// The margin in pixels between the bottom side of the stack and the bottom side of the parent
        /// </summary>
        public float BottomMargin { get; set; }

        /// <summary>
        /// The width of each individual item in the stack
        /// </summary>
        public float ItemWidth { get; set; }

        /// <summary>
        /// The height of each individual item in the stack
        /// </summary>
        public float ItemHeight { get; set; }

        /// <summary>
        /// The horizontal alignment of the items in the stack
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// The vertical alignment of the items in the stack
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// The alignment of the entire stack.
        /// </summary>
        public StackAlignment StackAlignment;

        
        /// <summary>
        /// Create a new StackLayoutManager with the desired alignment
        /// </summary>
        /// <param name="alignment"></param>
        public StackLayoutManager(StackAlignment alignment = StackAlignment.Horizontal)
        {
            _elements = new List<BaseInteractiveUIElement>();
            StackAlignment = alignment;
        }
         
        public override void ArrangeItems()
        {

            float elementWidth;
            float elementHeight;
            Vector2 itemOffset = new Vector2();

            switch (StackAlignment)
            {

                //VERTICAL STACK
                case StackAlignment.Vertical:

                    switch (HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            itemOffset.X = LeftMargin;
                            elementWidth = ItemWidth;
                            break;
                        case HorizontalAlignment.Center:
                            itemOffset.X = (Width - LeftMargin)/2;
                            elementWidth = ItemWidth;
                            break;
                        case HorizontalAlignment.Right:
                            itemOffset.X = Width - RightMargin - ItemWidth;
                            elementWidth = ItemWidth;
                            break;
                        case HorizontalAlignment.Stretch:
                            itemOffset.X = LeftMargin;
                            elementWidth = Width - LeftMargin - RightMargin;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    switch (VerticalAlignment)
                    {
                        case VerticalAlignment.Top:
                            itemOffset.Y = TopMargin;
                            elementHeight = ItemHeight;

                            foreach (var element in _elements)
                            {
                                element.Transform.LocalPosition = itemOffset;
                                element.Width = elementWidth;
                                element.Height = elementHeight;

                                itemOffset.Y += elementHeight + Spacing;
                                Debug.Assert(itemOffset.Y <= Height, "The offset has overflown the size of the parent container");

                            }
                            break;
                        case VerticalAlignment.Bottom:
                            itemOffset.Y = Height - BottomMargin - _elements.Count*ItemHeight -
                                           (_elements.Count - 1)*Spacing;
                            elementHeight = ItemHeight;

                            foreach (var element in _elements)
                            {
                                element.Transform.LocalPosition = itemOffset;
                                element.Width = elementWidth;
                                element.Height = elementHeight;

                                itemOffset.Y += elementHeight + Spacing;
                                Debug.Assert(itemOffset.Y <= Height, "The offset has overflown the size of the parent container");
                            }
                            break;
                        case VerticalAlignment.Center:
                            itemOffset.Y = (Height - _elements.Count * ItemHeight - (_elements.Count - 1) * Spacing) / 2;
                            elementHeight = ItemHeight;
                            foreach (var element in _elements)
                            {
                                element.Transform.LocalPosition = itemOffset;
                                element.Width = elementWidth;
                                element.Height = elementHeight;

                                itemOffset.Y += elementHeight + Spacing;
                                Debug.Assert(itemOffset.Y <= Height, "The offset has overflown the size of the parent container");
                            }
                            break;
                        case VerticalAlignment.Stretch:
                            elementHeight = Height - TopMargin - BottomMargin - (_elements.Count - 1) * Spacing;
                            itemOffset.Y = TopMargin;
                            foreach (var element in _elements)
                            {
                                element.Transform.LocalPosition = itemOffset;
                                element.Width = elementWidth;
                                element.Height = elementHeight;

                                itemOffset.Y += elementHeight + Spacing;
                                Debug.Assert(itemOffset.Y <= Height, "The offset has overflown the size of the parent container");
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                // HORIZONTAL STACK
                case StackAlignment.Horizontal:

                    // take care of how elements are arranged vertically given the vertical alignment
                    switch (VerticalAlignment)
                    {
                        case VerticalAlignment.Top:
                            itemOffset.Y = TopMargin;
                            elementHeight = ItemHeight;
                            break;
                        case VerticalAlignment.Bottom:
                            itemOffset.Y = Height - BottomMargin - ItemHeight;
                            elementHeight = ItemHeight;
                            break;
                        case VerticalAlignment.Center:
                            itemOffset.Y = (Height - ItemHeight)/2;
                            elementHeight = ItemHeight;
                            break;
                        case VerticalAlignment.Stretch:
                            elementHeight = Height - TopMargin - BottomMargin;
                            itemOffset.Y = TopMargin;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // take care of how elements are arranged horizontally given the horizontal alignment
                    switch (HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            itemOffset.X = LeftMargin;
                            elementWidth = ItemWidth;

                            foreach (var element in _elements)
                            {
                                element.Transform.LocalPosition = itemOffset;
                                element.Width = elementWidth;
                                element.Height = elementHeight;
                                itemOffset.X = itemOffset.X + ItemWidth + Spacing;
                                Debug.Assert(itemOffset.X <= Width, "The offset has overflown the size of the parent container");
                            }
                            break;
                        case HorizontalAlignment.Center:
                            // for k elements, k * itemWidth, k - 1 * spacing, then divide by 2
                            itemOffset.X = (Width - _elements.Count*ItemWidth - (_elements.Count - 1)*Spacing)/2;
                            elementWidth = ItemWidth;

                            foreach (var element in _elements)
                            {
                                element.Transform.LocalPosition = itemOffset;
                                element.Width = elementWidth;
                                element.Height = elementHeight;
                                itemOffset.X = itemOffset.X + ItemWidth + Spacing;
                                Debug.Assert(itemOffset.X <= Width, "The offset has overflown the size of the parent container");
                            }
                            break;
                        case HorizontalAlignment.Right:
                            itemOffset.X = Width - RightMargin - _elements.Count*ItemWidth - (_elements.Count - 1)*Spacing;
                            elementWidth = ItemWidth;

                            foreach (var element in _elements)
                            {
                                element.Transform.LocalPosition = itemOffset;
                                element.Width = elementWidth;
                                element.Height = elementHeight;
                                itemOffset.X = itemOffset.X + ItemWidth + Spacing;
                                Debug.Assert(itemOffset.X <= Width, "The offset has overflown the size of the parent container");
                            }
                            break;
                        case HorizontalAlignment.Stretch:
                            // for k elements, k - 1 spacing, and left margin, and right margin
                            elementWidth = Width - LeftMargin - RightMargin - (_elements.Count - 1)*Spacing;
                            itemOffset.X = LeftMargin;

                            foreach (var element in _elements)
                            {
                                element.Transform.LocalPosition = itemOffset;
                                element.Width = elementWidth;
                                element.Height = elementHeight;
                                itemOffset.X = itemOffset.X + elementWidth + Spacing;
                                Debug.Assert(itemOffset.X <= Width, "The offset has overflown the size of the parent container");
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // set the height and width so function calls after this have the right value
            ItemWidth = elementWidth;
            ItemHeight = elementHeight;
        }

        /// <summary>
        /// Add the element to the stack
        /// </summary>
        /// <param name="element"></param>
        public void AddElement(BaseInteractiveUIElement element)
        {
            _elements.Add(element);
        }

        /// <summary>
        /// Remove the element from the stack
        /// </summary>
        /// <param name="element"></param>
        public void Remove(BaseInteractiveUIElement element)
        {
            _elements.Remove(element);
        }

        /// <summary>
        /// Remove the element from the stack at the requested index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            _elements.RemoveAt(index);
        }

        /// <summary>
        /// Set the margins of the stack. If one margin given sets left, top, right, bottom, to that margin
        /// if two margins given sets left, right to first margin, top, bottom to second margin
        /// if four margins given sets left, top, right, bottom to each margin respectively.
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <param name="m3"></param>
        /// <param name="m4"></param>
        public void SetMargins(float m1, float? m2 = null, float? m3 = null, float? m4 = null)
        {
            if (m2 == null)
            {
                LeftMargin = m1;
                RightMargin = m1;
                TopMargin = m1;
                BottomMargin = m1;
            }
            else if (m3 == null || m4 == null)
            {
                LeftMargin = m1;
                RightMargin = m1;
                TopMargin = m2.Value;
                BottomMargin = m2.Value;
            }
            else
            {
                LeftMargin = m1;
                TopMargin = m2.Value;
                RightMargin = m3.Value;
                BottomMargin = m4.Value;
            }
        }
    }
}
