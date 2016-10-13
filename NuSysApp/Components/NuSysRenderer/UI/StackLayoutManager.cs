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

        public float Spacing { get; set; }

        public float LeftMargin { get; set; }
        public float TopMargin { get; set; }
        public float RightMargin { get; set; }
        public float BottomMargin { get; set; }

        public float ItemWidth { get; set; }
        public float ItemHeight { get; set; }


        public HorizontalAlignment HorizontalAlignment { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        private StackAlignment _stackAlignment;


        public enum StackAlignment
        {
            Horizontal, Vertical
        }

        public StackLayoutManager(StackAlignment alignment = StackAlignment.Horizontal)
        {
            _elements = new List<BaseInteractiveUIElement>();
            _stackAlignment = alignment;
        }
         
        public override void ArrangeItems(Vector2 offset)
        {

            float elementWidth;
            float elementHeight;
            Vector2 itemOffset = new Vector2();

            switch (_stackAlignment)
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

        public void AddElement(BaseInteractiveUIElement element)
        {
            _elements.Add(element);
        }

        public void Remove(BaseInteractiveUIElement element)
        {
            _elements.Remove(element);
        }

        public void RemoveAt(int index)
        {
            _elements.RemoveAt(index);
        }

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
