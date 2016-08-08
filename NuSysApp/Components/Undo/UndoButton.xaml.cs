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
    /// <summary>
    /// Primarily used for the library, where the undo button must be 
    /// "inactive" at most points, when there is nothing to undo
    /// </summary>
    public enum UndoButtonState
    {
        Active,InActive
    }

    /// <summary>
    /// Describes logic behind the UndoButton, which gives the user 6 seconds to do a single undo action.
    /// When the user wants to undo something, an UndoButton will take use the OriginalAction to generate
    /// a logical inverse action, and will then execute that inverse action.
    /// Note that there are two constructors -- one for a general undo button, and another for an undo button
    /// specifically in a FreeformViewer
    /// </summary>
    public sealed partial class UndoButton : AnimatableUserControl, IDisposable
    {
        public IUndoable OriginalAction { get; set; }
        public UndoButtonState State { get; set; }

        private Timer _timer;


        /// <summary>
        /// Creates a general undo button. Sets up UI, keeps track of original action, and calls a method to start a 6-second timer
        /// </summary>
        /// <param name="originalAction"></param>
        /// <param name="state"></param>
        public UndoButton(IUndoable originalAction, UndoButtonState state)
        {
            this.InitializeComponent();
            OriginalAction = originalAction;
            State = state;
            Loaded += UndoButton_Loaded;
        }

        /// <summary>
        /// Creates an UndoButton, puts it in the passed in freeform viewer, and moves it to the passed in point.
        /// </summary>
        /// <param name="originalAction"></param>
        /// <param name="ffvm"></param>
        /// <param name="originalPosition"></param>
        /// <param name="state"></param>
        public UndoButton(IUndoable originalAction, FreeFormViewerViewModel ffvm, Point position, UndoButtonState state)
        {
            // Sets up UI and instance varialbes, then adds handler
            this.InitializeComponent();
            OriginalAction = originalAction;
            State = state;
            Loaded += UndoButton_Loaded;

            // Add to workspace and move the button to the specified position
            ffvm.AtomViewList.Add(this);
            this.MoveTo(position);
        }

        /// <summary>
        /// Moves the undo button to position of the passed in point
        /// </summary>
        /// <param name="point"></param>
        private void MoveTo(Point point)
        {
            var transform = new TranslateTransform() {X=point.X, Y = point.Y};
            this.RenderTransform = transform;
        }

        /// <summary>
        /// Instantiates a 6-second timer that will prompt the undo button to delete itself when the
        /// timer ticks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoButton_Loaded(object sender, RoutedEventArgs e)
        {
            // Weird syntax since TimerCallback can't directly take in Dispose().
            _timer = new Timer(new TimerCallback(delegate(object state)
            {
                Dispose();
            }), null, 6000, Timeout.Infinite);
        }

       
        /// <summary>
        /// When the button is tapped, find and then do the logical inverse of the original
        /// action. Then remove the button from the workspace.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var undoAction = OriginalAction.GetInverse();
            undoAction.ExecuteAction();
            Dispose();
        }

        /// <summary>
        /// Removes button from workspace and removes handlers
        /// </summary>
        public async void Dispose()
        {
            await UITask.Run(
                delegate
                {
                    var wvm = SessionController.Instance.ActiveFreeFormViewer;
                    wvm.AtomViewList.Remove(this);

                    Loaded -= UndoButton_Loaded;
                });
            _timer.Dispose();
        }

      
    }
}
