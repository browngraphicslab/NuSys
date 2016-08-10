using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Windows.UI.Xaml;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// can be used to update properties for both LibraryElements and Nodes
    /// </summary>
    public class DebouncingDictionary
    {
        /// <summary>
        /// this dictionary will store properties that need to be updated not but necessarily stored on the server.  
        /// 
        /// </summary>
        private ConcurrentDictionary<string, object> _dict; 

        /// <summary>
        /// tells us whether we are in the middle of an update session of properties for this object
        /// </summary>
        private bool _timing = false;

        /// <summary>
        /// The timer which will keep track of the next time to send a non-saving request
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// the time between each non-saving message sent to the server
        /// </summary>
        private int _milliSecondDebounce = 30;

        /// <summary>
        /// the id of the object this debouncing dictionary updates
        /// </summary>
        private string _id;

        /// <summary>
        /// whether this debouncing dictionary points to a library element or an alias
        /// </summary>
        private bool _updateLibraryElement = false;


        /// <summary>
        /// the dictionary of proprties that will be saved after the save delay timer expires
        /// </summary>
        private ConcurrentDictionary<string, object> _serverDict;

        /// <summary>
        /// the delay that the debouncing dicitonary must be left alone before a saving update 
        /// </summary>
        private int _milliSecondServerSaveDelay = 800;

        /// <summary>
        /// the time that will send out a saving request every time it expires
        /// </summary>
        private Timer _serverSaveTimer;

        /// <summary>
        /// this constructor takes in the id of the alias or library element we are updating.  
        /// It also takes ina bool indicating if it is updating an alias or library element
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateLibraryElement"></param>
        public DebouncingDictionary(string id, bool updateLibraryElement = false)
        {
            _timer = new Timer(SendMessage, false, Timeout.Infinite, Timeout.Infinite);
            _serverSaveTimer = new Timer(SendMessage, true, Timeout.Infinite, Timeout.Infinite);
            _dict = new ConcurrentDictionary<string, object>();
            _serverDict = new ConcurrentDictionary<string, object>();
            _id = id;
            _updateLibraryElement = updateLibraryElement;
        }

        /// <summary>
        /// this constructor is just like the main one, except with a custom timer timeout value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="milliSecondDebounce"></param>
        /// <param name="updateLibraryElement"></param>
        public DebouncingDictionary(string id, int milliSecondDebounce, bool updateLibraryElement = false)
        {
            _timer = new Timer(SendMessage, false, Timeout.Infinite, Timeout.Infinite);
            _serverSaveTimer = new Timer(SendMessage, true, Timeout.Infinite, Timeout.Infinite);
            _dict = new ConcurrentDictionary<string, object>();
            _serverDict = new ConcurrentDictionary<string, object>();
            _milliSecondDebounce = _milliSecondDebounce;
            _id = id;
            _updateLibraryElement = updateLibraryElement;
        }


        /// <summary>
        /// call this method to add an object to be updated on other clients.
        /// This should use the same keys you would use if you were creating a new request with that property.  
        /// The update will happen within *debouncing_time* milliseconds max.  
        /// Save will occur *delay_time* milliseconds after interactions stop.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void Add(string id, object value)
        {
            //if we are not already timing
            if (!_timing)
            {
                //start timeing
                _timing = true;
                _dict.TryAdd(id, value);
                //add to the dictionary and server dictionary the latest values
                if (_serverDict.ContainsKey(id))
                {
                    _serverDict[id] = value;
                }
                else
                {
                    _serverDict.TryAdd(id, value);
                }
                //update the timers to start timing
                _timer?.Change(_milliSecondDebounce, _milliSecondDebounce);
                _serverSaveTimer?.Change(_milliSecondServerSaveDelay, _milliSecondServerSaveDelay);
            }
            else
            {
                //add the values to the dictionary
                if (_dict.ContainsKey(id))
                {
                    _dict[id] = value;
                    _serverDict[id] = value;
                }
                else
                {
                    _dict.TryAdd(id, value);
                    _serverDict.TryAdd(id, value);
                }
                //only update the save timer to reset its timeout
                _serverSaveTimer?.Change(_milliSecondServerSaveDelay, _milliSecondServerSaveDelay);
            }
        }

        /// <summary>
        /// called whenever either timer expires.  
        /// the state is a boolean representing whether the request should save
        /// </summary>
        /// <param name="state"></param>
        private async void SendMessage(object state)
        {
            bool saveToServer = (bool) state;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            Dictionary<string, object> d;
            if (saveToServer)
            {
                d = _serverDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                _serverDict.Clear();
                _serverSaveTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                d = _dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            if (!_updateLibraryElement && d.ContainsKey(NusysConstants.ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY))
            {
                Debug.WriteLine("Debounce dictionary had a previous 'id' value.  It was overritten with the original ID");
            }
            if(_updateLibraryElement && d.ContainsKey("contentId"))
            {
                Debug.WriteLine("Debounce dictionary had a previous 'contentId' value.  It was overritten with the original ID");
            }
            d[_updateLibraryElement ? "contentId":NusysConstants.ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY] = _id;
            var message = new Message(d);
            if (d.Count > 1)
            {
                Request request;
                if (_updateLibraryElement)
                {
                    request = new UpdateLibraryElementModelRequest(message);
                }
                else
                {
                    request = new ElementUpdateRequest(message, saveToServer);
                }
                SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            }
            _timing = false;
            _dict.Clear();
        }
    }
}
