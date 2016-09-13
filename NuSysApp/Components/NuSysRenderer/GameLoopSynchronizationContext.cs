using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class GameLoopSynchronizationContext : SynchronizationContext
    {
        ICanvasAnimatedControl control;


        // Constructor.
        public GameLoopSynchronizationContext(ICanvasAnimatedControl control)
        {
            this.control = control;
        }


        // Posts a single atomic action for asynchronous execution on the game loop thread.
        public override void Post(SendOrPostCallback callback, object state)
        {
            var action = control.RunOnGameLoopThreadAsync(() =>
            {
                // Re-register ourselves as the current synchronization context,
                // to work around CLR issues where this state can sometimes get nulled out.
                SynchronizationContext.SetSynchronizationContext(this);

                callback(state);
            });
        }


        // Runs an action, which could contain an arbitrarily complex chain of async awaits,
        // on the game loop thread. This helper registers a custom synchronization context
        // to make sure every await continuation in the chain remains on the game loop
        // thread, regardless of which thread the lower level async operations complete on.
        // It wraps the entire chain with a TaskCompletionSource in order to return a single
        // Task that will be signalled only when the whole chain has completed.
        public static async Task RunOnGameLoopThreadAsync(ICanvasAnimatedControl control, Func<Task> callback)
        {
            var completedSignal = new TaskCompletionSource<object>();

            await control.RunOnGameLoopThreadAsync(async () =>
            {
                try
                {
                    SynchronizationContext.SetSynchronizationContext(new GameLoopSynchronizationContext(control));

                    await callback();

                    completedSignal.SetResult(null);
                }
                catch (Exception e)
                {
                    completedSignal.SetException(e);
                }
            });

            await completedSignal.Task;
        }
    };
}
