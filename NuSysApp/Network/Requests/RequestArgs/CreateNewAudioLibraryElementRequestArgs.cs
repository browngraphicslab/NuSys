using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class CreateNewAudioLibraryElementRequestArgs : CreateNewLibraryElementRequestArgs
    {
        public CreateNewAudioLibraryElementRequestArgs() : base()
        {
            LibraryElementType = NusysConstants.ElementType.Audio;
        }

        /// <summary>
        /// the normalized start time of the audio library element being made.  
        /// Will default to 0 if not set.
        /// </summary>
        public double? StartTime { get; set; }


        /// <summary>
        /// the normalized  duration of the audio library element being made.  
        /// Will default to 1 if not set
        /// </summary>
        public double? Duration { get; set; }


        public override Message PackToRequestKeys()
        {
            var message = base.PackToRequestKeys();
            message[NusysConstants.NEW_AUDIO_LIBRARY_ELEMENT_REQUEST_TIME_START] = StartTime ?? 0;
            message[NusysConstants.NEW_AUDIO_LIBRARY_ELEMENT_REQUEST_TIME_DURATION] = Duration ?? 1;
            return message;
        }
    }
}
