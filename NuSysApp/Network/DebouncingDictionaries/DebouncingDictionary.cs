using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Debouncing Dictionary are a convenient way to make server calls to update properties of objects without spamming the server.  
    /// Debouncing dictionaries take arbitrary key value pairs for properties and then will occasionally make calls to update the server with the LATEST value for every key added.
    /// The idea is that when some things are added to this dictionary many times per millisecond, we still won't spam the server with that many calls.
    /// Instead, we will update the server with the most recent values of each property.  
    /// To add things that you want to update to this dictioary, use the "Add" method.  
    /// The keys in the add method will be the keys used in the Sql Table for the given object;
    /// </summary>
    public abstract class DebouncingDictionary
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
        public DebouncingDictionary(string id)
        {
            _timer = new Timer(SendMessage, false, Timeout.Infinite, Timeout.Infinite);
            _serverSaveTimer = new Timer(SendMessage, true, Timeout.Infinite, Timeout.Infinite);
            _dict = new ConcurrentDictionary<string, object>();
            _serverDict = new ConcurrentDictionary<string, object>();
            _id = id;
        }

        /// <summary>
        /// this constructor is just like the main one, except with a custom timer timeout value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="milliSecondDebounce"></param>
        /// <param name="updateLibraryElement"></param>
        public DebouncingDictionary(string id, int milliSecondDebounce)
        {
            _timer = new Timer(SendMessage, false, Timeout.Infinite, Timeout.Infinite);
            _serverSaveTimer = new Timer(SendMessage, true, Timeout.Infinite, Timeout.Infinite);
            _dict = new ConcurrentDictionary<string, object>();
            _serverDict = new ConcurrentDictionary<string, object>();
            _milliSecondDebounce = _milliSecondDebounce;
            _id = id;
        }


        /// <summary>
        /// call this method to add an object to be updated on other clients.
        /// This should use the same keys as the database uses.  
        /// If you dont know what those keys are, check the NusysConstants class or ask somebody. 
        /// The update will happen within *debouncing_time* milliseconds max.  
        /// Save will occur *delay_time* milliseconds after interactions stop.
        /// 
        /// When adding enum's here, see if you need to add it as an enum, or the stringified version of that enum.  
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void Add(string databaseKey, object value)
        {
            //if we are not already timing
            if (!_timing)
            {
                //start timeing
                _timing = true;
                _dict.TryAdd(databaseKey, value);
                //add to the dictionary and server dictionary the latest values
                if (_serverDict.ContainsKey(databaseKey))
                {
                    _serverDict[databaseKey] = value;
                }
                else
                {
                    _serverDict.TryAdd(databaseKey, value);
                }
                //update the timers to start timing
                _timer?.Change(_milliSecondDebounce, _milliSecondDebounce);
                _serverSaveTimer?.Change(_milliSecondServerSaveDelay, _milliSecondServerSaveDelay);
            }
            else
            {
                //add the values to the dictionary
                if (_dict.ContainsKey(databaseKey))
                {
                    _dict[databaseKey] = value;
                    _serverDict[databaseKey] = value;
                }
                else
                {
                    _dict.TryAdd(databaseKey, value);
                    _serverDict.TryAdd(databaseKey, value);
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

            Message messageToSend;
            if (!saveToServer)
            {
                messageToSend = new Message(_dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _dict.Clear();
            }
            else
            {
                messageToSend = new Message(_serverDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                _serverSaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _serverDict.Clear();
            }

            SendToServer(messageToSend, saveToServer, _id);//call the virtual method that should actually send the update request for each sub classs

            _timing = false;
        }

        /// <summary>
        /// this virtual method is called whenever the timer expires for this object's debouncing.
        /// The parameters will tell you what properties to update, whether to save them, and the id of the object that should be updated. 
        /// Every subclass of debouncing dictionary should override this method.
        /// </summary>
        /// <param name="message">
        /// The message that should be updated to the server for this object.
        /// </param>
        /// <param name="shouldSave">
        /// a boolean representing whether this message should be saved or just forwarded to other clients
        /// </param>
        /// <param name="objectId">
        /// The id of the object used to instantiate this debouncing dictionary. 
        /// The exact type of id this is will depend on the sub class of the abstract debouncing dictinary you are using.  
        /// </param>
        /// <returns></returns>
        protected virtual async Task SendToServer(Message message, bool shouldSave, string objectId){}
    }
}
