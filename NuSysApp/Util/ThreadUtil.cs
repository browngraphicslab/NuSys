using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;


namespace NuSysApp.Util
{
    class ThreadUtil
    {
        /// <summary>
        /// Renders image in background thread by taking in a bytestring, spawning a background thread, and sending the byteString to the background thread to render into an image.
        /// 
        /// TODO: Figure out why so many exceptions are thrown in spawning the thread and sending it the byte string
        /// </summary>
        /// <param name="byteString"></param>
        public static async void RenderImageInBackground(string byteString)
        {
            // trigger that will start immediately when background task registration is registered
            ApplicationTrigger trigger = new ApplicationTrigger();
            
            // the value set will be sent to the background thread when it is passed into the RequestAsync method
            ValueSet s = new ValueSet();
            s.Add(new KeyValuePair<string, object>("data", byteString));

            // spawn the background thread TODO: FIX MEMORY EXCEPTIONS that occur sometimes?
            var task = await RegisterBackgroundTask("RuntimeComponent1.ExampleBackgroundTask", "ExampleBackgroundTask", trigger);

           // Pass in value set so the background thread has access to the byteString TODO: figure out why we are getting an argument exception, especially since this was in fact valid before I refactored the code into this class
           await trigger.RequestAsync(s);
           
        }

        // Spawns a background thread by instantiating a BackgroundTaskBuilder and setting important properties. The thread is spawned when the BackgroundTaskRegistration is instantiated.
        private static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger)
        {
            // instantiate builder and set identifying properties
            var builder = new BackgroundTaskBuilder();
            builder.Name = name;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            // instantiate BackgroundTaskRegistration and spawn thread
            BackgroundTaskRegistration task = builder.Register();

            return task;
        }
    }
}
