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

                set { xMainGrid.Background = value; }
            }

        private SolidColorBrush _selectColor;
            private SolidColorBrush _unselectColor;
            public string KeyValue { set; get; }


            public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("KeyText", typeof(string), typeof(KeyboardKey), null);
            public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached("KeyValue", typeof(string), typeof(KeyboardKey), null);
            public static readonly DependencyProperty AdditionalContentProperty = DependencyProperty.Register("AdditionalContent", typeof(object), typeof(KeyboardKey), null);
        public KeyboardKey()
            {
                this.InitializeComponent();

                _unselectColor = new SolidColorBrush(Colors.Gray);
                _selectColor = new SolidColorBrush(Colors.Blue);
            }


            public void Select()
            {
                KeyColor = _selectColor;

            }

            public void Unselect()
            {
                KeyColor = _unselectColor;

            }

            public void Deactivate()
            {
                IsHitTestVisible = false;
                xTextBlock.Foreground = new SolidColorBrush(Colors.LightGray);
            }

            public void Activate()
            {
                IsHitTestVisible = true;
                xTextBlock.Foreground = new SolidColorBrush(Colors.White);

            }
        }
    }

