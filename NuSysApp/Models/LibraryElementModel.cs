using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public HashSet<string> Keywords {get; set; }
        public HashSet<Region> Regions { get; set; }

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
            Regions = new HashSet<Region>();
            SessionController.Instance.OnEnterNewCollection += OnSessionControllerEnterNewCollection;
        }
        public void UnPack(Message message)
        {
            if (message.ContainsKey("keywords"))
            {
                Keywords = new HashSet<string>(message.GetList<string>("keywords"));
            }
        }
        protected virtual void OnSessionControllerEnterNewCollection()
        {
            Data = null;
        }
    }
}
