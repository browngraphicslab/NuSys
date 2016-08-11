namespace NusysIntermediate
{
    public abstract class Region : LibraryElementModel
    {

        public string ClippingParentId { get; set; }

        public Region(string libraryElementId, NusysConstants.ElementType type) : base(libraryElementId, type)
        {

        }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            if (message.ContainsKey(NusysConstants.REGION_CLIPPING_PARENT_ID_KEY))
            {
                ClippingParentId = message.GetString(NusysConstants.REGION_CLIPPING_PARENT_ID_KEY);
            }
            base.UnPackFromDatabaseKeys(message);
        }
    }
}