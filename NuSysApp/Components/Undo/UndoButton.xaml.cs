﻿using System;
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
        ActiveUndo, ActiveRedo, Inactive
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
        public UndoButtonState State { get { return _state; } }

        //Whether the action was just executed. This is useful for calling things right after the action was executed (e.g., stopping manipulation delta caused by inertia after move undo)
        public bool ActionExecuted { set; get; }

        private Timer _timer;
        private UndoButtonState _state;

        /// <summary>
        /// Creates a general undo button. No input parameters so UndoButtons can be added in XAML.
        /// </summary>
        /// <param name="originalAction"></param>
        /// <param name="state"></param>
        public UndoButton()
        {
            this.InitializeComponent();
            _state=UndoButtonState.Inactive;
            ActionExecuted = false;

        }

        /// <summary>
        /// Moves the undo button to position of the passed in point
        /// </summary>
        /// <param name="point"></param>
        public void MoveTo(Point point)
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
        /// Activates the button by switching the state, adding the action reference, and starting the timer
        /// </summary>
        public void Activate(IUndoable action)
        {
            if(_state == UndoButtonState.Inactive)
            {
                _state = UndoButtonState.ActiveUndo;
            }else
            {
                _state = (_state == UndoButtonState.ActiveUndo) ? UndoButtonState.ActiveRedo : UndoButtonState.ActiveUndo;
            }

            Visibility = Visibility.Visible;
            OriginalAction = action;

            // Weird syntax since TimerCallback can't directly take in Dispose().
            _timer = new Timer(new TimerCallback(delegate (object state)
            {
                Dispose();
            }), null, 6000, Timeout.Infinite);

           
        }


        /// <summary>
        /// Deactivates button by removing timer and setting appropriate references
        /// </summary>
        public void Deactivate()
        {
            _state=UndoButtonState.Inactive;
            Visibility = Visibility.Collapsed;
            _timer?.Dispose();
        }
       
        /// <summary>
        /// When the button is tapped, find and then do the logical inverse of the original
        /// action. Then remove the button from the workspace.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UndoButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // Only conitinue if the button is active
            if (_state == UndoButtonState.Inactive)
            {
                return;
            }

            OriginalAction =  OriginalAction.GetInverse();
            OriginalAction.ExecuteAction();

            ActionExecuted = true;

            Activate(OriginalAction);
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
                    if (wvm.AtomViewList.Contains(this))
                    {
                        wvm.AtomViewList.Remove(this);
                        Loaded -= UndoButton_Loaded;
                        UndoCircle.Tapped -= UndoButton_OnTapped;
                    }
                    else
                    {
                       this.Deactivate();
                    }
                   
                });

            if (_timer!=null)
            {
                _timer.Dispose();
            }
           
        }

      
    }
}
