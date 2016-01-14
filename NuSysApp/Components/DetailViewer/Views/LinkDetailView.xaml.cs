using Windows.UI;
using System;
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
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Media.Animation;
using Windows.Media.SpeechSynthesis;
using Windows.Media.SpeechRecognition;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    
    public sealed partial class LinkDetailView : AnimatableUserControl
    {
        private SpeechRecognizer _recognizer;
        private bool _isRecording;
        private FreeFormNodeViewFactory _factory;

        public LinkDetailView(LinkViewModel vm)
        {

            InitializeComponent();
            DataContext = vm;

            _factory = new FreeFormNodeViewFactory();

            this.AddChildren();

        }

        public async Task AddChildren()
        {
            var vm = (LinkViewModel) DataContext;
            var atomvm1 = vm.Atom1;
            var atomvm2 = vm.Atom2;

            var atomview1 = await _factory.CreateFromSendable(atomvm1.Model, null);
            var atomview2 = await _factory.CreateFromSendable(atomvm2.Model, null);
            //var linkview = await _factory.CreateFromSendable(vm.Model, new List<FrameworkElement> {atomview1, atomview2});

            atomview1.RenderTransform = new CompositeTransform();
            atomview2.RenderTransform = new CompositeTransform();
            atomview1.IsHitTestVisible = false;
            atomview2.IsHitTestVisible = false;
            //linkview.RenderTransform = new CompositeTransform();

            //Canvas.SetLeft(atomview1, 0);
            Canvas.SetLeft(atomview2, xCanvas.ActualWidth - atomview2.Width);
            Canvas.SetTop(atomview2, xCanvas.ActualHeight - atomview2.Height);

            xCanvas.Children.Add(atomview1);
            xCanvas.Children.Add(atomview2);
            //xGrid.Children.Add(linkview);

        }

    }
}