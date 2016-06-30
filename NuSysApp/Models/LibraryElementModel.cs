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
        public Dictionary<string, MetadataEntry> Metadata { get; set; }
        public string Creator { set; get; }
        public string Timestamp { get; set; }//TODO maybe put in a timestamp, maybe remove the field from the library

        public string ServerUrl { get; set; }
       
        public LibraryElementModel(string id, ElementType elementType, Dictionary<string, MetadataEntry> metadata = null, string contentName = null, bool favorited = false)
        {
            Data = null;
            LibraryElementId = id;
            Title = contentName;
            Type = elementType;
            Favorited = favorited;
            Keywords = new HashSet<Keyword>();
            Metadata = metadata ?? new Dictionary<string, MetadataEntry>();
            Regions = new HashSet<Region>();
            if (Type == ElementType.Link)
            {
                
            }
            SessionController.Instance.OnEnterNewCollection += OnSessionControllerEnterNewCollection;
            

        }
        //FOR PDF DOWNLOADING  --HACKY AF
        //public static List<string> PDFStrings = new List<string>();

        public virtual async Task UnPack(Message message)
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
            if (message.GetString("creator_user_id") != null)
            {
                Creator = message.GetString("creator_user_id");
            }
            //TO DOWNLOAD PDFS
            /*
            if (Type == ElementType.PDF)
            {
                PDFStrings.Add(LibraryElementId);
            }*/

            //ADD IMMUTABLE DATA TO METADATA, so they can show up in md editor
            if (!Metadata.ContainsKey("Timestamp"))
            {
                Metadata.Add("Timestamp", new MetadataEntry("Timestamp", new List<string> {Timestamp}, MetadataMutability.IMMUTABLE));
            }
            if (!Metadata.ContainsKey("Creator"))
            {
                Metadata.Add("Creator", new MetadataEntry("Creator", new List<string> {Creator}, MetadataMutability.IMMUTABLE));
            }
            if (!Metadata.ContainsKey("Title"))
            {
                Metadata.Add("Title", new MetadataEntry("Title", new List<string> {Title}, MetadataMutability.IMMUTABLE));
            }
            if (!Metadata.ContainsKey("Type"))
            {
                Metadata.Add("Type", new MetadataEntry("Type", new List<string> {Type.ToString()}, MetadataMutability.IMMUTABLE));
            }
        }
        protected virtual void OnSessionControllerEnterNewCollection()
        {
            Data = null;
        }
        /*
         * Trent, Help ME.!!!!! He's talking about bio. What did I do to deserve this. 
         * Help me. He just won't stop. Dear god what have I done. Sahil save me.
         * This is my life now. I will suffer every day. Is there no way to escape this hell?
         * 
         */
    }
}
