namespace NusysIntermediate
{
    public abstract class Region : LibraryElementModel
    {

        public string ClippingParentId { get; set; }

        public Region(string libraryElementId, NusysConstants.ElementType type) : base(libraryElementId, type)
        {

        }
        

    }
}