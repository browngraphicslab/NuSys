using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class UndoButton : AnimatableUserControl, IDisposable
    {
        public IUndoable OriginalAction { get; set; }

        private Timer _timer;
        public UndoButton(IUndoable originalAction, Point position)
        {
            this.InitializeComponent();
            OriginalAction = originalAction;


            // Moves the button to the appropriate location
            var transform = new TranslateTransform();
            transform.X = position.X -60;
            transform.Y = position.Y;
            this.RenderTransform = transform;

            Loaded += UndoButton_Loaded;


        }

        private void UndoButton_Loaded(object sender, RoutedEventArgs e)
        {
            //Timer is called only once and lasts 6000 miliseconds.
            _timer = new Timer(new TimerCallback(SelfDestruct), null, 6000, Timeout.Infinite);
        }

        private void SelfDestruct(object state)
        {
            Dispose();
            _timer.Dispose();
        }
        /// <summary>
        /// TODO: COMMENT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var undoAction = OriginalAction.GetInverse();
            undoAction.ExecuteRequest();
            
            Dispose();
        }

        public async void Dispose()
        {
            await UITask.Run(
                delegate
                {
                    var wvm = SessionController.Instance.ActiveFreeFormViewer;
                    wvm.AtomViewList.Remove(this);

                    Loaded -= UndoButton_Loaded;
                });
        }
    }
}
