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
        /// the one timer that all debouncing dictionaries will use.  This will be running whenever any debouncing dictionaries are timing.
        /// </summary>
        private static Timer _debouncingTimer = new Timer(DebouncingTimerTick, null, Timeout.Infinite, Timeout.Infinite);

        /// <summary>
        /// the list of all the debouncing dictionaries currently waiting for the timer to send the signal to save their waiting dictionaries;
        /// </summary>
        private static List<DebouncingDictionary> _debouncingDictionariesToSave = new List<DebouncingDictionary>();

        /// <summary>
        /// a queue of dictionaries waiting to be updated
        /// </summary>
        private static Queue<DebouncingDictionary> _debouncingDictionariesToUpdate = new Queue<DebouncingDictionary>();

        /// <summary>
        /// the delay that the debouncing dicitonary must be left alone before a saving update 
        /// </summary>
        private static int _milliSecondServerSaveDelay = 800;

        /// <summary>
        /// the static boolean indicating if the debouncing timer is currently timing.  
        /// </summary>
        private static bool _timing = false;

        /// <summary>
        /// this dictionary will store properties that need to be updated not but necessarily stored on the server.  
        /// </summary>
        private ConcurrentDictionary<string, object> _dict; 

        /// <summary>
        /// the time between each non-saving message sent to the server
        /// </summary>
        private static int _milliSecondDebounce = 30;

        /// <summary>
        /// the id of the object this debouncing dictionary updates
        /// </summary>
        private string _id;

        /// <summary>
        /// whether this debouncing dictionary points to a library element or an alias
        /// </summary>
        private bool _updateLibraryElement = false;

        /// <summary>
        /// the ticks when the last item to be added to this dictionary was added. 
        /// This will increase whenever a new item is added to the dictionary
        /// </summary>
        public long TicksWhenSaveTimerStarted { get; private set; }

        /// <summary>
        /// the dictionary of proprties that will be saved after the save delay timer expires
        /// </summary>
        private ConcurrentDictionary<string, object> _serverDict;

        /// <summary>
        /// this constructor takes in the id of the alias or library element we are updating.  
        /// It also takes ina bool indicating if it is updating an alias or library element
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateLibraryElement"></param>
        public DebouncingDictionary(string id)
        {
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
            _dict[databaseKey] = value; //add to the dictionary and server dictionary the latest values
            _serverDict[databaseKey] = value;

            if (!_timing)
            {
                _debouncingTimer.Change(_milliSecondDebounce, _milliSecondDebounce); //set the timer to the correct time
                _timing = true; 
            }

            //if we are not already timing
            if (!_debouncingDictionariesToUpdate.Contains(this))
            {
                _debouncingDictionariesToUpdate.Enqueue(this);//add itself
            }
            if (!_debouncingDictionariesToSave.Contains(this))
            {
                _debouncingDictionariesToSave.Add(this);
            }
            TicksWhenSaveTimerStarted = DateTime.Now.Ticks;
        }

        /// <summary>
        /// called whenever either timer expires.  
        /// the state is a boolean representing whether the request should save
        /// </summary>
        /// <param name="state"></param>
        private async void SendMessage(bool saveToServer)
        {
            Message messageToSend;
            if (!saveToServer)
            {
                messageToSend = new Message(_dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                _dict.Clear();
            }
            else
            {
                messageToSend = new Message(_serverDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                _serverDict.Clear();
            }
            Task.Run(async delegate
            {
                SendToServer(messageToSend, saveToServer, _id);
                //call the virtual method that should actually send the update request for each sub classs
            });
        }

        /// <summary>
        /// this method is called by the static Timer, DebouncingTimer.
        /// This should be called whenever a save time or an update timer is waiting to be sent.  
        /// this method will check the dictionaries waiting to update or save and will check if they have expired. 
        /// </summary>
        /// <param name="state"></param>
        private static void DebouncingTimerTick(object state)
        {
            while (_debouncingDictionariesToUpdate.Count > 0)//send message for every update timer
            {
                _debouncingDictionariesToUpdate.Dequeue()?.SendMessage(false);
            }

            foreach (var dict in _debouncingDictionariesToSave.ToList())//check every waiting save timer
            {
                if (dict?.TicksWhenSaveTimerStarted == null)
                {
                    _debouncingDictionariesToSave.Remove(dict);
                }
                else
                {
                    if (DateTime.Now.Ticks - dict.TicksWhenSaveTimerStarted > TimeSpan.TicksPerMillisecond * _milliSecondServerSaveDelay)
                    {
                        dict.SendMessage(true);
                        _debouncingDictionariesToSave.Remove(dict);
                    }
                }
            }
            if (_debouncingDictionariesToSave.Count == 0)//if theres nothing waiting, set the timeout to be infinite
            {
                _debouncingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _timing = false;
            }
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
