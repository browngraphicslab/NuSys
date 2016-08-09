namespace NusysIntermediate
{
    public class VideoRegionModel : RectangleRegion
    {
        public VideoRegionModel(string libraryId) : base(libraryId, NusysConstants.ElementType.VideoRegion)
        {
        }
        public double Start { get; set; }
        public double End { get; set; }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            base.UnPackFromDatabaseKeys(message);
            if (message.ContainsKey(NusysConstants.VIDEO_REGION_START_KEY))
            {
                Start = message.GetDouble(NusysConstants.VIDEO_REGION_START_KEY);
            }
            if (message.ContainsKey(NusysConstants.VIDEO_REGION_END_KEY))
            {
                End = message.GetDouble(NusysConstants.VIDEO_REGION_END_KEY);
            }
        }
    }
}
