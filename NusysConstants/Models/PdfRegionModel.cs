namespace NusysIntermediate
{
    public class PdfRegionModel : RectangleRegion 
    {

        public int PageLocation { get; set; }
        public PdfRegionModel(string libraryId) : base(libraryId, NusysConstants.ElementType.PdfRegion)
        {
        }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            base.UnPackFromDatabaseKeys(message);
            if (message.ContainsKey(NusysConstants.PDF_REGION_PAGE_NUMBER_KEY))
            {
                PageLocation = message.GetInt(NusysConstants.PDF_REGION_PAGE_NUMBER_KEY);
            }
        }
    }
}
