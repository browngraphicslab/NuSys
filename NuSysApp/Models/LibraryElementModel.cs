using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class LibraryElementModel : BaseINPC
    {
        public HashSet<Keyword> Keywords {get; set; }

        public ElementType Type { get; set; }

        public string Data
        {
            get
            {
                if (string.IsNullOrEmpty(ContentId))
                {
                    return null;
                }
                var contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(ContentId);
                Debug.Assert(contentDataModel != null);

                return contentDataModel.Data;
            }
            set
            {
                if (string.IsNullOrEmpty(ContentId))
                {
                    return;
                }
                if (!SessionController.Instance.ContentController.ContentExists(ContentId))
                {
                    SessionController.Instance.ContentController.AddContentDataModel(ContentId, value);
                }
                SessionController.Instance.ContentController.GetContentDataModel(ContentId)?.SetData(value);
            }
        }

        public string LibraryElementId { get; set; }
        public string ContentId { get; set; }

        public string Title { get; set; }

        public bool Favorited { set; get; }
        public string LargeIconUrl { get; private set; }
        public string MediumIconUrl { get; private set; }
        public string SmallIconUrl { get; private set; }
        public ConcurrentDictionary<string, MetadataEntry> Metadata {
            get;
            set; }
        public string Creator { set; get; }
        public string Timestamp { get; set; }//TODO maybe put in a timestamp, maybe remove the field from the library

        public string LastEditedTimestamp { get; set; }

        public string ServerUrl { get; set; } 
        public Dictionary<string, MetadataEntry> FullMetadata
        {
            get
            {
                var metadata = new Dictionary<string, MetadataEntry>(Metadata);
                if (!metadata.ContainsKey("Timestamp"))
                {
                    metadata.Add("Timestamp", new MetadataEntry("Timestamp", new List<string> { Timestamp }, MetadataMutability.IMMUTABLE));
                }
                if (!metadata.ContainsKey("Creator"))
                {
                    metadata.Add("Creator", new MetadataEntry("Creator", new List<string> { Creator }, MetadataMutability.IMMUTABLE));
                }
                if (!metadata.ContainsKey("Title"))
                {
                    metadata.Add("Title", new MetadataEntry("Title", new List<string> { Title }, MetadataMutability.IMMUTABLE));
                }
                if (!metadata.ContainsKey("Type"))
                {
                    metadata.Add("Type", new MetadataEntry("Type", new List<string> { Type.ToString() }, MetadataMutability.IMMUTABLE));
                }
                if (!metadata.ContainsKey("Keywords"))
                {
                    var keywords = Keywords.Select(key => key.Text);
                    metadata.Add("Keywords", new MetadataEntry("Keywords", new List<string>(keywords), MetadataMutability.IMMUTABLE));
                }
                return metadata;
            }
        }
        public LibraryElementModel(string libraryElementId, ElementType elementType, Dictionary<string, MetadataEntry> metadata = null, string contentName = null, bool favorited = false)
        {
            Data = null;
            LibraryElementId = libraryElementId;
            Title = contentName;
            Type = elementType;
            Favorited = favorited;
            Keywords = new HashSet<Keyword>();
            Metadata = new ConcurrentDictionary<string, MetadataEntry>(metadata ?? new Dictionary<string, MetadataEntry>());
            Debug.Assert(!(Type == ElementType.Link && !(this is LinkLibraryElementModel)));
        }
        //FOR PDF DOWNLOADING  --HACKY AF
        //public static List<string> PDFStrings = new List<string>();

        public virtual async Task UnPack(Message message)
        {
            if (message.GetString("title") != null)
            {
                Title = message.GetString("title");
            }
            if (message.ContainsKey("keywords"))
            {
                Keywords = new HashSet<Keyword>(message.GetList<Keyword>("keywords"));
            }
            // unused for now
            //if (message.ContainsKey("regions"))
            //{
            //    Regions = new HashSet<Region>(message.GetList<Region>("regions"));
            //}
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
            if (message.GetString("library_element_creation_timestamp") != null)
            {
                Timestamp = message.GetString("library_element_creation_timestamp");
            }
            if (message.GetString("server_url") != null)
            {
                ServerUrl = message.GetString("server_url");
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

        public void SetMetadata(Dictionary<string, MetadataEntry> metadata)
        {
            Metadata = new ConcurrentDictionary<string, MetadataEntry>(metadata);
        }
        /*
* Trent, Help ME.!!!!! He's talking about bio. What did I do to deserve this. 
* Help me. He just won't stop. Dear god what have I done. Sahil save me.
* This is my life now. I will suffer every day. Is there no way to escape this hell?
* 
* Not nice Grant!!! I was just trying to be helpful and friendly! - Z
*/
    }
}
