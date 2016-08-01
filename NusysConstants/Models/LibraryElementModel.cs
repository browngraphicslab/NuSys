using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class LibraryElementModel
    {
        public HashSet<Keyword> Keywords {get; set; }

        public NusysConstants.ElementType Type { get; set; }

        public string LibraryElementId { get; set; }
        public string ContentDataModelId { get; set; }

        public string Title { get; set; }

        public bool Favorited { set; get; }
        public string LargeIconUrl { get; set; }
        public string MediumIconUrl { get; set; }
        public string SmallIconUrl { get; set; }
        public ConcurrentDictionary<string, MetadataEntry> Metadata {
            get;
            set; }
        public string Creator { set; get; }
        public string Timestamp { get; set; }//TODO maybe put in a timestamp, maybe remove the field from the library

        public string LastEditedTimestamp { get; set; }

        public string ServerUrl { get; set; } 

        public LibraryElementModel(string libraryElementId, NusysConstants.ElementType elementType)
        {
            LibraryElementId = libraryElementId;
            Type = elementType;
        }
        protected virtual void OnSessionControllerEnterNewCollection()
        {
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
        * 
        * hey guys let's all be nice and do nusys.  -T
        */
    }
}
