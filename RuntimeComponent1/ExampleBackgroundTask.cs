using System;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Media.Imaging;


namespace RuntimeComponent1
{

    public sealed  class ExampleBackgroundTask : XamlRenderingBackgroundTask //ignore the error, it will still compile

    {
        /// <summary>
        /// Executed when the thread is spawned. Extracts byteString from trigger details, converts it to a byte array, and uses a helper methods to write a bitmap image and save it as a storage file. MAKE SURE TO REGISTER THIS CLASS IN THE NUSYSAPP MANIFEST!!!!
        /// 
        /// TODO: FILL IN HELPER METHODS
        /// </summary>
        /// <param name="taskInstance"></param>
        protected override void OnRun(Windows.ApplicationModel.Background.IBackgroundTaskInstance taskInstance)
        {

            // Obtain the deferral, which needs to be marked as complete at end of method
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            // Obtain the byteString from the TriggerDetails
            var details = taskInstance.TriggerDetails as ApplicationTriggerDetails;
            object byteString;
            details.Arguments.TryGetValue("data", out byteString);

            // Generate a byte array from the byte string and render the bitmap image based on the byte array
            var byteArray = Convert.FromBase64String((string)byteString);
            this.RenderBitmapImageFromByteArray(byteArray);
       
            deferral.Complete();
        }


        // Renders a bitmap image based on the passed in byte array, then calls another method to save it as a storage file
        private void RenderBitmapImageFromByteArray(byte[] byteArray)
        {
            BitmapImage myImage = new BitmapImage();
            // TODO: render bitmap image

            this.SaveImageAsStorageFile(myImage);
        }

        // Saves the passed in BitmapImage as a storage file
        private void SaveImageAsStorageFile(BitmapImage myImage)
        {
            // TODO: save bitmap image as storage file
        }
    }

}