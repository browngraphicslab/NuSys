
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
    public sealed partial class RichTextNodeView : UserControl
    {
        private bool _isEditing; //bool used to enable and disable editing (texblock vs textbox) (see more in NodeViewModel.cs)
        //editing is handeled using methods: IsEditing, ToggleEditing (all in NodeViewModel.cs), Edit_Click (in this file)
        public RichTextNodeView(RichTextNodeViewModel vm)
        {
            this.DataContext = vm;
            _isEditing = false; //sets the text block to be in front of textbox so no editing is possible
            this.InitializeComponent();
            this.SetUpBindings();
            vm.PropertyChanged += new PropertyChangedEventHandler(Node_SelectionChanged);
            inkCanvas.InkPresenter.IsInputEnabled = false;
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
            Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch; //This line is setting the Devices that can be used to display ink
           
        }

        #region Helper Methods
        private void SetUpBindings()
        {
            Binding leftBinding = new Binding() { Path = new PropertyPath("X") };
            leftBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(Canvas.LeftProperty, leftBinding);

            Binding topBinding = new Binding() { Path = new PropertyPath("Y") };
            topBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(Canvas.TopProperty, topBinding);
        }

        #endregion Helper Methods

        #region Event Handlers
        private void UserControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            RichTextNodeViewModel vm = (RichTextNodeViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void Resizer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            RichTextNodeViewModel vm = (RichTextNodeViewModel)this.DataContext;
            vm.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }
        
        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            RichTextNodeViewModel vm = (RichTextNodeViewModel)this.DataContext;
            vm.ToggleSelection();
            e.Handled = true;

        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            RichTextNodeViewModel vm = (RichTextNodeViewModel)this.DataContext;
            vm.ToggleEditing();
            if (ManipulationMode == ManipulationModes.All)
            {
                ManipulationMode = ManipulationModes.None;
            }
            else
            {
                ManipulationMode = ManipulationModes.All;
            }
            #endregion Event Handlers
        }
        private void EditC_Click(object sender, RoutedEventArgs e)
        {
            var vm = (RichTextNodeViewModel)this.DataContext;
            vm.ToggleEditingC();
            inkCanvas.InkPresenter.IsInputEnabled = vm.IsEditingInk;   
            if (ManipulationMode == ManipulationModes.All)
            {
                ManipulationMode = ManipulationModes.None;
            }
            else
            {
                ManipulationMode = ManipulationModes.All;
            }
        }
        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            RichTextNodeViewModel vm = (RichTextNodeViewModel)this.DataContext;
            vm.Remove();
        }

        private void Rich_Tapped(object sender, RoutedEventArgs f)
        {
            int startSelection = textBlock.Document.Selection.StartPosition;
            int endSelection = textBlock.Document.Selection.EndPosition;
            string periodo = "";
            textBlock.Document.GetText(TextGetOptions.UseCrlf, out periodo);
            int eof = periodo.Length;
            periodo = "";
                ITextRange range = textBlock.Document.GetRange(--startSelection, endSelection);
                range.GetText(TextGetOptions.UseCrlf, out periodo);
            
            Debug.WriteLine(periodo.Length);
            string x = periodo.Trim();
            x.Replace("HYPERLINK \" \"", "");
            //string uriToLaunch = @"http://google.com";
            //var uri = new Uri(uriToLaunch);
            //Windows.System.Launcher.LaunchUriAsync(uri);

            Debug.WriteLine(x);
            //find the and then search through the .rtfio
        }

        private void Node_SelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsSelected"))
            {
                var vm = (RichTextNodeViewModel)this.DataContext;
                if (vm.IsSelected)
                {
                    slideout.Begin();
                }
                else
                {
                    slidein.Begin();
                    if (vm.IsEditing == true)
                    {
                        vm.ToggleEditing();
                        _isEditing = false;
                    }
                    if (vm.IsEditingInk == true)
                    {
                        vm.ToggleEditingC();
                        inkCanvas.InkPresenter.IsInputEnabled = vm.IsEditingInk;
                    }
                    if (ManipulationMode == ManipulationModes.All)
                    {
                        ManipulationMode = ManipulationModes.None;
                    }
                    else
                    {
                        ManipulationMode = ManipulationModes.All;
                    }
                }
            }
        }

    }
}