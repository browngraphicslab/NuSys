using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class LibraryElementModel
    {
        public HashSet<Keyword> Keywords { get; set; }

        public NusysConstants.ElementType Type { get; set; }

        public string LibraryElementId { get; set; }
        public string ContentDataModelId { get; set; }

        public string Title { get; set; }

        public bool Favorited { set; get; }
        public string LargeIconUrl { get; set; }
        public string MediumIconUrl { get; set; }
        public string SmallIconUrl { get; set; }
        public ConcurrentDictionary<string, MetadataEntry> Metadata { get; set; }

        /// <summary>
        /// the UserId of the person who made this libraryElementModel
        /// </summary>
        public string Creator { set; get; }

        public string Timestamp
        {
            get;
            set;
        }

        public string LastEditedTimestamp { get; set; }

        public string ServerUrl { get; set; }

        /// the access type enum for this library element.  
        /// If it is private and the creator of the library element wasn't the current user, the user shouldn't be able to see it
        public NusysConstants.AccessType AccessType { get; set; }
        public LibraryElementModel(string libraryElementId, NusysConstants.ElementType elementType)
        {
            LibraryElementId = libraryElementId;
            Type = elementType;
        }

        /// <summary>
        /// to be called directly after SQL queries on the server end.  
        /// Will unpack from a message of all properties
        /// </summary>
        /// <param name="message"></param>
        public virtual void UnPackFromDatabaseKeys(Message message)
        {
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY))
            {
               Title = message.GetString(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY))
            {
                ContentDataModelId = message.GetString(NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY))
            {
                Favorited = message.GetBool(NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY))
            {
                Keywords = message.GetHashSet<Keyword>(NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY))
            {
                LargeIconUrl = message.GetString(NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY))
            {
                MediumIconUrl = message.GetString(NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY))
            {
                SmallIconUrl = message.GetString(NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_METADATA_KEY))
            {
                Metadata = new ConcurrentDictionary<string, MetadataEntry>(message.GetDict<string, MetadataEntry>(NusysConstants.LIBRARY_ELEMENT_METADATA_KEY));
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_CREATOR_USER_ID_KEY))
            {
                Creator = message.GetString(NusysConstants.LIBRARY_ELEMENT_CREATOR_USER_ID_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY))
            {
                Timestamp = message.GetString(NusysConstants.LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY))
            {
                LastEditedTimestamp = message.GetString(NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY);
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY))
            {
                AccessType = message.GetEnum<NusysConstants.AccessType>(NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY);
            }
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
