﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using CommModule;

namespace BgTasks
{

    // This class illustrates one way to set up a RTC enabled transport when 
    // a system event (such as network state change) occurs.
    public sealed class NetworkChangeTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null)
            {
                Debug.WriteLine("NetworkChangeTask: taskInstance was null");
                return;
            }

            // In this example, the channel name has been hardcoded to lookup the property bag
            // for any previous contexts. The channel name may be used in more sophisticated ways
            // in case an app has multiple controlchanneltrigger objects.
            string channelId = "channelOne";
            if (((IDictionary<string, object>)CoreApplication.Properties).ContainsKey(channelId))
            {
                try
                {
                    var appContext = ((IDictionary<string, object>)CoreApplication.Properties)[channelId] as AppContext2;
                    if (appContext != null && appContext.CommInstance != null)
                    {
                        CommModule.CommModule commInstance = appContext.CommInstance;

                        // Clear any existing channels, sockets etc.
                        commInstance.Reset();

                        // Create RTC enabled transport
                        commInstance.SetupTransport(commInstance.serverUri);
                    }
                }
                catch (Exception exp)
                {
                    Debug.WriteLine("Registering with RTC broker failed with: " + exp.Message);
                }
            }
            else
            {
                Debug.WriteLine("Cannot find AppContext key channelOne");
            }

            Debug.WriteLine("Systemtask - " + taskInstance.Task.Name + " finished.");
        }
    }

    public sealed class PushNotifyTask : IBackgroundTask
    {
        void InvokeSimpleToast(string messageReceived)
        {
            // GetTemplateContent returns a Windows.Data.Xml.Dom.XmlDocument object containing
            // the toast XML
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            // You can use the methods from the XML document to specify all of the
            // required parameters for the toast
            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements.Item(0).AppendChild(toastXml.CreateTextNode("Push notification message:"));
            stringElements.Item(1).AppendChild(toastXml.CreateTextNode(messageReceived));

            // Audio tags are not included by default, so must be added to the
            // XML document
            string audioSrc = "ms-winsoundevent:Notification.IM";
            XmlElement audioElement = toastXml.CreateElement("audio");
            audioElement.SetAttribute("src", audioSrc);

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            toastNode.AppendChild(audioElement);

            // Create a toast from the Xml, then create a ToastNotifier object to show
            // the toast
            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public void Run(Windows.ApplicationModel.Background.IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null)
            {
                Debug.WriteLine("PushNotifyTask: taskInstance was null");
                return;
            }

            Debug.WriteLine("PushNotifyTask " + taskInstance.Task.Name + " Starting...");

            // Use the ControlChannelTriggerEventDetails object to derive the context for this background task.
            // The context happens to be the channelId that apps can use to differentiate between
            // various instances of the channel..
            var channelEventArgs = taskInstance.TriggerDetails as IControlChannelTriggerEventDetails;

            ControlChannelTrigger channel = channelEventArgs.ControlChannelTrigger;
            if (channel == null)
            {
                Debug.WriteLine("Channel object may have been deleted.");
                return;
            }

            string channelId = channel.ControlChannelTriggerId;

            if (((IDictionary<string, object>)CoreApplication.Properties).ContainsKey(channelId))
            {
                try
                {
                    string messageReceived = "PushNotification Received";
                    var appContext = ((IDictionary<string, object>)CoreApplication.Properties)[channelId] as AppContext2;

                    // Process any messages that have been enqueued by the receive completion handler.
                    bool result = AppContext2.messageQueue.TryDequeue(out messageReceived);
                    if (result)
                    {
                        Debug.WriteLine("Message: " + messageReceived);
                        InvokeSimpleToast(messageReceived);
                    }
                    else
                    {
                        Debug.WriteLine("There was no message for this push notification: ");
                    }
                }
                catch (Exception exp)
                {
                    Debug.WriteLine("PushNotifyTask failed with: " + exp.Message);
                }
            }
            else
            {
                Debug.WriteLine("Cannot find AppContext key " + channelId);
            }

            Debug.WriteLine("PushNotifyTask " + taskInstance.Task.Name + " finished.");
        }
    }
}