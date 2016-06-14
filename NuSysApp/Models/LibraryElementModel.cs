
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Diagnostics;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class LibraryElementModel : BaseINPC
    {

        public HashSet<string> Keywords { get; set; }
        public HashSet<string> Regions { get; set; }
        public ElementType Type { get; set; }

        public string Data{ get;set; }

        public string LibraryElementId { get; set; }
        public string Title { get; set; }

        public bool Favorited { set; get; }

        public Dictionary<string, Tuple<string, Boolean>> Metadata { get; set; }

        public string Creator { set; get; }
        public string Timestamp { get; set; }//TODO maybe put in a timestamp, maybe remove the field from the library

        public string ServerUrl { get; set; }
       
        public LibraryElementModel(string id, ElementType elementType, Dictionary<string, Tuple<string,Boolean>> metadata = null, string contentName = null, bool favorited = false)
        {
            Data = null;
            LibraryElementId = id;
            Title = contentName;
            Type = elementType;
            Favorited = favorited;
            Keywords = new HashSet<string>();
            Metadata = metadata;
            SessionController.Instance.OnEnterNewCollection += OnSessionControllerEnterNewCollection;
        }
        /// <summary>
        /// Checks if entry is valid, then adds its data to the Metadata dictionary and sends the updated dictionary to the server.
        /// </summary>
        /// <param name="entry"></param>
      
        public void AddMetadata(MetadataEntry entry)
        {
            //Keys should be unique; values obviously don't have to be.
            if (Metadata.ContainsKey(entry.Key) || string.IsNullOrEmpty(entry.Value) || string.IsNullOrEmpty(entry.Value) || string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                return;

            Metadata.Add(entry.Key, new Tuple<string, bool>(entry.Value, entry.Mutability));
            Task.Run(async delegate
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };

                var m = new Message();
                m["contentId"] = LibraryElementId;
                m["metadata"] = JsonConvert.SerializeObject(Metadata, settings);
                var request = new ChangeContentRequest(m);
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            });
            //OnMetadataChanged?.Invoke(this);
        }
        /// <summary>
        /// Checks if the key string is valid, then updates the metadata dictionary and sends a message to the server with the new dictionary.
        /// </summary>
        /// <param name="k"></param>
        public void RemoveMetadata(String k)
        {
            if (string.IsNullOrEmpty(k) || !Metadata.ContainsKey(k) || string.IsNullOrWhiteSpace(k))
                return;

            Metadata.Remove(k);

            Task.Run(async delegate
            {
                var m = new Message();
                m["contentId"] = LibraryElementId;
                m["metadata"] = JsonConvert.SerializeObject(Metadata);
                var request = new ChangeContentRequest(m);
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            });

            //OnMetadataChanged?.Invoke(this);

        }

        protected virtual void OnSessionControllerEnterNewCollection()
        {
            Data = null;
        }
        public long GetTimestampTicks()
        {
            if (!String.IsNullOrEmpty(Timestamp))
            {
                try
                {
                    return DateTime.Parse(Timestamp).Ticks;
                }
                catch (Exception e)
                {
                    return 0;
                }
            }

            return 0;
        }
    }
}
