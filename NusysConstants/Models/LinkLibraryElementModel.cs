namespace NusysIntermediate
{
    public class LinkLibraryElementModel: LibraryElementModel
    {
        public string InAtomId { get; set; }
        public string OutAtomId { get; set; }

        //public Color Color { get; set; }
        public LinkLibraryElementModel(string id): base(id, NusysConstants.ElementType.Link)
        {
        }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            if (message.ContainsKey(NusysConstants.LINK_LIBRARY_ELEMENT_IN_ID_KEY))
            {
                InAtomId = message.GetString(NusysConstants.LINK_LIBRARY_ELEMENT_IN_ID_KEY);
            }
            if (message.ContainsKey(NusysConstants.LINK_LIBRARY_ELEMENT_OUT_ID_KEY))
            {
                OutAtomId = message.GetString(NusysConstants.LINK_LIBRARY_ELEMENT_OUT_ID_KEY);
            }
            base.UnPackFromDatabaseKeys(message);
        }
    }
}
