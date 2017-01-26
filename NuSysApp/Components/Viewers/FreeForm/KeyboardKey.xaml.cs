using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

        public sealed partial class KeyboardKey : UserControl
        {

            public string KeyText
            {
                get { return xTextBlock.Text; }

                set { xTextBlock.Text = value; }
            }

            public string SuperscriptText
            {
                get { return xSuperscriptTextBlock.Text; }

                set { xSuperscriptTextBlock.Text = value; }
            }

            public object AdditionalContent
            {
                get { return (object)GetValue(AdditionalContentProperty); }
                set { SetValue(AdditionalContentProperty, value); }
            }

            public Brush KeyColor
            {
                get { return xMainGrid.Background; }

                set {xMainGrid.Background = value; }
            }

        public SolidColorBrush SelectColor
        {
            set { _selectColor = value; }
            get { return _selectColor; }
        }
        public SolidColorBrush UnselectColor
        {
            set { _unselectColor = value; }
            get { return _unselectColor; }
        }
        private SolidColorBrush _unselectColor;
        private SolidColorBrush _selectColor;
        private SolidColorBrush _highlightColor;
        private SolidColorBrush _highlightTextColor;
        private SolidColorBrush _unhighlightTextColor;



        public string KeyValue { set; get; }

        public double KeyFontSize
        {
            get { return xTextBlock.FontSize; }

            set { xTextBlock.FontSize = value; }
        }


        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("KeyText", typeof(string), typeof(KeyboardKey), null);
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached("KeyValue", typeof(string), typeof(KeyboardKey), null);
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("KeyColor", typeof(SolidColorBrush), typeof(KeyboardKey), null);
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.RegisterAttached("KeyFontSize", typeof(double), typeof(KeyboardKey), null);
        public static readonly DependencyProperty SelectColorProperty = DependencyProperty.Register("SelectColor", typeof(SolidColorBrush), typeof(KeyboardKey), null);
        public static readonly DependencyProperty UnSelectColorProperty = DependencyProperty.Register("UnselectColor", typeof(SolidColorBrush), typeof(KeyboardKey), null);
        public static readonly DependencyProperty AdditionalContentProperty = DependencyProperty.Register("AdditionalContent", typeof(object), typeof(KeyboardKey), null);

        public KeyboardKey()
            {
                _unselectColor = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
                _selectColor = new SolidColorBrush(Color.FromArgb(255, 0, 118, 215));
                _highlightColor = new SolidColorBrush(Color.FromArgb(255, 229, 229, 229));
                _highlightTextColor = new SolidColorBrush(Colors.Black);
                _unhighlightTextColor = new SolidColorBrush(Colors.White);


            this.InitializeComponent();
                SizeChanged += KeyboardKey_SizeChanged;

                
                //Key by default should be unselected
                Unselect();
            }

        private void SetFontSize()
        {
            var screenHeight = SessionController.Instance.ScreenHeight;

            xTextBlock.FontSize = 25/1080f * screenHeight;
            xSuperscriptTextBlock.FontSize = 11/1080f*screenHeight;
        }

        private void KeyboardKey_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetFontSize();
        }

        public void Select()
            {
                KeyColor = _selectColor;

            }

            public void Unselect()
            {
                KeyColor = _unselectColor;

            }

            public void Highlight()
            {
                KeyColor = _highlightColor;
                xTextBlock.Foreground = _highlightTextColor;

            }
            public void Unhighlight()
            {
                KeyColor = _unselectColor;
                xTextBlock.Foreground = _unhighlightTextColor;

            }

        public void Deactivate()
            {
                IsHitTestVisible = false;
            }

            public void Activate()
            {
                IsHitTestVisible = true;

            }

            private void XMainGrid_OnPointerEntered(object sender, PointerRoutedEventArgs e)
            {
                Highlight();
            }

            private void XMainGrid_OnPointerExited(object sender, PointerRoutedEventArgs e)
            {
                
                Unhighlight();
            }
        }
    }

