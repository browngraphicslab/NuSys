using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;

namespace NuSysApp
{
    public class AudioLibraryElementController : LibraryElementController
    {
        /// <summary>
        /// event fired whenever the start time changes
        /// </summary>
        public event RegionTimeChangedEventHandler TimeChanged;
        public delegate void RegionTimeChangedEventHandler(object sender, double start);

        /// <summary>
        /// event fired whenever the duration changes
        /// </summary>
        public EventHandler<double> DurationChanged;

        public AudioLibraryElementModel AudioLibraryElementModel
        {
            get { return base.LibraryElementModel  as AudioLibraryElementModel; }
        }
        public AudioLibraryElementController(AudioLibraryElementModel model) : base(model)
        {

        }
        public void SetStartTime(double startTime)
        {
            AudioLibraryElementModel.NormalizedStartTime = startTime;
            TimeChanged?.Invoke(this, AudioLibraryElementModel.NormalizedStartTime);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.AUDIO_LIBRARY_ELEMENT_START_TIME_KEY, AudioLibraryElementModel.NormalizedStartTime);
            }
        }

        /// <summary>
        /// setter for the normalized duration
        /// </summary>
        /// <param name="duration"></param>
        public void SetDuration(double duration)
        {
            AudioLibraryElementModel.NormalizedDuration = duration;
            DurationChanged?.Invoke(this,duration);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.AUDIO_LIBRARY_ELEMENT_DURATION_KEY, duration);
            }
        }
        public override void UnPack(Message message)
        {
            SetBlockServerBoolean(true);
            if (message.ContainsKey(NusysConstants.AUDIO_LIBRARY_ELEMENT_START_TIME_KEY))
            {
                SetStartTime(message.GetDouble(NusysConstants.AUDIO_LIBRARY_ELEMENT_START_TIME_KEY));
            }
            if (message.ContainsKey(NusysConstants.AUDIO_LIBRARY_ELEMENT_DURATION_KEY))
            {
                SetDuration(message.GetDouble(NusysConstants.AUDIO_LIBRARY_ELEMENT_DURATION_KEY));
            }
            base.UnPack(message);
            SetBlockServerBoolean(false);
        }
    }
}
