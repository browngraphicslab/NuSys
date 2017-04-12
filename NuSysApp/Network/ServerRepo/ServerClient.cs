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
        public enum ConnectionStrength
        {
            UnResponsive = 30000,//30 second timeout
            Terrible = 1000,
            Bad = 275,
            Okay = 120,
            Good = 80,
        }

        private MessageWebSocket _socket;
        private DataWriter _dataMessageWriter;

        private int _delayMilliseconds = 18;
        

        public double CurrentPing
        {
            get { return _queue.Any() ? _queue.Average()*_delayMilliseconds : (int)ConnectionStrength.UnResponsive; }
        }

        public event EventHandler<ConnectionStrength> ConnectionStrenthChanged;  

        /// <summary>
        /// Not constant-time getter for the current connection strength
        /// </summary>
        public ConnectionStrength Connection
        {
            get
            {
                var ping = CurrentPing;
                if (ping < (int) ConnectionStrength.Good)
                {
                    return ConnectionStrength.Good;
                }
                if (ping < (int)ConnectionStrength.Okay)
                {
                    return ConnectionStrength.Okay;
                }
                if (ping < (int)ConnectionStrength.Bad)
                {
                    return ConnectionStrength.Bad;
                }
                if (ping < (int)ConnectionStrength.Terrible)
                {
                    return ConnectionStrength.Terrible;
                }
                return ConnectionStrength.UnResponsive;
                ;
            }
        }

        public delegate void MessageRecievedEventHandler(Message message);
        public event MessageRecievedEventHandler OnMessageRecieved;

        /// <summary>
        /// event fired with a notification's message whenever a new notification comes in
        /// </summary>
        /// <param name="m"></param>
        public delegate void OnContentUpdatedEventHandler(Message notificationMessage);
        public event OnContentUpdatedEventHandler OnNewNotification;

        public static HashSet<string> NeededLibraryDataIDs = new HashSet<string>();
        private ConcurrentDictionary<string,Message> _returnMessages = new ConcurrentDictionary<string, Message>();
        private ConcurrentDictionary<string, byte> _requestEventDictionary = new ConcurrentDictionary<string, byte>();
        private ConcurrentDictionary<string, CallbackRequest<ServerRequestArgsBase, ServerReturnArgsBase>> _callbackDictionary;

        private ConcurrentFixedQueue<int> _queue;

        private ConnectionStrength _currentStrength = ConnectionStrength.Good;

        /// <summary>
        /// queue used to track server response times
        /// </summary>
        private ConcurrentQueue<int> _statusQueue;
        public string ServerBaseURI { get; private set; }
        public ServerClient()
        {
            _queue = new ConcurrentFixedQueue<int>(25);
        }

        /// <summary>
        /// method to gracefully close the connection to the server.
        /// </summary>
        public void CloseConnection()
        {
            _socket.Close(1000,"done"); //1000 means a graceful exit
        }

        public async Task Init()
        {
            _socket = new MessageWebSocket();
            _callbackDictionary = new ConcurrentDictionary<string, CallbackRequest<ServerRequestArgsBase, ServerReturnArgsBase>>();

            ServerBaseURI = "://" + NusysConstants.ServerName + "/api/";
            var credentials = GetUserCredentials();
            var uri = GetUri("nusysconnect/" + credentials, true);

            _socket.Control.MessageType = SocketMessageType.Utf8;
            _socket.MessageReceived += MessageRecieved;
            _socket.Closed += SocketClosed;

            _dataMessageWriter = new DataWriter(_socket.OutputStream);

            await _socket.ConnectAsync(uri);
        }

        private string GetUserCredentials()
        {
            return WaitingRoomView.ServerSessionID?.ToString();
        }

        private Uri GetUri(string additionToBase, bool useWebSocket = false)
        {
            var firstpart = useWebSocket ? "ws" : "http";
            firstpart += NusysConstants.TEST_LOCAL_BOOLEAN ? "" : "s";
            return new Uri(firstpart + ServerBaseURI + additionToBase);
        }



        private void SocketClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            //throw new Exception("Server client failed from web socket closing!");
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

                    await HandleIncomingMessage(read);

                }
            }
            catch (Exception e)
            {
                 throw new Exception("connection to server failed");
            }
        }

        private async Task HandleIncomingMessage(string read)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
            };

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
                //var message = dict[NusysConstants.REQUEST_ERROR_MESSAGE_KEY].ToString();
                if (dict.ContainsKey(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING))
                //if we can untangle a waiting request
                {
                    byte outByte;
                    _requestEventDictionary.TryRemove(
                        dict[NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING].ToString(),
                        out outByte);

                    CallbackRequest<ServerRequestArgsBase, ServerReturnArgsBase> outReq;
                    _callbackDictionary.TryRemove(dict[NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING].ToString(),
                        out outReq);
                    var callbackSuccess = outReq?.ExecuteCallback(false);
                    Debug.Assert(callbackSuccess != false);
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
        }

        public async Task SendMessageToServer(Message message)
        {
            message["system_sender_ip"] = WaitingRoomView.UserID;
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
        /// method to call to execute a callback request
        /// </summary>
        /// <param name="request"></param>
        public void ExecuteCallbackRequest(CallbackRequest<ServerRequestArgsBase, ServerReturnArgsBase> request)
        {
            var id = SessionController.Instance.GenerateId();
            _callbackDictionary[id] = request;
            request.CheckOutgoingRequest();
            SendMessageToServer(request.GetFinalMessage());
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

            _requestEventDictionary.TryAdd(mreId, 0);

            await SendMessageToServer(message).ConfigureAwait(false);

            int attempt = 0;

            while (_requestEventDictionary.ContainsKey(mreId))
            {
                attempt++;
                await Task.Delay(_delayMilliseconds);
                if (attempt*_delayMilliseconds > (int) ConnectionStrength.UnResponsive)
                {
                    ConnectionStrenthChanged?.Invoke(this, ConnectionStrength.UnResponsive);
                    _currentStrength = ConnectionStrength.UnResponsive;
                    _returnMessages.TryAdd(mreId,new Message()
                    {
                        {NusysConstants.REQUEST_SUCCESS_BOOL_KEY,false.ToString() }
                    });
                    break;
                }
            }

            _queue.EnQueue(attempt);

            RunPingAnalysis();

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
        /// private method to analyze the curent server delay
        /// </summary>
        private void RunPingAnalysis()
        {
            var strength = Connection;
            if (_currentStrength != strength)
            {
                ConnectionStrenthChanged?.Invoke(this,strength);
                _currentStrength = strength;
            }
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
            var id = message.GetString(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);

            if (_requestEventDictionary.ContainsKey(id))
            {
                byte outByte;
                _returnMessages.TryAdd(id, message);
                _requestEventDictionary.TryRemove(id, out outByte);
            }
            else if (_callbackDictionary.ContainsKey(id))
            {
                CallbackRequest < ServerRequestArgsBase, ServerReturnArgsBase > request;
                _callbackDictionary.TryRemove(id, out request);
                request.SetReturnMessage(message);
                var callbackSuccess = request?.ExecuteCallback(true);
                Debug.Assert(callbackSuccess != false);
            }
            else
            {
                Debug.Fail("shouldn't be here");
            }
        }
        

        public class IncomingDataReaderException : Exception
        {
            public IncomingDataReaderException(string s = "") : base("Error with incoming data reader message.  " + s) { }
        }
    }
}