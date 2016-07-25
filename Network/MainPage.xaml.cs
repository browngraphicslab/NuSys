using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Network
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MessageWebSocket socket;

        public MainPage()
        {
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                Debug.WriteLine("Deleting Background Task " + cur.Value.Name);
                cur.Value.Unregister(true);
            }

            this.InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

            await BackgroundExecutionManager.RequestAccessAsync();


            /*
            var myTaskBuilder = new BackgroundTaskBuilder();
            var myTrigger = new SystemTrigger(SystemTriggerType.NetworkStateChange, false);
            myTaskBuilder.SetTrigger(myTrigger);
            myTaskBuilder.TaskEntryPoint = "BackgroundTasks.NetworkChangeTask";
            myTaskBuilder.Name = "Network change task";
            var myTask = myTaskBuilder.Register();
            */
           
            //await RegisterWithCCTHelper("wss://nusysrepo.azurewebsites.net/api/values/a3ebb9f0-906d-4b55-ae05-53a6ad4d5a30");
            await RegisterWithCCTHelper("wss://nusysrepo.azurewebsites.net/api/values/a12a5964-c45f-4d86-bd05-6f8be8d69b99");
        }



        async Task<bool> RegisterWithCCTHelper(string serverUri)
        {
            bool result = false;
            socket = new MessageWebSocket();
            socket.MessageReceived += Socket_MessageReceived;
            socket.Closed += SocketOnClosed;
            socket.Control.MessageType = SocketMessageType.Utf8;
            
            // Specify the keepalive interval expected by the server for this app
            // in order of minutes.
            const int serverKeepAliveInterval = 30;

            // Specify the channelId string to differentiate this
            // channel instance from any other channel instance.
            // When background task fires, the channel object is provided
            // as context and the channel id can be used to adapt the behavior
            // of the app as required.
            const string channelId = "channelOne";

            // For websockets, the system does the keepalive on behalf of the app
            // But the app still needs to specify this well known keepalive task.
            // This should be done here in the background registration as well 
            // as in the package manifest.
            const string WebSocketKeepAliveTask = "Windows.Networking.Sockets.WebSocketKeepAlive";

            // Try creating the controlchanneltrigger if this has not been already 
            // created and stored in the property bag.
            ControlChannelTriggerStatus status;

            // Create the ControlChannelTrigger object and request a hardware slot for this app.
            // If the app is not on LockScreen, then the ControlChannelTrigger constructor will 
            // fail right away.
            ControlChannelTrigger channel;
            try
            {
                channel = new ControlChannelTrigger(channelId, serverKeepAliveInterval,
                                           ControlChannelTriggerResourceType.RequestHardwareSlot);
            }
            catch (UnauthorizedAccessException exp)
            {
                Debug.WriteLine("Is the app on lockscreen? " + exp.Message);
                return result;
            }

            Uri serverUriInstance;
            try
            {
                serverUriInstance = new Uri(serverUri);
            }
            catch (Exception exp)
            {
                Debug.WriteLine("Error creating URI: " + exp.Message);
                return result;
            }

            // Register the apps background task with the trigger for keepalive.
            var keepAliveBuilder = new BackgroundTaskBuilder();
            keepAliveBuilder.Name = "KeepaliveTaskForChannelOne";
            keepAliveBuilder.TaskEntryPoint = WebSocketKeepAliveTask;
            keepAliveBuilder.SetTrigger(channel.KeepAliveTrigger);
            keepAliveBuilder.Register();

            /*
            // Register the apps background task with the trigger for push notification task.
            var pushNotifyBuilder = new BackgroundTaskBuilder();
            pushNotifyBuilder.Name = "PushNotificationTaskForChannelOne";
            pushNotifyBuilder.TaskEntryPoint = "Background.PushNotifyTask";
            pushNotifyBuilder.SetTrigger(channel.PushNotificationTrigger);
            pushNotifyBuilder.Register();
            */

            // Tie the transport method to the ControlChannelTrigger object to push enable it.
            // Note that if the transport' s TCP connection is broken at a later point of time,
            // the ControlChannelTrigger object can be reused to plug in a new transport by
            // calling UsingTransport API again.
            try
            {
                channel.UsingTransport(socket);

                // Connect the socket
                //
                // If connect fails or times out it will throw exception.
                // ConnectAsync can also fail if hardware slot was requested
                // but none are available
                await socket.ConnectAsync(serverUriInstance);

                Debug.WriteLine("CONNECTED.");

                // Call WaitForPushEnabled API to make sure the TCP connection has 
                // been established, which will mean that the OS will have allocated 
                // any hardware slot for this TCP connection.
                //
                // In this sample, the ControlChannelTrigger object was created by 
                // explicitly requesting a hardware slot.
                //
                // On systems that without connected standby, if app requests hardware slot as above, 
                // the system will fallback to a software slot automatically.
                //
                // On systems that support connected standby,, if no hardware slot is available, then app 
                // can request a software slot by re-creating the ControlChannelTrigger object.
                status = channel.WaitForPushEnabled();
                if (status != ControlChannelTriggerStatus.HardwareSlotAllocated
                    && status != ControlChannelTriggerStatus.SoftwareSlotAllocated)
                {
                    throw new Exception(string.Format("Neither hardware nor software slot could be allocated. ChannelStatus is {0}", status.ToString()));
                }

                // Store the objects created in the property bag for later use.
                CoreApplication.Properties.Remove(channel.ControlChannelTriggerId);

           //     var appContext = new AppContext(this, socket, channel, channel.ControlChannelTriggerId);
           //     ((IDictionary<string, object>)CoreApplication.Properties).Add(channel.ControlChannelTriggerId, appContext);
                result = true;

                // Almost done. Post a read since we are using streamwebsocket
                // to allow push notifications to be received.
               // PostSocketRead(MAX_BUFFER_LENGTH);
            }
            catch (Exception exp)
            {
                Debug.WriteLine("RegisterWithCCTHelper Task failed with: " + exp.Message);

                // Exceptions may be thrown for example if the application has not 
                // registered the background task class id for using real time communications 
                // broker in the package manifest.
            }
            return result;
        }

        private void SocketOnClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
        }

        private void Socket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (DataReader reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    string read = reader.ReadString(reader.UnconsumedBufferLength);
                    Debug.WriteLine(read + "\r\n");

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("couldn't get reader");
            }
        }

    }
}
