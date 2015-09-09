namespace NuSysApp
{
    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    /// <summary>
    /// Allows to wait for a task to be on the UI thread and waited for.
    /// Based on http://briandunnington.github.io/uitask.html
    /// </summary>
    public static class UITask
    {
        private static CoreDispatcher dispatcher;

        public static Task Run(Action action)
        {
            if (dispatcher == null)
                dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            var tcs = new TaskCompletionSource<bool>();
            var ignore = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }

        public static Task Run(Task task)
        {
            return Run(() => task);
        }

        public static Task Run(Func<Task> taskFunc)
        {
            if (dispatcher == null)
                dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            var tcs = new TaskCompletionSource<bool>();
            var ignore = dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    await taskFunc();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }
    }
}
