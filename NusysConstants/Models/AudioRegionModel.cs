namespace NusysIntermediate
{
    public class AudioRegionModel: Region
    {
        public double Start { set; get; }
        public double End { set; get; }

        public delegate void TimeChangeHandler();
        public event TimeChangeHandler OnTimeChange;


        public AudioRegionModel(string libraryId) : base(libraryId, NusysConstants.ElementType.AudioRegion)
        {
        }
        public override void UnPackFromDatabaseKeys(Message message)
        {
            base.UnPackFromDatabaseKeys(message);
            if (message.ContainsKey(NusysConstants.AUDIO_REGION_START_KEY))
            {
                Start = message.GetDouble(NusysConstants.AUDIO_REGION_START_KEY);
            }
            if (message.ContainsKey(NusysConstants.AUDIO_REGION_END_KEY))
            {
                End = message.GetDouble(NusysConstants.AUDIO_REGION_END_KEY);
            }
        }
    }
}
