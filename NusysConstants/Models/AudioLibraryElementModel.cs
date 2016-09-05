using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class AudioLibraryElementModel : LibraryElementModel
    {
        /// <summary>
        /// the NORMALIZED start time
        /// </summary>
        public double NormalizedStartTime { get; set; }

        /// <summary>
        /// the NORMALIZED end time
        /// </summary>
        public double NormalizedEndTime { get; set; }
        
        public AudioLibraryElementModel(string libraryId, NusysConstants.ElementType elementType =  NusysConstants.ElementType.Audio) : base(libraryId, elementType)
        {
        }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            base.UnPackFromDatabaseKeys(message); //TODO fix
            if (message.ContainsKey(NusysConstants.AUDIO_LIBRARY_ELEMENT_START_TIME_KEY))
            {
                NormalizedStartTime = message.GetDouble(NusysConstants.AUDIO_LIBRARY_ELEMENT_START_TIME_KEY);
            }
            if (message.ContainsKey(NusysConstants.AUDIO_LIBRARY_ELEMENT_END_TIME_KEY))
            {
                NormalizedEndTime = message.GetDouble(NusysConstants.AUDIO_LIBRARY_ELEMENT_END_TIME_KEY);
            }
        }
    }
}
