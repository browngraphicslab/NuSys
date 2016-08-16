using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class ReadonlyFloatingMenuButtonView : UserControl
    {
        /// <summary>
        /// Allows button text "caption" to be set in xaml
        /// </summary>
        public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register("ButtonText", typeof(string), typeof(ReadonlyFloatingMenuButtonView), new PropertyMetadata(null));

        /// <summary>
        /// To modify the button caption, this property must be changed
        /// </summary>
        public string ButtonText {
            set
            {
                this.SetValue(ButtonTextProperty, value);
                xTextBox.Text = value;
            }
            get { return this.GetValue(ButtonTextProperty) as string; }
        }

        /// <summary>
        /// Returns if the button is active, aka when the action can be done
        /// </summary>
        public bool IsActive
        {
            get;set;
        }

        /// <summary>
        /// Describes the button on the readonly floating menu, which is displayed in readonly mode
        /// </summary>
        public ReadonlyFloatingMenuButtonView()
        {
            this.InitializeComponent();
            this.Deactivate();
        }

        /// <summary>
        /// Activates the button by modifying UI elements
        /// </summary>
        public void Activate()
        {
            this.IsActive = true;
            xContainer.Opacity = 1;
        }

        /// <summary>
        /// Deactivates the button and modifies UI elements
        /// </summary>
        public void Deactivate()
        {
            this.IsActive = false;
            xContainer.Opacity = .5;
        }
    }
}
