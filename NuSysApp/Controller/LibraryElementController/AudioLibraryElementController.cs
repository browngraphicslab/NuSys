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
            startTime = Math.Min(Math.Max(startTime, 0), 1-Constants.MinimumVideoAndAudioDuration);
            AudioLibraryElementModel.NormalizedStartTime = startTime;
            if (startTime + AudioLibraryElementModel.NormalizedDuration > 1)
            {
                SetDuration(1-startTime);
            }
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
            duration = Math.Max(Math.Min(duration,1-AudioLibraryElementModel.NormalizedStartTime), Constants.MinimumVideoAndAudioDuration);
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
