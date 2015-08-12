
﻿using System.Diagnostics;
﻿using System;
﻿using System.ComponentModel;
﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class RichTextNodeView2 : UserControl
    {
        public RichTextNodeView2(RichTextNodeViewModel vm) {
            this.InitializeComponent();
            this.DataContext = vm;                     
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.ToggleEditing();
        }

        private void OnInkClick(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs f)
        {
            int startSelection = textBlock.Document.Selection.StartPosition;
            int endSelection = textBlock.Document.Selection.EndPosition;
            string periodo = "";
            textBlock.Document.GetText(TextGetOptions.UseCrlf, out periodo);
            int eof = periodo.Length;
            periodo = "";
                ITextRange range = textBlock.Document.GetRange(--startSelection, endSelection);
                range.GetText(TextGetOptions.UseCrlf, out periodo);
            
            string x = periodo.Trim();
            x.Replace("HYPERLINK \" \"", "");
            //string uriToLaunch = @"http://google.com";
            //var uri = new Uri(uriToLaunch);
            //Windows.System.Launcher.LaunchUriAsync(uri);

            //find the and then search through the .rtfio
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var vm = (RichTextNodeViewModel)this.DataContext;
            vm.WorkSpaceViewModel.CheckForNodeNodeIntersection(vm); //TODO Eventually need to remove   
            e.Handled = true;
        }
    }
}