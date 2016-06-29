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
        public HashSet<Keyword> Keywords {get; set; }
        public HashSet<Region> Regions { get; set; }

        public ElementType Type { get; set; }

        public string Data{ get;set; }

        public string LibraryElementId { get; set; }
        public string Title { get; set; }

        public bool Favorited { set; get; }
        public string LargeIconUrl { get; private set; }
        public string MediumIconUrl { get; private set; }
        public string SmallIconUrl { get; private set; }
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
            Keywords = new HashSet<Keyword>();
            Metadata = metadata;
            Regions = new HashSet<Region>();
            SessionController.Instance.OnEnterNewCollection += OnSessionControllerEnterNewCollection;
        }
        //public static List<string> PDFStrings = new List<string>();
        public async Task UnPack(Message message)
        {
            if (message.ContainsKey("keywords"))
            {
                Keywords = new HashSet<Keyword>(message.GetList<Keyword>("keywords"));
            }
            if (message.ContainsKey("regions"))
            {
                Regions = new HashSet<Region>(message.GetList<Region>("regions"));
            }
            if (message.GetString("small_thumbnail_url") != null)
            {
                SmallIconUrl = message.GetString("small_thumbnail_url");
            }
            if (message.GetString("medium_thumbnail_url") != null)
            {
                MediumIconUrl = message.GetString("medium_thumbnail_url");
            }
            if (message.GetString("large_thumbnail_url") != null)
            {
                LargeIconUrl = message.GetString("large_thumbnail_url");
            }
            //TO DOWNLOAD PDFS
            /*
            if (Type == ElementType.PDF)
            {
                PDFStrings.Add(LibraryElementId);
            }*/
        }
        protected virtual void OnSessionControllerEnterNewCollection()
        {
            Data = null;
        }
    }
}
