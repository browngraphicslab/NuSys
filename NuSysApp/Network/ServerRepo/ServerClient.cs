using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Data.Xml.Dom;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using Windows.UI.Input.Inking;
using NusysIntermediate;

namespace NuSysApp
{
    public class ServerClient
    {
        private MessageWebSocket _socket;
        private DataWriter _dataMessageWriter;

        public delegate void MessageRecievedEventHandler(Message message);
        public event MessageRecievedEventHandler OnMessageRecieved;

        public delegate void LockAddedEventHandler(object sender, string id, string userId);
        public event LockAddedEventHandler OnLockAdded;

        public delegate void LockRemovedEventHandler(object sender, string id);
        public event LockRemovedEventHandler OnLockRemoved;

        /// <summary>
        /// event fired with a notification's message whenever a new notification comes in
        /// </summary>
        /// <param name="m"></param>
        public delegate void OnContentUpdatedEventHandler(Message notificationMessage);
        public event OnContentUpdatedEventHandler OnNewNotification;

        public static HashSet<string> NeededLibraryDataIDs = new HashSet<string>();
        private ConcurrentDictionary<string,Message> _returnMessages = new ConcurrentDictionary<string, Message>();
        private ConcurrentDictionary<string, ManualResetEvent> _requestEventDictionary = new ConcurrentDictionary<string, ManualResetEvent>();
        public string ServerBaseURI { get; private set; }
        

        public ServerClient()
        {
            _socket = new MessageWebSocket();
            _socket.Control.MessageType = SocketMessageType.Utf8;
            _socket.MessageReceived += MessageRecieved;
            _socket.Closed += SocketClosed;
        }

        /// <summary>
        /// method to gracefully close the connection to the server.
        /// </summary>
        public void CloseConnection()
        {
            _dataMessageWriter.DetachStream();
        }

        public async Task Init()
        {
            ServerBaseURI = "://" + WaitingRoomView.ServerName + "/api/";
            var credentials = GetUserCredentials();
            var uri = GetUri("nusysconnect/" + credentials, true);
            _dataMessageWriter = new DataWriter(_socket.OutputStream);

            try
            {
                await _socket.ConnectAsync(uri);
            }
            catch (Exception e)
            {
                _socket = new MessageWebSocket();
                _socket.Control.MessageType = SocketMessageType.Utf8;
                _socket.MessageReceived += MessageRecieved;
                _socket.Closed += SocketClosed;
                await _socket.ConnectAsync(uri);
            }
        }

        private string GetUserCredentials()
        {
            return WaitingRoomView.ServerSessionID.ToString();
        }

        private Uri GetUri(string additionToBase, bool useWebSocket = false)
        {
            var firstpart = useWebSocket ? "ws" : "http";
            firstpart += NusysConstants.TEST_LOCAL_BOOLEAN ? "" : "s";
            return new Uri(firstpart + ServerBaseURI + additionToBase);
        }



        private void SocketClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            throw new Exception("Server client failed from web socket closing!");
        }

        /// <summary>
        /// method called by the network whenever we get a message from the server.  
        /// These messages can be notifications, returne requests, error messages, or other-client messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void MessageRecieved(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (DataReader reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    string read = reader.ReadString(reader.UnconsumedBufferLength);
                    //Debug.WriteLine(read + "\r\n");
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
                    };
                    Task.Run(async delegate
                    {
                        var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(read, settings);
                        string id = null;
                        if (dict.ContainsKey(NusysConstants.NOTIFICATION_TYPE_STRING_KEY)) //if this is a notification
                        {
                            OnNewNotification?.Invoke(new Message(dict));
                        }
                        else if (dict.ContainsKey(NusysConstants.REQUEST_ERROR_MESSAGE_KEY))
                            //if this is an error notification
                        {
                            Debug.WriteLine("  ******************* BEGIN SERVER ERROR MESSAGE *******************  ");
                            Debug.WriteLine(dict[NusysConstants.REQUEST_ERROR_MESSAGE_KEY].ToString());
                            Debug.WriteLine("  *******************  END SERVER ERROR MESSAGE  *******************  ");
                            if (dict.ContainsKey(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING))
                                //if we can untangle a waiting request
                            {
                                ManualResetEvent outMre;
                                _requestEventDictionary.TryRemove(
                                    dict.ContainsKey(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING).ToString(),
                                    out outMre);
                                outMre?.Set();
                            }
                        }
                        else if (dict.ContainsKey(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING))
                            //if we are getting the return of an awaiting request
                        {
                            await ReturnRequestAsync(new Message(dict));
                        }
                        else //else it must be a regular mesage from another client
                        {
                            OnMessageRecieved?.Invoke(new Message(dict));
                        }
                    });

                }
            }
            catch (Exception e)
            {
                SessionController.Instance.CaptureCurrentState();
            }
        }

        public async Task SendMessageToServer(Message message)
        {
            var serialized = message.GetSerialized();
            await SendToServer(serialized);
        }
        private async Task SendToServer(string message)
        {
            try
            {
                _dataMessageWriter.WriteString(message);
                await _dataMessageWriter.StoreAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Exception caught during writing to server data writer.  Reason: " + e.Message);
            }
        }

        /// <summary>
        /// Returns the byte array that should be written directly into a file for docx saving and loading
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<byte[]> GetDocxBytes(string id)
        {
            var url = GetUri("getworddoc/" + id);
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            string data;
            using (var content = response.Content)
            {
                data = await content.ReadAsStringAsync();
            }
            var list = JsonConvert.DeserializeObject<List<string>>(data);
            var converted = Convert.FromBase64String(list[0]);
            return converted;
        }


        /// <summary>
        /// Will send a dictionary to the server and manually wait for its return
        /// Later, another message will be called that will resumet this thread after placing the returned response in the _returnMessages dictionary
        /// THESE METHOD PAIRS SHOULD SIMULATE ACTUAL ASYNCHRONOUS CALLS
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<Message> WaitForRequestRequestAsync(Message message)
        {
            Debug.Assert(!message.ContainsKey(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING));
            var mreId = SessionController.Instance.GenerateId();
            message[NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING] = mreId;
            var mre = new ManualResetEvent(false);
            _requestEventDictionary.TryAdd(mreId, mre);

            Task.Run(async delegate
            {
                SendMessageToServer(message);
            });
            mre.WaitOne();
            if (!_returnMessages.ContainsKey(mreId))
            {
                return null;//only does this if the request failed
            }
            Message outMessage;
            _returnMessages.TryRemove(mreId, out outMessage);
            Debug.Assert(outMessage != null);
            return outMessage;
        }

        /// <summary>
        /// will be called when a message is recieved and is a get request
        /// will resume the waiting thread for the get request and place the message in the message dictionary
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task ReturnRequestAsync(Message message)
        {
            Debug.Assert(message.ContainsKey(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING));
            var mreId = message.GetString(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
            Debug.Assert(_requestEventDictionary.ContainsKey(mreId));
            ManualResetEvent mre;
            _requestEventDictionary.TryRemove(mreId, out mre);
            Debug.Assert(mre != null);
            _returnMessages.TryAdd(mreId, message);
            mre?.Set();
        }
        

        public class IncomingDataReaderException : Exception
        {
            public IncomingDataReaderException(string s = "") : base("Error with incoming data reader message.  " + s) { }
        }
    }
}