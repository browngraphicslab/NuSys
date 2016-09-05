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
        public event RegionTimeChangedEventHandler TimeChanged;
        public delegate void RegionTimeChangedEventHandler(object sender, double start, double end);

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
            TimeChanged?.Invoke(this, AudioLibraryElementModel.NormalizedStartTime, AudioLibraryElementModel.NormalizedEndTime);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.AUDIO_LIBRARY_ELEMENT_START_TIME_KEY, AudioLibraryElementModel.NormalizedStartTime);
            }
        }
        public void SetEndTime(double endTime)
        {
            AudioLibraryElementModel.NormalizedEndTime = endTime;
            TimeChanged?.Invoke(this, AudioLibraryElementModel.NormalizedStartTime, AudioLibraryElementModel.NormalizedEndTime);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.AUDIO_LIBRARY_ELEMENT_END_TIME_KEY, AudioLibraryElementModel.NormalizedEndTime);
            }
        }
        public override void UnPack(Message message)
        {
            SetBlockServerBoolean(true);
            if (message.ContainsKey(NusysConstants.AUDIO_LIBRARY_ELEMENT_START_TIME_KEY))
            {
                SetStartTime(message.GetDouble(NusysConstants.AUDIO_LIBRARY_ELEMENT_START_TIME_KEY));
            }
            if (message.ContainsKey(NusysConstants.AUDIO_LIBRARY_ELEMENT_END_TIME_KEY))
            {
                SetEndTime(message.GetDouble(NusysConstants.AUDIO_LIBRARY_ELEMENT_END_TIME_KEY));
            }
            base.UnPack(message);
            SetBlockServerBoolean(false);
        }
    }
}
