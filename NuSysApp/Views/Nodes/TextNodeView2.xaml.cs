
﻿using Windows.UI;
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class TextNodeView2 : UserControl
    {

        public TextNodeView2(TextNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            var vm = (TextNodeViewModel)this.DataContext;
            if (vm.IsEditingInk == true)
            {
                nodeTpl.ToggleInkMode();
            }
            vm.ToggleEditing();
        }

        private void OnInkClick(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();            
        }

        private void FormatText(object sender, RoutedEventArgs e)
        {
            ITextSelection selected = textBox.Document.Selection;
            if (selected != null)
            {
                ITextCharacterFormat characterFormat = selected.CharacterFormat;
                if (sender == Bold)
                {
                    characterFormat.Bold = FormatEffect.Toggle;
                }
                if (sender == Italic)
                {
                    characterFormat.Italic = FormatEffect.Toggle;
                }
                if (sender == Underline)
                {
                    if (characterFormat.Underline == UnderlineType.Single)
                    {
                        characterFormat.Underline = UnderlineType.None;
                    }
                    else
                    {
                        characterFormat.Underline = UnderlineType.Single;
                    }
                }
                if (sender == Size8)
                {
                    characterFormat.Size = 8;
                }
                if (sender == Size12)
                {
                    characterFormat.Size = 12;
                }
                if (sender == Size14)
                {
                    characterFormat.Size = 14;
                }
                if (sender == Size18)
                {
                    characterFormat.Size = 18;
                }
                if (sender == Size24)
                {
                    characterFormat.Size = 24;
                }
                if (sender == Red)
                {
                    characterFormat.ForegroundColor = Windows.UI.Color.FromArgb(100,255,0,0);
                }
                if (sender == Orange)
                {
                    characterFormat.ForegroundColor = Windows.UI.Color.FromArgb(100,255,128,0);
                }
                if (sender == Yellow)
                {
                    characterFormat.ForegroundColor = Windows.UI.Color.FromArgb(100,255,255,0);
                }
                if (sender == Green)
                {
                    characterFormat.ForegroundColor = Windows.UI.Color.FromArgb(100, 0, 255, 0);
                }
                if (sender == Blue)
                {
                    characterFormat.ForegroundColor = Windows.UI.Color.FromArgb(100, 0, 0, 255);
                }
                if (sender == Purple)
                {
                    characterFormat.ForegroundColor = Windows.UI.Color.FromArgb(100,127,0,255);
                }
                if (sender == Black)
                {
                    characterFormat.ForegroundColor = Windows.UI.Color.FromArgb(100, 0, 0, 0);
                }
                if (sender == White)
                {
                    characterFormat.ForegroundColor = Windows.UI.Color.FromArgb(100,255,255,255);
                }
                

                selected.CharacterFormat = characterFormat;

            }
        }

//        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
//        {
//            var vm = (TextNodeViewModel)this.DataContext;
//            vm.CreateAnnotation();
//            vm.WorkSpaceViewModel.CheckForNodeNodeIntersection(vm); //TODO Eventually need to remove 
//            if (vm.IsAnnotation)
//            {
//                SolidColorBrush backgroundColorBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 111, 138, 150));
//                nodeTpl.Background = backgroundColorBrush;
//            }
//            e.Handled = true;
//        }

    }
}
